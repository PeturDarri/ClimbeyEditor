using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sign : LevelObject
{
    private string _text;
    public string SignText
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
            text = SignText
        };

        return newBlock;
    }

    public override Dictionary<string, object> GetProperties()
    {
        return new Dictionary<string, object> {{"SignText", SignText}};
    }

    public void UpdateText()
    {
        GetComponentInChildren<Text>().text = _text;
    }
}
