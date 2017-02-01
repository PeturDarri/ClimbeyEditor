using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicBlock : LevelObject
{
    public ObjectType Type;
    public Shape Shape = Shape.None;

    private void Start()
    {
        LevelManager.instance.OnSave += OnSave;
    }

    private void OnSave()
    {
        LevelManager.instance.RegisterObject(GetBlock(LockX, LockY, LockZ));
    }

    public override LevelManager.Block GetBlock(bool x, bool y, bool z)
    {
        var newBlock = new LevelManager.Block();
        newBlock.Type = GetShape();
        newBlock.Size = transform.localScale;
        newBlock.Position = transform.position;
        newBlock.Rotation = transform.rotation;
        newBlock.LockX = x;
        newBlock.LockY = y;
        newBlock.LockZ = z;
        return newBlock;
    }

    private string GetShape()
    {
        if (Shape > 0)
            return Shape + "NoGrip";
        return Type.ToString();
    }
}
