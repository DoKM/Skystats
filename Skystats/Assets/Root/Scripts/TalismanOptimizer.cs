using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using NaughtyAttributes;
using UnityEngine.UI;
using TMPro;
using Helper;

public enum OptimizeType
{
    MELEE,
    MAGIC,
    RANGED,
    TANK,
}

// Need to create a seperate obj because JsonUtility doesn't support serialization of lists/arrays
[Serializable]
public class ReforgeList
{
    public List<Reforge> Reforges;
}

[Serializable]
public class Reforge
{
    public string Name;
    public List<ReforgeStatGroup> Stats;
}

[Serializable]
public class ReforgeStatGroup
{
    public string Name;
    public List<ReforgeStat> Stats;
}

[Serializable]
public class ReforgeStat
{
    public RARITY Rarity;
    public float Amount;
}

[Serializable]
public class AccessoryGroup
{
    public RARITY Rarity;
    public Reforge Reforge;
    public List<Item> Accessories;
}


public class TalismanOptimizer : MonoBehaviour
{
    #region Singleton
    public static TalismanOptimizer Instance;
    public void OnEnable()
    {
        if (Instance)
            Destroy(gameObject);
        else
            Instance = this;
    }

    #endregion

    
    public bool focusDps = true;
    [Space]
    public float ferocity, attackSpeed, previousDps, dps, previousDamage, newDamage;
    [Space]
    public ReforgeList reforgeData;

    [HideInInspector] public List<AccessoryGroup> finalReforges = new List<AccessoryGroup>();
    [HideInInspector] public List<Reforge> newReforges;

    [HideInInspector] public List<Item> reforgeItems;
    [HideInInspector] public List<Item> accessories;

    [HideInInspector] public List<Stat> accessoryStats;
    [HideInInspector] public List<Stat> baseAccessoryStats;
    [HideInInspector] public List<Stat> newReforgeStats;
    [HideInInspector] public List<Stat> oldReforgeStats;

    private Transform accessoryViewParent;
    private GameObject optimizedAccessoryPrefab;

    private void Start()
    {
        reforgeData = JsonUtility.FromJson<ReforgeList>(Resources.Load<TextAsset>("JSON Data/Reforges").text);
        accessoryViewParent = GameObject.FindGameObjectWithTag("OptimizedAccessories").transform;
        optimizedAccessoryPrefab = Resources.Load<GameObject>("Prefabs/Accessory Tier");

        Global.UpdateCanvasElement(accessoryViewParent as RectTransform);
        Global.UpdateCanvasElement(accessoryViewParent as RectTransform);
    }

    [Button]
    public void StartOptimize()
    {
        if (!string.IsNullOrEmpty(Main.Instance.uuid))
        {
            Optimize();
            RenderOptimizedTalismans(finalReforges);
        }
        else
            ErrorHandler.Instance.Push(new Error { ErrorCode = 511, ErrorHeader = "Optimizing failed", ErrorMessage = "Please load a profile before optimizing." });
    }

    public void RenderOptimizedTalismans(List<AccessoryGroup> accessories)
    {
        Main.Instance.ClearChildren(accessoryViewParent);

        var rarityOrder = new List<RARITY> { RARITY.VERY_SPECIAL, RARITY.SPECIAl, RARITY.SUPREME, RARITY.MYTHIC, RARITY.LEGENDARY,
                                             RARITY.EPIC, RARITY.RARE, RARITY.UNCOMMON, RARITY.COMMON, RARITY.UNKNOWN };
        var newAccessories = accessories.OrderBy(x => rarityOrder.IndexOf(x.Rarity)).ToList();

        foreach (var accessory in newAccessories)
        {
            var reforgeObj = Instantiate(optimizedAccessoryPrefab, accessoryViewParent);
            var tierText = reforgeObj.transform.FindChild("Tier").GetChild(0).GetComponent<TMP_Text>();
            var reforgeText = reforgeObj.transform.FindChild("Reforge").GetChild(0).GetComponent<TMP_Text>();

            var rarityColor = Global.GetRarityHexColor(accessory.Rarity);
            var rarityName = Global.ToTitleCase(accessory.Rarity.ToString());
            var amount = accessory.Accessories.Count;

            tierText.text = $"<color=#{rarityColor}>{rarityName}</color>  {amount}";
            reforgeText.text = $"{accessory.Reforge.Name}";

            Global.UpdateCanvasElement(reforgeObj.transform as RectTransform);
            Global.UpdateCanvasElement(reforgeObj.transform.parent as RectTransform);
        }

        var damageText = accessoryViewParent.Find("Damage (DND)").GetComponentInChildren<TMP_Text>();
        var text = $"Previous Damage <color=#C8C8C8>{Global.FormatAndRoundAmount(previousDamage)}</color>" +
                   $"\nNew Damage <color=#C8C8C8>{Global.FormatAndRoundAmount(newDamage)}</color>" +
                   $"\n\nPrevious DPS <color=#C8C8C8>{Global.FormatAndRoundAmount(previousDps)}</color>" +
                   $"\nNew DPS <color=#C8C8C8>{Global.FormatAndRoundAmount(dps)}</color>\n";

        if (focusDps)
            text += $"\nFerocity <color=#C8C8C8>{ferocity}</color>" +
                    $"\nAttack Speed <color=#C8C8C8>{attackSpeed}</color>\n";

        text += $"\nStrength <color=#C8C8C8>{newReforgeStats.First(x => x.StatType == STAT.Strength).BonusAmount}</color>" +
                $"\nCrit damage <color=#C8C8C8>{newReforgeStats.First(x => x.StatType == STAT.CritDamage).BonusAmount}</color>\n";

        if ((previousDamage >= newDamage && !focusDps) || (previousDps >= dps && focusDps))
            text += $"\n<color=#55FF55>Fully optimized!</color>";


        damageText.text = text;
        damageText.transform.parent.SetAsLastSibling();

        Global.UpdateCanvasElement(damageText.transform.parent as RectTransform);
        Global.UpdateCanvasElement(accessoryViewParent as RectTransform);
        Global.UpdateCanvasElement(accessoryViewParent as RectTransform);
    }

    public void ToggleDPS(bool on)
    {
        focusDps = on;
        if (!string.IsNullOrEmpty(Main.Instance.uuid))
            StartOptimize();
    }

    public void ToggleMenu()
    {
        var cg = accessoryViewParent.parent.GetComponent<CanvasGroup>();
        _ = cg.alpha == 0 ? cg.alpha = 1 : cg.alpha = 0;

        cg.blocksRaycasts = !cg.blocksRaycasts;
    }

    public void Optimize()
    {
        if (!string.IsNullOrEmpty(Main.Instance.uuid))
		{
			float newDamage = 0, newDps = 0;
			InitializeOptimization(out float oldDamage, out float oldDps, out List<Stat> newStats, out List<Item> newAccessories);

			foreach (var accessory in newAccessories)
			{
				if (accessory != null)
				{
					float bestDamage = 0f, bestDps = 0f;
					var bestStats = new List<Stat>();
					var bestReforge = new Reforge();

					foreach (var reforge in reforgeData.Reforges)
						CheckReforge(newStats, accessory, reforge, ref bestDamage, ref bestDps, ref bestStats, ref bestReforge);

					reforgeItems.Add(accessory);
					newReforges.Add(bestReforge);
					AddToReforges(accessory, bestReforge);

					if (bestDamage > newDamage) newDamage = bestDamage;
					if (bestDps > newDps) newDps = bestDps;

					newStats = bestStats;
				}
			}

			UpdateOptimizationStats(newDamage, newDps, oldDamage, oldDps, newStats);
		}
		else
            ErrorHandler.Instance.Push(new Error { ErrorCode = 511, ErrorHeader = "Optimizing failed", ErrorMessage = "Please load a profile before optimizing." });

    }

	private void UpdateOptimizationStats(float newDamage, float newDps, float oldDamage, float oldDps, List<Stat> newStats)
	{
		attackSpeed = newStats.First(x => x.StatType == STAT.BonusAttackSpeed).BonusAmount;
		ferocity = newStats.First(x => x.StatType == STAT.Ferocity).BonusAmount;
		newReforgeStats = newStats;

		previousDps = oldDps;
		dps = newDps;

		previousDamage = oldDamage;
		this.newDamage = newDamage;
	}

	private void CheckReforge(List<Stat> newStats, Item accessory, Reforge reforge, ref float bestDamage, ref float bestDps, ref List<Stat> bestStats, ref Reforge bestReforge)
	{
		var allStats = new List<Stat>(newStats.Count());
		newStats.ForEach((item) => { allStats.Add(new Stat { StatType = item.StatType, BonusAmount = item.BonusAmount }); });

		var newAccessoryStats = GetReforgeStat(reforge.Name, accessory.RarityTier);
		AddStats(allStats, newAccessoryStats);

		float attackSpeed = allStats.First(x => x.StatType == STAT.BonusAttackSpeed).BonusAmount;
		float ferocity = allStats.First(x => x.StatType == STAT.Ferocity).BonusAmount;

		var damage = DamagePrediction.Instance.StartDamageCalculation(allStats, false);
		var dps = DamagePrediction.Instance.CalculateCurrentDPS(damage, attackSpeed, ferocity);

		if (focusDps)
		{
			if (dps >= bestDps)
			{
				bestDps = dps;
				bestDamage = damage;
				bestStats = allStats;
				bestReforge = reforge;
			}
		}
		else
		{
			if (damage >= bestDamage)
			{
				bestDps = dps;
				bestDamage = damage;
				bestStats = allStats;
				bestReforge = reforge;
			}
		}
	}

	private void InitializeOptimization(out float oldDamage, out float oldDps, out List<Stat> newStats, out List<Item> newAccessories)
	{
		finalReforges.Clear();
		reforgeItems.Clear();
		newReforges.Clear();

		PlayerStats.ProcessStats(Main.Instance.currentProfile);
		DamagePrediction.Instance.StartDamageCalculation(false);
		accessoryStats = PlayerStats.GetItemListStats(accessories, PlayerStats.GetEmptyStatList()).Values.ToList();
		var baseStats = GetBaseAccessoryStats(accessories);

		baseAccessoryStats = new List<Stat>(baseStats.Count);
		baseStats.ForEach((stat) => { baseAccessoryStats.Add(new Stat(stat.StatType, stat.BonusAmount)); });

        var oldas = Stats.Instance.stats[STAT.BonusAttackSpeed].BonusAmount;
        var oldfer = Stats.Instance.stats[STAT.Ferocity].BonusAmount;

        oldDamage = DamagePrediction.Instance.expectedCritDamage;
		oldDps = DamagePrediction.Instance.CalculateCurrentDPS(oldDamage, oldas, oldfer);

		newStats = PlayerStats.GetStats(Main.Instance.currentProfile);

        oldReforgeStats = new List<Stat>(newStats.Count());
        newStats.ForEach((item) => { oldReforgeStats.Add(new Stat { StatType = item.StatType, BonusAmount = item.BonusAmount }); });

        AddStats(newStats, baseAccessoryStats);
		newStats = RemoveStats(newStats, accessoryStats);

        newAccessories = focusDps == true ? SortAccessories(accessories, true) : newAccessories = SortAccessories(accessories, false);
	}

    
	private List<Item> SortAccessories(List<Item> accessories, bool dps)
    {
        var rarityOrder = new List<RARITY> { RARITY.VERY_SPECIAL, RARITY.SPECIAl, RARITY.SUPREME, RARITY.MYTHIC, RARITY.LEGENDARY, 
                                             RARITY.EPIC, RARITY.RARE, RARITY.UNCOMMON, RARITY.COMMON, RARITY.UNKNOWN };
        
        if (dps)
        {
            rarityOrder = new List<RARITY> { RARITY.EPIC, RARITY.UNCOMMON, RARITY.VERY_SPECIAL, RARITY.SPECIAl, RARITY.SUPREME,
                                             RARITY.MYTHIC, RARITY.LEGENDARY, RARITY.RARE, RARITY.COMMON, RARITY.UNKNOWN };
        }

        var newAccessories = accessories.OrderBy(x => rarityOrder.IndexOf(x.RarityTier)).ToList();
        return newAccessories;
    }

    private void AddToReforges(Item accessory, Reforge reforge)
    {
        if (finalReforges.Any(x => x.Rarity == accessory.RarityTier && x.Reforge == reforge))
            finalReforges.First(x => x.Rarity == accessory.RarityTier && x.Reforge == reforge).Accessories.Add(accessory);
        else
        {
            var accessories = new List<Item>();
            accessories.Add(accessory);
            finalReforges.Add(new AccessoryGroup { Accessories = accessories, Rarity = accessory.RarityTier, Reforge = reforge });
        }
    }

    private void AddStats(List<Stat> originalStats, List<Stat> newStats)
    {
        foreach (var stat in newStats)
            if (originalStats.Any(x => x.StatType == stat.StatType))
                originalStats.Find(x => x.StatType == stat.StatType).BonusAmount += stat.BonusAmount;
    }


    private List<Stat> RemoveStats(List<Stat> originalStats, List<Stat> removeStats)
    {
        foreach (var stat in removeStats)
            if (originalStats.Any(x => x.StatType == stat.StatType))
                originalStats.Find(x => x.StatType == stat.StatType).BonusAmount -= stat.BonusAmount;

        return originalStats;
    }


    private List<Stat> GetBaseAccessoryStats(List<Item> accessories)
    {
        var baseStats = new List<Stat>();

        foreach (var accessory in accessories)
        {
            var stats = accessory.BonusStats;
            var accessoryReforgeStats = GetReforgeStat(accessory.Modifier, accessory.RarityTier);

            foreach (var stat in stats)
            {
                var reforgeStat = accessoryReforgeStats.Find((x) => x.StatType == stat.StatType);
                var newStat = new Stat { StatType = stat.StatType, BonusAmount = stat.BonusAmount };

                if (reforgeStat != null)
                    newStat.BonusAmount = stat.BonusAmount - reforgeStat.BonusAmount;

                if (baseStats.Any((x) => x.StatType == newStat.StatType))
                    baseStats.Find((x) => x.StatType == newStat.StatType).BonusAmount += newStat.BonusAmount;
                else
                    baseStats.Add(newStat);
            }
        }

        for (int i = 0; i < baseStats.Count; i++)
        {
            if (baseStats[i].BonusAmount == 0)
                baseStats.Remove(baseStats[i]);
        }


        return baseStats;
    }


    private List<Stat> GetReforgeStat(string reforgeName, RARITY rarity)
    {
        var reforge = reforgeData.Reforges.Find((x) => x.Name.ToLower() == reforgeName.ToLower());
        var stats = new List<Stat>();

        if (reforge != null)
        {
            foreach (var reforgeStat in reforge.Stats)
            {
                if (reforgeStat.Stats.Any((x) => x.Rarity == rarity))
                {
                    var correctStatAmount = reforgeStat.Stats.Find((x) => x.Rarity == rarity).Amount;
                    var correctStatType = (STAT)Enum.Parse(typeof(STAT), reforgeStat.Name.Replace(" ", ""), true);
                    var stat = new Stat { BonusAmount = correctStatAmount, StatType = correctStatType };
                    stats.Add(stat);
                }
            }
        }

        return stats;
    }


}
