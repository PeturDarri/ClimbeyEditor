using UnityEngine;
using System.Collections.Generic;

public class Waypoint : LevelObject
{
    public MovingBlock MovingBlock
    {
        get { return transform.parent.GetComponentInChildren<MovingBlock>(); }
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
                Rotation = Quaternion.identity,
                Size = Vector3.zero,
                Type = "Waypoint"
            };

            return newBlock;
        }
    }

    public override LevelManager.Block GetObject()
    {
        //Just here to be selectable
        return null;
    }

    public override List<LevelObject> Duplicate()
    {
        MovingBlock.AddWaypointLocal();
        return null;
    }

    public override void DoDestroy(bool destroy = true)
    {
        if (destroy)
        {
            if (MovingBlock != null)
            {
                SelectionManager.Instance.SetSelection(MovingBlock.transform);
            }
        }

        base.DoDestroy(destroy);
    }
}
