using AdvancedColorPicker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickColor : MonoBehaviour
{
	public Color selectedColor = Color.white;
	[Space]
	public RectTransform container;
	public PopupWindow prefab;

	private PopupWindow instance;

    public void OpenColorPicker ()
	{
		if (instance == null)
			instance = PopupWindow.Open(container, transform as RectTransform, prefab);

		var picker = instance.GetComponentInChildren<ColorPicker>();

		picker.CurrentColor = Color.green;
		picker.OnColorChanged.AddListener(OnColorChanged);
	}

	public void OnColorChanged (Color newColor)
	{
		selectedColor = newColor;
	}

}
