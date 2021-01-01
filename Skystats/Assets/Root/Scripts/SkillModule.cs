using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Helper;
using TMPro;

public class SkillModule : MonoBehaviour
{
    public Transform parent;
    public List<SkillObject> skillObjects;

    private void Start()
    {
        Main.Instance.OnLoadProfile += InstantiateModule;
        skillObjects = Global.FindObjectsOfTypeInTranform<SkillObject>(transform);

        if (Main.Instance.currentProfile != null && !string.IsNullOrEmpty(Main.Instance.currentProfile.FormattedUsername))
            InstantiateModule(Main.Instance, new OnLoadProfileEventArgs { profile = Main.Instance.currentProfile });
    }

    public void InstantiateModule(object sender, OnLoadProfileEventArgs e)
    {
        for (int i = 0; i < skillObjects.Count; i++)
        {
            var obj = skillObjects[i];

            if (e.profile.SkillData.ContainsKey(obj.selectedSkill))
            {
                var skillData = e.profile.SkillData[obj.selectedSkill];

                obj.skillData = skillData;
                if (skillData.SkillXP.RemainingXP == 0)
                    obj.ResetText();
                else
                    obj.UpdateText();
            }
            else
            {
                obj.skillData = new Skill();
                obj.ResetText();
            }
        }

        Global.UpdateCanvasElement(parent as RectTransform);

        var asltext = parent.Find("ASL").GetChild(0).GetComponent<TMP_Text>();
        var asl = "Average Skill Level <color=#CCCCCC>" + e.profile.AverageSkilLevelProgression.ToString("0.0") + "</color>";

        if (skillObjects.Count > 0)
            asltext.text = asl;
    }


}
