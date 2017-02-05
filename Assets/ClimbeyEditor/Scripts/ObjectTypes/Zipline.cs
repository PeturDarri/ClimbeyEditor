using System;
using System.Linq;
using UnityEngine;
public class Zipline : LevelObject
{
    public PoleBlock[] PoleBlocks = new PoleBlock[2];
    public GameObject Line;
    public GameObject PoleBlockPrefab;
    public bool canDupe = true;
    private Transform _lineTransform;

    public void Awake()
    {
        if (_lineTransform != null && PoleBlocks.Any()) return;
        _lineTransform = Instantiate(Line, transform.parent).transform;
        PoleBlocks[0] = CreatePole();
        PoleBlocks[0].ParentZip = this;
        PoleBlocks[0].transform.position = transform.position + transform.forward * 2;
        PoleBlocks[1] = CreatePole();
        PoleBlocks[1].ParentZip = this;
        PoleBlocks[1].transform.position = transform.position - transform.forward * 2;
    }

    public void SetPoles(LevelManager.Block pole1, LevelManager.Block pole2)
    {
        PoleBlocks[0].transform.position = pole1.Position;
        PoleBlocks[1].transform.position = pole2.Position;
    }

    private void Update()
    {
        canDupe = true;
        //Stretch line between poles
        _lineTransform.position = (PoleBlocks[0].Pivot.position + PoleBlocks[1].Pivot.position) / 2;
        _lineTransform.LookAt(PoleBlocks[0].Pivot.position);
        _lineTransform.localScale = new Vector3(0.05f, 0.05f, 0.05f) + Vector3.forward *
                                    (Vector3.Distance(_lineTransform.position, PoleBlocks[0].Pivot.position) * 2);
    }

    public override LevelManager.Block GetObject()
    {
        var poleArray = new LevelManager.Block[2];
        poleArray[0] = PoleBlocks[0].VirtualBlock;
        poleArray[1] = PoleBlocks[1].VirtualBlock;

        var newBlock = new LevelManager.ZiplineBlock
        {
            Type = "Zipline",
            Size = transform.localScale,
            Position = transform.position,
            Rotation = transform.rotation,
            LockX = LockX,
            LockY = LockY,
            LockZ = LockZ,
            PoleBlocks = poleArray
        };

        return newBlock;
    }

    public void SelfDestruct()
    {
        if (PoleBlocks[0] != null)
            Destroy(PoleBlocks[0].gameObject);
        if (PoleBlocks[1] != null)
            Destroy(PoleBlocks[1].gameObject);
        if (_lineTransform != null)
            Destroy(_lineTransform.gameObject);
        Destroy(gameObject);
    }

    private PoleBlock CreatePole()
    {
        var newPole = Instantiate(PoleBlockPrefab, transform.parent);
        return newPole.GetComponent<PoleBlock>();
    }
}
