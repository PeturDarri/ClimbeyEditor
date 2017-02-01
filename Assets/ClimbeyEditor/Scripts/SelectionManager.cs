using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using RuntimeGizmos;

public class SelectionManager : MonoBehaviour
{

    public static SelectionManager instance;

    public List<LevelObject> Selection;
    public Transform emptySelection;

    public bool isEmpty
    {
        get { return Selection.Count < 1; }
    }

    private Vector3 prevPos, prevSize, prevRot;
    private bool centerChanged;

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

        Selection = new List<LevelObject>();

        prevPos = Vector3.one;
        prevRot = Vector3.one;
        prevRot = Vector3.one;

        OnSelectionChanged += SelectionChanged;
    }

    private void Start()
    {
        GridManager.instance.GridDisabled += GridDisabled;
        GridManager.instance.OnSnap += Snap;
    }

    private void SelectionChanged()
    {
        CenterSelf();
    }

    private void Update()
    {
        UpdateTransform();
        SelectionHotkeys();
        if (!isEmpty)
        {
            TransformSelection();
        }
    }

    private void LateUpdate()
    {

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
                    Selection.Add(levelObject.GetComponent<LevelObject>());
                }
                else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift))
                {
                    //Remove from selection
                    Selection.Remove(levelObject.GetComponent<LevelObject>());
                }
                else
                {
                    ClearSelection();
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

    private void GridDisabled()
    {
        emptySelection.position = transform.position;
        emptySelection.rotation = transform.rotation;
        emptySelection.localScale = transform.localScale;
    }

    private void Snap()
    {
        TransformSelection();
    }

    private void ClearSelection()
    {
        Selection.Clear();
    }

    private void ResetPrev()
    {
        prevPos = transform.position;
        prevRot = transform.eulerAngles;
        prevSize = transform.localScale;
    }

    private void CenterSelf()
    {
        //Set parent position to center of all children
        var bounds = GetBounds();
        //transform.localScale = bounds.size;
        transform.position = transform.position + bounds.center;
        transform.localScale = bounds.size;
        transform.rotation = Quaternion.identity;
        emptySelection.position = emptySelection.position + bounds.center;
        emptySelection.localScale = bounds.size;
        emptySelection.rotation = Quaternion.identity;
        ResetPrev();

        centerChanged = true;
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
            else if (Input.GetKeyDown(KeyCode.O))
            {
                LevelManager.instance.LoadLevel("Itsa me.txt");
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
            var dupeList = Selection.ToList();
            ClearSelection();
            var newList = new List<LevelObject>();
            foreach (var dupe in dupeList)
            {
                var newObj = Instantiate(dupe);
                newObj.name = dupe.name;
                newObj.transform.position = dupe.transform.position;
                newObj.transform.parent = LevelManager.instance.transform;
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
        if (objList.Count > 0)
        {
            //Set all objects as children of parent
            foreach (var obj in objList)
            {
                AddToSelection(obj);
            }
        }
    }

    private Bounds GetBounds()
    {
        var children = Selection.ToList();
        if (children.Count == 0)
        {
            return new Bounds();
        }

        var bounds = new Bounds(children[0].transform.position, Vector3.zero);

        foreach(var child in children)
        {
            //Reset rotation
            var curRot = child.transform.rotation;
            child.transform.rotation = Quaternion.identity;
            var render = child.GetComponent<Renderer>();
            bounds.Encapsulate(render.bounds);
            child.transform.rotation = curRot;
        }

        var localCenter = bounds.center - transform.position;
        bounds.center = localCenter;

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

        Selection.Add(levelObject);
        levelObject.SetSelection(true);
    }

    private void DeleteSelection()
    {
        var oldList = Selection.ToList();
        ClearSelection();
        foreach (var obj in oldList)
        {
            Selection.Remove(obj);
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
        SetMultiSelection(LevelManager.instance.LevelObjects);

        if (OnSelectionChanged != null)
        {
            OnSelectionChanged();
        }
    }

    private void UpdateTransform()
    {
        transform.position = emptySelection.position;
        transform.rotation = emptySelection.rotation;
        transform.localScale = emptySelection.localScale;
    }

    public void TransformSelection()
    {
        if (centerChanged)
        {
            centerChanged = false;
        }
        else
        {
            var selection = Selection.ToList();
            foreach (var child in selection)
            {
                child.transform.eulerAngles += transform.eulerAngles - prevRot;
                child.transform.localScale += transform.localScale - prevSize;
                child.transform.position += transform.position - prevPos;
            }
        }

        prevRot = transform.eulerAngles;
        prevSize = transform.localScale;
        prevPos = transform.position;
    }
}
