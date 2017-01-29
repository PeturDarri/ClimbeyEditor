using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class LevelManager : MonoBehaviour
{

    public static LevelManager instance;

    public List<LevelObject> LevelObjects
    {
        get
        {
            var allChildren = GetComponentsInChildren<LevelObject>().ToList();
            var removedChildren = allChildren.ToList();

            //Remove all children that are children of LevelObjects
            foreach (var child in allChildren)
            {
                if (child.transform.parent.GetComponent<LevelObject>() != null)
                {
                    removedChildren.Remove(child);
                }
            }
            return removedChildren;
        }
    }

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
	}
}
