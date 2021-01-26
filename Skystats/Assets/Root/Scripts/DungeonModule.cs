using Helper;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DungeonModule : MonoBehaviour
{
    public Transform title;
    public TMP_Text description;

    private List<SkillObject> skillObjects;
    public GameObject apiOff;

    private void Start()
    {
        Main.Instance.OnLoadProfile += InstantiateModule;
        Main.Instance.OnSkillAPIOff += ToggleAPI;
        skillObjects = Global.FindObjectsOfTypeInTranform<SkillObject>(transform);

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
        var desc = "";
        if (!string.IsNullOrEmpty(e.profile.Dungeons.SelectedClass))
            desc += $"Selected class:<color=#AAAAAA> {Global.ToTitleCase(e.profile.Dungeons.SelectedClass)}</color>     ";
        desc += $"Journals: {e.profile.Dungeons.MaxJournals} <color=#AAAAAA>({e.profile.Dungeons.CollectedJournals}/135)</color>";
        description.text = desc;

        for (int i = 0; i < skillObjects.Count; i++)
        {
            var obj = skillObjects[i];

            if (e.profile.SkillData.ContainsKey(obj.selectedSkill))
            {
                var skillData = e.profile.SkillData[obj.selectedSkill];

                obj.skillData = skillData;
                if (skillData.SkillXP.RemainingXP == 0)
                    obj.ResetText();
                else if (obj != null)
                    obj.UpdateText();
            }
            else
            {
                obj.skillData = new Skill();
                obj.ResetText();
            }
        }

    }
}
