using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class LevelObject : MonoBehaviour
{
    public bool LockX, LockY, LockZ;
    public bool IsSelected;

    public virtual LevelManager.Block GetBlock(bool x, bool y, bool z)
    {
        return new LevelManager.Block();
    }

    public override bool Equals(object obj)
    {
        if(obj == null)return false;
        LevelObject other = obj as LevelObject;
        if(other == null)return false;
        return other.gameObject.GetInstanceID() == gameObject.GetInstanceID();
    }

    public override int GetHashCode()
    {
        return gameObject.GetInstanceID();
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