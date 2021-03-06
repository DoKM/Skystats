﻿using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;
using Helper;
using TMPro;

public class DamageModule : MonoBehaviour
{
    public TMP_Text critText, normalText, apsText, dpsText;
    private bool update = false;
    public GameObject apiOff;

    private void Start()
    {
        Main.Instance.OnLoadProfile += InstantiateModule;
        Main.Instance.OnSkillAPIOff += ToggleAPI;

        if (Main.Instance.currentProfile != null && !string.IsNullOrEmpty(Main.Instance.currentProfile.FormattedUsername))
            InstantiateModule(Main.Instance, new OnLoadProfileEventArgs { profile = Main.Instance.currentProfile });
    }

    public void ToggleAPI(object sender, OnSkillAPIOffArgs e)
    {
        apiOff.SetActive(false);
    }
    
    public void APIOff(bool on)
    {
        Global.FindObjectsOfTypeInTranform<ModuleDragger>(transform)[0].DisableObjects(!on);
        apiOff.SetActive(on);
    }
    
    public void InstantiateModule(object sender, OnLoadProfileEventArgs e)
    {
        update = true;
    }

	private void Update()
	{
		if (update)
		{
            if (DamagePrediction.Instance.reset)
            {
                critText.text = "Critical damage:<color=#AAAAAA> ?</color>";
                normalText.text = "Normal damage:<color=#AAAAAA> ?</color>";
            }
            else if (DamagePrediction.Instance.updateText)
            {
                apsText.text = $"Aps:<color=#AAAAAA> {Global.FormatAndRoundAmount(DamagePrediction.Instance.aps)}</color>";
                dpsText.text = $"Dps:<color=#AAAAAA> {Global.FormatAndRoundAmount(DamagePrediction.Instance.dps)}</color>";
                critText.text = $"Critical damage: <color=#AAAAAA>{Global.FormatAndRoundAmount(DamagePrediction.Instance.expectedCritDamage)}</color>";
                normalText.text = $"Normal damage: <color=#AAAAAA>{Global.FormatAndRoundAmount(DamagePrediction.Instance.expectedNormalDamage)}</color>";
            }
        }
	}

}

