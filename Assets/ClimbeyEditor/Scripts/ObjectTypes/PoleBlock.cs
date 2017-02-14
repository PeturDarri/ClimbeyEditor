using UnityEngine;
using System.Collections.Generic;
using UndoMethods;

public class PoleBlock : LevelObject
{
    public Zipline ParentZip
    {
        get { return transform.parent.GetComponentInChildren<Zipline>(); }
    }

    public Transform Pivot
    {
        get { return transform.GetChild(0); }
    }

    public LevelManager.Block VirtualBlock
    {
        get
        {
            var newBlock = new LevelManager.Block
            {
                LockX = LockX,
                LockY = LockY,
                LockZ = LockZ,
                Position = transform.position,
                Rotation = transform.rotation,
                Size = _startScale,
                Type = ""
            };

            return newBlock;
        }
    }

    public override LevelManager.Block GetObject()
    {
        //Just here to be selectable
        return null;
    }

    public override void DoDestroy(bool destroy = true)
    {
        UndoRedoManager.Instance().Push(DoDestroy, !destroy);
        transform.parent.gameObject.SetActive(!destroy);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        ParentZip.SelfDestruct();
    }

    public override List<LevelObject> Duplicate()
    {
        if (!ParentZip.canDupe) return null;
        ParentZip.canDupe = false;
        var newZip = Instantiate(transform.parent, LevelManager.Instance.transform);
        UndoRedoManager.Instance().Push(DoDestroy, gameObject.activeSelf, "Duplicate object");
        newZip.name = transform.parent.name;
        return new List<LevelObject> {newZip.GetComponentInChildren<Zipline>().PoleBlocks[0], newZip.GetComponentInChildren<Zipline>().PoleBlocks[1]};
    }
}
