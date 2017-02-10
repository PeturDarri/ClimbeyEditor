using UnityEngine;
using System.Collections.Generic;

public class PoleBlock : LevelObject
{
    public Zipline ParentZip;

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
                Rotation = Quaternion.Euler(_startRot),
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

    public override void OnDestroy()
    {
        base.OnDestroy();
        ParentZip.SelfDestruct();
    }

    public override List<LevelObject> Duplicate()
    {
        if (!ParentZip.canDupe) return null;
        ParentZip.canDupe = false;
        var newZip = Instantiate(ParentZip, transform.parent);
        newZip.name = ParentZip.name;
        return new List<LevelObject> {newZip.PoleBlocks[0], newZip.PoleBlocks[1]};
    }
}
