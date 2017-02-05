using UnityEngine;

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

    private void OnDestroy()
    {
        ParentZip.SelfDestruct();
    }

    public override LevelObject Duplicate()
    {
        if (!ParentZip.canDupe) return null;

        ParentZip.canDupe = false;
        var newZip = Instantiate(ParentZip, transform.parent);
        return newZip.PoleBlocks[0];
    }
}
