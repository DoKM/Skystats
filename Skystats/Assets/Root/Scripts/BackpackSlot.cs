using Helper;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BackpackSlot : Slot
{
    Transform openBpPreviewHolder;
    private void Start()
    {
        openBpPreviewHolder = Main.Instance.openBpPreviewHolder;
    }
    public override void ActivateTooltip(bool active)
    {
        if (currentHoldingItem.Backpack != null && currentHoldingItem.Backpack.BackpackContents != null)
        {
            FillBackpack(currentHoldingItem.Backpack);
            backpackHolder.transform.parent.gameObject.SetActive(true);
        }
        else
            ClearBackpack();

        base.ActivateTooltip(active);
    }

    private void FillBackpack(Backpack backpack)
    {
        if (backpackHolder.transform.childCount == backpack.BackpackSize)
        {
            for (int i = 0; i < backpackHolder.transform.childCount; i++)
            {
                var slot = backpackHolder.transform.GetChild(i).GetComponent<Slot>();
                slot.FillItem(backpack.BackpackContents[i]);
            }
        }
        else
        {
            ClearChildren(backpackHolder.transform);
            var slotObj = Resources.Load<GameObject>("Prefabs/Slot/Slot (0)");

            for (int i = 0; i < backpack.BackpackSize; i++)
            {
                var newSlot = Instantiate(slotObj, backpackHolder.transform).GetComponent<Slot>();
                newSlot.FillItem(backpack.BackpackContents[i]);
            }
        }

    }

    public void OnClick()
    {
        ClearChildren(openBpPreviewHolder);

        if (backpackHolder != null && backpackHolder.transform.childCount > 0)
        {
            List<GameObject> cloneSlots = new List<GameObject>();
            for (int i = 0; i < backpackHolder.transform.childCount; i++)
                cloneSlots.Add(backpackHolder.transform.GetChild(i).gameObject);

            foreach (var clone in cloneSlots)
            {
                var cloneObj = Instantiate(clone, openBpPreviewHolder);
                cloneObj.transform.localScale = Vector3.one;
                cloneObj.GetComponent<Slot>().FillItem(clone.GetComponent<Slot>().currentHoldingItem);
            }

            var infoList = Global.FindParentWithComponent<InformationList>(transform);
            infoList.ActivateMenu(MenuType.Backpack);
        }
    }

}
