using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class LevelObject : MonoBehaviour
{
    public ObjectType Type;
    public bool LockX, LockY, LockZ;
    public bool IsSelected;
    public bool Rotateable = true;
    public bool Scaleable = true;
    public string TypeString;
    public Vector3 _startScale, _startRot;

    public override bool Equals(object obj)
    {
        if(obj == null)return false;
        var other = obj as LevelObject;
        if(other == null)return false;
        return other.gameObject.GetInstanceID() == gameObject.GetInstanceID();
    }

    public override int GetHashCode()
    {
        return gameObject.GetInstanceID();
    }

    public virtual void Start()
    {
        LevelManager.instance.OnSave += OnSave;
        _startRot = transform.eulerAngles;
        _startScale = transform.localScale;
    }

    public void OnSave()
    {
        LevelManager.instance.RegisterObject(GetObject());
    }

    public virtual LevelManager.Block GetObject()
    {
        var newBlock = new LevelManager.Block()
        {
            Type = TypeString,
            Size = Scaleable ? transform.localScale : _startScale,
            Position = transform.position,
            Rotation = Rotateable ? transform.rotation : Quaternion.Euler(_startRot),
            LockX = LockX,
            LockY = LockY,
            LockZ = LockZ,
        };

        return newBlock;
    }

    public virtual LevelObject Duplicate()
    {
        return Instantiate(this, transform.parent);
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