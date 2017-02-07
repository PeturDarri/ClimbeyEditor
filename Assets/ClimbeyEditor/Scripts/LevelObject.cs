using System;
using UnityEngine;
using System.Collections.Generic;

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

    public virtual Dictionary<string, object> GetProperties()
    {
        return new Dictionary<string, object> {};
    }

    private void OnDestroy()
    {
        LevelManager.instance.OnSave -= OnSave;
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
    Cube = 0,
    Sphere = 1,
    Hemi = 2,
    Pipe = 3,
    Pyramid = 4
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