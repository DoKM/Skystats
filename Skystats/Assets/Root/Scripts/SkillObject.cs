using JetBrains.Annotations;
using LeTai.Asset.TranslucentImage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Helper;
using UnityEngine.UI.Michsky.UI.ModernUIPack;

public class SkillObject : MonoBehaviour
{
    public SKILL selectedSkill;
    [Space]
    public float maxWidth;

    private TMP_Text skillXpText;
    private TMP_Text levelText;

    private RectTransform progressBar;
    private Image icon;
    private GameObject model;

    private Dictionary<string, Sprite> skillSprites = new Dictionary<string, Sprite>();

    [HideInInspector] public Skill skillData;

    private void Awake()
    {
        foreach (var skill in Enum.GetValues(typeof(SKILL)).Cast<SKILL>())
            skillSprites.Add(skill.ToString(), Resources.Load<Sprite>("Skill icons/" + skill.ToString()));

        if (transform.Find("LevelBackground")) levelText = transform.Find("LevelBackground").Find("LevelText").GetComponent<TMP_Text>();
        else levelText = transform.Find("LevelText").GetComponent<TMP_Text>();

        if (transform.Find("IconBackground")) icon = transform.Find("IconBackground").Find("Icon").GetComponent<Image>();
        else if (transform.Find("3D")) model = transform.Find("3D").gameObject;
        else if (levelText.transform.Find("3D")) model = levelText.transform.Find("3D").gameObject;
        else icon = transform.Find("Icon").GetComponent<Image>();

        progressBar = transform.Find("Progress bar") as RectTransform;
        skillXpText = transform.Find("Text").GetComponent<TMP_Text>();
    }

    private void Start()
    {
        if (icon)
            icon.sprite = skillSprites[selectedSkill.ToString()];
    }

    private void ResetSkill(SKILL newSkill)
    {
        name = selectedSkill.ToString().ToUpper();

        if (icon)
            icon.sprite = skillSprites[selectedSkill.ToString()];

        ResetText();
    }

    public void UpdateText()
    {
        var remainingXP = skillData.SkillXP.RemainingXP;
        var nextLevelXP = Mathf.Clamp(skillData.SkillXP.NextLevelXP, 1, Mathf.Infinity);

        var progressToNextLevel = remainingXP / nextLevelXP * 100;
        progressToNextLevel = Mathf.Clamp(progressToNextLevel, 1, 100);
        progressToNextLevel = Mathf.Round(progressToNextLevel);

        skillXpText.text = progressToNextLevel.ToString() + "%";
        levelText.text = Global.ToTitleCase(selectedSkill.ToString().ToLower()) + " <color=#CCCCCC>" + skillData.SkillXP.Level.ToString() + "</color>";

        var progressBarWidth = Mathf.Clamp(remainingXP / nextLevelXP * maxWidth, maxWidth / 5, maxWidth);
        progressBar.sizeDelta = new Vector2(progressBarWidth, progressBar.rect.height);

        var newGradientString = "normalColor";
        if (skillData.IsMaxLevel)
            newGradientString = "maxColor";

        var gradients = GetComponentsInChildren<StaticGradient>();
        Skills.UpdateGradient((GradientType)Enum.Parse(typeof(GradientType), newGradientString), icon ? icon.transform : null, gradients);
        Global.UpdateCanvasElement(levelText.GetComponent<RectTransform>().parent as RectTransform);
    }

    public void ResetText()
    {
        skillXpText.text = "?";
        levelText.text = "?";

        Skills.UpdateGradient(GradientType.disabledColor, icon ? icon.transform : null, GetComponentsInChildren<StaticGradient>());
        Global.UpdateCanvasElement(levelText.rectTransform.parent as RectTransform);
        progressBar.sizeDelta = new Vector2(Mathf.Clamp(0, maxWidth / 5, maxWidth), progressBar.rect.height);
    }

    private void ChangeColor(Gradient newGradient)
    {
        for (int i = 0; i < transform.childCount; i++)
            if (transform.GetChild(i).GetComponent<UIGradient>())
                transform.GetChild(i).GetComponent<UIGradient>().EffectGradient = newGradient;
    }
}