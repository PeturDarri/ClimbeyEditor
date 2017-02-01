using System;
using UnityEngine;
using RuntimeGizmos;

public class GridManager: MonoBehaviour
{
    public static GridManager instance;

    // Vars
    private Vector3 prevPosition;
    private Vector3 prevRotation;
    public bool DoSnap = true;
    public float SnapValueX = 1;
    public float SnapValueY = 1;
    public float SnapValueZ = 1;
    public float SnapValueRot = 15;
    private Transform select;

    //Events
    //Events
    public delegate void GridEvent();

    public event GridEvent GridDisabled;

    public event GridEvent OnSnap;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        select = SelectionManager.instance.transform;
    }

    private void Update()
    {
        if ( DoSnap
             && !SelectionManager.instance.isEmpty
             && (select.position != prevPosition || select.eulerAngles != prevRotation) )
        {
            AutoSnap();
            prevPosition = select.position;
            prevRotation = select.eulerAngles;
        }

        DoSnap = Camera.main.GetComponent<TransformGizmo>().isTransforming;
        if (DoSnap)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                DoSnap = false;
            }
        }
        else
        {
            if (GridDisabled != null)
            {
                GridDisabled();
            }
        }
    }

    public void AutoSnap()
    {
        //Update the selection
        SelectionManager.instance.TransformSelection();

        //Get the selection bounds
        var bounds = SelectionManager.instance.SelectBounds;
        bounds.center = SelectionManager.instance.transform.position;

        // Snap the max
        Vector3 t = bounds.max;
        t.x = SnapRound( t.x, SnapValueX );
        t.y = SnapRound( t.y, SnapValueY );
        t.z = SnapRound( t.z, SnapValueZ );

        Vector3 tm = bounds.min;
        tm.x = SnapRound( tm.x, SnapValueX );
        tm.y = SnapRound( tm.y, SnapValueY );
        tm.z = SnapRound( tm.z, SnapValueZ );

        bounds.SetMinMax(tm, t);

        //Keep sizes from being zero
        Vector3 size = bounds.size;
        size.x = Mathf.Clamp(size.x, SnapValueX, float.MaxValue);
        size.y = Mathf.Clamp(size.y, SnapValueY, float.MaxValue);
        size.z = Mathf.Clamp(size.z, SnapValueZ, float.MaxValue);
        bounds.size = size;

        select.position = bounds.center;
        select.localScale = bounds.size;
        Debug.Log("Changing scale to " + bounds.size);

        //Rotation snap
        Vector3 r = select.eulerAngles;
        r.x = SnapRound( r.x, SnapValueRot ) % 360.0f;
        r.y = SnapRound( r.y, SnapValueRot ) % 360.0f;
        r.z = SnapRound( r.z, SnapValueRot ) % 360.0f;
        select.eulerAngles = r;

        if (OnSnap != null)
        {
            OnSnap();
        }
    }

    private float SnapRound( float input, float snapValue )
    {
        return snapValue * Mathf.Round((input / snapValue));
    }
}