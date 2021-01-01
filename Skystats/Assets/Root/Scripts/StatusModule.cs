using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Helper;
using System;

public class StatusModule : MonoBehaviour
{
	public Transform title;
	public TMP_Text onlineText, otherText;

	private void Start()
	{
		Main.Instance.OnLoadProfile += InstantiateModule;

		if (Main.Instance.currentProfile != null && !string.IsNullOrEmpty(Main.Instance.currentProfile.FormattedUsername))
			InstantiateModule(Main.Instance, new OnLoadProfileEventArgs { profile = Main.Instance.currentProfile });
	}

	public void InstantiateModule(object sender, OnLoadProfileEventArgs e)
	{
		if (e.profile.IsOnline) onlineText.text = "<color=#55FF55>ONLINE</color>";
		else onlineText.text = "<color=#FF5555>OFFLINE</color>";

		var otherString = $"Last updated: <color=#AAAAAA>{Global.CompareToNow(e.profile.LastSaveDateTime)}</color>";
		otherString += $"\nFirst login: <color=#AAAAAA>{e.profile.FirstSaveDateTime:ddd, dd MMM yyy HH:mm:ss}</color>";
		otherText.text = otherString;

		Global.UpdateCanvasElement(onlineText.transform.parent as RectTransform);
		Global.UpdateCanvasElement(title as RectTransform);

		for (int i = 0; i < 15; i++)
			Global.UpdateCanvasElement(transform as RectTransform);
	}

}
