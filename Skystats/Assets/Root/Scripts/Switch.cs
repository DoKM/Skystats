using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Helper;

public class Switch : MonoBehaviour
{
    public void Toggle ()
    {
        Global.UpdateScrollView();
        gameObject.SetActive(!gameObject.activeInHierarchy);
        Global.UpdateScrollView();
    }

    public void ToggleCanvas (CanvasGroup group)
	{
        group.alpha = group.alpha == 0 ? 1 : 0;
        group.blocksRaycasts = group.alpha != 0;
	}
}
