using System;
using System.Collections;
using System.Collections.Generic;
using Helper;
using SimpleJSON;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[Serializable]
public class Developer
{
    public Texture2D Head;
    public DateTime LastOnlineDate;
    public KeyValuePair<string, long> LastUpdatedProfile;
    public string UUID;
    public string Username;
}

public class DeveloperDisplay : MonoBehaviour
{
    public RawImage headIcon;
    public TMP_Text txt;
    public Developer dev;

    public void Refresh()
    {
        StartCoroutine(InstantiateDev());
    }

    public void Start()
    {
        StartCoroutine(InstantiateDev());
    }

    public IEnumerator InstantiateDev()
    {
        UpdateDisplay();

        yield return StartCoroutine(GetLastUpdatedProfile(dev.UUID));
        dev.LastOnlineDate = GetLastOnlineDateTime();
        UpdateDisplay();
    }

    public void OnClick()
    {
        Main.Instance.username = dev.Username;
        StartCoroutine(Main.Instance.IE_LoadProfiles());
    }

    public void UpdateDisplay()
    {
        headIcon.texture = dev.Head;
        txt.text = $"{dev.Username}\n<size=16><color=#CCCCCC>Last Online:</color> {Global.GetFormattedDateTime(dev.LastOnlineDate)}</size>";
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }

    public IEnumerator GetLastUpdatedProfile(string uuid)
    {
        dev.LastUpdatedProfile = new KeyValuePair<string, long>();
        var profilesWWW = UnityWebRequest.Get($"https://api.hypixel.net/skyblock/profiles?key={Credentials.key}&uuid={uuid}");
        yield return profilesWWW.SendWebRequest();
        var profiles = JSON.Parse(profilesWWW.downloadHandler.text)["profiles"];

        foreach (var kvp in profiles)
        {
            var profileID = kvp.Value["profile_id"];
            var lastUpdated = kvp.Value["members"][dev.UUID]["last_save"];

            if (lastUpdated > dev.LastUpdatedProfile.Value)
                dev.LastUpdatedProfile = new KeyValuePair<string, long>(profileID, lastUpdated);
        }
    }

    public DateTime GetLastOnlineDateTime()
    {
        var value = dev.LastUpdatedProfile.Value;
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        dateTime = dateTime.AddMilliseconds(value).ToLocalTime();

        return dateTime;
    }

}
