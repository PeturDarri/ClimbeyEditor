using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using SFB;

public class LevelManager : MonoBehaviour
{
    public List<PrefabBlock> PrefabBlocks;

    [Serializable]
    public class PrefabBlock
    {
        public GameObject Prefab;
    }

    //Events
    public delegate void OnSaveEvent();

    public event OnSaveEvent OnSave;

    public enum GamemodeType
    {
        Race = 0
    }

    [Serializable]
    public class Block
    {
        public string Type;
        public Vector3 Size, Position;
        public Quaternion Rotation;
        public bool LockX, LockY, LockZ;
    }

    [Serializable]
    public class MovingBlock : Block
    {
        public List<Block> Waypoints = new List<Block>();
        public float ArrivalTime;
        public float Speed;
        public bool PingPong;
        public bool WaitForPlayer;
    }

    [Serializable]
    public class SettingsBlock : Block
    {
        public int Checkpoints = 3;
        public GamemodeType Gamemode;
    }

    [Serializable]
    public class SignBlock : Block
    {
        public string Text;
    }

    [Serializable]
    public class ZiplineBlock : Block
    {
        public Block[] PoleBlocks;
    }

    [Serializable]
    public class LightBlock : Block
    {
        public float R, B, G;
    }

    [Serializable]
    public class ClimbeyGroup
    {
        public string Name;
        public Block[] BlocksArray;
        public ZiplineBlock[] ZiplinesArray;
        public LightBlock[] LightsArray;
    }

    [Serializable]
    public class Level
    {
        public Block[] LevelArray;
        public MovingBlock[] MovingArray;
        public SettingsBlock LevelSettings;
        public SignBlock[] SignsArray;
        public ZiplineBlock[] ZiplinesArray;
        public LightBlock[] LightsArray;
        public ClimbeyGroup[] GroupsArray;
    }

    public static LevelManager instance;
    public Level mainLevel;

    public List<LevelObject> LevelObjects
    {
        get
        {
            var wholeList = GetComponentsInChildren<LevelObject>().ToList();
            var removeList = wholeList;
            foreach (var old in wholeList)
            {
                if (old.transform.parent.GetComponent<LevelObject>() != null)
                {
                    removeList.Remove(old);
                }
            }
            return removeList;
        }
    }

    public List<Block> Blocks = new List<Block>();
    public List<SignBlock> SignBlocks = new List<SignBlock>();
    public List<LightBlock> LightBlocks = new List<LightBlock>();
    public List<ZiplineBlock> ZiplineBlocks = new List<ZiplineBlock>();
    public List<MovingBlock> MovingBlocks = new List<MovingBlock>();
    public List<ClimbeyGroup> Groups = new List<ClimbeyGroup>();
    public SettingsBlock Settings;

    // Use this for initialization
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        //Create standard start objects

    }

    public void SaveLevel(string levelName, string path = "")
    {
        if (OnSave != null)
        {
            OnSave();
        }

        if (Blocks.Count == 0)
        {
            return;
        }

        mainLevel = new Level
        {
            LevelArray = Blocks.ToArray(),
            MovingArray = MovingBlocks.ToArray(),
            LevelSettings = Settings,
            SignsArray = SignBlocks.ToArray(),
            ZiplinesArray = ZiplineBlocks.ToArray(),
            LightsArray = LightBlocks.ToArray(),
            GroupsArray = Groups.ToArray()
        };

        File.WriteAllText(path + levelName + ".txt", JsonUtility.ToJson(mainLevel, false));
        ClearLists();
    }

    public void LoadLevel(string levelFile = null)
    {
        if (levelFile == null)
        {
            // Open file with filter
            var extensions = new [] {
                new ExtensionFilter("Text file", "txt"),
                new ExtensionFilter("All Files", "*" ),
            };
            var path = StandaloneFileBrowser.OpenFilePanel("Load Level", "", extensions, false);
            if (path.Any())
            {
                levelFile = path[0];
            }
            else
            {
                Debug.LogError("No level file provided");
                return;
            }
        }
        var loadedLevel = new Level();
        JsonUtility.FromJsonOverwrite(File.ReadAllText(levelFile), loadedLevel);

        foreach (var block in loadedLevel.LevelArray)
        {
            var singleOrDefault = PrefabBlocks.SingleOrDefault(x => x.Prefab.name == block.Type);
            if (singleOrDefault == null) continue;
            var prefab = singleOrDefault.Prefab ?? PrefabBlocks[0].Prefab;

            var newBlock = Instantiate(prefab);

            newBlock.transform.position = block.Position;
            newBlock.transform.localScale = block.Size;
            newBlock.transform.rotation = block.Rotation;
            newBlock.transform.parent = transform;
        }
    }

    public void RegisterObject(Block obj)
    {
        Blocks.Add(obj);
    }

    public void RegisterObject(MovingBlock obj)
    {
        MovingBlocks.Add(obj);
    }

    public void RegisterObject(SignBlock obj)
    {
        SignBlocks.Add(obj);
    }

    public void RegisterObject(ZiplineBlock obj)
    {
        ZiplineBlocks.Add(obj);
    }

    public void RegisterObject(LightBlock obj)
    {
        LightBlocks.Add(obj);
    }

    public void RegisterObject(ClimbeyGroup obj)
    {
        Groups.Add(obj);
    }

    private void ClearLists()
    {
        Blocks.Clear();
        MovingBlocks.Clear();
        SignBlocks.Clear();
        ZiplineBlocks.Clear();
        LightBlocks.Clear();
        Groups.Clear();
        Settings = null;
    }
}