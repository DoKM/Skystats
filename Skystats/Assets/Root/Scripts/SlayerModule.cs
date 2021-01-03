using Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SlayerModule : MonoBehaviour
{
    public Transform parent, title;

	private void Start()
    {
        Main.Instance.OnLoadProfile += InstantiateModule;

        if (Main.Instance.currentProfile != null && !string.IsNullOrEmpty(Main.Instance.currentProfile.FormattedUsername))
            InstantiateModule(Main.Instance, new OnLoadProfileEventArgs { profile = Main.Instance.currentProfile });
    }

    public void InstantiateModule(object sender, OnLoadProfileEventArgs e)
    {
        parent.gameObject.SetActive(true);
        Main.Instance.ClearChildren(parent);

        if (e.profile.Slayers != null)
        {
            foreach (var slayer in e.profile.Slayers)
            {
                var slayerInstance = Instantiate(Resources.Load<GameObject>("Prefabs/Slayer"), parent).transform;
                var titleText = slayerInstance.Find("Title Background").GetChild(1).GetComponent<TMP_Text>();
                var titleIcon = slayerInstance.Find("Title Background").GetChild(0).GetComponent<Image>();
                var progressBar = slayerInstance.Find("Progress bar background").GetComponent<RectTransform>();
                var killText = slayerInstance.Find("Kills").GetChild(1).GetComponent<TMP_Text>();

                titleIcon.sprite = slayer.Icon;

                if (slayer.Level > 0 && slayer.ProgressXP.XPLadder.Length >= slayer.Level - 1)
                {
                    var remainingXP = slayer.ProgressXP.RemainingXP + slayer.ProgressXP.XPLadder[slayer.Level - 1];
                    var nextLevelXP = Mathf.Clamp(slayer.ProgressXP.NextLevelXP, 1, Mathf.Infinity);

                    var progressToNextLevel = Mathf.Round(Mathf.Clamp(remainingXP / nextLevelXP * 100, 1, 100));
                    var nextLevelText = "";
                    if (nextLevelXP != 0 && nextLevelXP != 1)
                        nextLevelText = $" / {Global.FormatAndRoundAmount(nextLevelXP)}";

                    
                    progressBar.GetComponentInChildren<TMP_Text>().text = $"{Global.FormatAndRoundAmount(remainingXP)}{nextLevelText}";
                    titleText.text = $"{Global.ToTitleCase(slayer.Name)} {slayer.Level}";

                    var progressBarWidth = Mathf.Clamp(progressToNextLevel / 100 * progressBar.rect.width, progressBar.rect.width / 5, progressBar.rect.width);
                    progressBar.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(progressBarWidth, progressBar.rect.height);
                }
                else
                {
                    titleText.text = $"{Global.ToTitleCase(slayer.Name)}";
                    progressBar.gameObject.SetActive(false);
                }

                Global.UpdateCanvasElement(titleIcon.transform.parent as RectTransform);

                var newGradientString = slayer.IsMaxLevel ? "maxColor" : "normalColor";
                Skills.UpdateGradient((GradientType)Enum.Parse(typeof(GradientType), newGradientString), null, progressBar.GetComponentsInChildren<StaticGradient>());

                string killString = "";
                if (slayer.Kills.Count > 0)
                    foreach (var kill in slayer.Kills)
                        killString += $"TIER {Global.ToRoman(kill.Tier)}: <color=#CCCCCC>{kill.Amount}</color>\n";
                else
                {
                    killString = "has not defeated this slayer.";
                    killText.transform.parent.Find("Title").GetComponent<TMP_Text>().text = $"\"{Main.Instance.username}\"";
                }

                killText.text = killString;
            }
        }

        for (int i = 0; i < 3; i++)
            Global.UpdateCanvasElement(title as RectTransform);
    }
}
