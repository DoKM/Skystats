using Helper;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExperimentItem : MonoBehaviour
{
    public GameObject stakePrefab, stakesParent;
	public TMP_Text descText, title;
	public Experiment currentExperiment;

	private void Start()
	{
		FillExperiment(currentExperiment);
	}

	public void FillExperiment (Experiment e)
	{
		currentExperiment = e;
		title.text = e.Name;

		var desc = $"";
		if (e.BonusClicks != 0)
			desc += $"Bonus Clicks:<color=#AAAAAA> {e.BonusClicks}</color>\n";
		if (e.LastAttempt != 0)
			desc += $"Last Attempted:<color=#AAAAAA> {Global.GetFormattedDateTime(Global.UnixTimeStampToDateTime(e.LastAttempt))}</color>\n";
		if (e.LastClaimed != 0)
			desc += $"Last Claimed:<color=#AAAAAA> {Global.GetFormattedDateTime(Global.UnixTimeStampToDateTime(e.LastClaimed))}</color>";

		descText.text = desc;

		FillStakes(e.Stakes);

		for (int i = 0; i < 5; i++)
		{ 
			Global.UpdateCanvasElement(transform.Find("Stakes") as RectTransform);
			Global.UpdateCanvasElement(transform as RectTransform);
		}
	}

	public void FillStakes(List<Stake> stakes)
	{
		foreach (var stake in stakes)
		{
			var stakeObj = Instantiate(Resources.Load<GameObject>("Prefabs/Stake"), transform.Find("Stakes"));

			var textObjs = Global.FindObjectsOfTypeInTranform<TMP_Text>(stakeObj.transform);
			Global.FindObjectsOfTypeInTranform<Image>(stakeObj.transform.GetChild(1))[0].sprite = Global.GetExperimentIcon(stake.Name);
			textObjs.Find(x => x.name == "StakeTitle").text = stake.Name;

			var desc = "";
			if (stake.Attempts != 0)
				desc += $"Attempts:<color=#AAAAAA> {stake.Attempts}</color>\n";
			if (stake.BestScore != 0)
				desc += $"Best Score:<color=#AAAAAA> {stake.BestScore}</color>\n";
			if (stake.Claims != 0)
				desc += $"Claims:<color=#AAAAAA> {stake.Claims}</color>";

			textObjs.Find(x => x.name == "DescriptionTitle").text = desc;
		}
	}
}
