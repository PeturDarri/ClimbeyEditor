using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UndoMethods;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InspectorManager : MonoBehaviour
{

    public static InspectorManager Instance;

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
    }

    private void Start()
    {
        SelectionManager.Instance.OnSelectionChanged += UpdatePanel;
        UndoRedoManager.Instance().UndoStackStatusChanged += UndoChanged;
        UndoRedoManager.Instance().RedoStackStatusChanged += UndoChanged;
    }

    private void UndoChanged(bool nothing)
    {
        UpdatePanel();
    }

    private void UpdatePanel()
    {
        var selection = SelectionManager.Instance.Selection;
        ClearPanel();
        if (selection.Count == 0) return;
        CreateHeader(selection.Count > 1 ? selection.Count + " selected" : selection[0].Type.ToString());
        if (selection.Count >= 1)
        {
            var allSame = true;
            var type = selection[0].GetType();
            foreach (var child in selection.ToList())
            {
                if (child.GetType() != type)
                {
                    allSame = false;
                }
            }

            if (!allSame)
            {
                return;
            }
        }
        var properties = selection[0].GetProperties();

        foreach (var field in properties)
        {
            if (field.Value is Enum)
            {
                var template = Instantiate((GameObject) Resources.Load("Unloadables/UI/Dropdown Template"),
                    transform);
                template.transform.localScale = Vector3.one;
                template.GetComponentInChildren<Text>().text = field.Key;
                var dropdown = template.GetComponentInChildren<Dropdown>();

                dropdown.options.Clear();

                var num = Enum.GetValues(field.Value.GetType());
                foreach (var theEnum in num)
                {
                    dropdown.options.Add(new Dropdown.OptionData(theEnum.ToString()));
                }

                var allSame = true;
                foreach (var child in selection)
                {
                    var childProperties = child.GetProperties();
                    if (childProperties[field.Key].ToString() != field.Value.ToString())
                    {
                        allSame = false;
                    }
                }

                dropdown.value = Enum.GetValues(typeof(Shape)).Cast<Shape>().ToList().IndexOf((Shape) field.Value);
                dropdown.captionText.text = allSame ? field.Value.ToString() : "--";
                dropdown.onValueChanged.AddListener(delegate { FieldChanged("Shape", dropdown.value); });
            }
            else if (field.Value is string)
            {
                var template = Instantiate((GameObject) Resources.Load("Unloadables/UI/Text Template"), transform);
                template.transform.localScale = Vector3.one;
                template.GetComponentInChildren<Text>().text = field.Key;
                var text = template.GetComponentInChildren<InputField>();
                var fieldString = field.Key;

                var allSame = true;
                foreach (var child in selection)
                {
                    var childProperties = child.GetProperties();
                    if (childProperties[field.Key].ToString() != field.Value.ToString())
                    {
                        allSame = false;
                    }
                }

                text.text = allSame ? field.Value.ToString() : "--";
                text.onEndEdit.AddListener(delegate { FieldChanged(fieldString, text.text); });
            }
            else if (field.Value is int)
            {
                var template = Instantiate((GameObject) Resources.Load("Unloadables/UI/Text Template"), transform);
                template.transform.localScale = Vector3.one;
                template.GetComponentInChildren<Text>().text = field.Key;
                var text = template.GetComponentInChildren<InputField>();
                text.contentType = InputField.ContentType.IntegerNumber;

                var allSame = true;
                foreach (var child in selection)
                {
                    var childProperties = child.GetProperties();
                    if (childProperties[field.Key].ToString() != field.Value.ToString())
                    {
                        allSame = false;
                    }
                }

                text.text = allSame ? field.Value.ToString() : "--";
                var fieldString = field.Key;
                text.onEndEdit.AddListener(delegate { FieldChanged(fieldString, Convert.ToInt32(text.text)); });
            }
            else if (field.Value is float)
            {
                var template = Instantiate((GameObject) Resources.Load("Unloadables/UI/Text Template"), transform);
                template.transform.localScale = Vector3.one;
                template.GetComponentInChildren<Text>().text = field.Key;
                var text = template.GetComponentInChildren<InputField>();
                text.contentType = InputField.ContentType.DecimalNumber;

                var allSame = true;
                foreach (var child in selection)
                {
                    var childProperties = child.GetProperties();
                    if (childProperties[field.Key].ToString() != field.Value.ToString())
                    {
                        allSame = false;
                    }
                }

                text.text = allSame ? field.Value.ToString() : "--";
                var fieldString = field.Key;
                text.onEndEdit.AddListener(delegate { FieldChanged(fieldString, float.Parse(text.text)); });
            }
            else if (field.Value is bool)
            {
                var template = Instantiate((GameObject) Resources.Load("Unloadables/UI/Bool Template"), transform);
                template.transform.localScale = Vector3.one;
                template.GetComponentInChildren<Text>().text = field.Key;
                var toggle = template.GetComponentInChildren<Toggle>();
                var fieldString = field.Key;

                var allSame = true;
                foreach (var child in selection)
                {
                    var childProperties = child.GetProperties();
                    if (childProperties[field.Key].ToString() != field.Value.ToString())
                    {
                        allSame = false;
                    }
                }
                toggle.isOn = allSame && (bool)field.Value;
                toggle.onValueChanged.AddListener(delegate { FieldChanged(fieldString, toggle.isOn); });
            }
            else if (field.Value is Delegate)
            {
                var template = Instantiate((GameObject) Resources.Load("Unloadables/UI/Button Template"), transform);
                template.transform.localScale = Vector3.one;
                template.GetComponentInChildren<Text>().text = field.Key;
                var button = template.GetComponentInChildren<Button>();
                var fieldMethod = field.Value;
                button.onClick.AddListener(delegate { ButtonPressed((Delegate)fieldMethod); });
            }
        }
    }

    private void ClearPanel()
    {
        foreach (var element in GetComponentsInChildren<RectTransform>().ToList())
        {
            if (element.name == name) continue;
            Destroy(element.gameObject);
        }
    }

    private void CreateHeader(string text)
    {
        var header = Instantiate((GameObject) Resources.Load("Unloadables/UI/Header"), transform);
        header.transform.localScale = Vector3.one;
        header.GetComponentInChildren<Text>().text = text;
    }

    //Events
    private void FieldChanged(string field, object value)
    {
        foreach (var select in SelectionManager.Instance.Selection.ToList())
        {
            var selectSave = select;
            SetProperty(field, selectSave, value);
        }

        UpdatePanel();
    }

    private static void SetProperty(string property, object obj, object value)
    {
        UndoRedoManager.Instance().Push(v=>SetProperty(property, obj, v), obj.GetType().GetProperty(property).GetValue(obj, null), "Change " + property);
        obj.GetType().GetProperty(property).SetValue(obj, value, null);
    }

    private static void ButtonPressed(Delegate method)
    {
        foreach (var select in SelectionManager.Instance.Selection.ToList())
        {
            method.DynamicInvoke();
        }
    }
}
