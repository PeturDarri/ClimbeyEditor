using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InspectorManager : MonoBehaviour
{

    public static InspectorManager instance;

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

    private void Start()
    {
        SelectionManager.instance.OnSelectionChanged += UpdatePanel;
    }

    private void UpdatePanel()
    {
        var selection = SelectionManager.instance.Selection;
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
            if (field.Key == "Shape")
            {
                var template = Instantiate((GameObject) Resources.Load("Unloadables/UI/Dropdown Template"),
                    transform);
                template.transform.localScale = Vector3.one;
                template.GetComponentInChildren<Text>().text = "Shape";
                var dropdown = template.GetComponentInChildren<Dropdown>();

                dropdown.options.Clear();
                //Go through all existing shapes and create the option list
                foreach (var shape in Enum.GetValues(typeof(Shape)).Cast<Shape>())
                {
                    dropdown.options.Add(new Dropdown.OptionData(shape.ToString()));
                }

                dropdown.captionText.text = selection.Count > 1 ? "--" : field.Value.ToString();
                dropdown.value = Enum.GetValues(typeof(Shape)).Cast<Shape>().ToList().IndexOf((Shape) field.Value);
                dropdown.onValueChanged.AddListener(delegate { FieldChanged("Shape", dropdown.value); });
            }
            else if (field.Value is string || field.Value is int || field.Value is float)
            {
                var template = Instantiate((GameObject) Resources.Load("Unloadables/UI/Text Template"), transform);
                template.transform.localScale = Vector3.one;
                template.GetComponentInChildren<Text>().text = field.Key;
                var text = template.GetComponentInChildren<InputField>();
                var fieldString = field.Key;
                text.text = selection.Count > 1 ? "--" : field.Value.ToString();
                text.onValueChanged.AddListener(delegate { FieldChanged(fieldString, Convert.ToInt32(text.text)); });
            }
            else if (field.Value is bool)
            {
                var template = Instantiate((GameObject) Resources.Load("Unloadables/UI/Bool Template"), transform);
                template.transform.localScale = Vector3.one;
                template.GetComponentInChildren<Text>().text = field.Key;
                var toggle = template.GetComponentInChildren<Toggle>();
                var fieldString = field.Key;
                toggle.isOn = selection.Count == 1 && (bool)field.Value;
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
    private static void FieldChanged(string field, object value)
    {
        foreach (var select in SelectionManager.instance.Selection.ToList())
        {
            select.GetType().GetProperty(field).SetValue(select, value, null);
        }
    }

    private static void ButtonPressed(Delegate method)
    {
        foreach (var select in SelectionManager.instance.Selection.ToList())
        {
            method.DynamicInvoke();
        }
    }
}
