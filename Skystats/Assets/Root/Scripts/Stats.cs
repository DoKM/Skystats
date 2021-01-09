using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Linq.Expressions;
using System.Data;
using System.Windows;
using System.Runtime.InteropServices;
using Helper;

public class Stats : MonoBehaviour
{
    #region Singleton 
    public static Stats Instance;
    private void OnEnable()
    {
        if (!Instance) Instance = this;
        else Destroy(this);
    }
    #endregion

    public List<STAT> statsToDisplay = new List<STAT>();
    public List<Color> colors = new List<Color>();

    [HideInInspector] public Dictionary<STAT, Stat> stats = new Dictionary<STAT, Stat>();
    [HideInInspector] public Dictionary<STAT, Stat> accessoryBonus = new Dictionary<STAT, Stat>();
    [HideInInspector] public Dictionary<STAT, Stat> activeWeaponBonus = new Dictionary<STAT, Stat>();
    [HideInInspector] public Dictionary<STAT, Stat> SlayerBonus = new Dictionary<STAT, Stat>();
    [HideInInspector] public Dictionary<STAT, Stat> bonus = new Dictionary<STAT, Stat>();

    [HideInInspector] public Dictionary<STAT, Stat> slayerBonus = new Dictionary<STAT, Stat>();
    [HideInInspector] public int cakeBagBonus, defusedTraps, melodyBonus, petMFBonus, fairySoulExchanges;
    public float totalStatBoost = 1;

    public bool godPotion, cookieBuff, cakeBuff;
    [HideInInspector] public Dictionary<STAT, Color> statColors = new Dictionary<STAT, Color>();
    [HideInInspector] public Profile currentProfile;

    private void Awake()
    {
        FillStatColors();
    }

    private void Start()
    {
        Main.Instance.OnLoadProfile += ProcessOnLoad;
        ResetStats(true);
    }

    public void ProcessOnLoad (object sender, OnLoadProfileEventArgs e)
	{
        PlayerStats.ProcessStats(e.profile);
    }

    public void FillStatColors()
    {
        statColors = Enumerable.Range(0, colors.Count()).ToDictionary(i => statsToDisplay[i], i => colors[i]);
    }

    public void GetStats()
    {
        GetStatDictionary();
    }

    public Dictionary<STAT, Stat> GetStatDictionary()
    {
        ResetStats(false);

        if (godPotion)
            stats = GetGodPotionStats(stats);
        if (cakeBuff)
            stats = GetCakeBuffStats(stats);
        if (cookieBuff)
            stats[STAT.MagicFind].BonusAmount += 15;

        stats = GetBaseStats(stats);
        stats = GetSkills(stats, currentProfile);
        stats = GetSlayer(stats, currentProfile);
        stats = GetFairyBonus(stats, currentProfile);
        stats = AddBonusStats(stats);
        stats = AddActivePetAbility(stats, Main.Instance.currentProfile.ActivePet);
        stats = MultiplyTotalBoost(stats);

        return stats;
    }

    public Dictionary<STAT, Stat> AddActivePetAbility(Dictionary<STAT, Stat> prevStats, Pet pet)
    {
        var newStats = prevStats;
        if (pet != null && !string.IsNullOrEmpty(pet.Name))
		{
            switch (pet.Name.ToLower())
            {
                case "turtle":
                    var turtleMult = pet.PetXP.Level / 100f * 20 / 100 + 1;
                    newStats[STAT.Defense].BonusAmount *= turtleMult;
                    break;
                case "tiger":
                    var tigerMult = pet.PetXP.Level / 100f + 1;
                    newStats[STAT.Ferocity].BonusAmount *= tigerMult;
                    break;
                case "hound":
                    var houndMult = pet.PetXP.Level / 100f * 10 / 100 + 1;
                    newStats[STAT.BonusAttackSpeed].BonusAmount *= houndMult;
                    break;
                case "griffin":
                    var griffinMult = pet.PetXP.Level / 100f * 15 / 100 + 1;
                    newStats[STAT.Strength].BonusAmount *= griffinMult;
                    break;
                case "black_cat":
                    var catMult = pet.PetXP.Level / 100f * 15 / 100 + 1;
                    newStats[STAT.PetLuck].BonusAmount *= catMult;
                    newStats[STAT.MagicFind].BonusAmount *= catMult;
                    break;
                case "dolphin":
                    var dolphinMult = pet.PetXP.Level / 100f * 10 / 100 + 1;
                    newStats[STAT.SeaCreatureChance].BonusAmount *= dolphinMult;
                    break;
                case "baby_yeti":
                    var yetiMult = pet.PetXP.Level / 100f;
                    var val = newStats[STAT.Strength].BonusAmount * yetiMult;
                    newStats[STAT.Defense].BonusAmount += val;
                    break;
                case "blue_whale":
                    var whaleMult = pet.PetXP.Level / 100f;
                    var whaleArchimedes = whaleMult * 20 / 100 + 1;
                    newStats[STAT.Health].BonusAmount *= whaleArchimedes;

                    var whaleBulkDef = 3 * whaleMult;
                    var whaleBulk = newStats[STAT.Health].BonusAmount / 20 * whaleBulkDef;
                    newStats[STAT.Defense].BonusAmount += whaleBulk;
                    break;
                case "elephant":
                    var eleFortressVal = pet.PetXP.Level / 100f;
                    var eleFortress = newStats[STAT.Speed].BonusAmount / 10 * eleFortressVal;
                    newStats[STAT.Health].BonusAmount += eleFortress;

                    var eleStompVal = 0.2f * pet.PetXP.Level;
                    var eleStomp = newStats[STAT.Speed].BonusAmount / 100 * eleStompVal;
                    newStats[STAT.Defense].BonusAmount += eleStomp;
                    break;
                case "ender_dragon":
                    if (pet.PetRarityTier == RARITY.LEGENDARY)
					{
                        var statBoost = pet.PetXP.Level * 0.1f;
                        totalStatBoost *= statBoost / 100 + 1;
                    }
                    break;
                default:
                    break;
            }
        }
        

		return newStats;
    }

    public Dictionary<STAT, Stat> MultiplyTotalBoost (Dictionary<STAT, Stat> prevStats)
	{
        var newStats = prevStats;

        foreach (var stat in newStats)
            stat.Value.BonusAmount *= Mathf.Clamp(totalStatBoost, 1, Mathf.Infinity);

        return newStats;
    }

    public Dictionary<STAT, Stat> GetGodPotionStats(Dictionary<STAT, Stat> prevStats)
    {
        var newStats = prevStats;

        newStats[STAT.Defense].BonusAmount += 66;
        newStats[STAT.Strength].BonusAmount += 78.75f;
        newStats[STAT.Speed].BonusAmount += 220;
        newStats[STAT.CritDamage].BonusAmount += 80;
        newStats[STAT.CritChance].BonusAmount += 25;
        newStats[STAT.MagicFind].BonusAmount += 85;
        newStats[STAT.PetLuck].BonusAmount += 20;

        return newStats;
    }

    public Dictionary<STAT, Stat> GetCakeBuffStats(Dictionary<STAT, Stat> prevStats)
    {
        var newStats = prevStats;

        newStats[STAT.Defense].BonusAmount += 10;
        newStats[STAT.Strength].BonusAmount += 3;
        newStats[STAT.Speed].BonusAmount += 10;
        newStats[STAT.MagicFind].BonusAmount += 1;
        newStats[STAT.PetLuck].BonusAmount += 1;
        newStats[STAT.Intelligence].BonusAmount += 5;
        newStats[STAT.SeaCreatureChance].BonusAmount += 1;
        newStats[STAT.Ferocity].BonusAmount += 2;

        return newStats;
    }

    public Dictionary<STAT, Stat> GetBaseStats(Dictionary<STAT, Stat> prevStats)
    {
        var newStats = prevStats;

        newStats[STAT.Health].BonusAmount += 100;
        newStats[STAT.Speed].BonusAmount += 100;
        newStats[STAT.CritChance].BonusAmount += 30;
        newStats[STAT.CritDamage].BonusAmount += 50;
        newStats[STAT.SeaCreatureChance].BonusAmount += 20;
        newStats[STAT.MagicFind].BonusAmount += 10;

        return newStats;
    }

    public Dictionary<STAT, Stat> GetSkills(Dictionary<STAT, Stat> prevStats, Profile profile)
    {
        var newStats = prevStats;
        if (profile.SkillData != null)
        {
            foreach (var item in profile.SkillData)
            {
                if (profile.SkillData.TryGetValue(item.Key, out var skillData))
                    if (skillData.StatsToIncrease != null)
					    for (int i = 0; i < skillData.StatsToIncrease.Length; i++)
                            newStats[skillData.StatsToIncrease[i]] = GetSkillStat(newStats[skillData.StatsToIncrease[i]], item.Value.StatBonus[i]);
            }
        }

        return newStats;
    }

    public Stat GetSkillStat(Stat oldStat, float bonus)
    {
        var newStat = oldStat;
        newStat.BonusAmount = oldStat.BonusAmount + bonus;

        return newStat;
    }

    public Dictionary<STAT, Stat> GetSlayer(Dictionary<STAT, Stat> prevStats, Profile profile)
    {
        var newStats = prevStats;
        if (profile.Slayers != null)
        {
            foreach (var slayer in profile.Slayers)
            {
                foreach (var stat in slayer.BonusStats)
                {
                    newStats[stat.Key].BonusAmount += stat.Value.BonusAmount;
                }
            }
        }
        return newStats;
    }

    public Dictionary<STAT, Stat> GetFairyBonus(Dictionary<STAT, Stat> prevStats, Profile profile)
    {
        var newStats = prevStats;

        if (profile.FairySoulData != null)
        {
            int healthBonus = 3;
            for (int i = 0; i < profile.FairySoulData.SoulExchanges; i++)
            {
                if (i % 2 == 0 && i != 0)
                    healthBonus++;

                newStats[STAT.Health].BonusAmount += healthBonus;

                if (i % 5 == 0)
                {
                    newStats[STAT.Defense].BonusAmount += 2;
                    newStats[STAT.Strength].BonusAmount += 2;
                }
                else
                {
                    newStats[STAT.Defense].BonusAmount++;
                    newStats[STAT.Strength].BonusAmount++;
                }

                if (i % 10 == 0 && i != 0)
                    newStats[STAT.Speed].BonusAmount++;
            }
        }
        return newStats;
    }

    public Dictionary<STAT, Stat> AddBonusStats(Dictionary<STAT, Stat> prevStats)
    {
        var newStats = prevStats;

        if (bonus != null)
        {
            foreach (var stat in bonus)
                newStats[stat.Key].BonusAmount += stat.Value.BonusAmount;
        }

        return newStats;
    }

    public void ResetStats(bool resetBonus)
    {
        var STATnames = (STAT[])Enum.GetValues(typeof(STAT));
        var emptyStatList = new List<Stat>();
        

        for (int i = 0; i < STATnames.Count(); i++)
        {
            emptyStatList.Add(new Stat
            {
                StatType = STATnames[i],
                BonusAmount = 0,
                BonusProcentBoost = ""
            });
        }

        var emptyList = Enumerable.Range(0, STATnames.Count()).ToDictionary(i => STATnames[i], i => emptyStatList[i]);
        stats = emptyList;

        if (resetBonus)
		{
            bonus = emptyList;
            totalStatBoost = 1;
        }
           
    }

    public void RenderStats()
    {
        
    }

    public void ToggleGodPotion(bool on)
    {
        godPotion = on;
    }


    public void ToggleCookie(bool on)
    {
        cookieBuff = on;
    }
    public void ToggleCakes(bool on)
    {
        cakeBuff = on;
    }

    public int Calculate2345SkillStat(int statLevel)
    {
        int bonus = 0;
        for (int i = 0; i < statLevel; i++)
        {
            if (i >= 0 && i <= 13)
                bonus += 2;
            else if (i > 13 && i <= 18)
                bonus += 3;
            else if (i > 18 && i <= 24)
                bonus += 4;
            else if (i > 24)
                bonus += 5;
        }

        return bonus;
    }

    public int Calculate122SkillStat(int statLevel)
    {
        int bonus = 0;
        for (int i = 0; i < statLevel; i++)
        {

            if (i >= 0 && i <= 14)
                bonus += 1;
            else if (i > 14)
                bonus += 2;
        }

        return bonus;
    }

    public float Calculate1SkillStat(int statLevel, float multiplier)
    {
        float bonus = 0;
        for (int i = 0; i < statLevel; i++)
            bonus += (1 * multiplier);

        return bonus;
    }
}
