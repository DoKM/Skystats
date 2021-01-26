using Helper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoModule : MonoBehaviour
{
    public Transform inventoryParent, quiverParent, potionsParent, fishParent, vaultParent, candyParent,
                     accessoryParent, enderchestParent, backpack;
    public InformationList infolist;
    public GameObject apiOff;
    
    private void Start()
    {
        Main.Instance.OnLoadProfile += InstantiateModule;
        Main.Instance.OnInventoryAPIOff += ToggleAPI;

        if (Main.Instance.currentProfile != null && !string.IsNullOrEmpty(Main.Instance.currentProfile.FormattedUsername))
            InstantiateModule(Main.Instance, new OnLoadProfileEventArgs { profile = Main.Instance.currentProfile });
    }
    
    public void ToggleAPI(object sender, OnInventoryAPIOffArgs e)
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
        var profile = e.profile;

        InstantiateModule(profile.InventoryData, inventoryParent);
        InstantiateModule(profile.QuiverData, quiverParent);
        InstantiateModule(profile.PotionData, potionsParent);
        InstantiateModule(profile.FishingBagData, fishParent);
        InstantiateModule(profile.VaultData, vaultParent);
        InstantiateModule(profile.CandyData, candyParent);

        InstantiateTalismanBagModule(profile, accessoryParent);
        InstantiateEnderchest(profile, enderchestParent);

        infolist.ActivateMenu(MenuType.Inventory);
        for (int i = 0; i < 4; i++)
            Global.UpdateInfoList();
    }

    public void InstantiateEnderchest(Profile profile, Transform objParent)
    {
        objParent.gameObject.SetActive(true);
        Main.Instance.ClearChildren(objParent);

        List<Item> enderchestData = profile.EnderchestData;
        var slotObject = Resources.Load<GameObject>("Prefabs/Slot/Slot (0)");

        Transform currentPage = null;
        var pageIndex = 0;

        if (enderchestData != null)
        {
            for (int i = 0; i < enderchestData.Count; i++)
            {
                if (i % 45 == 0)
                {
                    if (currentPage != null && pageIndex > 1)
                        currentPage.gameObject.SetActive(false);
                    else if (pageIndex == 1)
                        currentPage.gameObject.SetActive(true);

                    currentPage = Instantiate(Resources.Load<GameObject>("Prefabs/Slot/Page"), objParent).transform;
                    pageIndex++;
                }

                Slot itemSlot =
                (enderchestData[i].Backpack != null && enderchestData[i].Backpack.BackpackSize > 0)
                    ? Instantiate(Resources.Load<GameObject>("Prefabs/Slot/Backpack (0)"), currentPage).GetComponent<BackpackSlot>()
                    : Instantiate(Resources.Load<GameObject>("Prefabs/Slot/Slot (0)"), currentPage).GetComponent<Slot>();

                var item = enderchestData[i];
                item.ParentSlot = itemSlot;
                itemSlot.FillItem(item);
            }

            currentPage.gameObject.SetActive(false);
            enderchestParent.GetComponent<Enderchest>().selectedPage = 0;
        }
        else
        {
            currentPage = Instantiate(Resources.Load<GameObject>("Prefabs/Slot/Page"), objParent).transform;

            for (int i = 0; i < 45; i++)
            {
                Slot itemSlot = Instantiate(slotObject, currentPage).GetComponent<Slot>();
                itemSlot.FillItem(new Item());
            }
        }

        Global.UpdateCanvasElement(objParent as RectTransform);
    }

    public void InstantiateTalismanBagModule(Profile profile, Transform objParent)
    {
        objParent.gameObject.SetActive(true);
        Main.Instance.ClearChildren(objParent);

        List<Item> sortedAccessories = Items.SortItems(profile.AccessoryData);
        GameObject slotObject = Resources.Load<GameObject>("Prefabs/Slot/Slot (0)");

        if (sortedAccessories != null)
        {
            for (int i = 0; i < sortedAccessories.Count; i++)
            {
                Slot itemSlot = Instantiate(slotObject, objParent).GetComponent<Slot>();
                var item = sortedAccessories[i];
                item.ParentSlot = itemSlot;

                itemSlot.FillItem(item);
            }
        }
        else
        {
            for (int i = 0; i < 28; i++)
            {
                Slot itemSlot = Instantiate(slotObject, objParent).GetComponent<Slot>();
                itemSlot.FillItem(new Item());
            }
        }

        Global.UpdateCanvasElement(objParent as RectTransform);
    }


    private void InstantiateModule(List<Item> data, Transform objParent)
    {
        if (objParent != null)
            objParent.gameObject.SetActive(true);
        Main.Instance.ClearChildren(objParent);

        if (data != null)
        {
            for (int i = 0; i < data.Count; i++)
            {
                Slot itemSlot = (data[i].Backpack != null && data[i].Backpack.BackpackSize > 0)
                         ? itemSlot = Instantiate(Resources.Load<GameObject>("Prefabs/Slot/Backpack (0)"), objParent).GetComponent<BackpackSlot>()
                         : itemSlot = Instantiate(Resources.Load<GameObject>("Prefabs/Slot/Slot (0)"), objParent).GetComponent<Slot>();

                if (data[i] != null)
                {
                    var item = data[i];
                    item.ParentSlot = itemSlot;
                    itemSlot.FillItem(item);
                }
                else
                    itemSlot.FillItem(new Item());
            }
        }
        else
        {
            for (int i = 0; i < 36; i++)
            {
                Slot itemSlot = Instantiate(Resources.Load<GameObject>("Prefabs/Slot/Slot (0)"), objParent).GetComponent<Slot>();
                itemSlot.FillItem(new Item());
            }
        }
    }


}
