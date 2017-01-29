using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class LevelObject : MonoBehaviour
{

    public ObjectType Type;
    public Vector3 Size;
    public Vector3 Position;
    public Quaternion Rotation;
    public bool LockX;
    public bool LockY;
    public bool LockZ;

    private bool _selected;

    public LevelObject()
    {
        DefaultInit();
    }

    public LevelObject(ObjectType type)
    {
        DefaultInit();
        Type = type;
    }

    public LevelObject(ObjectType type, Vector3 position)
    {
        DefaultInit();
        Type = type;
        Position = position;
    }

    private void DefaultInit()
    {
        Type = ObjectType.Grabbable;
        Size = new Vector3(1, 1, 1);
        Position = Vector3.zero;
        Rotation = Quaternion.identity;
        LockX = false;
        LockY = false;
        LockZ = false;
    }

    // Use this for initialization
	void Start()
	{

	}
	
	// Update is called once per frame
    void Update()
    {

    }

    public void SetSelection(bool selected)
    {
        _selected = selected;
    }
}

public enum ObjectType
{
    CameraRig,
    FinishLine,
    LevelSign,
    Grabbable,
    Metal,
    Icy,
    Glass,
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
