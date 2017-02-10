using System.Collections;
using System.Collections.Generic;
using UndoMethods;
using UnityEngine;

public class ToolManager : MonoBehaviour {

    public static ToolManager Instance;
    // Use this for initialization
	void Awake()
	{
	    if (Instance == null)
	    {
	        Instance = this;
	    }
	    else if (Instance != this)
	    {
	        Destroy(gameObject);
	    }
	}

    public void SpawnObject(GameObject spawnObject)
    {
        SelectionManager.Instance.CreateObject(spawnObject);
    }
}