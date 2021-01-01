using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PetSlot : Slot
{
	public override void ActivateTooltip(bool active)
	{
		if (active)
		{
			var petXPBar = Instantiate(this.petXPBar, tooltipHolder.transform);
			petXPBar.name = "PetXP";
			var txt = petXPBar.GetComponentInChildren<Text>();

			var petProgress = Mathf.Clamp(currentHoldingItem.PetXP.RemainingXP / Mathf.Clamp(currentHoldingItem.PetXP.NextLevelXP, 1, Mathf.Infinity) * 100, 1, 100);
			var xp = $"<color=#CCCCCC>({currentHoldingItem.PetXP.RemainingXP}/{currentHoldingItem.PetXP.NextLevelXP})</color>";
			if (currentHoldingItem.PetXP.NextLevelXP == 0)
				xp = $"<color=#CCCCCC>{currentHoldingItem.PetXP.RemainingXP}</color>";

			txt.text = $"{Mathf.Round(petProgress)}%\n{xp}";

			var xpBar = petXPBar.GetComponentInChildren<Image>().transform as RectTransform;
			var progressBarWidth = Mathf.Clamp(petProgress / 100 * xpBar.rect.width, xpBar.rect.width / 5, xpBar.rect.width);
			xpBar.sizeDelta = new Vector2(progressBarWidth, xpBar.sizeDelta.y);
		}

		base.ActivateTooltip(active);
	}
}
