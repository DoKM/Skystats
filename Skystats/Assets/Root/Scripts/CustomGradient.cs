using AdvancedColorPicker;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomGradient : MonoBehaviour
{
    public GradientType gradientType;
    public ThemeGradient selectedGradient;

    public Color selectedColor = Color.white;
    [Space]
    public RectTransform container;
    public PopupWindow prefab;

    private PopupWindow instance;

    public void Open(bool left)
    {
        if (GradientManager.Instance.currentTheme.ThemeColors.FindIndex((x) => x.Type == gradientType) >= 0)
            selectedGradient = GradientManager.Instance.GetCurrentThemeColor(gradientType);

        if (instance == null)
        {
            instance = PopupWindow.Open(container, transform as RectTransform, prefab);

            var picker = instance.GetComponentInChildren<ColorPicker>();

            if (left)
            {
                picker.CurrentColor = selectedGradient.Color.colorKeys[0].color;
                picker.OnColorChanged.AddListener(ChangeLeftColor);
            }
            else
            {
                picker.CurrentColor = selectedGradient.Color.colorKeys[1].color;
                picker.OnColorChanged.AddListener(ChangeRightColor);
            }
        }
    }

    public void ChangeRightColor(Color newColor)
    {
		var newKeys = new Dictionary<GradientColorKey, GradientAlphaKey>
		{
			{ selectedGradient.Color.colorKeys[0], selectedGradient.Color.alphaKeys[0] },
			{ new GradientColorKey(newColor, 1), new GradientAlphaKey(1, 1) }
		};

		selectedGradient.Color.SetKeys(newKeys.Keys.ToArray(), newKeys.Values.ToArray());
        var gradRef = GradientManager.Instance.currentTheme.ThemeColors.Find(x => x.Type == gradientType);
        gradRef = new ThemeGradient { Color = selectedGradient.Color, Type = gradientType, GradientName = gradRef.GradientName };
        UpdateGradients();
    }

    public void ChangeLeftColor(Color newColor)
    {
		var newKeys = new Dictionary<GradientColorKey, GradientAlphaKey>
		{
			{ new GradientColorKey(newColor, 0), new GradientAlphaKey(1, 0) },
			{ selectedGradient.Color.colorKeys[1], selectedGradient.Color.alphaKeys[1] }
		};

		selectedGradient.Color.SetKeys(newKeys.Keys.ToArray(), newKeys.Values.ToArray());
        var gradRef = GradientManager.Instance.currentTheme.ThemeColors.Find(x => x.Type == gradientType);
        gradRef = new ThemeGradient { Color = selectedGradient.Color, Type = gradientType, GradientName = gradRef.GradientName };
        UpdateGradients();
    }

    public void UpdateGradients ()
	{
        var gradients = FindObjectsOfType<StaticGradient>();
		foreach (var item in gradients)
		{
            if (item.enabledGradient == gradientType)
			{
                Debug.Log($"Updating {item.name}");
                item.currGrad.enabled = false;
                item.currGrad.enabled = true;
            }
        }
	}

}
