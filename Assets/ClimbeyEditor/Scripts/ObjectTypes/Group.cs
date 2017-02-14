using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UndoMethods;
using UnityEngine;

public class Group : LevelObject
{
    public string Name { get; set; }

    private delegate void breakGroup();

    public List<LevelObject> GroupData
    {
        get
        {
            var objs = GetComponentsInChildren<LevelObject>().ToList();
            objs.Remove(this);
            return objs;
        }
    }

    public LevelManager.ClimbeyGroup VirtualGroup()
    {
        var newGroup = new LevelManager.ClimbeyGroup();
        var blockList = new List<LevelManager.Block>();
        var ziplineList = new List<LevelManager.ZiplineBlock>();
        var lightList = new List<LevelManager.LightBlock>();
        foreach (var child in GroupData)
        {
            if (child.Type == ObjectType.Zipline)
            {
                var block = child.GetObject();
                ziplineList.Add((LevelManager.ZiplineBlock)block);
            }
            else if (child.Type == ObjectType.Lamp)
            {
                var block = child.GetObject();
                lightList.Add((LevelManager.LightBlock)block);
            }
            else
            {
                var block = child.GetObject();
                blockList.Add(block);
            }
        }

        newGroup.BlocksArray = blockList.ToArray();
        newGroup.ZiplinesArray = ziplineList.ToArray();
        newGroup.LightsArray = lightList.ToArray();

        return newGroup;
    }

    public override void Start()
    {
        base.Start();
        if (GroupData.Count > 0) return;

        var group = LevelManager.Instance.Groups.Single(g => g.Name == Name);

        if (group.BlocksArray != null && group.BlocksArray.Any())
            foreach (var block in group.BlocksArray)
            {
                var prefab = (GameObject) Resources.Load("Level Objects/" + block.Type);
                if (prefab == null) continue;
                var newBlock = Instantiate(prefab);
                var blockComp = newBlock.GetComponent<LevelObject>();
                newBlock.transform.parent = transform;

                newBlock.transform.localPosition = block.Position;
                newBlock.transform.localScale = block.Size;
                newBlock.transform.localRotation = block.Rotation;
                if (blockComp == null) continue;
                blockComp.LockX = block.LockX;
                blockComp.LockY = block.LockY;
                blockComp.LockZ = block.LockZ;
            }

        if (group.LightsArray != null && group.LightsArray.Any())
            foreach (var light in group.LightsArray)
            {
                var prefab = (GameObject) Resources.Load("Level Objects/" + light.Type);
                if (prefab == null) continue;
                var newBlock = Instantiate(prefab);
                var lightComp = newBlock.GetComponent<Lamp>();
                newBlock.transform.parent = transform;

                newBlock.transform.position = light.Position;
                newBlock.transform.localScale = light.Size;
                newBlock.transform.rotation = light.Rotation;
                lightComp.LockX = light.LockX;
                lightComp.LockY = light.LockY;
                lightComp.LockZ = light.LockZ;
                lightComp.Color = new Color(light.R, light.G, light.B);
            }

        if (group.ZiplinesArray != null && group.ZiplinesArray.Any())
            foreach (var zip in group.ZiplinesArray)
            {
                var prefab = (GameObject) Resources.Load("Level Objects/" + zip.Type);
                if (prefab == null) continue;
                var newBlock = Instantiate(prefab, transform);
                var zipComp = newBlock.GetComponent<Zipline>();

                zipComp.SetPoles(zip.PoleBlocks[0], zip.PoleBlocks[1]);
            }
    }

    public override LevelManager.Block GetObject()
    {
        var newBlock = new LevelManager.Block
        {
            Type = "Group:" + Name,
            Size = transform.localScale,
            Position = transform.position,
            Rotation = transform.rotation,
            LockX = LockX,
            LockY = LockY,
            LockZ = LockZ
        };
        return newBlock;
    }

    public override Dictionary<string, object> GetProperties()
    {
        breakGroup breakGroup = BreakGroup;
        return new Dictionary<string, object> {{"Name", Name}, {"Break Group", breakGroup}};
    }

    public override Bounds GetBounds()
    {
        if (GroupData.Count == 0)
        {
            return new Bounds();
        }

        var bounds = new Bounds(GroupData[0].transform.position, Vector3.zero);

        foreach(var child in GroupData.ToList())
        {
            bounds.Encapsulate(child.GetBounds());
        }

        return bounds;
    }

    public override List<LevelObject> Duplicate()
    {
        var newObj = Instantiate(gameObject, transform.parent);
        UndoRedoManager.Instance().Push(newObj.GetComponent<LevelObject>().DoDestroy, newObj.activeSelf, "Duplicate group");
        newObj.transform.position = transform.position;
        newObj.name = name;
        newObj.GetComponent<Group>().Name = Name;
        return new List<LevelObject> {newObj.GetComponent<LevelObject>()};
    }

    public void BreakGroup()
    {
        BreakMakeGroup();
    }

    public void BreakMakeGroup(List<LevelObject> levelObjects = null)
    {
        using (new UndoTransaction("Break group"))
        {
            if (levelObjects == null || levelObjects.Count == 0)
            {
                UndoRedoManager.Instance().Push(BreakMakeGroup, GroupData, "Break group");
                var groupList = GroupData.ToList();
                foreach (var child in groupList)
                {
                    child.transform.parent = LevelManager.Instance.transform;
                }
                SelectionManager.Instance.ClearAndMulti(groupList);
                DoDestroy();
            }
            else
            {
                UndoRedoManager.Instance().Push(BreakMakeGroup, new List<LevelObject>(), "Create group");
                foreach (var child in levelObjects)
                {
                    child.transform.parent = transform;
                }
                CenterSelf();
                SelectionManager.Instance.SetSelection(transform);
            }
        }
    }

    public void CenterSelf()
    {
        //Set parent position to center of all children
        var bounds = GetBounds();

        var oldList = GroupData.ToList();
        foreach (var child in oldList)
        {
            child.transform.parent = transform.parent;
        }

        transform.position = transform.position + bounds.center;

        foreach (var child in oldList)
        {
            child.transform.parent = transform;
        }
    }
}
