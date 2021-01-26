using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Helper;
using System;

public class ProfileModule : MonoBehaviour
{
	public Transform title;
    public TMP_Text userInfoText;

    public Profile currentProfile;

	private void Start()
	{
		Main.Instance.OnLoadProfile += InstantiateModule;

		if (Main.Instance.currentProfile != null && !string.IsNullOrEmpty(Main.Instance.currentProfile.FormattedUsername))
			InstantiateModule(Main.Instance, new OnLoadProfileEventArgs { profile = Main.Instance.currentProfile });
	}

	public void InstantiateModule(object sender, OnLoadProfileEventArgs e)
	{
		//string username, double purse, double bank, int souls, DateTime lastLogin
		var newString = $"User: {e.profile.FormattedUsername}";
		if (e.profile.BankingData.PurseBalance != -1) newString += $"\nPurse:<color=#FFAA00> {Global.FormatAndRoundAmount((float)e.profile.BankingData.PurseBalance)}</color>";
		if (e.profile.BankingData.BankBalance != -1) newString += $"\nBank:<color=#FFAA00> {Global.FormatAndRoundAmount((float)e.profile.BankingData.BankBalance)}</color>";
		if (e.profile.CollectedFairySouls != -1) newString += $"\nFairy Souls:<color=#FF55FF> {e.profile.CollectedFairySouls}/209</color>";

		userInfoText.text = newString;
		Global.UpdateCanvasElement(userInfoText.transform.parent as RectTransform);
		Global.UpdateCanvasElement(title as RectTransform);

		for (int i = 0; i < 5; i++)
			Global.UpdateCanvasElement(transform as RectTransform);

		currentProfile = e.profile;
		StartCoroutine(Refresh());
	}

	public IEnumerator Refresh()
	{
		yield return new WaitForSeconds(300);
		InstantiateModule(null, new OnLoadProfileEventArgs(){ profile = currentProfile });
	}

}
