using Helper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WardrobeModule : MonoBehaviour
{
    public Transform parentObj, title;

    private void Start()
    {
        Main.Instance.OnLoadProfile += InstantiateModule;

        if (Main.Instance.currentProfile != null && !string.IsNullOrEmpty(Main.Instance.currentProfile.FormattedUsername))
            InstantiateModule(Main.Instance, new OnLoadProfileEventArgs { profile = Main.Instance.currentProfile });
    }

    public void InstantiateModule(object sender, OnLoadProfileEventArgs e)
    {
        Main.Instance.ClearChildren(parentObj);

        var wardrobeData = e.profile.WardrobeData;
        if (wardrobeData != null)
        {
            foreach (var currentSlot in wardrobeData)
            {
                if (currentSlot != null)
                {
                    var newWardrobeSlotObj = Instantiate(Resources.Load<GameObject>("Prefabs/Slot/Wardrobe Slot (0)"), parentObj).transform;
                    for (int i = 0; i < 4; i++)
                    {
                        var armorSlot = newWardrobeSlotObj.GetChild(i).GetComponent<WardrobeSlot>();

                        if (currentSlot.ArmorPieces.Count >= i && currentSlot.ArmorPieces[i] != null && armorSlot != null)
                        {
                            currentSlot.ArmorPieces[i].ParentSlot = armorSlot;
                            if (currentSlot.ArmorPieces.Count - 1 >= i)
                                armorSlot.FillItem(currentSlot.ArmorPieces[i]);
                            else
                                armorSlot.FillItem(new Item());
                        }
                        else
                            armorSlot.FillItem(new Item());
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < 16; i++)
            {
                var newWardrobeSlotObj = Instantiate(Resources.Load<GameObject>("Prefabs/Slot/Wardrobe Slot (0)"), parentObj).transform;
                for (int j = 0; j < 4; j++)
                {
                    var armorSlot = newWardrobeSlotObj.GetChild(j).GetComponent<WardrobeSlot>();
                    armorSlot.FillItem(new Item());
                }
            }
        }

        for (int i = 0; i < 3; i++)
            Global.UpdateCanvasElement(title as RectTransform);
    }
}
