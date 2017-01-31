using UnityEngine;

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
    private Transform emptySelection;

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

        emptySelection = SelectionManager.instance.transform;
    }

    public void Update()
    {
        // Check if we should snap
        emptySelection = SelectionManager.instance.transform;

        if ( DoSnap
             && !SelectionManager.instance.isEmpty
             && (emptySelection.position != prevPosition || emptySelection.eulerAngles != prevRotation) )
        {
            AutoSnap();
            prevPosition = emptySelection.position;
            prevRotation = emptySelection.eulerAngles;
        }
    }

    private void AutoSnap()
    {
        //Get the selection bounds
        var bounds = SelectionManager.instance.SelectBounds;

        // Snap the max
        bounds.center = emptySelection.position;
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

        emptySelection.position = bounds.center;
        emptySelection.localScale = bounds.size;

        //Rotation snap
        Vector3 r = emptySelection.eulerAngles;
        r.x = SnapRound( r.x, SnapValueRot ) % 360.0f;
        r.y = SnapRound( r.y, SnapValueRot ) % 360.0f;
        r.z = SnapRound( r.z, SnapValueRot ) % 360.0f;
        emptySelection.eulerAngles = r;
    }

    private float SnapRound( float input, float snapValue )
    {

        return snapValue * Mathf.Round((input / snapValue));
    }
}