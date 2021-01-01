using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI.Michsky.UI.ModernUIPack;

[RequireComponent(typeof(UIGradient))]
public class StaticGradient : MonoBehaviour
{
    public bool change;
    public GradientType enabledGradient;
    public ThemeGradient activeGrad;
    public UIGradient currGrad;

    private void Start()
    {
        currGrad = GetComponent<UIGradient>();
        var themeColor = GradientManager.Instance.GetCurrentThemeColor(enabledGradient);
        currGrad.EffectGradient = themeColor.Color;
    }

    private void Update()
    {
        if (GradientManager.Instance.currentTheme.ThemeColors.FindIndex((x) => x.Type == enabledGradient) < 0)
            Debug.LogError("THIS GRADIENT NAME WAS NOT FOUND IN GRADIENT MANAGER");

        if (currGrad.EffectGradient != GradientManager.Instance.GetCurrentThemeColor(enabledGradient).Color)
		{
            activeGrad = GradientManager.Instance.GetCurrentThemeColor(enabledGradient);
            currGrad.UpdateGradient(activeGrad.Color);
        }
    }
}
