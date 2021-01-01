using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SKILL
{
    combat,
    foraging,
    mining,
    farming,
    fishing,
    alchemy,
    enchanting,
    carpentry,
    runecrafting,
    taming,
    catacombs,
    mage,
    berserk,
    archer,
    healer,
    tank
};

public class SkillManager : MonoBehaviour
{
	#region Singleton
	public static SkillManager Instance;
    
	private void OnEnable()
	{
		if (!Instance) Instance = this;
		else Destroy(this);
    }
	#endregion

    [HideInInspector] public Dictionary<SKILL, Skill> skills = new Dictionary<SKILL, Skill>();
}
