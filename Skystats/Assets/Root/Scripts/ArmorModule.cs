using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Helper;

public class ArmorModule : MonoBehaviour
{
    public Transform objParent;

    private void Start()
    {
        Main.Instance.OnLoadProfile += InstantiateModule;

        if (Main.Instance.currentProfile != null && !string.IsNullOrEmpty(Main.Instance.currentProfile.FormattedUsername))
            InstantiateModule(Main.Instance, new OnLoadProfileEventArgs { profile = Main.Instance.currentProfile });
    }

    public void InstantiateModule (object sender, OnLoadProfileEventArgs e)
	{
        bool off = !objParent.gameObject.activeSelf;
        if (off)
            objParent.gameObject.SetActive(true);


        WardrobeSlots equippedArmor = e.profile.EquippedArmorData;
        if (equippedArmor != null)
        {
            for (int i = 0; i < 4; i++)
            {
                var armorSlot = objParent.GetChild(i).GetComponent<Slot>();
                if (equippedArmor.ArmorPieces.Count >= i)
                {
                    if (equippedArmor.ArmorPieces[i] != null && armorSlot != null)
                    {
                        equippedArmor.ArmorPieces[i].ParentSlot = armorSlot;
                        if (equippedArmor.ArmorPieces.Count - 1 >= i)
                            armorSlot.FillItem(equippedArmor.ArmorPieces[i]);
                        else
                            armorSlot.FillItem(new Item());
                    }
                    else
                        armorSlot.FillItem(new Item());
                }
                else
                    armorSlot.FillItem(new Item());
            }
        }

        for (int i = 0; i < 10; i++)
            Global.UpdateCanvasElement(objParent.parent.parent as RectTransform);

        if (off)
            objParent.gameObject.SetActive(false);
    }

}
