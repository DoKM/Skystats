using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timestamp : MonoBehaviour
{
    public long endTime;
    public string prefix;
    public string suffix;
    public string hexTimeColor;

    private bool expired;
    private TMP_Text text;

    private void OnEnable()
    {
        text = GetComponent<TMP_Text>();
        InvokeRepeating(nameof(UpdateTimer), 0, 1);
    }

    public void UpdateTimer()
    {
        var time = GetTimeUntil(endTime);
        if (expired)
            text.text = $"<color=#FFFF55> Unclaimed!</color>{suffix}";
        else
            text.text = $"{prefix}<color=#{hexTimeColor}> {time}</color>{suffix}";
    }

    public string GetTimeUntil(long unixTimestamp)
    {
        DateTime daysLeft = Helper.Global.UnixTimeStampToDateTime(unixTimestamp);
        DateTime startDate = DateTime.Now;
        TimeSpan timeSpan = daysLeft - startDate;

        expired = startDate > daysLeft;

        return $"{timeSpan.Days}d {timeSpan.Hours}h {timeSpan.Minutes}m {timeSpan.Seconds}s";
    }


}
