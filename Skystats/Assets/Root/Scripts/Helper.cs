using fNbt;
using LeTai.Asset.TranslucentImage;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Michsky.UI.ModernUIPack;

namespace Helper
{
    public static class Ranks
    {
        public static string GetFormattedName(JSONNode playerData)
        {
            var rankName = "";
            var packageName = "";

            if (playerData.HasKey("prefix"))
                rankName = playerData["prefix"].Value;
            else if (playerData.HasKey("rank"))
                packageName = playerData["rank"].Value;
            else if (playerData.HasKey("monthlyPackageRank") && playerData["monthlyPackageRank"].Value != "NONE")
                packageName = playerData["monthlyPackageRank"].Value;
            else if (playerData.HasKey("newPackageRank"))
                packageName = playerData["newPackageRank"].Value;
            else if (playerData.HasKey("packageRank"))
                packageName = playerData["packageRank"].Value;
            else
                rankName = "§8";

            var plusColor = "§f";
            if (playerData.HasKey("rankPlusColor"))
                plusColor = GetColorFromName(playerData["rankPlusColor"].Value);

            var plusPlusColor = "§f";
            if (playerData.HasKey("monthlyRankColor"))
                plusPlusColor = GetColorFromName(playerData["monthlyRankColor"].Value);

            if (rankName == "")
                rankName = GetRankPrefix(packageName, plusColor, plusPlusColor);

            var playerName = playerData["displayname"].Value;
            var formattedUsername = $"{rankName} {playerName}";

            return formattedUsername;
        }

        public static string GetColorFromName(string name)
        {
            switch (name)
            {
                case "BLACK":
                    return "§0";
                case "DARK_GREEN":
                    return "§2";
                case "DARK_AQUA":
                    return "§3";
                case "DARK_RED":
                    return "§4";
                case "DARK_PURPLE":
                    return "§5";
                case "GOLD":
                    return "§6";
                case "GRAY":
                    return "§7";
                case "DARK_GRAY":
                    return "§8";
                case "BLUE":
                    return "§9";
                case "GREEN":
                    return "§a";
                case "AQUA":
                    return "§b";
                case "RED":
                    return "§c";
                case "LIGHT_BLUE":
                    return "§b";
                case "YELLOW":
                    return "§e";
                case "WHITE":
                    return "§f";
                default:
                    return "§f";
            }
        }

        public static string GetRankPrefix(string rankID, string plusColor, string plusPlusColor)
        {
            var rankPrefix = "§8";
            switch (rankID)
            {
                case "VIP":
                    rankPrefix = "§a[VIP]";
                    break;
                case "VIP_PLUS":
                    rankPrefix = "§a[VIP§6+§a]";
                    break;
                case "MVP":
                    rankPrefix = "§b[MVP]";
                    break;
                case "MVP_PLUS":
                    rankPrefix = $"§b[MVP{plusColor}+§b]";
                    break;
                case "SUPERSTAR":
                    rankPrefix = $"{plusPlusColor}[MVP{plusColor}++{plusPlusColor}]";
                    break;
                case "HELPER":
                    rankPrefix = "§a[HELPER]";
                    break;
                case "MODERATOR":
                    rankPrefix = "§2[MOD]";
                    break;
                case "ADMIN":
                    rankPrefix = "§c[ADMIN]";
                    break;
                case "YOUTUBER":
                    rankPrefix = "§c[§fYOUTUBE§c]";
                    break;
                case "MOJANG":
                    rankPrefix = "§6[MOJANG]";
                    break;
                case "SLOTH":
                    rankPrefix = "§c[SLOTH]";
                    break;
            }

            return rankPrefix;
        }
    }

    public static class Pets
    {
        public static JSONNode petLib = JSON.Parse(Resources.Load<TextAsset>("Pet Data/Pet data").text);

        public static List<Pet> SortPets(List<Pet> pets)
        {
            var sortedPets = pets;
            sortedPets = sortedPets.OrderByDescending((x) => x.Name).ToList();
            sortedPets = sortedPets.OrderByDescending((x) => x.PetXP.Level).ToList();
            sortedPets = sortedPets.OrderBy((x) => (int)x.PetRarityTier).ToList();

            return sortedPets;
        }

        public static Pet GetPetFromContainer(JSONNode petInfo, Item petItem)
        {
            var petType = petInfo["type"].Value;
            var heldItem = petInfo["heldItem"].Value;
            var exp = petInfo["exp"].AsFloat;
            var usedPetCandy = petInfo["candyUsed"].AsInt;

            var petTier = Global.GetRarity(petInfo["tier"]);
            var originalPetTier = petTier;
            if (heldItem != "")
                if (petLib["held_items"][heldItem].HasKey("boost_tier"))
                    petTier--;

            var skinLink = "";
            var skinName = "";

            foreach (var skin in petLib["skins"])
            {
                if (skin.Value["texture"] == petItem.TextureLink)
                {
                    skinLink = petItem.TextureLink;
                    skinName = skin.Value["name"].Value;
                    break;
                }
            }

            var pet = new Pet
            {
                Name = petType,
                HeldItemName = heldItem,
                PetXP = Global.GetXP(exp, GetPetXPLadder(petTier), 1),
                PetJSONData = petLib["pets"][petType],
                PetRarityTier = petTier,
                OriginalPetRarityTier = originalPetTier,
                UsedPetCandy = usedPetCandy,
                SkinLink = skinLink,
                SkinName = skinName,
                IsActive = false
            };
            pet = AddDescripionStats(pet);

            return pet;
        }

        public static Pet GetPet(JSONNode petNode)
        {
            var petName = petNode["type"].Value;
            int.TryParse(petNode["candyUsed"].Value, out var usedPetCandy);
            bool.TryParse(petNode["active"], out var isActive);

            // Get held item ID
            var heldItem = "";
            if (petNode.HasKey("heldItem"))
                heldItem = petNode["heldItem"].Value;

            string skinLink = "";
            string skinName = "";
            if (petNode["skin"] != null)
            {
                skinName = petNode["skin"].Value.Replace("PET_SKIN_", "");

                if (petLib["skins"].HasKey(skinName))
                {
                    skinLink = petLib["skins"][skinName]["texture"];
                    skinName = petLib["skins"][skinName]["name"];
                }
                else
                {
                    skinLink = "";
                    skinName = "";
                }
            }


            var petTier = Global.GetRarity(petNode["tier"]);
            var originalPetTier = petTier;
            if (heldItem != "")
                if (petLib["held_items"][heldItem].HasKey("boost_tier"))
                    petTier--;

            var pet = new Pet
            {
                Name = petName,
                HeldItemName = heldItem,
                PetXP = Global.GetXP(petNode["exp"], GetPetXPLadder(petTier), 1),
                PetJSONData = petLib["pets"][petName],
                PetRarityTier = petTier,
                OriginalPetRarityTier = originalPetTier,
                UsedPetCandy = usedPetCandy,
                SkinLink = skinLink,
                SkinName = skinName,
                IsActive = isActive
            };

            pet = AddDescripionStats(pet);
            return pet;
        }

        public static Pet AddDescripionStats(Pet pet)
        {
            var petDescriptionString = GetPetDescription(pet);
            var petDesc = Global.StringToList(petDescriptionString, "");
            var baseDescription = GetBaseDescription(pet);
            pet = AddHeldItem(pet);

            petDesc = AddHeldItemDescription(pet, petDesc);
            petDesc = ApplyHeldItemBonus(pet, petDesc, baseDescription);
            var petStats = Global.GetStatList(petDesc);
            petDesc = Formatting.FormatColorsStringList(petDesc);

            pet.Description = petDesc;
            pet.BonusStats = petStats;
            return pet;
        }

        public static int[] GetPetXPLadder(RARITY petRarityTier)
        {
            var xpLadderAsset = Resources.Load<TextAsset>("Pet Data/Pet XP Ladders");
            var xpLadders = JSON.Parse(xpLadderAsset.text);

            var rarityString = petRarityTier.ToString().ToUpper();
            var jsonLadder = xpLadders["XPLadders"][rarityString];

            int[] intLadder = new int[jsonLadder.Count];

            for (int i = 0; i < jsonLadder.Count; i++)
                intLadder[i] = jsonLadder[i].AsInt;

            return intLadder;
        }

        public static Item PetToItem(Pet pet)
        {
            if (pet != null && pet.Name != null)
            {
                var petLevel = "?";
                if (pet.PetXP != null)
                    petLevel = pet.PetXP.Level.ToString();

                var petLevelText = $"<color=#AAAAAA>[Lvl {petLevel}]</color> ";
                var petName = petLevelText + GetFormattedPetName(pet, true);
                var textureLink = pet.PetJSONData["texture"].Value;
                if (pet.SkinName != "")
                    textureLink = pet.SkinLink;

                var petItem = new Item
                {
                    Name = petName,
                    ItemDescription = pet.Description,
                    TextureLink = textureLink,
                    RarityTier = pet.PetRarityTier,
                    OriginalRarityTier = pet.OriginalPetRarityTier,
                    BonusStats = pet.BonusStats,
                    PetXP = pet.PetXP,
                    StackAmount = 1
                };

                return petItem;
            }

            return new Item();
        }

        public static List<Item> PetsToItemList(List<Pet> pets)
        {
            var itemPets = new List<Item>();
            foreach (var pet in pets)
                itemPets.Add(PetToItem(pet));

            return itemPets;
        }

        public static List<string> GetBaseDescription(Pet pet)
        {
            var baseStage = GetStageDesciption(pet.SkinName, pet.PetJSONData, 1);
            var descriptionString = "";
            for (int i = 0; i < baseStage.Count; i++)
                descriptionString += baseStage[i] + "\n";

            var baseDescriptionString = ScalePetDescription(descriptionString, pet);

            return Global.StringToList(baseDescriptionString, "");
        }

        public static Pet AddHeldItem(Pet pet)
        {
            var heldItemJsonString = Resources.Load<TextAsset>("Pet Data/Pet data");
            var heldItemNode = JSON.Parse(heldItemJsonString.text)["held_items"];

            var heldItem = heldItemNode[pet.HeldItemName.ToUpper()];
            var loreList = new List<string>();

            for (int i = 0; i < heldItem["lore"].Count; i++)
                loreList.Add(heldItem["lore"][i]);

            var bonusStats = new List<Stat>();
            foreach (var statkvp in heldItem["bonus"])
            {
                var statName = statkvp.Key;

                statName = Regex.Replace(statName, "_", " ");
                statName = Global.ToTitleCase(statName);
                statName = Regex.Replace(statName, " ", "");

                var stat = new Stat
                {
                    StatType = (STAT)Enum.Parse(typeof(STAT), statName),
                };

                var bonusNode = statkvp.Value;
                if (float.TryParse(bonusNode.Value, out var bonusAmount))
                    stat.BonusAmount = bonusAmount;
                else
                    stat.BonusProcentBoost = bonusNode.Value;

                bonusStats.Add(stat);
            }

            var newPet = pet;
            newPet.HeldItem = new HeldItem
            {
                Bonuses = bonusStats,
                Lore = loreList
            };

            return newPet;
        }

        public static List<string> AddHeldItemDescription(Pet pet, List<string> petDesc)
        {
            var newDesc = petDesc;

            foreach (var line in pet.HeldItem.Lore)
                newDesc.Add(line);

            return newDesc;
        }

        public static List<string> ApplyHeldItemBonus(Pet pet, List<string> petDesc, List<string> baseStats)
        {
            for (int i = 0; i < baseStats.Count; i++)
            {
                for (int j = 0; j < pet.HeldItem.Bonuses.Count; j++)
                {
                    var bonus = pet.HeldItem.Bonuses[j];
                    var bonusName = bonus.StatType.ToString();
                    bonusName = AddSpacing(bonusName);
                    bonusName += ":";

                    var matchingValues = baseStats.Where(s => s.Contains(bonusName));
                    if (matchingValues.Count() > 0 && matchingValues.First() == petDesc[i])
                    {
                        // Get the first and last number in the stat string, which is the amount to add to the stat
                        var numberCharArray = new char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };
                        var originalValueIndex = petDesc[i].IndexOfAny(numberCharArray, petDesc[i].IndexOf(":"));
                        var lastValueIndex = petDesc[i].LastIndexOfAny(numberCharArray);

                        float.TryParse(petDesc[i].Substring(originalValueIndex, lastValueIndex - originalValueIndex + 1), out var origValue);
                        var newStat = origValue;

                        // Check if the pet item multiplies or adds
                        if (bonus.BonusProcentBoost != null)
                        {
                            var boost = float.Parse(bonus.BonusProcentBoost.Replace("%", ""));
                            boost = (boost + 100) / 100;
                            newStat *= boost;
                        }
                        else
                            newStat += bonus.BonusAmount;

                        petDesc[i] = petDesc[i].Replace(origValue.ToString(), Global.FormatAndRoundAmount(newStat));
                    }
                    else if (i == baseStats.Count - 1 && matchingValues.Count() <= 0 && (bonus.BonusProcentBoost == "" || bonus.BonusProcentBoost == null))
                    {
                        // Special checks to make sure the new stat is in the correct position
                        // Griffin pet has an extra ability in the base endpoint so it needs to check
                        // a few things to find the new correct position
                        var emptySpacesIndex = Enumerable.Range(0, baseStats.Count).Where(k => baseStats[k] == "").ToList();
                        if (emptySpacesIndex.Count > 2)
                        {
                            var lastRowIndex = emptySpacesIndex[emptySpacesIndex.Count - 2];
                            petDesc.Insert(lastRowIndex, "§7" + bonusName + " §a+" + Global.FormatAndRoundAmount(bonus.BonusAmount));
                        }
                        else
                            petDesc.Insert(baseStats.Count - 1, "§7" + bonusName + " §a+" + Global.FormatAndRoundAmount(bonus.BonusAmount));
                    }
                }
            }

            return petDesc;
        }

        public static string AddSpacing(string original)
        {
            var newString = original;
            var matches = Regex.Matches(original, "[A-Z]");

            var offset = 0;
            foreach (Match match in matches)
            {
                if (match.Index != 0)
                {
                    newString = newString.Remove(match.Index + offset, 1).Insert(match.Index + offset, " " + match.Value);
                    offset = newString.Length - original.Length;
                }

            }

            return newString;
        }

        public static string GetFormattedPetName(Pet pet, bool includeSkinSymbol)
        {
            if (pet != null)
            {
                var name = "";
                if (pet.Name != null)
                    name = pet.Name.ToLower();
                name = Regex.Replace(name, "_", " ");
                name = Global.ToTitleCase(name);

                var petName = $"<color=#{Global.GetRarityHexColor(pet.PetRarityTier)}>{name}";
                if (pet.SkinName != "" && includeSkinSymbol)
                    petName += " ✦";
                petName += "</color>";

                return petName;
            }

            return "";
        }

        public static string GetPetDescription(Pet pet)
        {
            var petData = pet.PetJSONData;
            var stage = 1;
            var rarity = (int)pet.PetRarityTier;

            if (rarity <= 8) stage++;
            if (rarity <= 6) stage++;
            if (rarity <= 4) stage++;
            if (rarity <= 3) stage++;

            List<string> petDesc = GetStageDesciption(pet.SkinName, petData, stage);

            var descriptionString = "";
            for (int i = 0; i < petDesc.Count; i++)
                descriptionString += petDesc[i] + "\n";

            var description = ScalePetDescription(descriptionString, pet);
            return description;
        }

        public static List<string> GetStageDesciption(string formattedSkinName, JSONNode data, int stage)
        {
            List<string> petDesc = new List<string>();

            for (int i = 0; i < stage; i++)
            {
                if (i > 0)
                    petDesc.Add("");

                for (int j = 0; j < data["lore"][i].Count; j++)
                {
                    var newLine = data["lore"][i][j];
                    if (i == 0 && j == 0 && formattedSkinName != "")
                        newLine += $", {formattedSkinName} Skin";

                    petDesc.Add(newLine);
                }
            }
            return petDesc;
        }

        public static string ScalePetDescription(string petDesc, Pet pet)
        {
            var rarity = GetPetRarityValue(pet.PetRarityTier);
            var level = pet.PetXP.Level;

            var finalString = petDesc;
            var scaleStats = new List<KeyValuePair<int, int>>();
            var offset = 0;

            for (int i = 0; i < petDesc.Length; i++)
            {
                if (petDesc[i].ToString() == "$")
                {
                    if (scaleStats.Count == 0) scaleStats.Add(new KeyValuePair<int, int>(i, -1));
                    else
                    {
                        if (scaleStats[scaleStats.Count - 1].Value == -1)
                            scaleStats[scaleStats.Count - 1] = new KeyValuePair<int, int>(scaleStats[scaleStats.Count - 1].Key, i);
                        else
                            scaleStats.Add(new KeyValuePair<int, int>(i, -1));
                    }
                }
            }

            for (int i = 0; i < scaleStats.Count; i++)
            {
                if (scaleStats[i].Value != -1)
                {
                    var stat = petDesc.Substring(scaleStats[i].Key + 1, scaleStats[i].Value - scaleStats[i].Key - 1);

                    if (stat.Contains("|") && stat.Contains("~") && stat.Contains("("))
                    {
                        if (stat.Contains("(_"))
                        {
                            // Filter out the main-indicators we dont need
                            var basestatIndicators = new List<int>();

                            for (int j = 0; j < stat.Length; j++)
                                if (stat[j].ToString() == "(" || stat[j].ToString() == ")")
                                    basestatIndicators.Add(j);

                            // + 2 for offset and the extra _ after ( to indicate multiple possibilities
                            var basestat = stat.Substring(basestatIndicators[0] + 2, basestatIndicators[1] - basestatIndicators[0] - 2);

                            var multiplystatIndicators = new List<int>();

                            for (int j = 0; j < stat.Length; j++)
                                if (stat[j].ToString() == "~")
                                    multiplystatIndicators.Add(j);

                            // + 1 for offset
                            var multiplystat = stat.Substring(multiplystatIndicators[0] + 1, multiplystatIndicators[1] - multiplystatIndicators[0] - 1);

                            stat = stat.Replace("(_", "");
                            stat = stat.Replace(")", "");

                            stat = stat.Replace("~", "");
                            stat = stat.Replace("~", "");

                            var multiplyValues = GetRarityStats(multiplystat, "|");
                            var baseValues = GetRarityStats(basestat, "_");

                            float correctMultiply = 0, correctBase = 0;
                            if (multiplyValues.Count >= rarity)
                                correctMultiply = multiplyValues[rarity];
                            if (baseValues.Count >= rarity)
                                correctBase = baseValues[rarity];

                            correctMultiply /= 100;
                            correctMultiply *= level;

                            var finalValue = correctBase + correctMultiply;
                            finalString = finalString.Remove(scaleStats[i].Key - offset, scaleStats[i].Value - scaleStats[i].Key + 1);
                            finalString = finalString.Insert(scaleStats[i].Key - offset, Global.FormatAndRoundAmount(finalValue));
                            offset = petDesc.Length - finalString.Length;
                        }
                    }
                    else if (stat.Contains("|") && stat.Contains("~"))
                    {
                        stat = stat.Replace("~", "");

                        List<float> values = GetRarityStats(stat, "|");
                        var correctAmount = 0f;
                        if (values.Count >= rarity)
                            correctAmount = values[rarity];

                        correctAmount /= 100;
                        correctAmount *= level;

                        finalString = finalString.Remove(scaleStats[i].Key - offset, scaleStats[i].Value - scaleStats[i].Key + 1).Insert(scaleStats[i].Key - offset, Global.FormatAndRoundAmount(correctAmount));
                        offset = petDesc.Length - finalString.Length;

                    }
                    else if (stat.Contains("|") && !stat.Contains("~"))
                    {
                        List<int> vertLineLoc = new List<int>();
                        List<float> values = GetRarityStats(stat, "|");

                        var correctAmount = 0f;

                        if (values.Count >= rarity)
                            correctAmount = values[rarity];

                        finalString = finalString.Remove(scaleStats[i].Key - offset, scaleStats[i].Value - scaleStats[i].Key + 1).Insert(scaleStats[i].Key - offset, Global.FormatAndRoundAmount(correctAmount));
                        offset = petDesc.Length - finalString.Length;
                    }
                    else if (stat.Contains("(+") && !stat.Contains("|"))
                    {
                        List<KeyValuePair<int, int>> parenthesisLoc = new List<KeyValuePair<int, int>>();
                        for (int j = 0; j < stat.Length; j++)
                        {
                            if (stat[j].ToString() == "(")
                                if (parenthesisLoc.Count > 0)
                                {
                                    if (parenthesisLoc[parenthesisLoc.Count - 1].Key == -1)
                                        parenthesisLoc[parenthesisLoc.Count - 1] = new KeyValuePair<int, int>(j, parenthesisLoc[parenthesisLoc.Count - 1].Value);
                                    else
                                        parenthesisLoc.Add(new KeyValuePair<int, int>(j, -1));
                                }
                                else
                                    parenthesisLoc.Add(new KeyValuePair<int, int>(j, -1));
                            if (stat[j].ToString() == ")")
                                if (parenthesisLoc.Count > 0)
                                {
                                    if (parenthesisLoc[parenthesisLoc.Count - 1].Value == -1)
                                        parenthesisLoc[parenthesisLoc.Count - 1] = new KeyValuePair<int, int>(parenthesisLoc[parenthesisLoc.Count - 1].Key, j);
                                    else
                                        parenthesisLoc.Add(new KeyValuePair<int, int>(-1, j));
                                }
                                else
                                    parenthesisLoc.Add(new KeyValuePair<int, int>(-1, j));
                        }

                        var statString = stat.Substring(parenthesisLoc[0].Key + 1, parenthesisLoc[0].Value - parenthesisLoc[0].Key - 1);

                        if (statString.Contains("+"))
                            statString = statString.Replace("+", "");

                        float.TryParse(statString, out var baseStat);

                        var normalStatString = stat;
                        var parenthesisOffset = 0;

                        for (int j = 0; j < parenthesisLoc.Count; j++)
                        {
                            normalStatString = normalStatString.Remove(parenthesisLoc[j].Key + parenthesisOffset, parenthesisLoc[j].Value - parenthesisLoc[j].Key + 1);
                            parenthesisOffset += parenthesisLoc[j].Value - parenthesisLoc[j].Key;
                        }

                        float.TryParse(normalStatString, out var normalStat);
                        normalStat /= 100;
                        normalStat *= level;
                        normalStat += baseStat;

                        finalString = finalString.Remove(scaleStats[i].Key - offset, scaleStats[i].Value - scaleStats[i].Key + 1).Insert(scaleStats[i].Key - offset, Global.FormatAndRoundAmount(normalStat));
                        offset = petDesc.Length - finalString.Length;

                    }
                    else if (!stat.Contains("~") && !stat.Contains("|") && !stat.Contains("(+"))
                    {
                        float.TryParse(stat, out var statFloat);
                        statFloat /= 100;
                        statFloat *= level;

                        finalString = finalString.Remove(scaleStats[i].Key - offset, scaleStats[i].Value - scaleStats[i].Key + 1).Insert(scaleStats[i].Key - offset, Global.FormatAndRoundAmount(statFloat));
                        offset = petDesc.Length - finalString.Length;
                    }
                }
            }

            return finalString;
        }

        public static string ScalePetDescriptionTemp(string petDesc)
        {
            var level = 100;
            var rarity = GetPetRarityValue(RARITY.LEGENDARY);

            var matches = Regex.Matches(petDesc, @"\$(abs|lvl):(\((\d+|\d+\.\d+),(\d+|\d+\.\d+)\)|\((\d+|\d+\.\d+),(\d+|\d+\.\d+)\)\((\d+|\d+\.\d+),(\d+|\d+\.\d+)\)\((\d+|\d+\.\d+),(\d+|\d+\.\d+)\)\((\d+|\d+\.\d+),(\d+|\d+\.\d+)\)\((\d+|\d+\.\d+),(\d+|\d+\.\d+)\))\$");

            for (int i = matches.Count - 1; i >= 0; i--)
            {
                var match = matches[i];
                var endValue = ComputeStat(match, level, rarity);

                petDesc = petDesc.Remove(match.Index, match.Length).Insert(match.Index, endValue.ToString());
            }

            return petDesc;
        }

        public static float ComputeStat(Match statSpec, int level, int rarity)
        {
            if (statSpec.Groups.Count > 0)
            {
                bool isAbs = statSpec.Groups[0].Value == "abs";
                float multiplicationFactor;
                int captureOffset;

                if (isAbs)
                    multiplicationFactor = 1;
                else
                    multiplicationFactor = level / 100f;

                captureOffset = IsSimpleCase(statSpec.Groups) ? 0 : rarity;

                var a = float.Parse(statSpec.Groups[3 + captureOffset * 2].Value);
                var m = float.Parse(statSpec.Groups[4 + captureOffset * 2].Value);

                float statValue = multiplicationFactor * a + m;

                return statValue;
            }

            return 0;
        }

        public static bool IsSimpleCase(GroupCollection groups)
        {
            var fullEntries = 0;
            for (int i = 0; i < groups.Count; i++)
            {
                if (groups[i].Value != "")
                    fullEntries++;
            }

            return fullEntries <= 5;
        }

        public static int GetPetRarityValue(RARITY rarity)
        {
            switch (rarity)
            {
                case RARITY.COMMON:
                    return 0;
                case RARITY.UNCOMMON:
                    return 1;
                case RARITY.RARE:
                    return 2;
                case RARITY.EPIC:
                    return 3;
                case RARITY.LEGENDARY:
                    return 4;
                case RARITY.MYTHIC:
                    return 4;
            }

            return 0;
        }

        public static List<float> GetRarityStats(string stat, string indicator)
        {
            List<int> indicatorLoc = new List<int>();

            for (int j = 0; j < stat.Length; j++)
            {
                if (stat[j].ToString() == indicator)
                {
                    indicatorLoc.Add(j);
                }
            }

            List<float> values = new List<float>();

            for (int j = 0; j < indicatorLoc.Count; j++)
            {
                var res = 0f;

                if (j == 0)
                {
                    if (float.TryParse(stat.Substring(0, indicatorLoc[j]), out res))
                        values.Add(res);
                }

                if (j + 1 > indicatorLoc.Count - 1)
                    float.TryParse(stat.Substring(indicatorLoc[j] + 1, stat.Length - indicatorLoc[j] - 1), out res);
                else
                    float.TryParse(stat.Substring(indicatorLoc[j] + 1, indicatorLoc[j + 1] - indicatorLoc[j] - 1), out res);

                values.Add(res);
            }

            return values;
        }

    }

    public static class Formatting
    {
        public static Color DecimalToColor(int decimalColor)
        {
            var convertedColor = decimalColor.ToString("X");
            var hexColor = convertedColor;

            if (convertedColor.Length != 6)
                for (int i = 0; i < 6 - convertedColor.Length; i++)
                    hexColor = "1" + hexColor;

            hexColor = "#" + hexColor;

            ColorUtility.TryParseHtmlString(hexColor, out var finalColor);
            return finalColor;
        }

        public static List<string> FormatColorsStringList(List<string> originalStringList)
        {
            var formattedStringList = new List<string>();

            foreach (var str in originalStringList)
            {
                formattedStringList.Add(FormatColorString(str));
            }

            return formattedStringList;
        }

        public static string FormatColorString(string originalStr)
        {
            string formattedString = Regex.Replace(originalStr, "(§l|§m|§n|§r|§o)", "");
            formattedString = Regex.Replace(formattedString, "§k.", "");
            var matches = Regex.Matches(formattedString, "§.");

            foreach (Match match in matches)
            {
                formattedString = Regex.Replace(formattedString, match.Value, "</color><color=#" + Global.GetColor(match.Value) + ">");
            }

            if (!formattedString.EndsWith("</color>") && formattedString.Contains("<color=#") && formattedString != "")
                formattedString += "</color>";

            while (true)
                if (formattedString.StartsWith("</color>"))
                    formattedString = formattedString.Remove(0, 8);
                else if (formattedString.StartsWith(" </color>"))
                    formattedString = formattedString.Remove(0, 9);
                else
                    break;

            return formattedString;

        }

    }

    public static class Global
    {
        public static int GetMaxJournalAmount (string name)
		{
			switch (name)
			{
                case "karylles_diary": return 10;
                case "the_study": return 8;
                case "the_expedition_volume_1": return 4;
                case "uncanny_remains": return 7;
                case "the_expedition_volume_2": return 5;
                case "grim_adversity": return 9;
                case "the_expedition_volume_3": return 10;
                case "the_expedition_volume_4": return 11;
                case "the_walls": return 24;
                case "the_eye": return 8;
                case "aftermath": return 5;
                case "the_apprentice": return 6;
                case "the_follower": return 14;
                case "the_apprentice_2": return 14;
                case "necrons_magic_scroll": return 0;
                default: return 0;
            };
		}

        public static Sprite GetExperimentIcon(string type)
        {
            switch (type)
            {
                case "HIGH":
                    return Resources.Load<Sprite>($"Block Images/Textures/351-10");
                case "GRAND":
                    return Resources.Load<Sprite>($"Block Images/Textures/351-11");
                case "SUPREME":
                    return Resources.Load<Sprite>($"Block Images/Textures/351-14");
                case "TRANSCENDENT":
                    return Resources.Load<Sprite>($"Block Images/Textures/351-1");
                case "METAPHYSICAL":
                    return Resources.Load<Sprite>($"Block Images/Textures/351-13");
                default:
                    return Resources.Load<Sprite>($"Block Images/Textures/351-12");
            }
        }

        public static string GetExperimentName(string name)
        {
            switch (name.ToLower())
            {
                case "simon": return "Chronomatron";
                case "pairings" : return "Superpairs";
                case "numbers" : return "Ultrasequencer";
                default: return "";
            };
        }

        public static string GetStakeName (int tier, string experiment)
		{
            if (experiment == "Chronomatron")
			{
                switch (tier)
                {
                    case 0: return "HIGH";
                    case 1: return "GRAND";
                    case 2: return "SUPREME";
                    case 3: return "TRANSCENDENT";
                    case 5: return "METAPHYSICAL";
                    default: return "";
                };
            } else if (experiment == "Ultrasequencer")
            {
                switch (tier)
                {
                    case 1: return "SUPREME";
                    case 2: return "TRANSCENDENT";
                    case 3: return "METAPHYSICAL";
                    default: return "";
                };
            } else
            {
                switch (tier)
                {
                    case 0: return "BEGINNER";
                    case 1: return "HIGH";
                    case 2: return "GRAND";
                    case 3: return "SUPREME";
                    case 4: return "TRANSCENDENT";
                    case 5: return "METAPHYSICAL";
                    default: return "";
                };
            }

        }

        public static string GetArmorName (string originalName)
		{
            if (Regex.IsMatch(originalName, "(HELMET|FEDORA|HAT)")) return "HELMET";
            if (Regex.IsMatch(originalName, "(CHESTPLATE|JACKET|POLO|TUNIC)")) return "CHESTPLATE";
            if (Regex.IsMatch(originalName, "(LEGGINGS|PANTS|TROUSERS)")) return "LEGGINGS";
            if (Regex.IsMatch(originalName, "(BOOTS|OXFORDS|GALOSHES|SANDALS)")) return "BOOTS";

            return null;
        }

        public static string ReadFileFromPath (DirectoryInfo path, string dirName)
		{
            var result = "";
            var dirs = path.GetDirectories();

            for (int i = 0; i < dirs.Length; i++)
            {
                if (dirs[i].Name == dirName)
                    result = File.ReadAllText(dirs[i].FullName);
            }

            return result;
        }

        public static T FindParentWithComponent<T>(Transform parent)
        {
            if (parent.parent != null)
            {
                if (parent.parent.GetComponent<T>() != null)
                    return parent.parent.GetComponent<T>();
                else
                    return FindParentWithComponent<T>(parent.parent);
            } 

            return default(T);
        }

        public static List<T> FindObjectsOfTypeInTranform<T> (Transform transform)
		{
            List<T> results = new List<T>();

			for (int i = 0; i < transform.childCount; i++)
			{
                var tInChild = new List<T>();
                if (transform.GetChild(i).GetComponent<T>() != null)
                    results.Add(transform.GetChild(i).GetComponent<T>());
                else
                    tInChild = FindObjectsOfTypeInTranform<T>(transform.GetChild(i));

                foreach (var item in tInChild)
                    results.Add(item);
			}

            return results;
		}


        /// <summary>
        /// 
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string GetFormattedDateTime (DateTime time)
		{
            TimeSpan diff = DateTime.Now.Subtract(time);

            if (diff.TotalSeconds >= 0)
            {
                if (diff.Days >= 366)
                    return $"{Mathf.FloorToInt((float)diff.TotalDays / 365f)} {CheckSingleTime("year", Mathf.FloorToInt((float)diff.TotalDays / 365f))} ago";
                else if (diff.Days >= 31 && diff.Days < 366) return $"{Mathf.FloorToInt((float)diff.TotalDays / 30f)} {CheckSingleTime("month", Mathf.FloorToInt((float)diff.TotalDays / 30f))} ago";
                else if (diff.Days >= 7 && diff.Days < 31) return $"{Mathf.FloorToInt((float)diff.TotalDays / 7f)} {CheckSingleTime("week", Mathf.FloorToInt((float)diff.TotalDays / 7f))} ago";
                else if (diff.Days > 0 && diff.Days < 8) return $"{Mathf.FloorToInt((float)diff.TotalDays)} {CheckSingleTime("day", Mathf.FloorToInt((float)diff.TotalDays))} ago";
                else if (diff.Hours > 0)
                    return diff.Hours == 1 ? "1 hour ago" : $"{diff.Hours} hours ago";
                else if (diff.Minutes > 0) 
                    return diff.Minutes == 1 ? "1 minute ago" : $"{diff.Minutes} minutes ago";
                else return $"{diff.Seconds} {CheckSingleTime("second", diff.Seconds)} ago";
            }
            else
                throw new NotSupportedException($"DateTime in \"CompareToNow\" has to be in the past. Current difference (in seconds): {diff.TotalSeconds}");
        }

        public static string CheckSingleTime (string original, int amount)
		{
            return amount == 1 ? original : original + "s";
		}

        public static Texture2D ScalePointTexture(Texture2D tex)
        {
            // Scale the texture to correct size
            if (tex.height != 64)
                tex = ScaleTexture(tex, 64, 64);

            tex.filterMode = FilterMode.Point;
            return tex;
        }

        public static Texture2D ScaleTexture(Texture2D tex, int x, int y)
        {
            Color[] pix = tex.GetPixels(0, 0, tex.width, tex.height);
            Texture2D destTex = new Texture2D(x, y);

            destTex.SetPixels(0, 32, tex.width, tex.height, pix);
            destTex.Apply();

            return destTex;
        }

        public static string GetSlayerName (string original)
		{
			switch (original.ToUpper())
			{
                case "ZOMBIE":
                    return "REVENANT";
                case "WOLF":
                    return "SVEN";
                case "SPIDER":
                    return "TARANTULA";
                default:
                    return original;
			}
		}

        public static string ToRoman(int number)
        {
            if ((number < 0) || (number > 3999)) throw new ArgumentOutOfRangeException("Ínsert value betwheen 1 and 3999.");
            if (number < 1) return string.Empty;
            if (number >= 1000) return "M" + ToRoman(number - 1000);
            if (number >= 900) return "CM" + ToRoman(number - 900);
            if (number >= 500) return "D" + ToRoman(number - 500);
            if (number >= 400) return "CD" + ToRoman(number - 400);
            if (number >= 100) return "C" + ToRoman(number - 100);
            if (number >= 90) return "XC" + ToRoman(number - 90);
            if (number >= 50) return "L" + ToRoman(number - 50);
            if (number >= 40) return "XL" + ToRoman(number - 40);
            if (number >= 10) return "X" + ToRoman(number - 10);
            if (number >= 9) return "IX" + ToRoman(number - 9);
            if (number >= 5) return "V" + ToRoman(number - 5);
            if (number >= 4) return "IV" + ToRoman(number - 4);
            if (number >= 1) return "I" + ToRoman(number - 1);
            throw new ArgumentOutOfRangeException("Something unexpected occurred.");
        }

        public static XP GetXP(float xp, int[] xpLadder, int levelOffset)
        {
            for (int i = 0; i < xpLadder.Length; i++)
            {
                if (i == xpLadder.Length - 1)
                {
                    var XP = new XP
                    {
                        Level = Mathf.Clamp(i + 1, 1, xpLadder.Length),
                        RemainingXP = xp - xpLadder[i],
                        NextLevelXP = 0,
                        XPLadder = xpLadder
                    };

                    return XP;
                }
                else if (xp >= xpLadder[i] && xp < xpLadder[i + 1])
                {
                    var XP = new XP
                    {
                        Level = Mathf.Clamp(i + levelOffset, 1, xpLadder.Length),
                        RemainingXP = xp - xpLadder[i],
                        NextLevelXP = xpLadder[i + 1] - xpLadder[i],
                        XPLadder = xpLadder
                    };

                    return XP;
                }
            }

            return new XP
            {
                Level = 0,
                RemainingXP = 0,
                NextLevelXP = 0,
                XPLadder = new int[0]
            };
        }

        public static XP GetSlayerXP(float xp, int[] xpLadder, int levelOffset)
        {
            for (int i = 0; i < xpLadder.Length; i++)
            {
                if (i == xpLadder.Length - 1)
                {
                    var XP = new XP
                    {
                        Level = Mathf.Clamp(i + 1, 1, xpLadder.Length),
                        RemainingXP = xp - xpLadder[i],
                        NextLevelXP = 0,
                        XPLadder = xpLadder
                    };

                    return XP;
                }
                else if (xp >= xpLadder[i] && xp < xpLadder[i + 1])
                {
                    var XP = new XP
                    {
                        Level = Mathf.Clamp(i + levelOffset, 1, xpLadder.Length),
                        RemainingXP = xp - xpLadder[i],
                        NextLevelXP = xpLadder[i + 1],
                        XPLadder = xpLadder
                    };

                    return XP;
                }
            }

            return new XP
            {
                Level = 0,
                RemainingXP = 0,
                NextLevelXP = 0,
                XPLadder = new int[0]
            };
        }

        public static DateTime UnixTimeStampToDateTime(double timestamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dateTime = dateTime.AddMilliseconds(timestamp).ToLocalTime();
            return dateTime;
        }

        public static void UpdateInfoList()
        {
            UpdateScrollView();
        }

        public static void ChangeTranslucantImage(float translucancyAmount, GameObject obj)
        {
            obj.GetComponent<TranslucentImage>().spriteBlending = translucancyAmount;
        }

        public static bool DoesTagExist(string aTag)
        {
            try
            {
                GameObject.FindGameObjectsWithTag(aTag);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void UpdateScrollView()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(GameObject.FindGameObjectWithTag("Content").GetComponent<RectTransform>());
        }

        public static void UpdateCanvasElement(RectTransform element)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(element);
        }

        public static void SelectProfiles(string objName, Dictionary<string, string> profiles, Gradient selectedProfileGradient)
        {
            Main.Instance.input.SetActive(true);

            for (int i = 0; i < profiles.Count; i++)
            {
                var profile = Main.Instance.profileHolder.transform.Find(profiles.ElementAt(i).Key).gameObject;
                profile.SetActive(true);

                if (profiles.ElementAt(i).Key == objName)
                {
                    profile.GetComponent<TranslucentImage>().color = Color.white;
                    profile.GetComponent<UIGradient>().enabled = true;
                    profile.GetComponent<UIGradient>().EffectGradient = selectedProfileGradient;
                    profile.GetComponent<TranslucentImage>().spriteBlending = 0.9f;
                }
                else
                {
                    var tempColor = Color.black;
                    profile.GetComponent<UIGradient>().enabled = false;
                    profile.GetComponent<TranslucentImage>().color = tempColor;
                    profile.GetComponent<TranslucentImage>().spriteBlending = 0.3f;
                }
            }
        }

        public static List<Stat> GetStatList(List<string> description)
        {
            var statDictionary = GetStats(description);
            var statList = new List<Stat>();

            foreach (var stat in statDictionary)
            {
                var statName = Regex.Replace(stat.Key, "\\s+", "");
                var statType = (STAT)Enum.Parse(typeof(STAT), statName);

                var newStat = new Stat
                {
                    StatType = statType,
                    BonusAmount = stat.Value
                };

                statList.Add(newStat);
            }

            return statList;
        }

        public static Dictionary<string, float> GetStats(List<string> rawDesc)
        {
            Dictionary<string, float> finishedStats = new Dictionary<string, float>();

            // Loop through the whole desc
            for (int i = 0; i < rawDesc.Count; i++)
            {
                var rawDescLine = rawDesc[i];

                rawDescLine = Regex.Replace(rawDescLine, "\\([^)]*\\)", "");
                rawDescLine = Regex.Replace(rawDescLine, "§.", "");

                // Check if the desc line is a stat.
                if (rawDescLine.Contains(":") && (rawDescLine.Contains("+") || rawDescLine.Contains("-")) && rawDescLine.Any(char.IsDigit))
                {
                    // Split the line into two lines: the stat name and the value.
                    var newLines = Regex.Split(rawDescLine, ":");

                    // Remove special characters
                    // newLines[0] = stat name, newLines[1] = stat value
                    for (int j = 0; j < newLines.Length; j++)
                        newLines[j] = Regex.Replace(newLines[j], "(HP|%|\\+)", "");

                    // Add stat to bonusstats
                    var statName = Regex.Replace(newLines[0], "\\s+", "");
                    if (Enum.TryParse<STAT>(statName, out var statType))
                    {
                        float.TryParse(newLines[1], out var newValue);

                        if (!finishedStats.ContainsKey(newLines[0])) finishedStats.Add(newLines[0], newValue);
                        else finishedStats[newLines[0]] += newValue;
                    }
                }
            }

            return finishedStats;
        }
        public static string GetColor(string mcCode)
        {
            switch (mcCode)
            {
                case "§0":
                    return "000000";
                case "§1":
                    return "0000AA";
                case "§2":
                    return "00AA00";
                case "§3":
                    return "00AAAA";
                case "§4":
                    return "AA0000";
                case "§5":
                    return "AA00AA";
                case "§6":
                    return "FFAA00";
                case "§7":
                    return "AAAAAA";
                case "§8":
                    return "555555";
                case "§9":
                    return "5555FF";
                case "§a":
                    return "55FF55";
                case "§b":
                    return "55FFFF";
                case "§c":
                    return "FF5555";
                case "§d":
                    return "FF55FF";
                case "§e":
                    return "FFFF55";
                case "§f":
                    return "FFFFFF";
                case "§l":
                    return "bold";
                case "§g": // Gradient 
                    return "301C46";

                default:
                    return "FFFFFF";
            }
        }

        public static string ToTitleCase(string str)
        {
            if (str != null)
            {
                var newStr = str.ToLower();
                newStr = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(newStr);

                return newStr;
            }

            return "";
        }


        public static List<string> StringToList(string orig, string regexPattern)
        {
            if (regexPattern != string.Empty)
                orig = Regex.Replace(orig, regexPattern, "");

            var finishedList = orig.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.None).ToList();
            return finishedList;
        }

        public static string ListToString(List<string> orig)
        {
            var finishedString = "";
			foreach (var str in orig)
                finishedString += $"\n{str}";

            return finishedString;
        }

        public static string GetRarityHexColor(RARITY tier)
        {
            switch (tier)
            {
                case RARITY.COMMON:
                    return "FFFFFF";
                case RARITY.UNCOMMON:
                    return "55FF55";
                case RARITY.RARE:
                    return "5555FF";
                case RARITY.EPIC:
                    return "AA00AA";
                case RARITY.LEGENDARY:
                    return "FFAA00";
                case RARITY.MYTHIC:
                    return "FF55FF";
                case RARITY.SUPREME:
                    return "AA0000";
                case RARITY.SPECIAl:
                    return "FF5555";
                case RARITY.VERY_SPECIAL:
                    return "AA0000";
                case RARITY.UNKNOWN:
                    return "555555";
            }

            return "555555";
        }

        public static RARITY GetRarity(string str)
        {
            str = str.ToUpper();

            if (str.Contains("UNCOMMON"))
                return RARITY.UNCOMMON;
            else if (str.Contains("COMMON"))
                return RARITY.COMMON;
            else if (str.Contains("RARE"))
                return RARITY.RARE;
            else if (str.Contains("EPIC"))
                return RARITY.EPIC;
            else if (str.Contains("LEGENDARY"))
                return RARITY.LEGENDARY;
            else if (str.Contains("MYTHIC"))
                return RARITY.MYTHIC;
            else if (str.Contains("SUPREME"))
                return RARITY.SUPREME;
            else if (str.Contains("VERY SPECIAL"))
                return RARITY.VERY_SPECIAL;
            else if (str.Contains("SPECIAL"))
                return RARITY.SPECIAl;

            return RARITY.COMMON;
        }

        public static string FormatAndRoundAmount(float amount)
        {
            amount = (float)Math.Round(amount, 1);
            return string.Format("{0:#,##0.##}", amount);
        }

    }

    public static class Items
    {
        public static void GetCompoundData(Profile newProfile, List<Item> data, NbtList invContents)
        {
            var activeAccessories = new List<Item>();
            var weapons = new List<Item>();
            var pets = new List<Pet>();

            if (newProfile.AccessoryData != null) activeAccessories = newProfile.AccessoryData;
            if (newProfile.Weapons != null) weapons = newProfile.Weapons;

            foreach (NbtCompound itemCompound in invContents)
            {
                if (itemCompound.TryGet("tag", out NbtCompound tag))
                {
                    Item item = GetItemObject(itemCompound, tag, newProfile);
                    data.Add(item);
                    if (item != null && item.ItemDescription != null)
                    {
                        if (item.ItemDescription.Count > 1)
                        {
                            string lastDescriptionLine = item.ItemDescription[item.ItemDescription.Count - 1];

                            if (lastDescriptionLine.Contains("CCESSORY"))
                                activeAccessories.Add(item);

                            if (lastDescriptionLine.Contains("SWORD") || lastDescriptionLine.Contains("BOW") || lastDescriptionLine.Contains("WEAPON"))
                                if (!weapons.Exists((x) => x.ItemDescription == item.ItemDescription))
                                    weapons.Add(item);
                        }
                    }
                }
                else
                {
                    data.Add(new Item());
                }
            }

            newProfile.Weapons = weapons;
            newProfile.AccessoryData = activeAccessories;
        }

        public static List<Item> GetDataModule(Profile originalProfile, JSONNode profileData, string moduleName)
        {
            var moduleData = new List<Item>();

            if (profileData.HasKey(moduleName))
            {
                NbtCompound moduleTag = Parsing.DecodeGzippedBase64(profileData[moduleName]["data"].Value.ToString(), $"{moduleName}Data.nbt");
                var moduleContents = moduleTag.Get<NbtList>("i");

                foreach (NbtCompound itemCompound in moduleContents)
                {
                    if (itemCompound.TryGet("tag", out NbtCompound tag))
                    {
                        Item item = Items.GetItemObject(itemCompound, tag, originalProfile);
                        moduleData.Add(item);
                    }
                    else
                        moduleData.Add(new Item());
                }
            }

            return moduleData;
        }

        public static List<Item> SortItems(List<Item> itemsToSort)
        {
            var sortedAccessories = itemsToSort;

            if (itemsToSort != null && itemsToSort.Count > 0)
                sortedAccessories = itemsToSort.OrderBy((x) => (int)x.RarityTier).ToList();

            return sortedAccessories;
        }

        public static string GetTextureLink(NbtCompound skullOwner)
        {
            string base64TextureLink = skullOwner.Get<NbtCompound>("Properties").Get<NbtList>("textures").Get<NbtCompound>(0).Get<NbtString>("Value").Value;
            JSONNode textureLinkNode = Parsing.Base64ToJSON(base64TextureLink);

            string textureLink = textureLinkNode["textures"]["SKIN"]["url"].Value;
            return textureLink;
        }

        public static string GetItemID(NbtCompound itemTag)
        {
            var id = itemTag.Get<NbtShort>("id").Value.ToString();
            var damage = itemTag.Get<NbtShort>("Damage").Value.ToString();

            string itemID = id + "-" + damage;
            return itemID;
        }

        public static List<string> NbtToStringList(NbtList originalList)
        {
            var newList = new List<string>();

            if (originalList != null && originalList != new NbtList())
                for (int i = 0; i < originalList.Count; i++)
                    newList.Add(ReplaceNonWorkingChars(originalList[i].StringValue));

            return newList;
        }

        public static string ReplaceNonWorkingChars(string original)
        {
            for (int i = 0; i < original.Length; i++)
            {
                switch (original[i])
                {
                    case '☘':
                        original = original.Remove(i, 1).Insert(i, "Ʊ");
                        break;
                    case '⸕':
                        original = original.Remove(i, 1).Insert(i, "↑");
                        break;
                }
            }

            return original;
        }
        
        public static Item GetItemObject(NbtCompound itemCompound, NbtCompound tag, Profile currentProfile)
        {
            if (tag.TryGet<NbtCompound>("ExtraAttributes", out var extraAttributes))
            {
                var display = tag.Get<NbtCompound>("display");
                var nbtDescription = display.Get<NbtList>("Lore");
                var internalItemID = "";

                if (extraAttributes.TryGet<NbtString>("id", out var internalItemIDTag))
                    internalItemID = internalItemIDTag.StringValue;

                List<string> stringDescription = NbtToStringList(nbtDescription);
                string itemName = display.Get<NbtString>("Name").StringValue;
                if (internalItemID.Contains("STARRED")) 
                    itemName = itemName.Insert(2, "↑ ");
                itemName = Formatting.FormatColorString(itemName);

                var itemIDString = "";
                if (extraAttributes.TryGet<NbtString>("id", out var nbtID))
                    itemIDString = nbtID.StringValue;

                var backpack = new Backpack();
                var textureLink = "";
                var minecraftItemID = "";
                

                // Get texture link/id 
                if (tag.TryGet<NbtCompound>("SkullOwner", out var skullOwner))
                    textureLink = GetTextureLink(skullOwner);
                else
                    minecraftItemID = GetItemID(itemCompound);

                // Get optional backpack data
                if (itemIDString.Contains("_BACKPACK"))
                    backpack = GetBackpackData(tag, extraAttributes, itemIDString, currentProfile);

                // Amount of items 
                byte stackAmount = 1;
                if (itemCompound.TryGet<NbtByte>("Count", out var stackAmountTag))
                    stackAmount = stackAmountTag.ByteValue;

                // Get modifier/reforge
                string modifier = "";
                if (extraAttributes.TryGet<NbtString>("modifier", out var modifierTag))
                    modifier = modifierTag.StringValue;

                bool leather = false;
                var armorColor = Color.white;
                if (display.TryGet<NbtInt>("color", out var color))
                {
                    leather = true;
                    armorColor = Formatting.DecimalToColor(color.IntValue);
                }

                var isRarityUpgraded = extraAttributes.TryGet("rarity_upgrades", out var _);

                var item = new Item
                {
                    Name = itemName,
                    ItemIDString = minecraftItemID,
                    InternalItemIDString = internalItemID,
                    TextureLink = textureLink,
                    IsLeather = leather,
                    IsRarityUpgraded = isRarityUpgraded,
                    Modifier = modifier,
                    Enchantments = Enchantments.GetItemEnchantments(extraAttributes),
                    StackAmount = stackAmount,
                    ItemDescription = stringDescription,
                    Backpack = backpack,
                    LeatherArmorColor = armorColor
                };

                item.RarityTier = GetItemRarity(item);
                var originalRarity = item.RarityTier;

                if (internalItemID.Contains("STARRED") && internalItemID.Contains("SHADOW_ASSASSIN"))
                    originalRarity++;

               if (isRarityUpgraded)
				{
                    originalRarity++;
                    stringDescription.Insert(stringDescription.Count - 1, "<color=#555555>(Recombobulated)</color>");
                }

                if (item.Modifier.ToLower().Contains("renowned"))
                    if (Stats.Instance != null)
                        Stats.Instance.totalStatBoost *= 1.01f;

                stringDescription = Enchantments.FormatEnchantments(item);
                List<string> formattedDescription = Formatting.FormatColorsStringList(stringDescription);

                item.BonusStats = Global.GetStatList(stringDescription);
                item.OriginalRarityTier = originalRarity;
                item.ItemDescription = formattedDescription;

                return item;
            }
            else
            {
                var item = new Item();
                return item;
            }

        }

        public static Backpack GetBackpackData(NbtCompound tag, NbtCompound extraAttributes, string itemID, Profile currentProfile)
        {
            var textureLink = GetTextureLink(tag.Get<NbtCompound>("SkullOwner"));
            var backpackDataTagName = itemID.ToLower() + "_data";

            if (extraAttributes.TryGet<NbtByteArray>(backpackDataTagName, out var backpackContentNbtArray))
            {
                var backpackDataByteArray = backpackContentNbtArray.Value;
                var backpackData = Parsing.DecodeNBTData(backpackDataByteArray, "backpackData.nbt");
                var backpackContents = new List<Item>();
                var weapons = new List<Item>();
                if (currentProfile.Weapons != null)
                    weapons = currentProfile.Weapons;

                var backpackRoot = backpackData.Get<NbtList>("i");
                int backpackSize = backpackRoot.Count;

                for (int i = 0; i < backpackSize; i++)
                {
                    NbtCompound itemCompound = backpackRoot.ElementAt(i) as NbtCompound;
                    if (itemCompound.TryGet("tag", out NbtCompound itemTag))
                    {
                        var item = GetItemObject(itemCompound, itemTag, currentProfile);
                        backpackContents.Add(item);
                        if (item != null && item.ItemDescription != null)
                        {
                            if (item.ItemDescription.Count > 1)
                            {
                                string lastDescriptionLine = item.ItemDescription[item.ItemDescription.Count - 1];
                                if (lastDescriptionLine.Contains("SWORD") || lastDescriptionLine.Contains("BOW") || lastDescriptionLine.Contains("WEAPON"))
                                    if (!weapons.Exists((x) => x.ItemDescription == item.ItemDescription))
                                        weapons.Add(item);
                            }
                        }
                    }
                    else
                        backpackContents.Add(new Item());
                }

                var newBackpack = new Backpack
                {
                    BackpackContents = backpackContents,
                    BackpackSize = backpackSize,
                    TextureLink = textureLink
                };

                return newBackpack;
            }

            return new Backpack();
        }


        public static RARITY GetItemRarity(Item item)
        {
            if (item != null)
            {
                if (item.ItemDescription != null && item.ItemDescription.Count > 0)
                {
                    string indicatorString = item.ItemDescription[item.ItemDescription.Count - 1];
                    return Global.GetRarity(indicatorString);
                }
            }

            return RARITY.UNKNOWN;
        }

    }

    public static class Parsing
    {
        public static NbtCompound DecodeNBTData(byte[] nbtData, string path)
        {
            File.WriteAllBytes($"{Application.persistentDataPath}/nbt data/{path}", nbtData);

            var nbtFile = new NbtFile();
            nbtFile.LoadFromFile($"{Application.persistentDataPath}/nbt data/{path}");

            return nbtFile.RootTag;
        }

        public static JSONNode Base64ToJSON(string base64string)
        {
            if (base64string.Length % 4 != 0)
                if ((base64string.Length + 1) % 4 == 0)
                    base64string += "=";
                else if ((base64string.Length + 2) % 4 == 0)
                    base64string += "==";
                else
                    base64string += "===";

            var decodedSkinData = Convert.FromBase64String(base64string);
            var skinLink = Encoding.UTF8.GetString(decodedSkinData);
            return JSON.Parse(skinLink);
        }

        public static NbtCompound DecodeGzippedBase64(string baseString, string nbtFilePath)
        {
            // Check if string length is equal to base64 string length
            if (baseString.Length % 4 != 0)
                if ((baseString.Length + 1) % 4 == 0)
                    baseString += "=";
                else if ((baseString.Length + 2) % 4 == 0)
                    baseString += "==";
                else
                    baseString += "===";

            var decodedString = Convert.FromBase64String(baseString);
            var nbtData = decodedString;

            // Read the data
            if (nbtData.Length > 0)
                return DecodeNBTData(nbtData, $"{nbtFilePath}");

            return new NbtCompound();
        }

    }

    public static class Enchantments
    {
        public static JSONNode enchantmentLib = JSON.Parse(Resources.Load<TextAsset>("JSON Data/Enchantments").text);

        public static List<Enchantment> GetItemEnchantments(NbtCompound extraAttributes)
        {
            if (extraAttributes.TryGet("enchantments", out NbtCompound enchantmentsCompound))
            {
                var itemEnchantments = new List<Enchantment>();

                foreach (NbtInt enchantmentTag in enchantmentsCompound)
                {
                    var enchantment = new Enchantment
                    {
                        EnchantName = enchantmentTag.Name,
                        EnchantLevel = enchantmentTag.IntValue
                    };

                    enchantment.UltimateEnchant = enchantmentTag.Name.Contains("ultimate_");
                    itemEnchantments.Add(enchantment);
                }

                return itemEnchantments;
            }

            return new List<Enchantment>();
        }

        public static List<string> FormatEnchantments(Item item)
        {
            List<string> finishedInvDescList = new List<string>();

            if (item != null)
            {
                for (int k = 0; k < item.ItemDescription.Count; k++)
                {
                    if (item.Enchantments.Count > 0)
                    {
                        var newString = item.ItemDescription.ElementAt(k);
                        // Enchantment name, Index of enchantment's name
                        var maxEnchants = new List<KeyValuePair<string, int>>();

                        // Loop through all the enchantments the item has
                        for (int l = 0; l < item.Enchantments.Count; l++)
                        {
                            var enchantment = item.Enchantments[l];
                            if (enchantmentLib.HasKey(enchantment.EnchantName))
                            {
                                var enchantJSON = enchantmentLib[enchantment.EnchantName];
                                var enchantName = enchantJSON["formattedName"].Value;

                                // Check if the enchantment is max level
                                var max = enchantment.EnchantLevel >= enchantJSON["maxLevel"].AsInt;

                                if (max)
                                    if (newString.Contains(enchantName))
                                        if (newString[newString.IndexOf(enchantName) - 1].ToString() == "9")
                                            maxEnchants.Add(new KeyValuePair<string, int>(enchantName, newString.IndexOf(enchantName)));
                            }
                        }

                        // Change all the normal color codes from the max enchants to the gradient tag (§g)
                        for (int l = 0; l < maxEnchants.Count; l++)
                            newString = newString.Remove(maxEnchants.ElementAt(l).Value - 2, 2).Insert(maxEnchants.ElementAt(l).Value - 2, "§g");

                        finishedInvDescList.Add(newString);
                    }
                    else finishedInvDescList.Add(item.ItemDescription.ElementAt(k));
                }
            }

            return finishedInvDescList;
        }

    }

    public static class PlayerStats
    {
        public static void ProcessStats(Profile profile)
        {
            if (Stats.Instance != null) 
            {
                Stats.Instance.ResetStats(true);
                var bonusStats = GetEmptyStatList();

                bonusStats = GetEquippedArmorStats(profile, bonusStats);
                bonusStats = GetAccessoryStats(profile, bonusStats);
                bonusStats = GetActivePetStats(profile, bonusStats);
                if (profile.ActiveWeapon != null)
                    bonusStats = GetActiveWeaponStats(profile, bonusStats);

                foreach (var item in bonusStats)
                    Stats.Instance.bonus[item.Key].BonusAmount += item.Value.BonusAmount;

                Stats.Instance.GetStats();
                Stats.Instance.RenderStats();
            }
        }

        public static List<Stat> GetStats(Profile profile)
        {
            Stats.Instance.ResetStats(true);
            var bonusStats = GetEmptyStatList();

            bonusStats = GetEquippedArmorStats(profile, bonusStats);
            bonusStats = GetActivePetStats(profile, bonusStats);
            bonusStats = GetAccessoryStats(profile, bonusStats);
            if (profile.ActiveWeapon != null)
                bonusStats = GetActiveWeaponStats(profile, bonusStats);

            foreach (var item in bonusStats)
                Stats.Instance.bonus[item.Key].BonusAmount += item.Value.BonusAmount;

            bonusStats = Stats.Instance.GetStatDictionary();
            var list = bonusStats.Values.ToList();

            return list;
        }

        public static List<Stat> GetStatsNoAccessories(Profile profile)
        {
            Stats.Instance.ResetStats(true);
            var bonusStats = GetEmptyStatList();

            bonusStats = GetEquippedArmorStats(profile, bonusStats);
            bonusStats = GetActivePetStats(profile, bonusStats);
            if (profile.ActiveWeapon != null)
                bonusStats = GetActiveWeaponStats(profile, bonusStats);

            foreach (var item in bonusStats)
                Stats.Instance.bonus[item.Key].BonusAmount += item.Value.BonusAmount;

            bonusStats = Stats.Instance.GetStatDictionary();
            var list = bonusStats.Values.ToList();

            return list;
        }

        public static Dictionary<STAT, Stat> GetEquippedArmorStats(Profile profile, Dictionary<STAT, Stat> originalStats)
        {
            var equippedArmor = profile.EquippedArmorData.ArmorPieces;
            var newStats = GetItemListStats(equippedArmor, originalStats);

            int supPieces = 0;
			foreach (var piece in equippedArmor)
			{
                if (piece != null && piece.InternalItemIDString != null)
                    if (piece.InternalItemIDString.ToLower().Contains("superior"))
                        supPieces++;
			}

            if (supPieces >= 4)
                Stats.Instance.totalStatBoost *= 1.05f;

            return newStats;
        }

        public static Dictionary<STAT, Stat> GetAccessoryStats(Profile profile, Dictionary<STAT, Stat> originalStats)
        {
            var accessories = profile.AccessoryData;
            var newStats = GetItemListStats(accessories, originalStats);

            return newStats;
        }

        public static Dictionary<STAT, Stat> GetActivePetStats(Profile profile, Dictionary<STAT, Stat> originalStats)
        {
            var newStats = originalStats;

            if (profile.ActivePet != null)
            {
                var activePet = profile.ActivePet.BonusStats;
                newStats = GetItemStats(activePet, originalStats);
            }

            return newStats;
        }

        public static Dictionary<STAT, Stat> GetActiveWeaponStats(Profile profile, Dictionary<STAT, Stat> originalStats)
        {
            var activePet = profile.ActiveWeapon.BonusStats;
            var newStats = GetItemStats(activePet, originalStats);

            return newStats;
        }

        public static Dictionary<STAT, Stat> GetItemListStats(List<Item> itemList, Dictionary<STAT, Stat> originalStats)
        {
            var newStats = originalStats;

            if (itemList != null && itemList.Count > 0)
                foreach (var item in itemList)
                    newStats = GetItemStats(item.BonusStats, newStats);

            return originalStats;
        }

        public static Dictionary<STAT, Stat> GetItemStats(List<Stat> bonusStats, Dictionary<STAT, Stat> originalStats)
        {
            var newStats = originalStats;

            if (bonusStats != null)
            {
                foreach (var stat in bonusStats)
                {
                    if (newStats.ContainsKey(stat.StatType))
                        newStats[stat.StatType].BonusAmount += stat.BonusAmount;
                    else
                        newStats.Add(stat.StatType, stat);
                }
            }

            return newStats;
        }

        public static Dictionary<STAT, Stat> GetSlayerStats(SLAYERTYPE slayerType, int slayerLevel)
        {
            var finishedStats = GetEmptyStatList();

            switch (slayerType)
            {
                case SLAYERTYPE.Zombie:
                    int bonus = 2;
                    for (int i = 0; i < slayerLevel; i++)
                    {
                        if (i % 2 == 0 && i != 0)
                            bonus++;
                        finishedStats[STAT.Health].BonusAmount += bonus;
                    }
                    break;
                case SLAYERTYPE.Spider:
                    for (int i = 0; i < slayerLevel; i++)
                    {
                        if (i <= 3) finishedStats[STAT.CritDamage].BonusAmount++;
                        else if (i > 3 && i <= 5) finishedStats[STAT.CritDamage].BonusAmount += 2;
                        else if (i == 6) finishedStats[STAT.CritChance].BonusAmount++;
                        else if (i > 6) finishedStats[STAT.CritDamage].BonusAmount += 3;
                    }
                    break;
                case SLAYERTYPE.Wolf:
                    for (int i = 0; i < slayerLevel; i++)
                    {
                        if (i == 0 || i == 2) finishedStats[STAT.Speed].BonusAmount++;
                        if (i == 1 || i == 3) finishedStats[STAT.Health].BonusAmount += 2;
                        if (i == 4) finishedStats[STAT.CritDamage].BonusAmount++;
                        if (i == 5) finishedStats[STAT.Health].BonusAmount += 3;
                        if (i == 6) finishedStats[STAT.CritDamage].BonusAmount += 2;
                        if (i == 7) finishedStats[STAT.Speed].BonusAmount++;
                        if (i == 8) finishedStats[STAT.Health].BonusAmount += 5;
                    }
                    break;
                default:
                    break;
            }

            return finishedStats;
        }

        public static Dictionary<STAT, Stat> GetEmptyStatList()
        {
            var STATnames = (STAT[])Enum.GetValues(typeof(STAT));
            var emptyStatList = new List<Stat>();

            for (int i = 0; i < STATnames.Count(); i++)
            {
                emptyStatList.Add(new Stat
                {
                    StatType = STATnames[i],
                    BonusAmount = 0
                });
            }

            var emptyStats = Enumerable.Range(0, STATnames.Count()).ToDictionary(i => STATnames[i], i => emptyStatList[i]);
            return emptyStats;
        }

    }

    public static class Skills
    {
        public static void UpdateGradient(GradientType gradientName, Transform icon, StaticGradient[] staticGr)
        {
            for (int j = 0; j < staticGr.Length; j++)
                staticGr[j].enabledGradient = gradientName;

            if (icon != null && icon.parent.TryGetComponent<UIGradient>(out var iconGradient))
                iconGradient.Offset = gradientName == GradientType.maxColor ? 1 : 0;
            
        }

        public static Skill GetSkill(JSONNode skillNode, string skillName)
        {
            var skillType = (SKILL)Enum.Parse(typeof(SKILL), skillName);
            var xp = GetSkillXP(skillNode, skillType);
            var maxLevel = xp.Level == xp.XPLadder.Length;

            var skill = new Skill
            {
                SkillName = skillType,
                SkillXP = xp,
                IsMaxLevel = maxLevel,
                IncludeInAverage = IncludeInAverage(skillType),
                StatsToIncrease = GetSkillStatMatch(skillType),
                StatBonus = GetSkillBonus(skillType, xp.Level)
            };

            return skill;
        }

        public static float GetAverageSkillLevel(Dictionary<SKILL, Skill> skills, bool includeProgression)
        {
            var totalLevel = 0f;
            var totalSkills = 0f;

            foreach (var skill in skills)
            {
                if (skill.Value.IncludeInAverage)
                {
                    var skillXP = skill.Value.SkillXP;
                    if (includeProgression)
                        if (skillXP.NextLevelXP > 0)
                            totalLevel += skillXP.RemainingXP / skillXP.NextLevelXP;

                    totalLevel += skillXP.Level;
                    totalSkills++;
                }
            }

            if (totalSkills == 0 && totalLevel == 0)
                return 0f;

            var average = totalLevel / totalSkills;
            return average;
        }

        public static bool IncludeInAverage(SKILL skill)
        {
            switch (skill)
            {
                case SKILL.carpentry:
                case SKILL.runecrafting:
                case SKILL.catacombs:
                case SKILL.mage:
                case SKILL.berserk:
                case SKILL.archer:
                case SKILL.healer:
                case SKILL.tank:
                    return false;

                default:
                    return true;
            }
        }

        public static STAT[] GetSkillStatMatch(SKILL skill)
        {
			switch (skill)
			{
				case SKILL.combat:
					return new STAT[1] { STAT.CritChance };
				case SKILL.foraging:
					return new STAT[1] { STAT.Strength };
				case SKILL.mining:
					return new STAT[1] { STAT.Defense };
				case SKILL.farming:
					return new STAT[1] { STAT.Health };
				case SKILL.fishing:
					return new STAT[1] { STAT.Health };
				case SKILL.taming:
					return new STAT[1] { STAT.PetLuck };
				case SKILL.alchemy:
                    return new STAT[1] { STAT.Intelligence };
				case SKILL.enchanting:
                    return new STAT[2] { STAT.Intelligence, STAT.AbilityDamage };
                default: return null;
			}
		}

		public static float[] GetSkillBonus(SKILL skillType, int level)
        {
            if (Stats.Instance != null)
			{
                switch (skillType)
                {
                    case SKILL.taming:
                    case SKILL.combat:
                        return new float[1] { Stats.Instance.Calculate1SkillStat(level, 0.5f) };
                    case SKILL.enchanting:
                        return new float[2] { Stats.Instance.Calculate122SkillStat(level), Stats.Instance.Calculate1SkillStat(level, 0.5f) };
                    case SKILL.alchemy:
                    case SKILL.foraging:
                    case SKILL.mining:
                        return new float[1] { Stats.Instance.Calculate122SkillStat(level) };
                    case SKILL.farming:
                    case SKILL.fishing:
                        return new float[1] { Stats.Instance.Calculate2345SkillStat(level) };

                    default:
                        return new float[1] { Stats.Instance.Calculate1SkillStat(level, 1) };
                }
            } else
            {
                return null;
            }

        }

        public static XP GetSkillXP(JSONNode skillNode, SKILL skillType)
        {
            var skillXP = Global.GetXP(skillNode.AsFloat, GetSkillXPLadder(skillType), 0);
            return AddSkillOffset(skillXP, skillType);
        }

        public static XP AddSkillOffset(XP originalXP, SKILL skill)
        {
            switch (skill)
            {
                case SKILL.enchanting:
                case SKILL.taming:
                    if (originalXP.Level < originalXP.XPLadder.Length)
                        originalXP.Level++;
                    return originalXP;
                default:
                    return originalXP;
            }
        }
        
        public static int[] GetSkillXPLadder(SKILL skillType)
        {
            var skillLaddersTextAsset = Resources.Load<TextAsset>("JSON Data/Skill XP Ladders");
            var skillLadders = JSON.Parse(skillLaddersTextAsset.text)["XPLadders"];

            var jsonLadder = skillLadders["NORMAL"];
            switch (skillType)
            {
                case SKILL.enchanting:
                case SKILL.farming:
                case SKILL.mining:
                    jsonLadder = skillLadders["SIXTY"];
                    break;
                case SKILL.runecrafting:
                    jsonLadder = skillLadders["RUNECRAFTING"];
                    break;

                case SKILL.catacombs:
                case SKILL.berserk:
                case SKILL.mage:
                case SKILL.tank:
                case SKILL.archer:
                case SKILL.healer:
                    jsonLadder = skillLadders["DUNGEONS"];
                    break;
            }

            int[] intLadder = new int[jsonLadder.Count];

            for (int i = 0; i < jsonLadder.Count; i++)
                intLadder[i] = jsonLadder[i].AsInt;

            return intLadder;
        }
	}

}

