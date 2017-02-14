using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UndoMethods;
using UnityEngine;

public class MovingBlock : LevelObject
{
    public List<Waypoint> Waypoints
    {
        get { return transform.parent.GetComponentsInChildren<Waypoint>().ToList(); }
    }
    public float ArrivalTime { get; set; }
    public bool PingPong { get; set; }
    public float Speed { get; set; }
    public bool WaitForPlayer { get; set; }

    private delegate void selectWaypoints();

    private delegate void addWaypoint();

    private delegate void startPreview();

    private delegate void stopPreview();

    private Vector3 _position;
    private bool isPreviewing;
    public LineRenderer _lr;

    public override void Start()
    {
        base.Start();
        //Make sure we are in a group
        if (transform.parent != LevelManager.Instance.transform) return;
        var parent = new GameObject("MovingBlockGroup");
        parent.transform.parent = LevelManager.Instance.transform;
        transform.parent = parent.transform;
    }

    public override LevelManager.Block GetObject()
    {
        StopPreview();
        var wayList = Waypoints.Select(way => way.VirtualBlock).ToList();

        var newBlock = new LevelManager.MovingBlock()
        {
            Type = "MovingBlock",
            Size = transform.localScale,
            Position = transform.position,
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
        selectWaypoints selectWaypoints = SelectWaypoints;
        addWaypoint addWaypoint = AddWaypointLocal;
        startPreview startPreview = StartPreview;
        stopPreview stopPreview = StopPreview;
        return new Dictionary<string, object> {{"ArrivalTime", ArrivalTime}, {"PingPong", PingPong}, {"Speed", Speed}, {"WaitForPlayer", WaitForPlayer}, {"Select All Waypoints", selectWaypoints}, {"Add Waypoint", addWaypoint}, {"Start Preview", startPreview}, {"Stop Preview", stopPreview}};
    }

    public override List<LevelObject> Duplicate()
    {
        var dupeGroup = Instantiate(transform.parent, transform.parent.parent);
        dupeGroup.name = transform.parent.name;
        dupeGroup.position = transform.parent.position;
        var move = dupeGroup.GetComponentInChildren<MovingBlock>();
        move.ArrivalTime = ArrivalTime;
        move.PingPong = PingPong;
        move.Speed = Speed;
        move.WaitForPlayer = WaitForPlayer;
        var newList = new List<LevelObject> {move};
        newList.AddRange(dupeGroup.GetComponentsInChildren<Waypoint>());
        return newList;
    }

    public override void DoDestroy(bool destroy = true)
    {
        UndoRedoManager.Instance().Push(DoDestroy, !destroy);
        transform.parent.gameObject.SetActive(!destroy);
    }

    public override void OnDestroy()
    {
        Destroy(transform.parent.gameObject);
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
        _lr.enabled = false;
        if (IsSelected || anyWaypointSelected) DrawLine(Color.red);
        foreach (var way in Waypoints)
        {
            way.GetComponent<Renderer>().enabled = IsSelected || anyWaypointSelected;
            way.GetComponent<Collider>().enabled = IsSelected || anyWaypointSelected;
            way.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        }
    }

    public void SelectWaypoints()
    {
        var list = new List<LevelObject> {this};
        list.AddRange(Waypoints.ToArray());
        SelectionManager.Instance.SetMultiSelection(list);
    }

    public void AddWaypointLocal()
    {
        var way = Instantiate((GameObject) Resources.Load("Level Objects/Waypoint"), transform.parent);
        way.transform.position = CameraManager.Instance.GetTarget();
        SelectionManager.Instance.SetSelection(way.transform);
    }

    public void StartPreview()
    {
        _position = transform.position;
        StartCoroutine(PreviewRoutine());
    }

    public void StopPreview()
    {
        if (!isPreviewing) return;
        isPreviewing = false;
        transform.position = _position;
    }

    private IEnumerator PreviewRoutine()
    {
        isPreviewing = true;
        var way2Go = Waypoints.ToList();

        while (isPreviewing)
        {
            for (var i = 0; i <= way2Go.Count; i++)
            {
                if (PingPong && way2Go.Count == 1)
                {
                    isPreviewing = false;
                    break;
                }

                if (i == way2Go.Count)
                {
                    if (PingPong && isPreviewing)
                    {
                        way2Go.Reverse();
                        i = 0;
                    }
                    else
                    {
                        isPreviewing = false;
                        break;
                    }
                }

                var way = way2Go[i];

                var spd = ArrivalTime > 0
                    ? Vector3.Distance(transform.position, way.transform.position) / ArrivalTime
                    : Speed;

                while (transform.position != way.transform.position && isPreviewing)
                {
                    transform.position = Vector3.MoveTowards(transform.position, way.transform.position,
                    (spd - (Vector3.Distance(transform.position, way.transform.position) < spd / 2
                         ? spd / 2 - Vector3.Distance(transform.position, way.transform.position)
                         : 0)) * Time.fixedDeltaTime);

                    yield return 0;
                }
            }
        }

        transform.position = _position;
    }

    private void DrawLine(Color color)
    {
        _lr.enabled = true;
        _lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
        _lr.startColor = color;
        _lr.endColor = color;
        _lr.startWidth = 0.1f;

        _lr.numPositions = Waypoints.Count + 1;
        _lr.SetPosition(0, isPreviewing ? _position : transform.position);
        for (var i = 0; i < Waypoints.Count; i++)
        {
            _lr.SetPosition(i + 1, Waypoints[i].transform.position);
        }
    }
}
