using Helper;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StatModule : MonoBehaviour
{
    private Dictionary<STAT, Stat> savedStats = new Dictionary<STAT, Stat>();

    private void Start()
    {
        Main.Instance.OnLoadProfile += InstantiateModule;

        if (Main.Instance.currentProfile != null && !string.IsNullOrEmpty(Main.Instance.currentProfile.FormattedUsername))
            Invoke("Refresh", 0.1f);
    }

	private void Update()
	{
        if (savedStats.ContainsKey(STAT.Health))
            if (savedStats[STAT.Health].BonusAmount != Stats.Instance.stats[STAT.Health].BonusAmount)
                Refresh();
	}

    public void Refresh ()
	{
        InstantiateModule(Main.Instance.gameObject, new OnLoadProfileEventArgs { profile = Main.Instance.currentProfile });
	}

    public void InstantiateModule(object sender, OnLoadProfileEventArgs e)
    {
        savedStats = Stats.Instance.stats;
        var statObjects = Global.FindObjectsOfTypeInTranform<StatDisplay>(transform).ToList();
       
        foreach (var obj in statObjects)
        {
            var statName = Pets.AddSpacing(obj.statToDisplay.ToString());
            if (Stats.Instance.statColors.ContainsKey(obj.statToDisplay))
            {
                var bonus = string.IsNullOrEmpty(savedStats[obj.statToDisplay].BonusProcentBoost)
                            ? Global.FormatAndRoundAmount(savedStats[obj.statToDisplay].BonusAmount).ToString()
                            : savedStats[obj.statToDisplay].BonusProcentBoost;

                obj.icon.sprite = obj.iconSprite;
                obj.icon.color = Stats.Instance.statColors[obj.statToDisplay];
                obj.text.text = "<color=#" + ColorUtility.ToHtmlStringRGBA(obj.icon.color) + ">" + statName + "</color>:  " + bonus;
            }

            Global.UpdateCanvasElement(obj.transform as RectTransform);
            Global.UpdateCanvasElement(obj.transform.parent as RectTransform);
        }
    }
}
