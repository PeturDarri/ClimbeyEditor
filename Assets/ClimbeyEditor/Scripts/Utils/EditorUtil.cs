using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class EditorUtil
{
	//Here to ignore mouse events if user is interacting with UI
	public static bool GetMouseButtonDown(int button)
	{
		return !EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(button);
	}

	public static bool GetMouseButton(int button)
	{
		return !EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButton(button);
	}

	public static bool GetMouseButtonUp(int button)
	{
		return Input.GetMouseButtonUp(button);
	}

	public static bool IsMouseOverUI()
	{
		return EventSystem.current.IsPointerOverGameObject();
	}
}
