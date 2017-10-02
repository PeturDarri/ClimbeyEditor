using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

	public void SwitchSelectionToObject(GameObject spawnObject)
	{
		if (spawnObject.GetComponent<BasicBlock>() == null) return;
		using (new UndoTransaction("Change selection type"))
		{
			var selection = SelectionManager.Instance.Selection.ToList();
			var newSelection = new List<LevelObject>();
			foreach (var levelObject in selection)
			{
				if (levelObject.GetType() == typeof(BasicBlock))
				{
					var block = (BasicBlock) levelObject;
					
					var newBlock = Instantiate(spawnObject, LevelManager.Instance.transform).GetComponent<BasicBlock>();
					UndoRedoManager.Instance().Push(newBlock.DoDestroy, true, "Create new block");
					newBlock.transform.position = block.transform.position;
					newBlock.transform.rotation = block.transform.rotation;
					newBlock.transform.localScale = block.transform.localScale;
					newBlock.Shape = block.Shape;
					SelectionManager.Instance.AddToSelection(newBlock);
					block.DoDestroy();
					newSelection.Add(newBlock);
				}
			}
			SelectionManager.Instance.ClearAndMulti(newSelection);
		}
	}

    public void SpawnObject(GameObject spawnObject)
    {
        SelectionManager.Instance.CreateObject(spawnObject);
    }
}