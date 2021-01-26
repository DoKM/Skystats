using System;
using System.Collections;
using System.Collections.Generic;
using Discord;
using UnityEngine;

public class DiscordHandler : MonoBehaviour
{
    public bool log;
    public static DiscordHandler Instance;
    public void OnEnable()
    {
        if (!Instance) Instance = this;
        else Destroy(this);
    }

    private Discord.Discord discord;
    public Activity currentActivity;

    private void Start()
    {
        discord = new Discord.Discord(ClientID.ID, (System.Int64)Discord.CreateFlags.Default);
        InvokeRepeating(nameof(UpdateTime), 1, 1);
    }

    public void UpdateActivity(string state, string details)
    {
        var activityManager = discord.GetActivityManager();
        currentActivity = new Activity
        {
            State = state,
            Details = details,
            Assets =
            {
                LargeImage = "logo_skystats",
                LargeText = "Skystats"
            },
            Instance = true
        };
        
        activityManager.UpdateActivity(currentActivity, result =>
        {
            if (log)
            {
                if (result == Result.Ok)
                    Debug.Log("Updated status!");
                else 
                    Debug.Log("Updating status failed :(");
            }
        });
    }

    private void UpdateTime()
    {
        var time = DateTime.Now - Main.Instance.startDateTime;
        var str = $"{time.Minutes:00}:{time.Seconds:00} elapsed";
        if (time.Hours != 0)
            str = str.Insert(0, $"{time.Hours:00}:");
        
        if (!string.IsNullOrEmpty(Main.Instance.username) && currentActivity.Details != "Idle")
            currentActivity.Details = Main.Instance.showCurrentViewingProfile 
                ? $"On {Main.Instance.username}'s profile" 
                : $"On someone's profile";
        
        UpdateActivity(str, currentActivity.Details);
    }

    private void Update()
    {
        if (discord != null)
            discord.RunCallbacks();
    }

    private void OnApplicationQuit()
    {
        if (discord != null)
            discord.Dispose();
    }
}
