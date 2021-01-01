using Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class PetModule : MonoBehaviour
{
    public Transform parent, activePetParent, title;
    private Item activePetItem;
    private Pet activePet;

    private void Start()
    {
        Main.Instance.OnLoadProfile += InstantiateModule;

        if (Main.Instance.currentProfile != null && !string.IsNullOrEmpty(Main.Instance.currentProfile.FormattedUsername))
            InstantiateModule(Main.Instance, new OnLoadProfileEventArgs { profile = Main.Instance.currentProfile });
    }

    public void InstantiateModule(object sender, OnLoadProfileEventArgs e)
    {
        activePet = new Pet();
        if (e.profile.PetData != null)
        {
            foreach (var pet in e.profile.PetData)
                if (pet.IsActive)
                {
                    e.profile.PetData.Remove(activePet);
                    activePet = pet;
                    break;
                }
        }

        activePetItem = Pets.PetToItem(activePet);

        activePet.BonusStats = activePetItem.BonusStats;
        e.profile.ActivePet = activePet;

        InstantiatePets(e.profile);
        Invoke("InstantiateActivePet", 0.1f);

        for (int i = 0; i < 3; i++)
		{
            Global.UpdateCanvasElement(title as RectTransform);
            Global.UpdateCanvasElement(parent as RectTransform);
        }
    }

	private void InstantiateActivePet()
	{
        var wasInactive = activePetParent.gameObject.activeSelf == false;
        if (wasInactive)
            activePetParent.gameObject.SetActive(true);

        var petSlot = activePetParent.Find("Pet slot").GetComponent<PetSlot>();
        petSlot.FillItem(activePetItem);

        var petNameText = activePetParent.Find("PetName").GetComponent<TMP_Text>();
        var petName = Pets.GetFormattedPetName(activePet, false);

        var petTier = Regex.Replace(activePet.PetRarityTier.ToString(), "_", " ");
        if (activePetItem != null && activePetItem.ItemDescription != null)
            petTier = $"<color=#{Global.GetRarityHexColor(activePet.PetRarityTier)}>{Global.ToTitleCase(petTier)}</color> ";
        else
            petTier = $"<color=#999999>None</color>";

        var petSkin = "";
        if (activePet.SkinName != "")
            petSkin = $"<color=#{Global.GetRarityHexColor(activePet.PetRarityTier)}>{activePet.SkinName}</color> ";

        petNameText.text = petTier + petSkin + petName;

        var petLevelText = activePetParent.Find("PetLevel").GetComponent<TMP_Text>();
        if (activePet.PetXP != null)
            petLevelText.text = $"Level {activePet.PetXP.Level}";
        else
            petLevelText.text = "No level";

        Global.UpdateCanvasElement(activePetParent as RectTransform);
        if (wasInactive)
            activePetParent.gameObject.SetActive(false);
    }

	public void InstantiatePets (Profile profile)
	{
        if (parent != null)
		{
            var wasInactive = parent.gameObject.activeSelf == false;
            if (wasInactive)
                parent.gameObject.SetActive(true);
            Main.Instance.ClearChildren(parent);

            GameObject slotObject = Resources.Load<GameObject>("Prefabs/Slot/Pet (0)");

            if (profile.PetData != null)
            {
                var sortedPets = Pets.SortPets(profile.PetData);
                List<Item> sortedPetItems = Pets.PetsToItemList(sortedPets);

                if (sortedPetItems != null)
                {
                    for (int i = 0; i < sortedPetItems.Count; i++)
                    {
                        PetSlot itemSlot = Instantiate(slotObject, parent).GetComponent<PetSlot>();
                        var item = sortedPetItems[i];
                        item.ParentSlot = itemSlot;

                        itemSlot.FillItem(item);
                    }
                }
                else
                {
                    for (int i = 0; i < 24; i++)
                    {
                        PetSlot itemSlot = Instantiate(slotObject, parent).GetComponent<PetSlot>();
                        itemSlot.FillItem(new Item());
                    }
                }
            }
            else
            {
                for (int i = 0; i < 24; i++)
                {
                    PetSlot itemSlot = Instantiate(slotObject, parent).GetComponent<PetSlot>();
                    itemSlot.FillItem(new Item());
                }
            }


            Global.UpdateCanvasElement(parent as RectTransform);
            Global.UpdateScrollView();

            if (wasInactive)
                parent.gameObject.SetActive(false);
        }
    }

}
