using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Helper;
using UnityEngine.UI;

public class Switch : MonoBehaviour
{
    public CanvasGroup group;

    public void Toggle ()
    {
        Global.UpdateScrollView();
        gameObject.SetActive(!gameObject.activeInHierarchy);
        Global.UpdateScrollView();
    }

    public void Toggle(Button button)
    {
        Global.UpdateScrollView();
        gameObject.SetActive(!gameObject.activeInHierarchy);
        Global.UpdateScrollView();

        UpdateButton(button, gameObject.activeInHierarchy == true);
    }

    public void ToggleCanvas (CanvasGroup group)
	{
        group.alpha = group.alpha == 0 ? 1 : 0;
        group.blocksRaycasts = group.alpha != 0;
	}

    public void ToggleCanvas(Button button)
    {
        group.alpha = group.alpha == 0 ? 1 : 0;
        group.blocksRaycasts = group.alpha != 0;

        UpdateButton(button, group.alpha != 0);
    }

    private void UpdateButton (Button button, bool active)
	{
        var newColors = button.colors;
        newColors.normalColor = active ? Color.white : new Color(1, 1, 1, 0);
        button.colors = newColors;
    }

}
