using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LeTai.Asset.TranslucentImage;
using Helper;

public enum MenuType
{
    Inventory, 
    Accessories,
    Enderchest,
    Potions,
    Quiver,
    Vault,
    Fishies,
    Candy,
    Backpack
}

public class InformationList : MonoBehaviour
{
    public List<InfoMenu> menuParents;
    public RectTransform main, tabparent;
    public MenuType selectedMenu;

    private void Start()
    {
        ActivateMenu(MenuType.Inventory);
    }

    public void ActivateMenu(MenuType menu)
    {
        var selectedMenu = menuParents.Find(x => x.type == menu);

        for (int i = 0; i < menuParents.Count; i++)
        {
            var enabled = false;
            if (menuParents[i] == selectedMenu)
                enabled = true;

            SetMenu(menuParents[i], enabled);

            Global.UpdateCanvasElement(menuParents[i].transform as RectTransform);
            for (int j = 0; j < 3; j++)
                Global.UpdateCanvasElement(main);
        }
    }

    public void SetMenu (InfoMenu menu, bool enabled) 
	{
        if (menu.tab)
		{
            var tabImage = menu.tab.GetComponent<TranslucentImage>();
            tabImage.color = enabled ? new Color(1, 1, 1, 0.7f) : new Color(0, 0, 0, 0.7f);
            tabImage.spriteBlending = enabled ? 0.9f : 0.3f;
        }
        menu.gameObject.SetActive(enabled);
    }

}
