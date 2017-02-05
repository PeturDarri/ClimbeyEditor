using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sign : LevelObject
{
    private string _text;
    public string Text
    {
        get { return _text; }
        set
        {
            _text = value;
            UpdateText();
        }
    }

    public override LevelManager.Block GetObject()
    {
        var newBlock = new LevelManager.SignBlock()
        {
            Type = "Sign",
            Size = transform.localScale,
            Position = transform.position,
            Rotation = transform.rotation,
            LockX = LockX,
            LockY = LockY,
            LockZ = LockZ,
            text = Text
        };

        return newBlock;
    }

    public void UpdateText()
    {
        GetComponentInChildren<Text>().text = _text;
    }
}
