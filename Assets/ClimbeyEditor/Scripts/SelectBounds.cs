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
    private Vector3 bLeft1, tLeft1, bRight1, tRight1, bLeft2, tLeft2, bRight2, tRight2;
    private Vector3 localRotation;

    //Handles
    //private Vector3 handleFront, handleBehind, handleRight, handleLeft, handleTop, handleBottom;
    private List<Vector3> handles = new List<Vector3>();
    private Vector3 selectedHandle;
    private float minHandleDistance = 8;
    private float moveSpeedMultiplier = 1;
    private float scaleSpeedMultiplier = 1;
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

    // Use this for initialization
	private void Awake()
	{
		
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
        bounds = SelectionManager.instance.SelectBounds;

        //Calculate 8 corners and save
        bLeft1 = new Vector3(-bounds.extents.x, -bounds.extents.y, -bounds.extents.z);
        tLeft1 = new Vector3(-bounds.extents.x, +bounds.extents.y, -bounds.extents.z);
        bRight1 = new Vector3(+bounds.extents.x, -bounds.extents.y, -bounds.extents.z);
        tRight1 = new Vector3(+bounds.extents.x, +bounds.extents.y, -bounds.extents.z);
        bLeft2 = new Vector3(-bounds.extents.x, -bounds.extents.y, +bounds.extents.z);
        tLeft2 = new Vector3(-bounds.extents.x, +bounds.extents.y, +bounds.extents.z);
        bRight2 = new Vector3(+bounds.extents.x, -bounds.extents.y, +bounds.extents.z);
        tRight2 = new Vector3(+bounds.extents.x, +bounds.extents.y, +bounds.extents.z);
    }

    private void UpdateHandles()
    {
        //Calculate handle locations
        handles.Clear();
        handles.Add(RotatePointAroundPivot(select.position + new Vector3(0, 0, bounds.extents.z), select.position, select.eulerAngles));
        handles.Add(RotatePointAroundPivot(select.position + new Vector3(0, 0, -bounds.extents.z), select.position, select.eulerAngles));
        handles.Add(RotatePointAroundPivot(select.position + new Vector3(bounds.extents.x, 0, 0), select.position, select.eulerAngles));
        handles.Add(RotatePointAroundPivot(select.position + new Vector3(-bounds.extents.x, 0, 0), select.position, select.eulerAngles));
        handles.Add(RotatePointAroundPivot(select.position + new Vector3(0, bounds.extents.y, 0), select.position, select.eulerAngles));
        handles.Add(RotatePointAroundPivot(select.position + new Vector3(0, -bounds.extents.y, 0), select.position, select.eulerAngles));
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

    IEnumerator ScaleHandleRoutine()
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
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 mousePosition = Geometry.LinePlaneIntersect(mouseRay.origin, mouseRay.direction,
                originalTargetPosition, planeNormal);

            if (previousMousePosition != Vector3.zero && mousePosition != Vector3.zero)
            {
                if (moveType == MoveType.Move)
                {
                    projectedAxis = (mousePosition - previousMousePosition).normalized;
                    float moveAmount = ExtVector3.MagnitudeInDirection(mousePosition - previousMousePosition, projectedAxis) * moveSpeedMultiplier;
                    emptySelection.Translate(projectedAxis * moveAmount, Space.World);
                }
                else
                {
                    Vector3 projected = projectedAxis;
                    float scaleAmount =
                        ExtVector3.MagnitudeInDirection(mousePosition - previousMousePosition, projected) *
                        scaleSpeedMultiplier;
                    var positive = new Vector3(Mathf.Abs(axis.x), Mathf.Abs(axis.y), Mathf.Abs(axis.z));
                    emptySelection.localScale += (positive * scaleAmount);
                    emptySelection.position += (axis * scaleAmount) / 2;
                }
            }

            previousMousePosition = mousePosition;
            yield return null;
        }
        GetComponent<TransformGizmo>().isTransforming = false;
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

            var center = SelectionManager.instance.transform.position;
            //bLeft1
            GL.Vertex(RotatePointAroundPivot(center + (bounds.center + bLeft1), center, select.eulerAngles));
            GL.Vertex(RotatePointAroundPivot(center + (bounds.center + tLeft1), center, select.eulerAngles));

            GL.Vertex(RotatePointAroundPivot(center + (bounds.center + bLeft1), center, select.eulerAngles));
            GL.Vertex(RotatePointAroundPivot(center + (bounds.center + bRight1), center, select.eulerAngles));

            GL.Vertex(RotatePointAroundPivot(center + (bounds.center + bRight1), center, select.eulerAngles));
            GL.Vertex(RotatePointAroundPivot(center + (bounds.center + tRight1), center, select.eulerAngles));

            GL.Vertex(RotatePointAroundPivot(center + (bounds.center + bLeft1), center, select.eulerAngles));
            GL.Vertex(RotatePointAroundPivot(center + (bounds.center + bLeft2), center, select.eulerAngles));

            GL.Vertex(RotatePointAroundPivot(center + (bounds.center + tLeft1), center, select.eulerAngles));
            GL.Vertex(RotatePointAroundPivot(center + (bounds.center + tRight1), center, select.eulerAngles));

            GL.Vertex(RotatePointAroundPivot(center + (bounds.center + tLeft1), center, select.eulerAngles));
            GL.Vertex(RotatePointAroundPivot(center + (bounds.center + tLeft2), center, select.eulerAngles));

            //tRight2
            GL.Vertex(RotatePointAroundPivot(center + (bounds.center + tRight2), center, select.eulerAngles));
            GL.Vertex(RotatePointAroundPivot(center + (bounds.center + bRight2), center, select.eulerAngles));

            GL.Vertex(RotatePointAroundPivot(center + (bounds.center + tRight2), center, select.eulerAngles));
            GL.Vertex(RotatePointAroundPivot(center + (bounds.center + tLeft2), center, select.eulerAngles));

            GL.Vertex(RotatePointAroundPivot(center + (bounds.center + tLeft2), center, select.eulerAngles));
            GL.Vertex(RotatePointAroundPivot(center + (bounds.center + bLeft2), center, select.eulerAngles));

            GL.Vertex(RotatePointAroundPivot(center + (bounds.center + tRight2), center, select.eulerAngles));
            GL.Vertex(RotatePointAroundPivot(center + (bounds.center + tRight1), center, select.eulerAngles));

            GL.Vertex(RotatePointAroundPivot(center + (bounds.center + bRight2), center, select.eulerAngles));
            GL.Vertex(RotatePointAroundPivot(center + (bounds.center + bLeft2), center, select.eulerAngles));

            GL.Vertex(RotatePointAroundPivot(center + (bounds.center + bRight2), center, select.eulerAngles));
            GL.Vertex(RotatePointAroundPivot(center + (bounds.center + bRight1), center, select.eulerAngles));
        }


        GL.End();
    }
}
