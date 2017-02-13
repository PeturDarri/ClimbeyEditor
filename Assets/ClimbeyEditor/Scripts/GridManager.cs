using System;
using UnityEngine;
using RuntimeGizmos;
using UnityEditor;

public class GridManager: MonoBehaviour
{
    public static GridManager Instance;

    // Vars
    private Vector3 prevPosition, prevRotation, prevSize;
    public bool DoSnap = true;
    public float SnapValue = 0.5f;
    public float SnapValueRot = 15;
    private Transform select;

    //Events
    public delegate void GridEvent();

    public event GridEvent GridDisabled;

    public event GridEvent OnSnap;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        select = SelectionManager.Instance.transform;
    }

    private void Update()
    {
        if ( DoSnap
             && !SelectionManager.Instance.isEmpty
             && (select.position != prevPosition || select.eulerAngles != prevRotation || select.localScale != prevSize))
        {
            AutoSnap();
            prevPosition = select.position;
            prevRotation = select.eulerAngles;
            prevSize = select.localScale;
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
        SelectionManager.Instance.TransformSelection();

        //Get the selection bounds
        var bounds = SelectionManager.Instance.GetBounds();
        bounds.center = SelectionManager.Instance.transform.position;

        if (SelectionManager.Instance.Selection.Count == 1)
        {
            bounds.size = SelectionManager.Instance.Selection[0].transform.localScale;
        }

        //Rotation snap
        var r = select.eulerAngles;
        r.x = SnapRound( r.x, SnapValueRot ) % 360.0f;
        r.y = SnapRound( r.y, SnapValueRot ) % 360.0f;
        r.z = SnapRound( r.z, SnapValueRot ) % 360.0f;
        select.eulerAngles = r;

        // Snap the max
        var t = bounds.max;
        t.x = SnapRound( t.x, SnapValue );
        t.y = SnapRound( t.y, SnapValue );
        t.z = SnapRound( t.z, SnapValue );

        var tm = bounds.min;
        tm.x = SnapRound( tm.x, SnapValue );
        tm.y = SnapRound( tm.y, SnapValue );
        tm.z = SnapRound( tm.z, SnapValue );

        bounds.SetMinMax(tm, t);

        //Keep sizes from being zero
        var size = bounds.size;
        size.x = Mathf.Clamp(size.x, SnapValue, float.MaxValue);
        size.y = Mathf.Clamp(size.y, SnapValue, float.MaxValue);
        size.z = Mathf.Clamp(size.z, SnapValue, float.MaxValue);
        bounds.size = size;

        select.position = bounds.center;
        select.localScale = bounds.size;

        if (OnSnap != null)
        {
            OnSnap();
        }
    }

    private static float SnapRound( float input, float snapValue )
    {
        return snapValue * Mathf.Round((input / snapValue));
    }

    public void DoubleHalveGrid(bool doDouble)
    {
        SnapValue = doDouble ? SnapValue * 2 : SnapValue / 2;
    }
}