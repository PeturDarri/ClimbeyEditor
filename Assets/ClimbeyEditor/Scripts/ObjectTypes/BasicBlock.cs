using System;
using System.Collections;
using System.Collections.Generic;
using UndoMethods;
using UnityEngine;

public class BasicBlock : LevelObject
{   
    public Shape _shape;
    public Shape Shape
    {
        get { return _shape; }
        set
        {
            _shape = value;
            UpdateShape();
        }
    }

    public override void Start()
    {
        base.Start();
        GetComponent<MeshFilter>().mesh = LevelManager.Instance.Shapes[(int)Shape];
    }

    public override LevelManager.Block GetObject()
    {
        var newBlock = new LevelManager.Block
        {
            Type = GetShape(),
            Size = transform.localScale,
            Position = transform.position,
            Rotation = transform.rotation,
            LockX = LockX,
            LockY = LockY,
            LockZ = LockZ
        };
        return newBlock;
    }

    public override Dictionary<string, object> GetProperties()
    {
        return new Dictionary<string, object> {{"Shape", Shape}};
    }

    public string GetShape()
    {
        if (Shape <= 0) return Type.ToString();
        var stringType = "Grip";
        switch (Type)
        {
            case ObjectType.Grabbable:
                stringType = "Grip";
                break;
            case ObjectType.Metal:
                stringType = "NoGrip";
                break;
            case ObjectType.Icy:
                stringType = "Ice";
                break;
            case ObjectType.Glass:
                stringType = "Seethrough";
                break;
            case ObjectType.Lamp:
                stringType = "Light";
                break;
        }
        return Shape + stringType;
    }

    public Mesh UpdateShape()
    {
        var filter = GetComponent<MeshFilter>();
        filter.mesh = LevelManager.Instance.Shapes[(int) _shape];
        return LevelManager.Instance.Shapes[(int) _shape];
    }
}
