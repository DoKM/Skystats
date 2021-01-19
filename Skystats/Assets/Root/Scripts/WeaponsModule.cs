using Helper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponsModule : MonoBehaviour
{
    public Transform objParent;

    private void Start()
    {
        Main.Instance.OnLoadProfile += InstantiateModule;

        if (Main.Instance.currentProfile != null && Main.Instance.currentProfile != new Profile())
            InstantiateModule(Main.Instance, new OnLoadProfileEventArgs { profile = Main.Instance.currentProfile });
    }

    public void InstantiateModule(object sender, OnLoadProfileEventArgs e)
    {
        Main.Instance.ClearChildren(objParent);

        List<Item> sortedWeapons = Items.SortItems(e.profile.Weapons);
        GameObject slotObject = Resources.Load<GameObject>("Prefabs/Slot/Weapon (0)");

        if (sortedWeapons != null)
        {
            for (int i = 0; i < sortedWeapons.Count; i++)
            {
                Slot itemSlot = Instantiate(slotObject, objParent).GetComponent<Slot>();

                var item = sortedWeapons[i];
                item.ParentSlot = itemSlot;
                itemSlot.FillItem(item);
            }
        }
        else
        {
            for (int i = 0; i < 12; i++)
            {
                Slot itemSlot = Instantiate(slotObject, objParent).GetComponent<Slot>();
                itemSlot.FillItem(new Item());
            }
        }

        if (e.profile != null && e.profile.ActiveWeapon != null)
        {
            Slot weaponSlot = e.profile.ActiveWeapon.ParentSlot;

            if (weaponSlot != null)
                weaponSlot.ToggleActive();
        }
        
		for (int i = 0; i < 3; i++)
		    Global.UpdateCanvasElement(objParent as RectTransform);
    }
}
