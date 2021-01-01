using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CATEGORY
{
    weapon,
    armor,
    accessories,
    consumables,
    blocks,
    misc
}

[Serializable]
public class Profile
{
    public string FormattedUsername { get; set; }
    public string ProfileID { get; set; }
    public string CuteName { get; set; }
    public DateTime LastSaveDateTime { get; set; }
    public DateTime FirstSaveDateTime { get; set; }
    public bool IsOnline { get; set; }
    public int SelectedWardrobeIndex { get; set; }
    public int SelectedWeaponIndex { get; set; }
    public int CollectedFairySouls { get; set; }
    public float AverageSkilLevelProgression { get; set; }
    public float AverageSkilLevel { get; set; }
    public BankingData BankingData { get; set; }
    public WardrobeSlots EquippedArmorData { get; set; }
    public FairySouls FairySoulData { get; set; }
    public Item ActiveWeapon { get; set; }
    public Pet ActivePet { get; set; }
    public Dictionary<SKILL, Skill> SkillData { get; set; }
    public List<Slayer> Slayers { get; set; }
    public List<Stat> ProfileStats { get; set; }
    public List<Item> InventoryData { get; set; }
    public List<Item> AccessoryData { get; set; }
    public List<Item> EnderchestData { get; set; }
    public List<Item> FishingBagData { get; set; }
    public List<Item> QuiverData { get; set; }
    public List<Item> VaultData { get; set; }
    public List<Item> PotionData { get; set; }
    public List<Item> CandyData { get; set; }
    public List<Pet> PetData { get; set; }
    public List<Auction> PlayerAuctions { get; set; }
    public List<WardrobeSlots> WardrobeData { get; set; }
    public List<Item> Weapons { get; set; }
}

[Serializable]
public class AuctionPage
{
    public int Number { get; set; }
    public List<Auction> Auctions { get; set; }
}

[Serializable]
public class Auction
{
    public string AuctioneerUUID { get; set; }
    public string FormattedAuctioneerUsername { get; set; }
    public bool HasEnded { get; set; }
    public bool IsBIN { get; set; }
    public CATEGORY Category { get; set; }
    public List<Bid> Bids { get; set; }
    public Item AuctionItem { get; set; }
    public Bid HighestBid { get; set; }
    public float Price { get; set; }
    public long UnixBeginTimestamp { get; set; }
    public long UnixEndTimestamp { get; set; }
}

[Serializable]
public class Bid
{
    public string BidderUUID { get; set; }
    public string BidderProfileID { get; set; }
    public float Amount { get; set; }
    public long UnixTimestamp { get; set; }
}


[Serializable]
public class Skill
{
    public SKILL SkillName { get; set; }
    public XP SkillXP { get; set; }
    public bool IsMaxLevel { get; set; }
    public bool IncludeInAverage { get; set; }
    public STAT StatToIncrease { get; set; }
    public float StatBonus { get; set; }
}

[Serializable]
public class WardrobeSlots
{
    public int SlotIndex { get; set; }
    public List<Item> ArmorPieces { get; set; } = new List<Item>();
}

