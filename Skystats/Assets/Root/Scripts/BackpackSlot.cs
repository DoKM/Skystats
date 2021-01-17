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
    private Transform openBpPreviewHolder;

    private void Start() { }

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
                slot.stackText.GetComponent<Canvas>().sortingLayerName = "Blur";
                slot.stackText.GetComponent<Canvas>().sortingOrder = 4;
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
                newSlot.stackText.GetComponent<Canvas>().sortingLayerName = "Blur";
                newSlot.stackText.GetComponent<Canvas>().sortingOrder = 4;
                newSlot.FillItem(backpack.BackpackContents[i]);
            }
        }

    }

    public void OnClick()
    {
        var infoModule = Global.FindParentWithComponent<InfoModule>(tooltip.GetComponent<Tooltip>().currentDisplayingSlot.transform);
        openBpPreviewHolder = infoModule.backpack;
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
                var cloneSlot = cloneObj.GetComponent<Slot>();
                cloneSlot.FillItem(clone.GetComponent<Slot>().currentHoldingItem);

                cloneSlot.stackText.GetComponent<Canvas>().sortingLayerName = "Default";
                cloneSlot.stackText.GetComponent<Canvas>().sortingOrder = 1;
            }

            var infoList = infoModule.infolist;
            infoList.ActivateMenu(MenuType.Backpack);
            for (int i = 0; i < 3; i++)
                Global.UpdateCanvasElement(infoList.transform as RectTransform);
        }

        ActivateTooltip(false);
    }

}
