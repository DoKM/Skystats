using Helper;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[Serializable]
public class Favorite
{
    public Texture2D Head;
    public DateTime LastOnlineDate;
    public KeyValuePair<string, long> LastUpdatedProfile;
    public string DirString;
    public string UUID;
    public string Username;
}

public class FavoriteDisplay : MonoBehaviour
{
    public RawImage headIcon;
    public TMP_Text txt;
    public Favorite favorite;

    public void Refresh ()
	{
        StartCoroutine(InstantiateFavorite());
	}

    public IEnumerator InstantiateFavorite()
    {
        LoadFavorite();
        UpdateDisplay();

        yield return StartCoroutine(GetLastUpdatedProfile(favorite.Username));
        favorite.LastOnlineDate = GetLastOnlineDateTime();
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        headIcon.texture = favorite.Head;
        txt.text = $"{favorite.Username}\n<size=16><color=#CCCCCC>Last Online:</color> {Global.GetFormattedDateTime(favorite.LastOnlineDate)}</size>";
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }

    public void SaveFavorite()
    {
        var favDir = new DirectoryInfo(favorite.DirString);

        if (!favDir.Exists)
            favDir.Create();

        File.WriteAllBytes($"{favorite.DirString}/headTexture.png", favorite.Head.EncodeToPNG());
        File.WriteAllText($"{favorite.DirString}/uuid.txt", favorite.UUID);
        File.WriteAllText($"{favorite.DirString}/username.txt", favorite.Username);
    }

    public IEnumerator GetHead(string trimmedUUID)
    {
        var headWWW = UnityWebRequestTexture.GetTexture($"https://crafatar.com/renders/head/{trimmedUUID}?overlay=true");
        yield return headWWW.SendWebRequest();

        while (!headWWW.isDone)
            yield return null;

        if (!headWWW.isHttpError && !headWWW.isNetworkError)
            favorite.Head = ((DownloadHandlerTexture)headWWW.downloadHandler).texture;
    }

    public void LoadFavorite()
    {
        if (File.Exists($"{favorite.DirString}/uuid.txt"))
            favorite.UUID = File.ReadAllText($"{favorite.DirString}/uuid.txt");

        if (File.Exists($"{favorite.DirString}/username.txt"))
            favorite.Username = File.ReadAllText($"{favorite.DirString}/username.txt");

        if (File.Exists($"{favorite.DirString}/headTexture.png"))
        {
            var tex = new Texture2D(1, 1);
            tex.LoadImage(File.ReadAllBytes($"{favorite.DirString}/headTexture.png"));
            favorite.Head = tex;
        }
        else
            StartCoroutine(GetHead(favorite.UUID));
    }

    public void DeleteFavorite()
    {
        if (Main.Instance.favoriteUsernames.Contains(favorite.Username))
            Main.Instance.favoriteUsernames.Remove(favorite.Username);

        // Check if we're not deleting anything that's outside of our data folder.
        if (favorite.DirString.StartsWith(Application.persistentDataPath))
            Directory.Delete(favorite.DirString, true);

        Destroy(gameObject);
    }

    public IEnumerator GetLastUpdatedProfile(string username)
    {
        favorite.LastUpdatedProfile = new KeyValuePair<string, long>();
        var profilesWWW = UnityWebRequest.Get($"https://api.hypixel.net/skyblock/profiles?key={Credentials.key}&uuid={favorite.UUID}");
        yield return profilesWWW.SendWebRequest();
        var profiles = JSON.Parse(profilesWWW.downloadHandler.text)["profiles"];

        foreach (var kvp in profiles)
        {
            var profileID = kvp.Value["profile_id"];
            var lastUpdated = kvp.Value["members"][favorite.UUID]["last_save"];

            if (lastUpdated > favorite.LastUpdatedProfile.Value)
                favorite.LastUpdatedProfile = new KeyValuePair<string, long>(profileID, lastUpdated);
        }
    }

    public DateTime GetLastOnlineDateTime()
    {
        var value = favorite.LastUpdatedProfile.Value;
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        dateTime = dateTime.AddMilliseconds(value).ToLocalTime();

        return dateTime;
    }
}
