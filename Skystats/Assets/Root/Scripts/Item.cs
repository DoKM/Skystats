using fNbt;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum STAT
{
    AbilityDamage,
    Damage,
    Health,
    Defense,
    Strength,
    Speed,
    CritDamage,
    CritChance,
    BonusAttackSpeed,
    Intelligence,
    SeaCreatureChance,
    MagicFind,
    PetLuck,
    Ferocity
}

public enum ARMOR
{
    HELMET,
    CHESTPLATE,
    LEGGINGS,
    BOOTS
}

public enum SLAYERTYPE
{
    Zombie,
    Spider,
    Wolf
}

public enum BANKACTION
{
    Withdraw,
    Deposit
}

[Serializable]
public class Pet
{
    public string Name { get; set; }
    public string HeldItemName { get; set; }
    public int UsedPetCandy { get; set; }
    public string SkinLink { get; set; }
    public string SkinName { get; set; }
    public bool IsActive { get; set; }
    public List<string> Description { get; set; }
    public HeldItem HeldItem { get; set; }
    public List<Stat> BonusStats { get; set; }
    public RARITY PetRarityTier { get; set; }
    public JSONNode PetJSONData { get; set; }
    public XP PetXP { get; set; }
}

[Serializable]
public class XP
{
    public int Level { get; set; }
    public float RemainingXP { get; set; }
    public float NextLevelXP { get; set; }
    public int[] XPLadder { get; set; }
}

[Serializable]
public class HeldItem
{
    public List<Stat> Bonuses { get; set; }
    public List<string> Lore { get; set; }
}

[Serializable]
public class Item
{
    public string Name { get; set; }
    public string TextureLink { get; set; }
    public string ItemIDString { get; set; }
    public string InternalItemIDString { get; set; }
    public bool IsLeather { get; set; }
    public bool IsRarityUpgraded { get; set; }
    public string Modifier { get; set; } // Internal name, in-game name is Reforge
    public byte StackAmount { get; set; }
    public List<string> ItemDescription { get; set; }
    public List<Stat> BonusStats { get; set; }
    public List<Enchantment> Enchantments { get; set; }
    public Backpack Backpack { get; set; }
    public RARITY RarityTier { get; set; } = RARITY.UNKNOWN;
    public Color LeatherArmorColor { get; set; }
    public Slot ParentSlot { get; set; }
    public XP PetXP { get; set; }
}

[Serializable]
public class FairySouls
{
    public int CollectedSouls { get; set; }
    public int ExcessSouls { get; set; }
    public int SoulExchanges { get; set; }
}

[Serializable]
public class Slayer
{
    public string Name { get; set; }
    public int Level { get; set; }
    public bool IsMaxLevel { get; set; }
    public Sprite Icon { get; set; }
    public SLAYERTYPE Type { get; set; }
    public List<SlayerKill> Kills { get; set; }
    public XP ProgressXP { get; set; }
    public Dictionary<STAT, Stat> BonusStats { get; set; }
}

[Serializable]
public class SlayerKill
{
    public SLAYERTYPE Type { get; set; }
    public int Tier { get; set; }
    public int Amount { get; set; }
}

[Serializable]
public class Stat
{
    public STAT StatType { get; set; }
    public float BonusAmount { get; set; }
    public string BonusProcentBoost { get; set; }
    public Stat(STAT type, float amount)
	{
        StatType = type;
        BonusAmount = amount;
	}

    public Stat(STAT type, float amount, string bonus)
    {
        StatType = type;
        BonusAmount = amount;
        BonusProcentBoost = bonus;
    }

    public Stat() { }
}

[Serializable]
public class Backpack
{
    public List<Item> BackpackContents { get; set; }
    public int BackpackSize { get; set; }
    public string TextureLink { get; set; }
}

[Serializable]
public class BankingData
{
    public double PurseBalance { get; set; }
    public double BankBalance { get; set; }
    public List<Transaction> Transactions { get; set; }
}

[Serializable]
public class Transaction
{
    public double TransferredAmount { get; set; }
    public long TimeStamp { get; set; }
    public BANKACTION action { get; set; }
    public string InitiatorName { get; set; }
}

[Serializable]
public class Enchantment
{
    public string EnchantName { get; set; }
    public int EnchantLevel { get; set; }
    public bool UltimateEnchant { get; set; }
}
