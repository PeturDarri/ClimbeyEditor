using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using SFB;

public class LevelManager : MonoBehaviour
{

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
        public string text;
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
            var removeList = wholeList.ToList();
            foreach (var old in wholeList)
            {
                if (old.transform.parent.GetComponent<LevelObject>() != null || old.GetComponent<Collider>() == null)
                {
                    removeList.Remove(old);
                }
            }
            return removeList;
        }
    }

    public List<Mesh> Shapes;

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

    public void SaveLevel(string levelName = "")
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

        // Open file with filter
        var extensions = new [] {
            new ExtensionFilter("Text file", "txt"),
            new ExtensionFilter("All Files", "*" ),
        };
        var browser = StandaloneFileBrowser.SaveFilePanel("Save Level", "", "level.txt", extensions);
        File.WriteAllText(browser, JsonUtility.ToJson(mainLevel, false));
        ClearLists();
    }

    public void LoadLevel(string levelFile = null)
    {
        if (string.IsNullOrEmpty(levelFile))
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
        SelectionManager.instance.SelectAll();
        SelectionManager.instance.DeleteSelection();
        var loadedLevel = new Level();
        JsonUtility.FromJsonOverwrite(File.ReadAllText(levelFile), loadedLevel);

        foreach (var block in loadedLevel.LevelArray)
        {
            var prefab = (GameObject) Resources.Load("Level Objects/" + block.Type);
            if (prefab == null) continue;
            var newBlock = Instantiate(prefab);
            var blockComp = newBlock.GetComponent<LevelObject>();
            newBlock.transform.parent = transform;

            newBlock.transform.position = block.Position;
            newBlock.transform.localScale = block.Size;
            newBlock.transform.rotation = block.Rotation;
            if (blockComp == null) continue;
            blockComp.LockX = block.LockX;
            blockComp.LockY = block.LockY;
            blockComp.LockZ = block.LockZ;
        }

        if (loadedLevel.SignsArray != null && loadedLevel.SignsArray.Any())
            foreach (var sign in loadedLevel.SignsArray)
            {
                var prefab = (GameObject) Resources.Load("Level Objects/Sign");
                if (prefab == null) continue;
                var newBlock = Instantiate(prefab);
                var signComp = newBlock.GetComponent<Sign>();
                newBlock.transform.parent = transform;

                newBlock.transform.position = sign.Position;
                newBlock.transform.localScale = sign.Size;
                newBlock.transform.rotation = sign.Rotation;
                signComp.LockX = sign.LockX;
                signComp.LockY = sign.LockY;
                signComp.LockZ = sign.LockZ;
                signComp.SignText = sign.text;
            }

        if (loadedLevel.LightsArray != null && loadedLevel.LightsArray.Any())
            foreach (var light in loadedLevel.LightsArray)
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

        if (loadedLevel.ZiplinesArray != null && loadedLevel.ZiplinesArray.Any())
            foreach (var zip in loadedLevel.ZiplinesArray)
            {
                Debug.Log(zip.Type);
                var prefab = (GameObject) Resources.Load("Level Objects/" + zip.Type);
                if (prefab == null) continue;
                var newBlock = Instantiate(prefab, transform);
                var zipComp = newBlock.GetComponent<Zipline>();

                zipComp.SetPoles(zip.PoleBlocks[0], zip.PoleBlocks[1]);
            }
        
        if (loadedLevel.MovingArray != null && loadedLevel.MovingArray.Any())
            foreach (var move in loadedLevel.MovingArray)
            {
                Debug.Log(move.Type);
                var prefab = (GameObject) Resources.Load("Level Objects/MovingBlock");
                if (prefab == null) continue;
                var emptyGroup = new GameObject("MovingBlockGroup");
                emptyGroup.transform.parent = transform;

                var newBlock = Instantiate(prefab, emptyGroup.transform);
                newBlock.transform.position = move.Position;
                newBlock.transform.rotation = move.Rotation;
                newBlock.transform.localScale = move.Size;

                if (move.Waypoints != null && move.Waypoints.Any())
                foreach (var waypoint in move.Waypoints)
                {
                    var newWay = (GameObject) Resources.Load("Level Objects/Waypoint");
                    newWay = Instantiate(newWay, emptyGroup.transform);
                    newWay.transform.position = waypoint.Position;
                    newWay.transform.localScale = waypoint.Size;
                    newWay.transform.rotation = waypoint.Rotation;
                }
            }
        
    }

    public void RegisterObject(Block obj)
    {
        if (obj == null) return;
        Blocks.Add(obj);
    }

    public void RegisterObject(MovingBlock obj)
    {
        if (obj == null) return;
        MovingBlocks.Add(obj);
    }

    public void RegisterObject(SignBlock obj)
    {
        if (obj == null) return;
        SignBlocks.Add(obj);
    }

    public void RegisterObject(ZiplineBlock obj)
    {
        if (obj == null) return;
        ZiplineBlocks.Add(obj);
    }

    public void RegisterObject(LightBlock obj)
    {
        if (obj == null) return;
        LightBlocks.Add(obj);
    }

    public void RegisterObject(ClimbeyGroup obj)
    {
        if (obj == null) return;
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