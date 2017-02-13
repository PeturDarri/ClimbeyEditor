using System;
using UnityEngine;
using System.Collections.Generic;
using UndoMethods;

public class LevelObject : MonoBehaviour
{
    public ObjectType Type;
    public bool LockX, LockY, LockZ;
    public bool IsSelected;
    public bool Rotateable = true;
    public bool Scaleable = true;
    public string TypeString;
    public Vector3 _startScale, _startRot;
    public bool canDestroy = true;

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
        LevelManager.Instance.OnSave += OnSave;
        _startRot = transform.eulerAngles;
        _startScale = transform.localScale;
    }

    public virtual Dictionary<string, object> GetProperties()
    {
        return new Dictionary<string, object> {};
    }

    public virtual void DoDestroy(bool destroy = true)
    {
        UndoRedoManager.Instance().Push(DoDestroy, !destroy);
        gameObject.SetActive(!destroy);
    }

    public virtual void OnDestroy()
    {
        LevelManager.Instance.OnSave -= OnSave;
    }

    public void OnSave()
    {
        LevelManager.Instance.RegisterObject(GetObject());
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

    public virtual List<LevelObject> Duplicate()
    {
        var newObj = Instantiate(gameObject, transform.parent);
        UndoRedoManager.Instance().Push(newObj.GetComponent<LevelObject>().DoDestroy, newObj.activeSelf, "Duplicate object");
        newObj.transform.position = transform.position;
        newObj.name = name;
        return new List<LevelObject> {newObj.GetComponent<LevelObject>()};
    }

    public virtual Bounds GetBounds()
    {
        return GetComponent<Collider>().bounds;
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