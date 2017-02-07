using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovingBlock : LevelObject
{
    public List<Waypoint> Waypoints
    {
        get { return transform.parent.GetComponentsInChildren<Waypoint>().ToList(); }
    }
    public float ArrivalTime;
    public bool PingPong;
    public float Speed;
    public bool WaitForPlayer;

    private delegate void AddWaypoint();
    private Vector3 _position;

    public override LevelManager.Block GetObject()
    {
        var wayList = Waypoints.Select(way => way.VirtualBlock).ToList();

        var newBlock = new LevelManager.MovingBlock()
        {
            Type = "MovingBlock",
            Size = transform.localScale,
            Position = _position,
            Rotation = transform.rotation,
            LockX = LockX,
            LockY = LockY,
            LockZ = LockZ,
            ArrivalTime = ArrivalTime,
            PingPong = PingPong,
            Speed = Speed,
            WaitForPlayer = WaitForPlayer,
            Waypoints = wayList
        };

        return newBlock;
    }

    public override Dictionary<string, object> GetProperties()
    {
        AddWaypoint addWaypoint = AddWaypointLocal;
        return new Dictionary<string, object> {{"ArrivalTime", ArrivalTime}, {"PingPong", PingPong}, {"Speed", Speed}, {"WaitForPlayer", WaitForPlayer}, {"Add Waypoint", addWaypoint}};
    }

    private void Update()
    {
        var anyWaypointSelected = false;

        foreach (var way in Waypoints)
        {
            if (way.IsSelected)
            {
                anyWaypointSelected = true;
            }
        }

        //Draw waypoints and lines between them
        foreach (var way in Waypoints)
        {
            way.GetComponent<Renderer>().enabled = IsSelected || anyWaypointSelected;
            way.GetComponent<Collider>().enabled = IsSelected || anyWaypointSelected;
            way.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        }
    }

    private void AddWaypointLocal()
    {
        Debug.Log("Add waypoint!");
    }

}
