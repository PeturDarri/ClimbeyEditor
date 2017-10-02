using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridSizeText : MonoBehaviour
{
	private float _snapValue;
	private Text _text;

	private void Start()
	{
		_text = GetComponent<Text>();
	}

	private void Update()
	{
		if (Math.Abs(GridManager.Instance.SnapValue - _snapValue) > 0.0000001f)
		{
			_snapValue = GridManager.Instance.SnapValue;
			UpdateText();
		}
	}

	private void UpdateText()
	{
		_text.text = _snapValue.ToString();
	}
}
