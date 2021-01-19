using fNbt;
using JetBrains.Annotations;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Advertisements.Purchasing;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.UI.Michsky.UI.ModernUIPack;
using Helper;
using LeTai.Asset.TranslucentImage;

public enum SKINVERSION
{
    OLD, // 64x32 skins
    NEW, // 64x64 skins
    SLIM // 64x64 skins, but with slim arms
}

public enum RARITY
{
    VERY_SPECIAL,
    SPECIAl,
    SUPREME,
    MYTHIC,
    LEGENDARY,
    EPIC,
    RARE,
    UNCOMMON,
    COMMON,
    UNKNOWN
}

public class OnLoadProfileEventArgs : EventArgs
{
    public Profile profile;
}

public class Main : MonoBehaviour
{
    #region Singleton
    public static Main Instance;
    [HideInInspector] public Gradient selectedProfileGradient;
    private void OnEnable()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    #endregion

    #region Variables
    public event EventHandler<OnLoadProfileEventArgs> OnLoadProfile;
    public Shader transparentCutout;
    public Shader transparentCutoutMask;
    public bool helpTooltips { get; set; }
    public Profile currentProfile;

    private Dictionary<string, TMP_InputField> usernameInput = new Dictionary<string, TMP_InputField>();
    private Dictionary<string, string> profiles = new Dictionary<string, string>(); // Profile id, cute name
    public List<string> favoriteUsernames = new List<string>();

    private Transform dungeonsParent,
                      activePetParent, petParent, weaponParent, favoriteParent, wardrobeParent,
                      slayerParent;
    private Animator load;
    private TMP_InputField nameInputField;
    private KeyValuePair<string, long> lastUpdatedProfile = new KeyValuePair<string, long>("", 0);
    private JSONNode currentProfileData;

    [HideInInspector] public GameObject input, profileHolder;
    [HideInInspector] public Transform openBpPreviewHolder;
    [HideInInspector] public string username, formattedUsername, profileID, uuid;

    #endregion

    #region Initialization

    private void Awake()
    {
        //PlayerPrefs.DeleteKey("m_k");
        Credentials.key = Credentials.LoadKey();
        GameObject.FindGameObjectWithTag("KeyInput").GetComponent<TMP_InputField>().text = Credentials.key;

        CreateDirs();
        GetParents();

        var tempInputs = GameObject.FindGameObjectsWithTag("Input");
        usernameInput.Clear();
        for (int i = 0; i < tempInputs.Length; i++)
            usernameInput.Add(tempInputs[i].name, tempInputs[i].GetComponent<TMP_InputField>());

        StartCoroutine(UpdateFavorites());
    }

    private void GetParents()
    {
        profileHolder = GameObject.FindGameObjectWithTag("Profile Holder");
        favoriteParent = GameObject.FindGameObjectWithTag("FavoriteParent").transform;
        input = GameObject.FindGameObjectWithTag("Input");

        load = GameObject.FindGameObjectWithTag("Load").GetComponent<Animator>();
    }

    private void CreateDirs()
    {
        CreateDir("saved textures");
        CreateDir("nbt data");

        CreateDir("favorites");
        CreateDir("backgrounds");
    }

    private void CreateDir(string path)
    {
        DirectoryInfo dir = new DirectoryInfo($"{Application.persistentDataPath}/{path}");
        if (!dir.Exists) dir.Create();
    }

    private void Start()
    {
        selectedProfileGradient = GradientManager.Instance.GetCurrentThemeColor(GradientType.maxColor).Color;

        Global.UpdateScrollView();
        Global.UpdateCanvasElement(favoriteParent as RectTransform);
    }
    #endregion

    #region Key
    public void UpdateKeyInputField(TMP_InputField field)
    {
        Credentials.UpdateKey(field.text);
        RefreshCurrentFavorites();
    }
    #endregion

    #region Favorites

    public void RefreshCurrentFavorites ()
	{
		for (int i = 0; i < favoriteParent.childCount; i++)
		{
            var fav = favoriteParent.GetChild(i);

            if (fav.GetComponent<FavoriteDisplay>())
                fav.GetComponent<FavoriteDisplay>().Refresh();
            else if (fav.GetComponent<DeveloperDisplay>())
                fav.GetComponent<DeveloperDisplay>().Refresh();
        }
	}

    public IEnumerator UpdateFavorites()
    {
        var drInfo = new DirectoryInfo($"{Application.persistentDataPath}/favorites");
        var dirs = drInfo.GetDirectories();

        for (int i = 0; i < dirs.Length; i++)
        {
            if (dirs[i].Attributes.HasFlag(FileAttributes.Directory))
                favoriteUsernames.Add(dirs[i].Name);
        }

        ClearChildren(favoriteParent);
        if (favoriteUsernames != null && favoriteUsernames.Count > 0)
            foreach (var favoriteUsername in favoriteUsernames)
                StartCoroutine(InstantiateFavorite(favoriteUsername, dirs.First(x => x.Name == favoriteUsername), false));

        Global.UpdateCanvasElement(favoriteParent as RectTransform);
        Global.UpdateCanvasElement(favoriteParent.parent as RectTransform);
        yield return null;
    }

    public void AddFavoriteCurrentUser()
    {
        if (!favoriteUsernames.Contains(username))
        {
            favoriteUsernames.Add(username);
            StartCoroutine(InstantiateFavorite(username, null, true));
        }
    }

    private IEnumerator InstantiateFavorite(string username, DirectoryInfo path, bool save)
    {
        var favoriteObject = Instantiate(Resources.Load<GameObject>("Prefabs/Favorite"), favoriteParent);
        favoriteObject.name = Global.ToTitleCase(username);

        var uuid = path != null ? Global.ReadFileFromPath(path, "uuid") : this.uuid;
        var favoriteDisplay = favoriteObject.GetComponent<FavoriteDisplay>();
        var favorite = new Favorite
        {
            Username = username,
            UUID = uuid,
            DirString = $"{Application.persistentDataPath}/favorites/" + username
        };

        favoriteDisplay.favorite = favorite;

        yield return favoriteDisplay.StartCoroutine(favoriteDisplay.GetHead(uuid));
        yield return favoriteDisplay.StartCoroutine(favoriteDisplay.InstantiateFavorite());
        if (save) favoriteDisplay.SaveFavorite();

        var favButton = favoriteObject.GetComponent<Button>();
        favButton.onClick.RemoveAllListeners();
        favButton.onClick.AddListener(() => StartCoroutine(LoadFavorite(username)));

        Global.UpdateCanvasElement(favoriteParent as RectTransform);
    }

    public IEnumerator LoadFavorite(string username)
    {
        this.username = username;
        yield return StartCoroutine(IE_LoadProfiles());
    }
    #endregion

    #region Get User

    public void ChangeUser(string selectedField)
    {
        if (string.IsNullOrEmpty(Credentials.key))
            ErrorHandler.Instance.Push(new Error { ErrorCode = 513, ErrorHeader = "No API key found", ErrorMessage = "Please enter your API key in the Settings menu." });
        else
        {
            string field = selectedField;
            if (selectedField.Contains("^"))
                field = selectedField.Replace("^", "");

            if (!usernameInput.ContainsKey(field))
                Debug.Log("FIELD " + field + " DOES NOT EXIST, PLEASE ENTER A NEW NAME");

            nameInputField = usernameInput[field];
            var input = Regex.Replace(nameInputField.text, "[^a-zA-Z0-9_]", "");

            if (username != null && input.ToLower() == username.ToLower())
                return;

            username = input;
            nameInputField.text = username;

            StartCoroutine(IE_LoadProfiles());
        }
    }

    public IEnumerator IE_LoadProfiles()
    {
        if (string.IsNullOrEmpty(Credentials.key))
            ErrorHandler.Instance.Push(new Error { ErrorCode = 513, ErrorHeader = "No API key found", ErrorMessage = "Please enter your API key in the Settings menu." });
        else
        {
            bool hasError = false;
            load.SetTrigger("Load");
            yield return StartCoroutine(IE_GetUUID((x) => uuid = x, (y) => hasError = y));
            if (!hasError)
            {
                Debug.Log("Starting skin change...");
                yield return StartCoroutine(IE_GetSkinTexture((x) => hasError = x));

                if (!hasError)
                {
                    yield return StartCoroutine(IE_GetProfiles((x) => hasError = x));
                    if (uuid != null && !hasError)
                    {
                        yield return StartCoroutine(IE_ReloadLastUpdatedProfile((x) => hasError = x));
                        if (!hasError)
                        {
                            yield return StartCoroutine(IE_InstantiateProfiles(profiles));
                            yield return StartCoroutine(IE_StartReload(lastUpdatedProfile.Key));
                        }
                        else
                        {
                            ErrorHandler.Instance.Push(new Error { ErrorCode = 517, ErrorHeader = "Error loading profile", ErrorMessage = "Please try another profile." });
                            load.SetTrigger("Stop Load");
                        }
                    }
                    else
                    {
                        ErrorHandler.Instance.Push(new Error { ErrorCode = 517, ErrorHeader = "Error loading profile", ErrorMessage = "Please try another profile." });
                        load.SetTrigger("Stop Load");
                    }
                }
                else
                {
                    ErrorHandler.Instance.Push(new Error { ErrorCode = 518, ErrorHeader = "Error getting player skin", ErrorMessage = "Please try another profile." });
                    load.SetTrigger("Stop Load");
                }
            }
            else
            {
                ErrorHandler.Instance.Push(new Error { ErrorCode = 519, ErrorHeader = "Error getting player UUID", ErrorMessage = "Please try another user." });
                load.SetTrigger("Stop Load");
            }

        }

        Debug.Log("Done: IE_LoadProfiles");
    }

    private IEnumerator IE_GetUUID(Action<string> uuid, Action<bool> hasError)
    {
        if (username != null)
        {
            // Get user's uuid from username
            var uuidReq = UnityWebRequest.Get($"http://api.minetools.eu/uuid/{username}");
            yield return uuidReq.SendWebRequest();
            if (!uuidReq.isNetworkError && !uuidReq.isHttpError && JSON.Parse(uuidReq.downloadHandler.text)["id"] != null)
            {
                uuid(JSON.Parse(uuidReq.downloadHandler.text)["id"]);
                hasError(false);
            }
            else
            {
                ErrorHandler.Instance.Push(new Error { ErrorCode = 502, ErrorHeader = "Unable to process UUID", ErrorMessage = $"No UUID found with user \"{username}\", please try another user." });
                hasError(true);
                load.SetTrigger("Stop Load");
            }

        }
        Debug.Log("Done: IE_GetUUID");
    }

    private IEnumerator IE_GetSkinTexture(Action<bool> hasError)
    {
        // Get texture link
        var skinWWW = UnityWebRequest.Get($"https://sessionserver.mojang.com/session/minecraft/profile/{uuid}");
        yield return skinWWW.SendWebRequest();

        if (skinWWW.isHttpError || skinWWW.isNetworkError)
        {
            hasError(false);
            ErrorHandler.Instance.Push(new Error { ErrorCode = 514, ErrorHeader = "Unable to reach Mojang servers", ErrorMessage = $"The Mojang servers are experiencing issues, please try again in a few minutes." });
            load.SetTrigger("Stop Load");
        }
        else
        {
            if (JSON.Parse(skinWWW.downloadHandler.text) == null)
            {
                hasError(false);
                ErrorHandler.Instance.Push(new Error { ErrorCode = 514, ErrorHeader = "Unable find ", ErrorMessage = $"The Mojang servers are experiencing issues, please try again in a few minutes." });
                load.SetTrigger("Stop Load");
            }
            else
            {
                var profileData = Parsing.Base64ToJSON(JSON.Parse(skinWWW.downloadHandler.text)["properties"][0]["value"].Value);
                var skinData = profileData["textures"]["SKIN"];
                var textureURL = skinData["url"].Value;

                // Get the actual texture and apply it
                var textureWWW = UnityWebRequestTexture.GetTexture($"https://api.allorigins.win/raw?url={textureURL}");
                yield return textureWWW.SendWebRequest();

                if (textureWWW.isHttpError || textureWWW.isNetworkError)
                {
                    hasError(true);
                    ErrorHandler.Instance.Push(new Error { ErrorCode = 507, ErrorHeader = "Unable to get user skin texture", ErrorMessage = $"User \"{username}\" is invalid, please try another user." });
                    load.SetTrigger("Stop Load");
                }
                else
                {
                    hasError(false);
                    Material mat = Resources.Load<Material>("Materials/skin_mat");

                    Texture2D texture = ((DownloadHandlerTexture)textureWWW.downloadHandler).texture;
                    texture.filterMode = FilterMode.Point;
                    texture = Global.ScalePointTexture(texture);
                    mat.mainTexture = texture;
                }
            }
        }

        Debug.Log("Done: IE_GetSkinTexture");
    }

    /// <summary>
    /// Get the user's profiles
    /// </summary>
    /// <param name="hasError"></param>
    /// <returns></returns>
    private IEnumerator IE_GetProfiles(Action<bool> hasError)
    {
        Debug.Log("Start: IE_GetProfiles");
        // Clear profiles so we don't load the wrong profiles
        profiles.Clear();

        var profileWWW = UnityWebRequest.Get($"https://api.hypixel.net/player?key={Credentials.key}&uuid={uuid}");
        yield return profileWWW.SendWebRequest();

        if (!profileWWW.isNetworkError && !profileWWW.isHttpError)
        {
            // Get user's skyblock profiles
            var profileJSON = JSON.Parse(profileWWW.downloadHandler.text);
            if (profileJSON != null && profileJSON.HasKey("player") && profileJSON["player"].HasKey("stats") && profileJSON["player"]["stats"].HasKey("SkyBlock") && profileJSON["player"]["stats"]["SkyBlock"].HasKey("profiles"))
            {
                username = profileJSON["player"]["displayname"].Value;
                var rankUsername = Regex.Replace($"{Ranks.GetFormattedName(profileJSON["player"])}", "&", "§");
                formattedUsername = Formatting.FormatColorString(rankUsername);

                var profileData = profileJSON["player"]["stats"]["SkyBlock"]["profiles"];
                for (int i = 0; i < profileData.Count; i++)
                    profiles.Add(profileData[i]["profile_id"], profileData[i]["cute_name"]);
                hasError(false);
            }
            else
            {
                hasError(true);
                ErrorHandler.Instance.Push(new Error { ErrorCode = 504, ErrorHeader = "No Skyblock profiles found", ErrorMessage = $"User \"{username}\" has no valid Skyblock profiles, please select a valid user." });
                load.SetTrigger("Stop Load");
            }

        }
        else
        {
            hasError(true);
            ErrorHandler.Instance.Push(new Error { ErrorCode = 503, ErrorHeader = "No Skyblock profiles found", ErrorMessage = $"User \"{username}\" has no valid Skyblock profiles, please select a valid user." });
            load.SetTrigger("Stop Load");
        }
        
        Debug.Log("Done: IE_GetProfiles");
    }

    /// <summary>
    /// Reload the profile the user has last logged in to
    /// </summary>
    /// <param name="hasError"></param>
    /// <returns></returns>
    private IEnumerator IE_ReloadLastUpdatedProfile(Action<bool> hasError)
    {
        if (profiles.Count > 0)
        {
            // Send request
            lastUpdatedProfile = new KeyValuePair<string, long>();
            var profilesWWW = UnityWebRequest.Get($"https://api.hypixel.net/skyblock/profiles?key={Credentials.key}&uuid={uuid}");
            yield return profilesWWW.SendWebRequest();
            
            if (!profilesWWW.isNetworkError && !profilesWWW.isHttpError)
            {
                var profiles = JSON.Parse(profilesWWW.downloadHandler.text);
#if UNITY_EDITOR
                File.WriteAllText(Application.dataPath + "/profilesData.txt", profiles.ToString());
#endif
                // Add profiles to list
                foreach (var kvp in profiles["profiles"])
                {
                    var profileID = kvp.Value["profile_id"];
                    var lastUpdated = kvp.Value["members"][uuid]["last_save"];

                    if (lastUpdated > lastUpdatedProfile.Value)
                        lastUpdatedProfile = new KeyValuePair<string, long>(profileID, lastUpdated);
                }
                hasError(false);
            }
            else
            {
                load.SetTrigger("Stop Load");
                hasError(true);
                ErrorHandler.Instance.Push(new Error { ErrorCode = 508, ErrorHeader = "No Skyblock profiles found", ErrorMessage = $"User \"{username}\" has no valid Skyblock profiles, please select a valid user." });
            }
        }

        Debug.Log($"Done: IE_ReloadLastUpdatedProfile, Selected profile:{lastUpdatedProfile.Key}");
    }

    
    /// <summary>
    /// Instantiate the profile prefabs
    /// </summary>
    /// <param name="profiles"></param>
    /// <returns></returns>
    private IEnumerator IE_InstantiateProfiles(Dictionary<string, string> profiles)
    {
        yield return null;
        // Load prefab
        var profilePrefab = Resources.Load<GameObject>("Profile Prefab");

        // Clear its children
        for (int i = 0; i < profileHolder.transform.childCount; i++)
            Destroy(profileHolder.transform.GetChild(i).gameObject);

        if (profiles.Count > 0)
        {
            // Loop through profiles
            for (int i = 0; i < profiles.Keys.Count; i++)
            {
                // Instantiate prefab
                GameObject newProfile = Instantiate(profilePrefab, profileHolder.transform);

                newProfile.name = profiles.Keys.ToList()[i];

                // Correct its values
                newProfile.GetComponentInChildren<TMP_Text>().text = profiles.Values.ElementAt(i);
                newProfile.GetComponent<TranslucentImage>().source = FindObjectOfType<TranslucentImageSource>();
                newProfile.GetComponent<TranslucentImage>().spriteBlending = 0.3f;

                newProfile.GetComponent<Button>().onClick.RemoveAllListeners();
                newProfile.GetComponent<Button>().onClick.AddListener(
                    delegate
                    {
                        Refresh(newProfile.name);
                    });

                newProfile.GetComponent<Button>().onClick.AddListener(
                    delegate
                    {
                        Global.ChangeTranslucantImage(0.8f, newProfile);
                    });
            }
        }
        Debug.Log("Done: IE_InstantiateProfiles");
    }

    /// <summary>
    /// Enable a single profile button, and disable the rest
    /// </summary>
    /// <param name="selectedProfileID"></param>
    public void ToggleButtons(string selectedProfileID)
    {
        for (int i = 0; i < profileHolder.transform.childCount; i++)
            profileHolder.transform.GetChild(i).GetComponent<Button>().interactable = !(profileHolder.transform.GetChild(i).name == selectedProfileID);
    }

    /// <summary>
    /// Start the reload of the selected profile
    /// </summary>
    /// <param name="selectedProfile"></param>
    /// <returns></returns>
    public IEnumerator IE_StartReload(string selectedProfile)
    {
        profileID = selectedProfile;
        ToggleButtons(selectedProfile);

        // Load the correct profile
        Global.SelectProfiles(selectedProfile, profiles, selectedProfileGradient);
        yield return StartCoroutine(IE_ReloadData(profileID));
        load.SetTrigger("Stop Load");

        // Change the username input fields to the correct username
        foreach (var field in usernameInput)
            if (field.Value != null)
                field.Value.SetTextWithoutNotify(username);
    }

    public void Refresh(string profile)
    {
        load.SetTrigger("Load");
        if (profile != null)
            StartCoroutine(IE_StartReload(profile));
    }

    public void Refresh()  {  Refresh(profileID);  }

    #endregion

    #region Main Methods

    /// <summary>
    /// Retrieve the profile data and create/fill a new profile object
    /// </summary>
    /// <param name="profileID"></param>
    /// <returns></returns>
    private IEnumerator IE_ReloadData(string profileID)
    {
        // Send request
        var selectedProfileWWW = UnityWebRequest.Get($"https://api.hypixel.net/skyblock/profile?key={Credentials.key}&profile={profileID}");
        yield return selectedProfileWWW.SendWebRequest();

        JSONNode selectedProfileData = JSON.Parse(selectedProfileWWW.downloadHandler.text);

        if (profileID != null && selectedProfileData != null)
        {
            // Check if the profile exists
            if (selectedProfileData.HasKey("profile"))
            {
                // Check if the user is in this profile
                if (selectedProfileData["profile"].HasKey("members") && selectedProfileData["profile"]["members"].HasKey(uuid))
                {
                    // Create profile object
                    yield return StartCoroutine(CreateProfile(selectedProfileData, profileID));

                    // Disable main menu
                    var mainMenu = GameObject.FindGameObjectWithTag("Main Menu");
                    var cg = mainMenu.GetComponent<CanvasGroup>();
                    if (cg.alpha != 0)
                        mainMenu.GetComponent<Switch>().ToggleCanvas(cg);
                }
                else
                    ErrorHandler.Instance.Push(new Error { ErrorCode = 505, ErrorHeader = "No members found with given profile", ErrorMessage = $"Profile \"{profileID}\" has no members linked to it, please choose another profile." });

            }
            else
                ErrorHandler.Instance.Push(new Error { ErrorCode = 506, ErrorHeader = "No profile found", ErrorMessage = $"Profile \"{profileID}\" has no profile data." });
        }

        Debug.Log("Done: IE_ReloadData");
    }

    /// <summary>
    /// Create a profile object
    /// </summary>
    /// <param name="data"></param>
    /// <param name="profileID"></param>
    /// <returns></returns>
    public IEnumerator CreateProfile(JSONNode data, string profileID)
    {
        // Send request
        var statusWWW = UnityWebRequest.Get($"https://api.hypixel.net/status?key={Credentials.key}&uuid={uuid}");
        yield return statusWWW.SendWebRequest();

        var isOnline = !statusWWW.isNetworkError && !statusWWW.isHttpError && JSON.Parse(statusWWW.downloadHandler.text)["session"]["online"].AsBool;

        // Check if profile contains the user
        if (data["profile"]["members"].HasKey(uuid))
        {
            // Reset stuff
            SkillManager.Instance.skills.Clear();
            currentProfileData = data;
            var profileData = currentProfileData["profile"]["members"][uuid];
            var lastOnline = Global.UnixTimeStampToDateTime(profileData["last_save"]);

            yield return null;
            
            // Create profile object
            currentProfile = new Profile
            {
                FormattedUsername = formattedUsername,
                ProfileID = profileID,
                CuteName = profiles[profileID],
                LastSaveDateTime = lastOnline,
                FirstSaveDateTime = Global.UnixTimeStampToDateTime(profileData["first_join"]),
                IsOnline = isOnline,
                CollectedFairySouls = profileData["fairy_souls_collected"]
            };
            
            // Fill left out fields
            yield return StartCoroutine(FillProfile(currentProfile, profileData));
            // Update stats
            Stats.Instance.currentProfile = currentProfile;

            load.SetTrigger("Stop Load");
            // Instantiate every module by invoking event
            OnLoadProfile?.Invoke(this, new OnLoadProfileEventArgs { profile = currentProfile });
            TalismanOptimizer.Instance.accessories = currentProfile.AccessoryData;
        }
        else
            ErrorHandler.Instance.Push(new Error { ErrorCode = 507, ErrorHeader = "User is not in selected profile", ErrorMessage = $"User \"{username}\" is not linked with profile {profileID}." });

        Global.UpdateInfoList();
    }

    /// <summary>
    /// Retrieve and fill the profile object with the correct JSON data
    /// </summary>
    /// <param name="profile"></param>
    /// <param name="profileData"></param>
    /// <returns></returns>
    public IEnumerator FillProfile(Profile profile, JSONNode profileData)
    {
        GetBankingData(profile, profileData);
        GetInventoryData(profile, profileData);
        GetWardrobeData(profile, profileData);
        GetEquippedArmorData(profile, profileData);
        MergeWardrobeEquippedArmor(profile);
        GetAccesoryData(profile, profileData);
        GetEnderchestData(profile, profileData);
        GetPetData(profile, profileData);
        GetDungeonSkillData(profile, profileData);
        GetSkillData(profile, profileData);
        GetSlayerData(profile, profileData);
        GetFairyData(profile, profileData);
        GetVaultData(profile, profileData);
        GetExperiments(profile, profileData);
        GetDungeons(profile, profileData);

        profile.QuiverData = Items.GetDataModule(profile, profileData, "quiver");
        profile.PotionData = Items.GetDataModule(profile, profileData, "potion_bag");
        profile.FishingBagData = Items.GetDataModule(profile, profileData, "fishing_bag");
        profile.CandyData = Items.GetDataModule(profile, profileData, "candy_inventory_contents");

        yield return StartCoroutine(GetPlayerAuctions(profile, (x) => profile = x));

        PlayerStats.ProcessStats(profile);
        profile = GetBestWeapon(profile);
    }

    /// <summary>
    /// Retrieve dungeon data from API response
    /// </summary>
    /// <param name="originalProfile"></param>
    /// <param name="profileData"></param>
    public void GetDungeons(Profile originalProfile, JSONNode profileData)
	{
        if (!profileData.HasKey("dungeons")) return;

        // Create dungeons object
        originalProfile.Dungeons = new Dungeons
        {
            SelectedClass = profileData["dungeons"]["selected_dungeon_class"].Value,
            Journals = new List<Journal>(),
            MaxJournals = 0,
            CollectedJournals = 0
        };

        // Get journals
		foreach (var journalNode in profileData["dungeons"]["dungeon_journal"]["journal_entries"])
        {
            // Create journal object
            var journal = new Journal
            {
                Name = journalNode.Key,
                AmountCollected = journalNode.Value.Count,
                MaxAmount = Global.GetMaxJournalAmount(journalNode.Key.ToLower())
            };

            originalProfile.Dungeons.CollectedJournals += journal.AmountCollected;
            if (journal.AmountCollected == journal.MaxAmount)
                originalProfile.Dungeons.MaxJournals++;

            // Fill
            originalProfile.Dungeons.Journals.Add(journal);
		}
    }

    /// <summary>
    /// Retrieve experiment data from API
    /// </summary>
    /// <param name="originalProfile"></param>
    /// <param name="profileData"></param>
    public void GetExperiments (Profile originalProfile, JSONNode profileData)
	{
        if (!profileData.HasKey("experimentation")) return;
        originalProfile.Experiments = new List<Experiment>();

        // Loop through each field
		foreach (var node in profileData["experimentation"])
        {
            switch (node.Key)
            {
                // Fill resets
                case "claims_resets":
                    originalProfile.AvailableResets = node.Value.AsInt;
                    break;
                // Fill reset timestamp
                case "claims_resets_timestamp":
                    originalProfile.ResetTimestamp = node.Value.AsLong;
                    break;
                // Get experiment
                default:
                {
                    // Create experiment
                    var experiment = new Experiment
                    {
                        Name = Global.GetExperimentName(node.Key),
                        Stakes = new List<Stake>()
                    };

                    // Add fields
                    if (node.Value.HasKey("last_attempt")) experiment.LastAttempt = node.Value["last_attempt"].AsLong;
                    if (node.Value.HasKey("last_claimed")) experiment.LastClaimed = node.Value["last_claimed"].AsLong;
                    if (node.Value.HasKey("bonus_clicks")) experiment.BonusClicks = node.Value["bonus_clicks"].AsInt;

                    // Add completed stakes
                    for (int i = 0; i < 7; i++)
                    {
                        if (node.Value.HasKey($"claims_{i}"))
                        {
                            var stake = new Stake
                            {
                                Name = Global.GetStakeName(i, experiment.Name),
                                Attempts = node.Value[$"attempts_{i}"].AsInt,
                                BestScore = node.Value[$"best_score_{i}"].AsInt,
                                Claims = node.Value[$"claims_{i}"].AsInt
                            };

                            experiment.Stakes.Add(stake);
                        }
                    }

                    // Fill experiment
                    originalProfile.Experiments.Add(experiment);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Get profile banking data from API
    /// </summary>
    /// <param name="originalProfile"></param>
    /// <param name="profileData"></param>
    public void GetBankingData(Profile originalProfile, JSONNode profileData)
    {
		var bankingData = new BankingData
		{
			PurseBalance = double.TryParse(profileData["coin_purse"].Value, out var purseBalance) ? purseBalance : -1,
            BankBalance = double.TryParse(currentProfileData["profile"]["banking"]["balance"].Value, out var bankBalance) == true ? bankBalance : -1
		};
        
        originalProfile.BankingData = bankingData;
    }

    /// <summary>
    /// Get profile inventory data from API
    /// </summary>
    /// <param name="originalProfile"></param>
    /// <param name="profileData"></param>
    public void GetInventoryData(Profile originalProfile, JSONNode profileData)
	{
		if (!profileData.HasKey("inv_contents")) return;
       
		NbtCompound invCompoundTag = Parsing.DecodeGzippedBase64(profileData["inv_contents"]["data"].Value.ToString(), "invData.nbt");

		var inventoryData = new List<Item>();
		Items.GetCompoundData(originalProfile, inventoryData, invCompoundTag.Get<NbtList>("i"));

		// Sort inventory
		for (int i = 0; i < 9; i++)
		{
			var _ = inventoryData[0];
            inventoryData.Remove(_);
			inventoryData.Add(_);
		}

        originalProfile.InventoryData = inventoryData;
    }

    /// <summary>
    /// Get vault data from API
    /// </summary>
    /// <param name="originalProfile"></param>
    /// <param name="profileData"></param>
	public void GetVaultData(Profile originalProfile, JSONNode profileData)
	{
		if (!profileData.HasKey("personal_vault_contents")) return;

		NbtCompound vaultTag = Parsing.DecodeGzippedBase64(profileData["personal_vault_contents"]["data"].Value.ToString(), "vaultData.nbt");
        
		var vaultData = new List<Item>();
		var vaultContents = vaultTag.Get<NbtList>("i");

		Items.GetCompoundData(originalProfile, vaultData, vaultContents);

		originalProfile.VaultData = vaultData;
    }
    
    /// <summary>
    /// Get accessory data from API
    /// </summary>
    /// <param name="originalProfile"></param>
    /// <param name="profileData"></param>
	public void GetAccesoryData(Profile originalProfile, JSONNode profileData)
	{
		if (!profileData.HasKey("talisman_bag")) return;

		NbtCompound talismanBag = Parsing.DecodeGzippedBase64(profileData["talisman_bag"]["data"].Value.ToString(), "talismanBagData.nbt");

		var talismanBagData = originalProfile.AccessoryData;
		var talismanBagContents = talismanBag.Get<NbtList>("i");

        // Loop through accessories
		foreach (NbtCompound talismanCompound in talismanBagContents)
		{
			if (talismanCompound.TryGet<NbtCompound>("tag", out var tag))
			{
                // Add accessory to list
				Item talisman = Items.GetItemObject(talismanCompound, tag, originalProfile);
				talismanBagData.Add(talisman);
			}
		}

		originalProfile.AccessoryData = talismanBagData;
    }

    /// <summary>
    /// Get wardrobe data from API
    /// </summary>
    /// <param name="originalProfile"></param>
    /// <param name="profileData"></param>
	public void GetWardrobeData(Profile originalProfile, JSONNode profileData)
	{
		if (!profileData.HasKey("wardrobe_contents")) return;

		NbtCompound wardrobe = Parsing.DecodeGzippedBase64(profileData["wardrobe_contents"]["data"].Value.ToString(), "wardrobeData.nbt");

		var wardrobeData = new List<WardrobeSlots>();
		var wardrobeContents = wardrobe.Get<NbtList>("i");

		var wardrobePage = 0;
		// 48 slots in the api / 4 = 12 wardrobe slots
        if (wardrobeContents != null)
            for (int j = 0; j < wardrobeContents.Count / 4; j++)
            {
                var wardrobeSlot = new WardrobeSlots {SlotIndex = j};

                // If we reach a new page, we need to add an offset
                // of 36 to the index.
                if ((j % 9) == 0 && j != 0)
                    wardrobePage++;

                for (int i = 0; i < 4; i++)
                {
                    var firstPageIndex = j + (9 * i);
                    var pageOffset = wardrobePage * 27;

                    var finalPieceIndex = firstPageIndex + pageOffset;

                    if (finalPieceIndex < wardrobeContents.Count)
                    {
                        NbtCompound armorCompound = wardrobeContents.Get<NbtCompound>(finalPieceIndex);
                        if (armorCompound.TryGet<NbtCompound>("tag", out var tag))
                        {
                            Item armorPiece = Items.GetItemObject(armorCompound, tag, originalProfile);
                            wardrobeSlot.ArmorPieces.Add(armorPiece);
                        }
                        else
                            wardrobeSlot.ArmorPieces.Add(new Item());
                    }
                }

                wardrobeData.Add(wardrobeSlot);
            }

        originalProfile.WardrobeData = wardrobeData;
	}

	public void GetEquippedArmorData(Profile originalProfile, JSONNode profileData)
	{
		if (!profileData.HasKey("inv_armor")) return;

		NbtCompound armor = Parsing.DecodeGzippedBase64(profileData["inv_armor"]["data"].Value.ToString(), "armorData.nbt");
#if UNITY_EDITOR
        File.WriteAllText(Application.dataPath + "/armor.txt", armor.ToString());
#endif
        var armorContents = armor.Get<NbtList>("i");

		// Index from api begins at 1 (why?????)
		var selectedWardrobeSlot = profileData["wardrobe_equipped_slot"].AsInt;
		if (selectedWardrobeSlot > 0)
			selectedWardrobeSlot--;

		WardrobeSlots slot = new WardrobeSlots { SlotIndex = selectedWardrobeSlot };

		foreach (NbtCompound armorCompound in armorContents)
        {
            var item = new Item();
			if (armorCompound.TryGet<NbtCompound>("tag", out var tag))
			{
				Item armorPiece = Items.GetItemObject(armorCompound, tag, originalProfile);
                item = armorPiece;
            }
            
			slot.ArmorPieces.Add(item);
		}

		slot.ArmorPieces.Reverse();
		originalProfile.EquippedArmorData = slot;
	}

    /// <summary>
    /// Remove equipped armor from wardrobe.
    /// TODO: make this actually work?
    /// </summary>
    /// <param name="originalProfile"></param>
	public void MergeWardrobeEquippedArmor(Profile originalProfile)
	{
		if (originalProfile == null || originalProfile.WardrobeData == null || originalProfile.EquippedArmorData == null) return;
        WardrobeSlots equippedArmor = originalProfile.EquippedArmorData;

        if (equippedArmor.SlotIndex != -1 && equippedArmor != null && equippedArmor != new WardrobeSlots())
				if (originalProfile.WardrobeData != null && originalProfile.WardrobeData != new List<WardrobeSlots>())
					if (equippedArmor.ArmorPieces.Count > 0 && currentProfile.WardrobeData.Count > 0)
						if (equippedArmor.SlotIndex <= originalProfile.WardrobeData.Count - 1)
							originalProfile.WardrobeData[equippedArmor.SlotIndex].ArmorPieces = equippedArmor.ArmorPieces;
						else
							originalProfile.WardrobeData.Add(equippedArmor);
	}

    /// <summary>
    /// Retrieve enderchest data from API
    /// </summary>
    /// <param name="originalProfile"></param>
    /// <param name="profileData"></param>
	public void GetEnderchestData(Profile originalProfile, JSONNode profileData)
	{
		if (!profileData.HasKey("ender_chest_contents")) return;

		NbtCompound enderchest = Parsing.DecodeGzippedBase64(profileData["ender_chest_contents"]["data"].Value.ToString(), "enderchestData.nbt");

		var enderchestData = new List<Item>();
        var weapons = originalProfile.Weapons != null ? originalProfile.Weapons : new List<Item>();
		var enderchestContents = enderchest.Get<NbtList>("i");

		foreach (NbtCompound itemCompound in enderchestContents)
		{
			if (itemCompound.TryGet<NbtCompound>("tag", out var tag))
			{
				Item item = Items.GetItemObject(itemCompound, tag, originalProfile);
				enderchestData.Add(item);

                // Check if item is a weapon
				if (item != null && item.ItemDescription != null)
				{
                    if (item.ItemDescription.Count > 1)
                    {
                        string lastDescriptionLine = item.ItemDescription[item.ItemDescription.Count - 1];
                        if (lastDescriptionLine.Contains("SWORD") || lastDescriptionLine.Contains("BOW") || lastDescriptionLine.Contains("WEAPON"))
                            if (!weapons.Exists((x) => x.ItemDescription == item.ItemDescription))
                                weapons.Add(item);
                    }
                }

			}
			else
				enderchestData.Add(new Item());
		}

		originalProfile.Weapons = weapons;
		originalProfile.EnderchestData = enderchestData;
	}

    /// <summary>
    /// Retrieve pet data from API
    /// </summary>
    /// <param name="originalProfile"></param>
    /// <param name="profileData"></param>
	public void GetPetData(Profile originalProfile, JSONNode profileData)
	{
		if (!profileData.HasKey("pets")) return;

		var petData = originalProfile.PetData != null ? originalProfile.PetData : new List<Pet>();
		var petsContents = profileData["pets"].Values;

        // Add all the pets
		foreach (var petNode in petsContents)
			petData.Add(Pets.GetPet(petNode));

        // Find active pet, and remove it from the pet list
        var activePet = new Pet();
        if (petData != null)
        {
            foreach (var pet in petData)
                if (pet.IsActive)
                {
                    activePet = pet;
                    petData.Remove(activePet);
                    break;
                }
        }

        // Get Item object from active pet
        var activePetItem = Pets.PetToItem(activePet);
        activePet.BonusStats = activePetItem.BonusStats;

        originalProfile.PetData = petData;
        originalProfile.ActivePet = activePet;
	}

    /// <summary>
    /// Retrieve dungeon skills (i.e catacombs, mage, berserker)
    /// </summary>
    /// <param name="profile"></param>
    /// <param name="profileData"></param>
	public void GetDungeonSkillData(Profile profile, JSONNode profileData)
    {
        if (!profileData.HasKey("dungeons")) return;

        // Loop through all the dungeon skills and add them
        var skills = new Dictionary<SKILL, Skill>();
        foreach (var skillNode in profileData["dungeons"]["player_classes"])
        {
            var skill = Skills.GetSkill(skillNode.Value["experience"], skillNode.Key.ToLower());
            skills.Add(skill.SkillName, skill);
        }

        // Add catacombs skill
        var catacombs = Skills.GetSkill(profileData["dungeons"]["dungeon_types"]["catacombs"]["experience"], "catacombs");
        skills.Add(catacombs.SkillName, catacombs);

        profile.SkillData = skills;
    }
    
    /// <summary>
    /// Get normal skill data
    /// </summary>
    /// <param name="profile"></param>
    /// <param name="profileData"></param>
    public void GetSkillData(Profile profile, JSONNode profileData)
    {
        var skills = profile.SkillData != null ? profile.SkillData : new Dictionary<SKILL, Skill>();

        // Loop through all the fields in the api data 
        // *hypixel moment* 
        foreach (var item in profileData)
        {
            if (item.Key.Contains("experience_skill_"))
            {
                var skillName = item.Key.Replace("experience_skill_", "");
                var skill = Skills.GetSkill(item, skillName);
                skills.Add(skill.SkillName, skill);
            }
        }

        // Get average skill
        profile.AverageSkilLevelProgression = Skills.GetAverageSkillLevel(skills, true);
        profile.AverageSkilLevel = Skills.GetAverageSkillLevel(skills, false);
        profile.SkillData = skills;
    }

    /// <summary>
    /// Retrieve slayer data from API
    /// </summary>
    /// <param name="profile"></param>
    /// <param name="profileData"></param>
    public void GetSlayerData(Profile profile, JSONNode profileData)
    {
        if (!profileData.HasKey("slayer_bosses")) return;

        var slayers = new List<Slayer>();
        foreach (var item in profileData["slayer_bosses"])
        {
            // Get slayer name
            string slayerName = Global.ToTitleCase(item.Key);
            var level = 0;
            for (int i = 0; i < item.Value["claimed_levels"].Count; i++)
                level++;

            // Get slayer progress
            int xp = item.Value["xp"].AsInt;
            var slayerType = (SLAYERTYPE)Enum.Parse(typeof(SLAYERTYPE), slayerName);

            // Get kills
            Dictionary<STAT, Stat> slayerStats = PlayerStats.GetSlayerStats(slayerType, level);
            var kills = GetSlayerKills(item.Value, slayerType);

            // Get slayer xp
            var slayerData = JSON.Parse(Resources.Load<TextAsset>("JSON Data/Slayer XP Ladders").text);
            var jsonLadder = slayerData["XPLadders"][slayerName.ToUpper()].AsArray;
            int[] intLadder = new int[jsonLadder.Count];

            for (int i = 0; i < jsonLadder.Count; i++)
                intLadder[i] = jsonLadder[i].AsInt;

            var slayerXp = Global.GetSlayerXP(xp, intLadder, 0);
            var isMax = level == intLadder.Length;

            // Get slayer icon
            var icon = Resources.Load<Sprite>($"Slayers/{slayerName}");

            // Create slayer object, add it
            var slayer = new Slayer
            {
                Name = Global.GetSlayerName(slayerName),
                Level = level,
                Icon = icon,
                IsMaxLevel = isMax,
                ProgressXP = slayerXp,
                BonusStats = slayerStats,
                Type = slayerType,
                Kills = kills
            };

            slayers.Add(slayer);
        }
        profile.Slayers = slayers;
    }

    /// <summary>
    /// Helper method for slayers
    /// </summary>
    /// <param name="jsonData"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public List<SlayerKill> GetSlayerKills(JSONNode jsonData, SLAYERTYPE type)
    {
        var kills = new List<SlayerKill>();

        foreach (var entry in jsonData)
        {
            if (entry.Key.StartsWith("boss_kills_tier_"))
            {
                var tier = int.Parse(entry.Key.Substring(16, entry.Key.Length - 16)) + 1;
                var amount = entry.Value.AsInt;
                kills.Add(new SlayerKill { Tier = tier, Amount = amount, Type = type });
            }
        }

        return kills;
    }

    /// <summary>
    /// Retrieve fairy soul data from API
    /// </summary>
    /// <param name="profile"></param>
    /// <param name="profileData"></param>
    public void GetFairyData(Profile profile, JSONNode profileData)
    {
        var fairyData = new FairySouls
        {
            CollectedSouls = profileData["fairy_souls_collected"].AsInt,
            SoulExchanges = profileData["fairy_exchanges"].AsInt,
            ExcessSouls = profileData["fairy_souls"].AsInt
        };

        profile.FairySoulData = fairyData;
    }

    /// <summary>
    /// Retrieve and process player auctions
    /// </summary>
    /// <param name="profile"></param>
    /// <param name="reference"></param>
    /// <returns></returns>
    public IEnumerator GetPlayerAuctions(Profile profile, Action<Profile> reference)
    {
        // Send auctions request
        var auctionsWWW = UnityWebRequest.Get($"https://api.hypixel.net/skyblock/auction?profile={profileID}&uuid={uuid}&key={Credentials.key}");
        yield return auctionsWWW.SendWebRequest();

        var auctionsJSON = JSON.Parse(auctionsWWW.downloadHandler.text)["auctions"];
        var auctions = new List<Auction>();
        
        // Loop through auctions
        foreach (var auctionNode in auctionsJSON)
        {
            var bids = GetAuctionBids(auctionNode.Value["bids"]);
            bids.OrderBy((x) => x.Amount);

            NbtCompound auctionCompound = Parsing.DecodeGzippedBase64(auctionNode.Value["item_bytes"]["data"].Value, "auctionData.nbt").Get<NbtList>("i").Get<NbtCompound>(0);

            var auctioneer = auctionNode.Value["auctioneer"].Value;
            var item = Items.GetItemObject(auctionCompound, auctionCompound.Get<NbtCompound>("tag"), profile);

            var startTime = auctionNode.Value["start"].AsLong;
            var endTime = auctionNode.Value["end"].AsLong;
            
            // Create auction object
            var newAuction = new Auction
            {
                AuctioneerUUID = auctioneer,
                FormattedAuctioneerUsername = formattedUsername,
                IsBIN = auctionNode.Value["bin"].AsBool,
                Category = (CATEGORY)Enum.Parse(typeof(CATEGORY), auctionNode.Value["category"]),
                HasEnded = auctionNode.Value["claimed"].AsBool,
                Bids = bids,
                AuctionItem = item,
                HighestBid = bids.Count > 0 ? bids[bids.Count - 1] : null,
                Price = bids.Count > 0 ? bids[bids.Count - 1].Amount : auctionNode.Value["starting_bid"].AsFloat,
                UnixEndTimestamp = endTime,
                UnixBeginTimestamp = startTime
            };

            if (!newAuction.HasEnded)
                auctions.Add(newAuction);
        }

        profile.PlayerAuctions = auctions;
        currentProfile = profile;
        reference(profile);
    }
    
    /// <summary>
    /// Get the bids of a specific auction
    /// </summary>
    /// <param name="bidsNode"></param>
    /// <returns></returns>
    public List<Bid> GetAuctionBids(JSONNode bidsNode)
    {
        var bids = new List<Bid>();

        // loop through bids
        foreach (var bidNode in bidsNode)
        {
            // Create bid object, add it to the list
            var newBid = new Bid
            {
                BidderUUID = bidNode.Value["bidder"].Value,
                BidderProfileID = bidNode.Value["profile_id"].Value,
                Amount = bidNode.Value["amount"].AsFloat,
                UnixTimestamp = bidNode.Value["timestamp"].AsLong
            };

            bids.Add(newBid);
        }

        return bids;
    }
    
    /// <summary>
    /// Retrieve the weapon that deals the most damage
    /// </summary>
    /// <param name="originalProfile"></param>
    /// <returns></returns>
    public Profile GetBestWeapon(Profile originalProfile)
    {
        var newProfile = originalProfile;
        if (newProfile.Weapons != null)
        {
            List<Item> weapons = newProfile.Weapons;
            Item bestWeapon = new Item();

            // Loop through weapons
            foreach (var weapon in weapons)
            {
                // Check if it deals more damage
                var weaponDamage = DamagePrediction.Instance.CalculateItemDamage(weapon);
                if (weaponDamage > DamagePrediction.Instance.CalculateItemDamage(bestWeapon))
                    bestWeapon = weapon;
            }

            newProfile.ActiveWeapon = bestWeapon;
        }
        else
            DamagePrediction.Instance.ResetDamage();

        return newProfile;
    }

    #endregion

    #region Instantiate Modules
    public void ClearChildren(Transform parent)
    {
        if (parent != null)
            for (int i = 0; i < parent.childCount; i++)
                if (!parent.GetChild(i).name.Contains("(DND)"))
                    Destroy(parent.GetChild(i).gameObject);
    }
    #endregion
}