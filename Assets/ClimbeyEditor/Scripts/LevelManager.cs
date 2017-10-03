using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using UnityEngine;
using SFB;
using UndoMethods;

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
        public int Checkpoints;
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

    public static LevelManager Instance;
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
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        //Load standard start level
        LoadLevel(Application.dataPath + "/startlevel.txt");
    }

    public void SaveLevel(string levelName = "")
    {
        if (OnSave != null)
        {
            OnSave();
        }

        if (Blocks.Count == 0)
        {
            ClearLists();
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
        ClearLists();
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
        SelectionManager.Instance.ClearSelection();
        UndoRedoManager.Instance().Clear();
        foreach (var obj in LevelObjects)
        {
            Destroy(obj.gameObject);
        }
        var loadedLevel = new Level();
        JsonUtility.FromJsonOverwrite(File.ReadAllText(levelFile), loadedLevel);

        if (loadedLevel.GroupsArray != null && loadedLevel.GroupsArray.Any())
        Groups.AddRange(loadedLevel.GroupsArray);

        foreach (var block in loadedLevel.LevelArray)
        {
            if (block.Type.Contains("Group:"))
            {
                CreateGroup(block);
                continue;
            }
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

            if (block.Type == "[CameraRig]")
                CameraManager.Instance.Camera.GetComponent<MouseOrbit>().Focus(newBlock.transform);
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
                var prefab = (GameObject) Resources.Load("Level Objects/" + zip.Type);
                if (prefab == null) continue;
                var newBlock = Instantiate(prefab, transform);
                var zipComp = newBlock.GetComponent<Zipline>();

                zipComp.SetPoles(zip.PoleBlocks[0], zip.PoleBlocks[1]);
            }
        
        if (loadedLevel.MovingArray != null && loadedLevel.MovingArray.Any())
            foreach (var move in loadedLevel.MovingArray)
            {
                var prefab = (GameObject) Resources.Load("Level Objects/MovingBlock");
                if (prefab == null) continue;
                var emptyGroup = new GameObject("MovingBlockGroup");
                emptyGroup.transform.parent = transform;

                var newBlock = Instantiate(prefab, emptyGroup.transform);
                newBlock.transform.position = move.Position;
                newBlock.transform.rotation = move.Rotation;
                newBlock.transform.localScale = move.Size;
                var newMove = newBlock.GetComponent<global::MovingBlock>();
                newMove.BlockType = move.Type == "MovingDeath"
                    ? global::MovingBlock.Type.Death
                    : global::MovingBlock.Type.Normal;
                newMove.ArrivalTime = move.ArrivalTime;
                newMove.PingPong = move.PingPong;
                newMove.Speed = move.Speed;
                newMove.WaitForPlayer = move.WaitForPlayer;

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
        if (loadedLevel.LevelSettings != null)
        {
            var prefab = (GameObject) Resources.Load("Level Objects/LevelSign");
            var newBlock = Instantiate(prefab, transform);
            newBlock.transform.position = loadedLevel.LevelSettings.Position;
            newBlock.transform.rotation = loadedLevel.LevelSettings.Rotation;
            newBlock.transform.localScale = Vector3.one;
            var settings = newBlock.GetComponent<LevelSign>();
            settings.Checkpoints = loadedLevel.LevelSettings.Checkpoints;
            settings.Gamemode = loadedLevel.LevelSettings.Gamemode;
        }

        SelectionManager.Instance.ClearSelection();
        UndoRedoManager.Instance().Clear();
    }

    public void RegisterObject(Block obj)
    {
        if (obj == null) return;
        switch (obj.GetType().ToString())
        {
            case "LevelManager+Block":
                Blocks.Add(obj);
                break;
            case "LevelManager+SignBlock":
                SignBlocks.Add((SignBlock) obj);
                break;
            case "LevelManager+LightBlock":
                LightBlocks.Add((LightBlock) obj);
                break;
            case "LevelManager+ZiplineBlock":
                ZiplineBlocks.Add((ZiplineBlock) obj);
                break;
            case "LevelManager+MovingBlock":
                MovingBlocks.Add((MovingBlock) obj);
                break;
            case "LevelManager+SettingsBlock":
                Settings = (SettingsBlock) obj;
                break;
        }
    }

    public void NewGroup(ClimbeyGroup group)
    {
        if (Groups.Any(g => g.Name == group.Name)) return;
        Groups.Add(group);
    }

    private void CreateGroup(Block groupBlock)
    {
        var groupName = groupBlock.Type.Split(':')[1];
        var prefab = (GameObject) Resources.Load("Level Objects/ClimbeyGroup");
        var newGroup = Instantiate(prefab, transform);
        var groupComp = newGroup.GetComponent<Group>();
        newGroup.transform.position = groupBlock.Position;
        newGroup.transform.localScale = groupBlock.Size;
        newGroup.transform.rotation = groupBlock.Rotation;
        groupComp.Name = groupName;
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