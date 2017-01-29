using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolManager : MonoBehaviour {

    public static ToolManager instance;
    // Use this for initialization
	void Awake()
	{
	    if (instance == null)
	    {
	        instance = this;
	    }
	    else if (instance != this)
	    {
	        Destroy(gameObject);
	    }
	}
	
	// Update is called once per frame
	void Update()
	{
		
	}

    public GameObject SpawnObject(GameObject spawnObject)
    {
        var obj = Instantiate(spawnObject, CameraManager.instance.target.position, Quaternion.identity);
        CameraManager.instance.SetTarget(obj.transform);
        obj.name = spawnObject.name;
        SelectionManager.instance.SetSelection(obj.transform);
        return obj;
    }
}
