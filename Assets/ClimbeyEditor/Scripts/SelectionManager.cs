using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using RuntimeGizmos;
using UnityEngine.EventSystems;

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
    private bool centerChanged, isDragging, shouldDrawBox;
    private Vector3 prevMouse, mousePos;
    public Texture selectionBox;

    public bool isTransforming
    {
        get { return Camera.main.GetComponent<TransformGizmo>().isTransforming; }
    }

    public TransformType transformType
    {
        get { return Camera.main.GetComponent<TransformGizmo>().type; }
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
        DragSelection();
        SelectionHotkeys();
        if (!isTransforming) return;
        TransformSelection();
        UpdateTransform();
    }

    public void SetSelection(Transform levelObject)
    {
        if (levelObject != null)
        {
            if (levelObject.GetComponent<LevelObject>() == null) return;
            if (Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftShift))
            {
                //Add to selection
                AddToSelection(levelObject.GetComponent<LevelObject>());
            }
            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift))
            {
                //Remove from selection
                RemoveFromSelection(levelObject.GetComponent<LevelObject>());
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
            //Set selection
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
        foreach (var obj in Selection.ToList())
        {
            RemoveFromSelection(obj);
        }
        //Selection.Clear();
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
        transform.eulerAngles = GetChildRotation();
        emptySelection.position = emptySelection.position + bounds.center;
        emptySelection.localScale = bounds.size;
        emptySelection.eulerAngles = GetChildRotation();
        ResetPrev();

        centerChanged = true;
    }

    private Vector3 GetChildRotation()
    {
        return Selection.Count > 0 ? Selection.Count > 1 ? Vector3.zero : Selection[0].transform.eulerAngles : Vector3.zero;
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

            }
            else if (Input.GetKeyDown(KeyCode.O))
            {
                LevelManager.instance.LoadLevel();
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
            else if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                GridManager.instance.DoubleHalveGrid(true);
            }
            else if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                GridManager.instance.DoubleHalveGrid(false);
            }
        }
    }

    private void DuplicateSelection()
    {
        if (isEmpty) return;
        var dupeList = Selection.ToList();
        ClearSelection();
        var newList = new List<LevelObject>();
        foreach (var dupe in dupeList)
        {
            var newObj = dupe.Duplicate();
            if (newObj == null) continue;
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

    private void SetMultiSelection(List<LevelObject> objList)
    {
        if (objList.Count <= 0) return;
        //Set all objects as children of parent
        foreach (var obj in objList)
        {
            AddToSelection(obj);
        }
    }

    public Bounds GetBounds(bool local = false)
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
            child.transform.rotation = (local) ? Quaternion.identity : curRot;
            var collide = child.GetComponent<Collider>();
            bounds.Encapsulate(collide.bounds);
            child.transform.rotation = curRot;
        }

        var localCenter = bounds.center - transform.position;
        bounds.center = localCenter;

        return bounds;
    }

    public void AddToSelection(LevelObject levelObject)
    {
        if (Selection.Contains(levelObject)) return;
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
        levelObject.IsSelected = true;
    }

    public void DeleteSelection()
    {
        var oldList = Selection.ToList();
        ClearSelection();
        foreach (var obj in oldList)
        {
            RemoveFromSelection(obj);
            Destroy(obj.gameObject);
            //Debug.Log("Destroy!");
        }

        if (OnSelectionChanged != null)
        {
            OnSelectionChanged();
        }
    }

    public void RemoveFromSelection(LevelObject obj)
    {
        obj.IsSelected = false;
        Selection.Remove(obj);
    }

    public void SelectAll()
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
                child.transform.position += transform.position - prevPos;
                if (child.Rotateable && transformType == TransformType.Rotate)
                    child.transform.eulerAngles += transform.eulerAngles - prevRot;
                if(child.Scaleable && transformType == TransformType.Bounds)
                    child.transform.localScale += transform.localScale - prevSize;
            }
        }

        prevRot = transform.eulerAngles;
        prevSize = transform.localScale;
        prevPos = transform.position;
    }

    private void DragSelection()
    {
        if (GetMouseButtonDown(0))
        {
            prevMouse = mousePos;
            isDragging = true;
        }

        if (GetMouseButton(0))
        {
            if (Vector2.Distance(mousePos, prevMouse) > 10 && CameraManager.instance.cameraState == CameraManager.CameraState.Free && !isTransforming)
            {
                shouldDrawBox = true;
            }
        }

        if (GetMouseButtonUp(0))
        {
            var box = FromDragPoints(prevMouse, mousePos);
            if (isDragging && shouldDrawBox)
            {
                SelectDrag(box);
            }
            isDragging = false;
            shouldDrawBox = false;
        }
    }

    private void OnGUI()
    {
        mousePos = Event.current.mousePosition;
        //Draw selection box
        if (!isDragging || !shouldDrawBox) return;
        var box = FromDragPoints(prevMouse, mousePos);
        GUI.color = new Color32(66, 134, 244, 50);
        GUI.DrawTexture(box, selectionBox);
    }

    private void SelectDrag(Rect box)
    {
        var selected = new List<LevelObject>();

        foreach (var child in LevelManager.instance.LevelObjects)
        {
            var point = CameraManager.instance.Camera.WorldToScreenPoint(child.transform.position);
            point.y = Mathf.Abs(point.y - Screen.height);

            if (box.Contains(point))
            {
                selected.Add(child);
            }
        }

        Debug.Log(selected.Count);

        ClearSelection();
        SetMultiSelection(selected);

        if (OnSelectionChanged != null)
        {
            OnSelectionChanged();
        }
    }

    private Rect FromDragPoints(Vector2 p1, Vector2 p2)
    {
        var d = p1 - p2;
        var r = new Rect
        {
            x = d.x < 0 ? p1.x : p2.x,
            y = d.y < 0 ? p1.y : p2.y,
            width = Mathf.Abs(d.x),
            height = Mathf.Abs(d.y)
        };
        return r;
    }

    //Here to ignore mouse events if user is interacting with UI
    private bool GetMouseButtonDown(int button)
    {
        return !EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(button);
    }

    private bool GetMouseButton(int button)
    {
        return !EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButton(button);
    }

    private bool GetMouseButtonUp(int button)
    {
        return Input.GetMouseButtonUp(button);
    }
}
