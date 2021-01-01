using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundObj : MonoBehaviour
{
    public Background bg;
	public Image icon;

    public void UpdateIcon ()
	{
		icon.sprite = bg.image;
	}

	public void UpdateBackground ()
	{
		BackgroundHandler.Instance.SwapBackground(bg);
		BackgroundHandler.Instance.SaveActiveBackground();
	}

}
