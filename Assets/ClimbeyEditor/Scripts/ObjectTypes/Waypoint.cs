using UnityEngine;

public class Waypoint : LevelObject
{

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

    public override LevelObject Duplicate()
    {
        //No dupe
        return null;
    }
}
