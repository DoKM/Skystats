using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatDisplay : MonoBehaviour
{
    public STAT statToDisplay;

	[HideInInspector] public Image icon;
	[HideInInspector] public Sprite iconSprite;
	[HideInInspector] public TMP_Text text;
	[HideInInspector] public string statName;

	private void Awake ()
	{
		statName = AddSpacing(statToDisplay.ToString());

		icon = transform.Find("Icon").GetComponent<Image>();
		iconSprite = Resources.Load<Sprite>("Stat icons/" + statName);
		text = transform.Find("Text").GetComponent<TMP_Text>();
	}

	private void Start()
	{
		var iconColor = Stats.Instance.statColors[statToDisplay];

		icon.sprite = iconSprite;
		icon.color = iconColor;
		text.text = "<color=#" + ColorUtility.ToHtmlStringRGBA(icon.color) + ">" + statName + "</color>:  ";
	}

	public string AddSpacing(string original)
	{
		var newString = original;
		var matches = Regex.Matches(original, "[A-Z]");

		var offset = 0;
		foreach (Match match in matches)
		{
			if (match.Index != 0)
			{
				newString = newString.Remove(match.Index + offset, 1).Insert(match.Index + offset, " " + match.Value);
				offset = newString.Length - original.Length;
			}
		}

		return newString;
	}

}
