using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Helper;

public enum ENEMY
{
    zombie = 100,
    magma_cube = 200,
    spider = 1000,
    crypt = 2000,
    guardian = 5000,
    enderman = 4500,
    zealot = 13000,
    dragon = 12500000
}

[Serializable]
public class Enemy
{
    public ENEMY type;
    public float hp;
    public float damage;
}

public enum REFORGE
{
    gentle,
    odd,
    fast,
    fair,
    epic,
    sharp,
    heroic,
    spicy,
    legendary,
    fabled,
    suspicious,
    gilded,
    warped,
    deadly,
    fine,
    grand,
    hasty,
    neat,
    rapid,
    unreal,
    awkward,
    rich,
    precise,
    spiritual
}

public class DamagePrediction : MonoBehaviour
{
    #region Singleton 
    public static DamagePrediction Instance;
    private void OnEnable()
    {
        if (!Instance) Instance = this;
        else Destroy(this);
    }
    #endregion

    public bool updateDamage, updateDps;
    public bool soulEater;
    public float extraStr, extraCd, expectedNormalDamage, expectedCritDamage, aps, dps;
    [Space]
    public ENEMY targetEnemyType;
    public Enemy targetEnemy;
    public List<Enemy> enemies;

    private List<Enchantment> weaponEnchantments = new List<Enchantment>(); // Name, level

    [HideInInspector] public float health, strength, critDamage, combatLevel, weaponDamage, attackSpeed, ferocity;
    [HideInInspector] public REFORGE weaponReforge;
    [HideInInspector] public bool hasWeapon, reset, updateText;

	private void Update()
	{
        if (updateDamage)
        {
            GetDamage();
            Debug.Log("Done!");
            updateDamage = false;
        }

        if (updateDps)
        {
            GetDps();
            Debug.Log("Done!");
            updateDps = false;
        }
    }

	private void Start()
    {
        Main.Instance.OnLoadProfile += CalculateDamageOnProfileLoad;
    }

    public void CalculateDamageOnProfileLoad (object sender, OnLoadProfileEventArgs e)
	{
        Invoke("GetDamage", 0.1f);
        Invoke("GetDps", 0.2f);
    }

    public void GetDamage() { StartDamageCalculation(true); }
    public void GetDps() { CalculateCurrentDPS(expectedCritDamage, attackSpeed, ferocity); }

    public void ResetDamage() { reset = true; }

    public float CalculateCurrentDPS (float damage, float attackSpeed, float ferocity)
	{
        float attackTime = 0.4f - Mathf.Clamp(attackSpeed, 0, 100) * 0.002f;
        aps = 1 / attackTime * (1 + Mathf.Clamp(ferocity, 0, Mathf.Infinity) / 100);
        dps = aps * damage;

        return dps;
	}

    public float CalculateItemDamage(Item item)
    {
        if (SkillManager.Instance != null && SkillManager.Instance.skills.ContainsKey(SKILL.combat))
        {
            var combatSkill = SkillManager.Instance.skills[SKILL.combat];
            combatLevel = combatSkill.SkillXP.Level;
        }

        var itemStrength = 1f;
        var itemDamage = 1f;
        var itemCritDamage = 1f;

        if (item != null && item.BonusStats != null)
        {
            weaponEnchantments = item.Enchantments;
            if (item.BonusStats.Exists((x) => x.StatType == STAT.Strength))
                itemStrength = item.BonusStats.Find((x) => x.StatType == STAT.Strength).BonusAmount;
            if (item.BonusStats.Exists((x) => x.StatType == STAT.Damage))
                itemDamage = item.BonusStats.Find((x) => x.StatType == STAT.Damage).BonusAmount;
            if (item.BonusStats.Exists((x) => x.StatType == STAT.CritDamage))
                itemCritDamage = item.BonusStats.Find((x) => x.StatType == STAT.CritDamage).BonusAmount;
        }
        else
            weaponEnchantments = new List<Enchantment>();

        if (Stats.Instance != null)
		{
            weaponDamage = Stats.Instance.stats[STAT.Damage].BonusAmount + itemDamage;
            health = Stats.Instance.stats[STAT.Health].BonusAmount;
            strength = Stats.Instance.stats[STAT.Strength].BonusAmount + itemStrength;
            critDamage = Stats.Instance.stats[STAT.CritDamage].BonusAmount + itemCritDamage;
        }
        
        // Calculate damage. Values based on wiki page: https://hypixel-skyblock.fandom.com/wiki/Damage_Calculation
        GetFinalDamage(true);
        return expectedCritDamage;
    }

    public void StartDamageCalculation(bool updateText)
    {
        if (Stats.Instance != null)
        StartDamageCalculation(Stats.Instance.stats.Values.ToList(), updateText);
        reset = false;
    }

    public float StartDamageCalculation (List<Stat> stats, bool updateText)
	{
        // Get required data
        if (Main.Instance.currentProfile.ActiveWeapon != null)
            weaponEnchantments = Main.Instance.currentProfile.ActiveWeapon.Enchantments;

        if (SkillManager.Instance.skills.ContainsKey(SKILL.combat))
        {
            var combatSkill = SkillManager.Instance.skills[SKILL.combat];
            combatLevel = combatSkill.SkillXP.Level;
        }

        if (stats.Any(x => x.StatType == STAT.Damage))
            weaponDamage = stats.Find(x => x.StatType == STAT.Damage).BonusAmount;
        if (stats.Any(x => x.StatType == STAT.Health))
            health = stats.Find(x => x.StatType == STAT.Health).BonusAmount;
        if (stats.Any(x => x.StatType == STAT.Strength))
            strength = stats.Find(x => x.StatType == STAT.Strength).BonusAmount;
        if (stats.Any(x => x.StatType == STAT.CritDamage))
            critDamage = stats.Find(x => x.StatType == STAT.CritDamage).BonusAmount;
        if (stats.Any(x => x.StatType == STAT.Ferocity))
            ferocity = stats.Find(x => x.StatType == STAT.Ferocity).BonusAmount;
        if (stats.Any(x => x.StatType == STAT.BonusAttackSpeed))
            attackSpeed = stats.Find(x => x.StatType == STAT.BonusAttackSpeed).BonusAmount;

        // Calculate damage. Values based on wiki page: https://hypixel-skyblock.fandom.com/wiki/Damage_Calculation
        GetFinalDamage(updateText);
        return expectedCritDamage;
    } 

    private void GetFinalDamage(bool update)
    {
        updateText = update;

        targetEnemy = enemies.Find(x => x.type == targetEnemyType);
        float enchantments = GetEnchantments();

        var baseDamage = (5 + weaponDamage + (strength / 5)) * (1 + strength / 100);
        float damageMultiplier = GetDamageMultiplier(enchantments);

        var finalDamage = baseDamage * damageMultiplier;
        expectedNormalDamage = finalDamage;

        finalDamage *= 1 + critDamage / 100;
        expectedCritDamage = finalDamage;
    }

    private float GetEnchantments()
    {
        var enchantments = 0f;
        var mainDamageEnchant = new Enchantment(); // Sharpness, Smite, Bane Of Arthropods

        if (weaponEnchantments != null)
        {
            for (int i = 0; i < weaponEnchantments.Count(); i++)
            {
                var enchantLevel = weaponEnchantments.ElementAt(i).EnchantLevel;
                var enchantName = weaponEnchantments.ElementAt(i).EnchantName;
                enchantName = Regex.Replace(enchantName, "(_)", " ").ToUpper();

                switch (enchantName)
                {
                    case "SMITE":
                        if (targetEnemyType == ENEMY.zombie || targetEnemyType == ENEMY.crypt && mainDamageEnchant == new Enchantment())
                            enchantments += enchantLevel * 0.08f;
                        break;
                    case "BANE OF ARTHROPODS":
                        if (targetEnemyType == ENEMY.spider && mainDamageEnchant == new Enchantment())
                            enchantments += enchantLevel * 0.08f;
                        break;
                    case "SHARPNESS":
                        if (mainDamageEnchant == new Enchantment())
                            enchantments += enchantLevel * 0.05f;
                        break;
                    case "GIANT KILLER":
                        var rawLevel = (targetEnemy.hp - health) / health * 0.1f * enchantLevel;
                        enchantments += Mathf.Clamp(rawLevel, 0, 0.25f);
                        break;
                    case "ENDER SLAYER":
                        if (targetEnemyType == ENEMY.enderman || targetEnemyType == ENEMY.zealot)
                            enchantments += enchantLevel * 0.12f;
                        break;
                    case "DRAGON HUNTER":
                        if (targetEnemyType == ENEMY.dragon)
                            enchantments += enchantLevel * 0.08f;
                        break;
                    case "EXECUTE":
                        // Not of use right now: player is always at max hp.
                        // (2000 - 2000) / 2000 * 0.002 * enchantLevel = 0.
                        break;
                    case "CUBISM":
                        if (targetEnemyType == ENEMY.magma_cube)
                            enchantments += enchantLevel * 0.10f;
                        break;
                    case "FIRST STRIKE":
                        // We are calculating damage which is always on first hit.
                        // TODO: make a bool for first hit ot not
                        enchantments += enchantLevel * 0.25f;
                        break;
                    case "IMPALING":
                        if (targetEnemyType == ENEMY.guardian)
                            enchantments += enchantLevel * 0.10f;
                        break;
                    case "ULTIMATE SOUL EATER":
                        if (soulEater)
                            strength += targetEnemy.damage * (0.15f * enchantLevel);
                        break;
                }
            }
        }

        return enchantments;
    }

    private float GetDamageMultiplier(float enchantments)
    {
        var multiplier = 1 + (combatLevel * 0.04f) + enchantments;
        return multiplier;
    }
}
