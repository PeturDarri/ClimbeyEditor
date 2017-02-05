using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RuntimeGizmos;
using UnityEngine;

public class SelectBounds : MonoBehaviour
{
    public static bool ShowBounds = true;
    public static bool ShowHandles;
    public Color LineColor;
    public Material lineMaterial;
    public Texture HandleTexture;

    private Bounds bounds;
    private TransformSpace transformSpace = TransformSpace.Global;
    private Vector3 localRotation;

    //Handles
    //private Vector3 handleFront, handleBehind, handleRight, handleLeft, handleTop, handleBottom;
    private List<Vector3> handles = new List<Vector3>();
    private Vector3 selectedHandle;
    private float minHandleDistance = 8;
    private float moveSpeedMultiplier = 1;
    private float scaleSpeedMultiplier = 1;
    private bool symmetrical;
    private enum MoveType
    {
        Stretch,
        Move
    };
    private MoveType moveType = MoveType.Stretch;

    private Transform select
    {
        get { return SelectionManager.instance.transform; }
    }

    private void OnGUI()
    {
        if (!ShowHandles)
            return;
        //Draw handles
        var sortedHandles = handles.OrderBy(o => Vector3.Distance(o, Camera.main.transform.position))
            .ToList();
        sortedHandles.Reverse();

        for (int i = 0; i < sortedHandles.Count; i++)
        {
            var handle = sortedHandles[i];

            if (handle == Vector3.zero)
                break;
            var handlePos = Camera.main.WorldToScreenPoint(handle);
            handlePos.y = Screen.height - handlePos.y;
            var pos = new Rect(new Vector2(handlePos.x - 4, handlePos.y - 4), new Vector2(8, 8));
            GUI.color = new Color(1, 1, 1, 1 * ((float) (i + 1) / sortedHandles.Count));
            GUI.DrawTexture(pos, HandleTexture);
        }
    }

    private void Update()
    {
        ShowHandles = GetComponent<TransformGizmo>().type == TransformType.Bounds && !SelectionManager.instance.isEmpty;
        transformSpace = GetComponent<TransformGizmo>().space;

        symmetrical = Input.GetKey(KeyCode.LeftShift);

        if (ShowHandles)
        {
            UpdateHandles();
            if (!GetComponent<TransformGizmo>().isTransforming)
                GetHandle();
            ScaleHandle();
        }
    }

    private void LateUpdate()
    {
        if (ShowBounds)
        {
            UpdateBounds();
        }
    }

    private void UpdateBounds()
    {
        bounds = SelectionManager.instance.GetBounds();
    }

    private void UpdateHandles()
    {
        var newBounds = SelectionManager.instance.Selection.Count == 1
            ? GetBoundsWithoutRotation(SelectionManager.instance.Selection[0].transform)
            : bounds;
        //Calculate handle locations
        handles.Clear();
        handles.Add(RotatePointAroundPivot(select.position + new Vector3(0, 0, newBounds.extents.z), select.position, select.eulerAngles));
        handles.Add(RotatePointAroundPivot(select.position + new Vector3(0, 0, -newBounds.extents.z), select.position, select.eulerAngles));
        handles.Add(RotatePointAroundPivot(select.position + new Vector3(newBounds.extents.x, 0, 0), select.position, select.eulerAngles));
        handles.Add(RotatePointAroundPivot(select.position + new Vector3(-newBounds.extents.x, 0, 0), select.position, select.eulerAngles));
        handles.Add(RotatePointAroundPivot(select.position + new Vector3(0, newBounds.extents.y, 0), select.position, select.eulerAngles));
        handles.Add(RotatePointAroundPivot(select.position + new Vector3(0, -newBounds.extents.y, 0), select.position, select.eulerAngles));
    }

    private void GetHandle()
    {
        if (Input.GetMouseButton(0))
        {
            selectedHandle = Vector3.zero;

            //Get shortest distance
            var mousePos = Input.mousePosition;
            var closestDistance = minHandleDistance + 1;
            var i = -1;
            foreach (var handle in handles)
            {
                var distance = Vector2.Distance(Camera.main.WorldToScreenPoint(handle), mousePos);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    i = handles.IndexOf(handle);
                }
            }

            if (i >= 0)
            {
                selectedHandle = handles[i];
                moveType = MoveType.Stretch;
            }
            else
            {
                selectedHandle = Vector3.right;
                moveType = MoveType.Move;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            selectedHandle = Vector3.zero;
        }
    }

    private void ScaleHandle()
    {
        if(selectedHandle != Vector3.zero && Input.GetMouseButtonDown(0))
        {
            StartCoroutine(ScaleHandleRoutine());
        }
    }

    private IEnumerator ScaleHandleRoutine()
    {
        var target = select;
        var originalTargetPosition = target.position;
        var planeNormal = (transform.position - target.position).normalized;
        var axis = (selectedHandle - target.position).normalized;
        var projectedAxis = Vector3.ProjectOnPlane(axis, planeNormal).normalized;
        var previousMousePosition = Vector3.zero;
        var emptySelection = SelectionManager.instance.emptySelection;
        GetComponent<TransformGizmo>().isTransforming = true;

        while (!Input.GetMouseButtonUp(0) && CameraManager.instance.cameraState == CameraManager.CameraState.Free)
        {
            var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            var mousePosition = Geometry.LinePlaneIntersect(mouseRay.origin, mouseRay.direction,
                originalTargetPosition, planeNormal);

            if (previousMousePosition != Vector3.zero && mousePosition != Vector3.zero)
            {
                if (moveType == MoveType.Move)
                {
                    projectedAxis = (mousePosition - previousMousePosition).normalized;
                    var moveAmount = ExtVector3.MagnitudeInDirection(mousePosition - previousMousePosition, projectedAxis) * moveSpeedMultiplier;
                    emptySelection.Translate(projectedAxis * moveAmount, Space.World);
                }
                else
                {
                    axis = transformSpace == TransformSpace.Global ? axis : (RotatePointAroundPivot(selectedHandle, originalTargetPosition, -select.eulerAngles) - originalTargetPosition).normalized;
                    projectedAxis = Vector3.ProjectOnPlane(axis, planeNormal).normalized;
                    var scaleAmount =
                        ExtVector3.MagnitudeInDirection(mousePosition - previousMousePosition, projectedAxis) *
                        scaleSpeedMultiplier;
                    var positive = new Vector3(Mathf.Abs(axis.x), Mathf.Abs(axis.y), Mathf.Abs(axis.z));
                    emptySelection.localScale += (positive * scaleAmount);

                    if (!symmetrical)
                    {
                        emptySelection.position +=(selectedHandle - originalTargetPosition).normalized * scaleAmount / 2;
                    }
                    else
                    {
                        emptySelection.position = originalTargetPosition;
                    }
                }
            }

            previousMousePosition = mousePosition;
            yield return null;
        }
        GetComponent<TransformGizmo>().isTransforming = false;
    }

    private Bounds GetBoundsWithoutRotation(Transform obj)
    {
        var prevRot = obj.rotation;
        obj.rotation = Quaternion.identity;
        var bound = obj.GetComponent<Collider>().bounds;
        obj.rotation = prevRot;
        return bound;
    }

    private static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) {
        var dir = point - pivot; // get point direction relative to pivot
        dir = Quaternion.Euler(angles) * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }

    private void OnPostRender()
    {
        // set the current material
        lineMaterial.SetPass( 0 );

        GL.Begin( GL.LINES );

        if(ShowBounds && !SelectionManager.instance.isEmpty)
        {
            GL.Color(LineColor);
            var selectBounds = bounds;
            selectBounds.center = select.position;
            if (SelectionManager.instance.Selection.Count > 1)
                DrawCube(selectBounds);

            foreach (var child in SelectionManager.instance.Selection)
            {
                var childBounds = GetBoundsWithoutRotation(child.transform);
                DrawCube(childBounds, child.transform.eulerAngles);
            }
        }

        GL.End();
    }

    private static void DrawCube(Bounds cube, Vector3 rotation = new Vector3())
    {
        //Calculate 8 corners and save
        var bLeft1 = new Vector3(-cube.extents.x, -cube.extents.y, -cube.extents.z);
        var tLeft1 = new Vector3(-cube.extents.x, +cube.extents.y, -cube.extents.z);
        var bRight1 = new Vector3(+cube.extents.x, -cube.extents.y, -cube.extents.z);
        var tRight1 = new Vector3(+cube.extents.x, +cube.extents.y, -cube.extents.z);
        var bLeft2 = new Vector3(-cube.extents.x, -cube.extents.y, +cube.extents.z);
        var tLeft2 = new Vector3(-cube.extents.x, +cube.extents.y, +cube.extents.z);
        var bRight2 = new Vector3(+cube.extents.x, -cube.extents.y, +cube.extents.z);
        var tRight2 = new Vector3(+cube.extents.x, +cube.extents.y, +cube.extents.z);

        var center = cube.center;

        //bLeft1
        GL.Vertex(RotatePointAroundPivot(center + bLeft1, center, rotation));
        GL.Vertex(RotatePointAroundPivot(center + tLeft1, center, rotation));

        GL.Vertex(RotatePointAroundPivot(center + bLeft1, center, rotation));
        GL.Vertex(RotatePointAroundPivot(center + bRight1, center, rotation));

        GL.Vertex(RotatePointAroundPivot(center + bRight1, center, rotation));
        GL.Vertex(RotatePointAroundPivot(center + tRight1, center, rotation));

        GL.Vertex(RotatePointAroundPivot(center + bLeft1, center, rotation));
        GL.Vertex(RotatePointAroundPivot(center + bLeft2, center, rotation));

        GL.Vertex(RotatePointAroundPivot(center + tLeft1, center, rotation));
        GL.Vertex(RotatePointAroundPivot(center + tRight1, center, rotation));

        GL.Vertex(RotatePointAroundPivot(center + tLeft1, center, rotation));
        GL.Vertex(RotatePointAroundPivot(center + tLeft2, center, rotation));

        //tRight2
        GL.Vertex(RotatePointAroundPivot(center + tRight2, center, rotation));
        GL.Vertex(RotatePointAroundPivot(center + bRight2, center, rotation));

        GL.Vertex(RotatePointAroundPivot(center + tRight2, center, rotation));
        GL.Vertex(RotatePointAroundPivot(center + tLeft2, center, rotation));

        GL.Vertex(RotatePointAroundPivot(center + tLeft2, center, rotation));
        GL.Vertex(RotatePointAroundPivot(center + bLeft2, center, rotation));

        GL.Vertex(RotatePointAroundPivot(center + tRight2, center, rotation));
        GL.Vertex(RotatePointAroundPivot(center + tRight1, center, rotation));

        GL.Vertex(RotatePointAroundPivot(center + bRight2, center, rotation));
        GL.Vertex(RotatePointAroundPivot(center + bLeft2, center, rotation));

        GL.Vertex(RotatePointAroundPivot(center + bRight2, center, rotation));
        GL.Vertex(RotatePointAroundPivot(center + bRight1, center, rotation));
    }
}
