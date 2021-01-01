using Helper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuctionsModule : MonoBehaviour
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
        var wasInactive = objParent.gameObject.activeSelf;
        if (!wasInactive)
            objParent.gameObject.SetActive(true);

        Main.Instance.ClearChildren(objParent);

        List<Auction> auctions = e.profile.PlayerAuctions;

        if (auctions != null)
        {
            for (int i = 0; i < auctions.Count; i++)
            {
                AuctionSlot itemSlot = Instantiate(Resources.Load<GameObject>("Prefabs/Slot/Auction (0)"), objParent).GetComponent<AuctionSlot>();
                var item = auctions[i].AuctionItem;

                item.ParentSlot = itemSlot;
                itemSlot.auction = auctions[i];
                itemSlot.FillItem(item);
            }
        }
        else
        {
            for (int i = 0; i < 12; i++)
            {
                Slot itemSlot = Instantiate(Resources.Load<GameObject>("Prefabs/Slot/Auction (0)"), objParent).GetComponent<Slot>();
                itemSlot.FillItem(new Item());
            }
        }

        Global.UpdateCanvasElement((RectTransform)objParent);
        Global.UpdateScrollView();

        if (!wasInactive)
            objParent.gameObject.SetActive(false);
    }
}
