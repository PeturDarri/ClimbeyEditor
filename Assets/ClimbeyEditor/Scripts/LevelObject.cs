using UnityEngine;
using Level;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class LevelObject : MonoBehaviour
{
    public bool LockX, LockY, LockZ;

    public virtual LevelManager.Block GetBlock(bool x, bool y, bool z)
    {
        return new LevelManager.Block();
    }

    public void SetSelection(bool selected)
    {
        //Select
    }
}

public enum Shape
{
    None,
    Sphere,
    Hemi,
    Pipe,
    Pyramid
}

public enum ObjectType
{
    Grabbable,
    Metal,
    Icy,
    Glass,
    CameraRig,
    FinishLine,
    LevelSign,
    Jumpy,
    Spikes,
    Lava,
    GravityField,
    MovingBlock,
    Zipline,
    Lamp,
    Waypoint,
    Sign,
    None
}