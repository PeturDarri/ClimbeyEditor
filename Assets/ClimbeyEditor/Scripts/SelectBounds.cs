using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectBounds : MonoBehaviour
{
    public bool ShowBounds;
    public Color LineColor;
    public Material lineMaterial;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

	}

    private Bounds CalculateLocalBounds(Transform Object)
    {
        Quaternion currentRotation = Object.rotation;
        Object.rotation = Quaternion.Euler(0f,0f,0f);

        Bounds bounds = new Bounds(Object.position, Vector3.zero);

        foreach(Renderer render in Object.GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(render.bounds);
        }

        Vector3 localCenter = bounds.center - Object.position;
        bounds.center = localCenter;

        Object.rotation = currentRotation;

        return bounds;
    }

    private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) {
        Vector3 dir = point - pivot; // get point direction relative to pivot
        dir = Quaternion.Euler(angles) * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }

    void OnPostRender()
    {
        Bounds bounds = CalculateLocalBounds(SelectionManager.instance.transform);

        //Calculate 8 corners and save
        var bLeft1 = new Vector3(-bounds.extents.x, -bounds.extents.y, -bounds.extents.z);
        var tLeft1 = new Vector3(-bounds.extents.x, +bounds.extents.y, -bounds.extents.z);
        var bRight1 = new Vector3(+bounds.extents.x, -bounds.extents.y, -bounds.extents.z);
        var tRight1 = new Vector3(+bounds.extents.x, +bounds.extents.y, -bounds.extents.z);
        var bLeft2 = new Vector3(-bounds.extents.x, -bounds.extents.y, +bounds.extents.z);
        var tLeft2 = new Vector3(-bounds.extents.x, +bounds.extents.y, +bounds.extents.z);
        var bRight2 = new Vector3(+bounds.extents.x, -bounds.extents.y, +bounds.extents.z);
        var tRight2 = new Vector3(+bounds.extents.x, +bounds.extents.y, +bounds.extents.z);


        // set the current material
        lineMaterial.SetPass( 0 );

        GL.Begin( GL.LINES );

        if(ShowBounds)
        {
            GL.Color(LineColor);

            Transform select = SelectionManager.instance.transform;
            Vector3 center = select.position;
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
