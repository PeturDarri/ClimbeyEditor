using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lamp : BasicBlock
{
    private Color _color;
    public Texture LampSprite;

    public bool hidden;

    public bool Hidden
    {
        get { return hidden; }
        set
        {
            hidden = value;
            GetComponent<MeshRenderer>().enabled = !value;
            _prevShape = value ? Shape : _prevShape;
            Shape = value ? Shape.Cube : _prevShape;
            //Scale bounds
            GetComponent<BoxCollider>().size = value ? new Vector3(1 / transform.localScale.x, 1 / transform.localScale.y, 1 / transform.localScale.z) : Vector3.one;
        }
    }

    //Save last shape in case user reverts hidden option
    private Shape _prevShape;

    public Color Color
    {
        get { return _color; }
        set
        {
            _color = value;
            UpdateLight();
        }
    }

    public int R
    {
        get { return (int)Color.r * 255; }
        set { Color = new Color(Convert.ToInt32(value) / 255f, Color.g, Color.b); }
    }

    public int G
    {
        get { return (int)Color.g * 255; }
        set { Color = new Color(Color.r, Convert.ToInt32(value) / 255f, Color.b); }
    }

    public int B
    {
        get { return (int)Color.b * 255; }
        set { Color = new Color(Color.r, Color.g, Convert.ToInt32(value) / 255f); }
    }

    public void Awake()
    {
        Color = Color.white;
    }

    public override void Start()
    {
        base.Start();
        if (transform.localScale == Vector3.zero)
        {
            transform.localScale = Vector3.one;
            Hidden = true;
        }
    }

    public override LevelManager.Block GetObject()
    {
        var newBlock = new LevelManager.LightBlock
        {
            Type = GetShape(),
            Size = Hidden ? Vector3.zero : transform.localScale,
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

    public override Dictionary<string, object> GetProperties()
    {
        return new Dictionary<string, object> {{"Shape", Shape}, {"R", (int)Color.r * 255}, {"G", (int)Color.g * 255}, {"B", (int)Color.b * 255}, {"Hidden", Hidden}};
    }

    public void UpdateLight()
    {
        GetComponent<Light>().color = _color;
        GetComponent<Renderer>().material.color = _color;
    }

    private void OnGUI()
    {
        if (!Hidden) return;
        var pos = Camera.main.WorldToScreenPoint(transform.position);
        if (pos.z < 0) return;
        var rect = new Rect(new Vector2(pos.x - 16, Screen.height - (pos.y + 16)), new Vector2(32, 32));
        var iconColor = Color;
        iconColor.a = Mathf.InverseLerp(30, 10, pos.z);
        var prevColor = GUI.color;
        GUI.color = iconColor;
        GUI.DrawTexture(rect, LampSprite);
        GUI.color = prevColor;
    }
}
