using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSign : LevelObject
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

    public int Checkpoints { get; set; }
    public LevelManager.GamemodeType Gamemode { get; set; }

    private void Awake()
    {
        SignText = "LevelSign";
    }

    public override LevelManager.Block GetObject()
    {
        var newBlock = new LevelManager.SettingsBlock()
        {
            Type = "LevelSign",
            Size = transform.localScale,
            Position = transform.position,
            Rotation = transform.rotation,
            LockX = LockX,
            LockY = LockY,
            LockZ = LockZ,
            Checkpoints = Checkpoints,
            Gamemode = Gamemode
        };

        return newBlock;
    }

    public override Dictionary<string, object> GetProperties()
    {
        return new Dictionary<string, object> {{"Checkpoints", Checkpoints}, {"Gamemode", Gamemode}};
    }

    public void UpdateText()
    {
        GetComponentInChildren<Text>().text = _text;
    }
}
