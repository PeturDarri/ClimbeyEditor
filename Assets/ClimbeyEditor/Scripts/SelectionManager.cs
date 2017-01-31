using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Level;
using RuntimeGizmos;

public class SelectionManager : MonoBehaviour
{

    public static SelectionManager instance;

    private List<LevelObject> _selection;
    public List<LevelObject> Selection
    {
        get { return GetSelection(); }
        private set { _selection = value; }
    }

    public bool isEmpty
    {
        get { return GetSelection().Count < 1; }
    }

    public Transform emptySelection;
    private Vector3 posOffset, rotOffset, sizeOffset;

    public Bounds SelectBounds
    {
        get { return GetBounds(); }
    }

    //Events
    public delegate void OnSelectionChangedEvent();
    public event OnSelectionChangedEvent OnSelectionChanged;

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

        if (emptySelection == null)
        {
            emptySelection = new GameObject("emptySelection").transform;
        }

        _selection = GetComponentsInChildren<LevelObject>().ToList();
        Selection = GetComponentsInChildren<LevelObject>().ToList();
    }

    private void Update()
    {
        SelectionHotkeys();
        TransformSelection();
    }

    public void SetSelection(Transform levelObject)
    {
        if (levelObject != null)
        {
            if (levelObject.GetComponent<LevelObject>() != null)
            {
                //Set selection
                if (Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftShift))
                {
                    //Add to selection

                    //Save current list
                    var oldList = GetSelection();
                    ClearSelection();
                    oldList.Add(levelObject.GetComponent<LevelObject>());

                    SetMultiSelection(oldList);
                }
                else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift))
                {
                    //Remove from selection
                    RemoveFromSelection(levelObject.GetComponent<LevelObject>());
                }
                else
                {
                    ClearSelection();

                    CenterSelf(levelObject.GetComponent<LevelObject>());
                    //Set object as selection
                    AddToSelection(levelObject.GetComponent<LevelObject>());
                }

                if (OnSelectionChanged != null)
                {
                    OnSelectionChanged();
                }
            }
        }
        else
        {
            ClearSelection();

            if (OnSelectionChanged != null)
            {
                OnSelectionChanged();
            }
        }
    }

    private List<LevelObject> GetSelection()
    {
        UpdateSelection();
        return _selection;
    }

    private void ClearSelection()
    {

        //Remove current selection
        foreach (var lvlObject in GetSelection())
        {
            //Set it to parent of SelectionManager (Level GameObject)
            RemoveFromSelection(lvlObject);
        }

        transform.rotation = Quaternion.identity;
    }

    private void UpdateSelection()
    {
        Selection = GetComponentsInChildren<LevelObject>().ToList();
    }

    private void CenterSelf(List<LevelObject> objList = null)
    {

        //Set parent position to center of all children
        var bounds = GetBounds(objList);
        transform.localScale = bounds.size;
        emptySelection.localScale = bounds.size;
        transform.position = transform.position + bounds.center;
        emptySelection.position = transform.position + bounds.center;
    }

    private void CenterSelf(LevelObject obj)
    {
        //Set parent position to center of all children
        var objList = new List<LevelObject> {obj};
        var bounds = GetBounds(objList);
        transform.localScale = bounds.size;
        emptySelection.localScale = bounds.size;
        transform.position = transform.position + bounds.center;
        emptySelection.position = transform.position + bounds.center;
    }

    private void SelectionHotkeys()
    {
        //Control
        if (Input.GetKey(KeyCode.LeftControl))
        {
            //Press Ctrl+D to duplicate target
            if (Input.GetKeyDown(KeyCode.D))
            {
                DuplicateSelection();
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                //Select all
                SelectAll();
            }
            else if (Input.GetKeyDown(KeyCode.G))
            {
                //GroupSelection();
            }
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                LevelManager.instance.SaveLevel("Itsa me");
            }
        }
        else
        {
            //Non-modifier

            //Press F to focus
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (!isEmpty)
                {
                    CameraManager.instance.SetTarget(transform);
                }
            }
            //Press Delete to delete selection
            else if (Input.GetKeyDown(KeyCode.Delete))
            {
                DeleteSelection();
            }
        }
    }

    private void DuplicateSelection()
    {
        if (!isEmpty)
        {
            var dupeList = GetSelection();
            ClearSelection();
            var newList = new List<LevelObject>();
            foreach (var dupe in dupeList)
            {
                var newObj = Instantiate(dupe);
                newObj.name = dupe.name;
                newObj.transform.position = dupe.transform.position;
                newList.Add(newObj);
            }
            SetMultiSelection(newList);

            if (OnSelectionChanged != null)
            {
                OnSelectionChanged();
            }
        }
    }

    private void SetMultiSelection(List<LevelObject> objList)
    {
        //Get all positions of children to list
        List<Vector3> posList = new List<Vector3>();
        foreach (var obj in objList)
        {
            posList.Add(obj.transform.position);
        }

        CenterSelf(objList);

        //Set all objects as children of parent
        foreach (var obj in objList)
        {
            AddToSelection(obj);
        }
    }

    private Bounds GetBounds(List<LevelObject> children = null)
    {
        if (children == null)
        {
            children = GetSelection();
            if (children.Count == 0)
            {
                return new Bounds();
            }
        }
        var currentRotation = transform.rotation;
        transform.rotation = Quaternion.Euler(0f,0f,0f);

        var bounds = new Bounds(children[0].transform.position, Vector3.zero);

        foreach(var child in children)
        {
            var render = child.GetComponent<Renderer>();
            bounds.Encapsulate(render.bounds);
        }

        var localCenter = bounds.center - transform.position;
        bounds.center = localCenter;

        transform.rotation = currentRotation;
        return bounds;
    }

    private void AddToSelection(LevelObject levelObject)
    {
        //Check if current LevelObject is a child of another LevelObject
        LevelObject parent;
        try
        {
            parent = levelObject.transform.parent.GetComponent<LevelObject>();
        }
        catch
        {
            parent = null;
        }

        while (parent != null)
        {
            levelObject = parent;
            parent = levelObject.transform.parent.GetComponent<LevelObject>();
        }

        levelObject.transform.parent = transform;
        levelObject.SetSelection(true);

        emptySelection.position = transform.position;
    }

    private void RemoveFromSelection(LevelObject levelObject)
    {
        if (levelObject.transform.parent == transform)
        {
            levelObject.transform.parent = transform.parent;
            levelObject.SetSelection(false);
        }
    }

    private void DeleteSelection()
    {
        var oldList = GetSelection();
        ClearSelection();
        foreach (var obj in oldList)
        {
            Destroy(obj.gameObject);
            //Debug.Log("Destroy!");
        }

        if (OnSelectionChanged != null)
        {
            OnSelectionChanged();
        }
    }

    private void SelectAll()
    {
        ClearSelection();
        SetMultiSelection(LevelManager.instance.LevelObjects);

        if (OnSelectionChanged != null)
        {
            OnSelectionChanged();
        }
    }

    private void TransformSelection()
    {
        transform.position = emptySelection.position;
        transform.eulerAngles = emptySelection.eulerAngles;
        transform.localScale = emptySelection.localScale;
    }
}
