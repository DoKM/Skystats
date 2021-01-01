using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Helper;

public class AuctionSlot : Slot
{
    public Auction auction;

    public override void ActivateTooltip(bool active)
    {
        if (active)
        {
            var auctionDescription = Instantiate(Resources.Load<GameObject>("Prefabs/Auction"), tooltipHolder.transform);
            var timestamp = auctionDescription.transform.Find("Timestamp").GetComponent<Timestamp>();
            timestamp.endTime = auction.UnixEndTimestamp;
            timestamp.prefix = "<color=#AAAAAA>Ends in:</color>";
            timestamp.hexTimeColor = "FFFF55";

            var sellerText = auctionDescription.transform.Find("Seller").GetComponent<TMP_Text>();
            var optionalBuyerUsername = ""; //auction.HighestBid.BidderUUID
            sellerText.text = $"<color=#AAAAAA>Seller:</color> {auction.FormattedAuctioneerUsername}{optionalBuyerUsername}";

            var priceText = auctionDescription.transform.Find("Price").GetComponent<TMP_Text>();
            priceText.text = $"<color=#AAAAAA>Price:</color> <color=#FFAA00>{Global.FormatAndRoundAmount(auction.Price)} coins</color>";
        }

        base.ActivateTooltip(active);
    }
}
