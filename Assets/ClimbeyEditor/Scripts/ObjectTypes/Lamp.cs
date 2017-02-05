using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Lamp : BasicBlock
{
    private Color _color;

    public Color Color
    {
        get { return _color; }
        set
        {
            _color = value;
            UpdateLight();
        }
    }

    public override LevelManager.Block GetObject()
    {
        var newBlock = new LevelManager.LightBlock
        {
            Type = GetShape(),
            Size = transform.localScale,
            Position = transform.position,
            Rotation = transform.rotation,
            LockX = LockX,
            LockY = LockY,
            LockZ = LockZ,
            R = Color.r,
            G = Color.g,
            B = Color.b
        };
        return newBlock;
    }

    public void UpdateLight()
    {
        GetComponent<Light>().color = _color;
        GetComponent<Renderer>().material.color = _color;
    }
}
