using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicBlock : LevelObject
{
    public Shape Shape = Shape.None;

    public override void Start()
    {
        base.Start();

        if (Shape > 0)
        {
            GetComponent<MeshFilter>().mesh = LevelManager.instance.Shapes[(int)Shape - 1];
        }
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
}
