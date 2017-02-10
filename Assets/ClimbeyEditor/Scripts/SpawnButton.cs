using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnButton : MonoBehaviour
{

    public GameObject spawnObject;

	// Use this for initialization
	void Start () {
	    GetComponent<Button>().onClick.AddListener(OnClick);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnClick()
    {
        ToolManager.Instance.SpawnObject(spawnObject);
    }
}
