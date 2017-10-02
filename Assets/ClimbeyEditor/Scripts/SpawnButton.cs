using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SpawnButton : MonoBehaviour
{
    public GameObject spawnObject;

	// Use this for initialization
	private void Start ()
	{
	    GetComponent<Button>().onClick.AddListener(OnClick);
	}
	
    private void OnClick()
    {
	    if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) &&
	        !SelectionManager.Instance.isEmpty && spawnObject.GetComponent<LevelObject>().GetType() == typeof(BasicBlock))
	    {
		    ToolManager.Instance.SwitchSelectionToObject(spawnObject);
		    return;
	    }

	    ToolManager.Instance.SpawnObject(spawnObject);
    }
}
