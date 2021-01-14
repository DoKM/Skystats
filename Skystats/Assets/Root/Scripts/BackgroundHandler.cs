using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Helper;
using System.IO;

[Serializable]
public class Background
{
    public string Name;
    public Sprite image;
    public Theme defaultTheme;
}

public class BackgroundHandler : MonoBehaviour
{
    #region Singleton
    public static BackgroundHandler Instance;
    private void OnEnable()
    {
        if (!Instance) Instance = this;
        else Destroy(this);
    }
    #endregion

    public List<Background> availableBackgrounds;
    public Background activeBg;
    [Space]
    public Image background;
    public Image backgroundIcon;
    public Transform backgroundHolder;

    public CanvasGroup cg;
    [HideInInspector] public bool updateTheme;
    public Theme @default;
    [Space]
    public string BackgroundName;
    public bool save;

	private void FixedUpdate()
	{
		if (save)
		{
            save = false;
            SaveBackgrounds();
		}
	}

	public void SaveBackgrounds ()
	{
		foreach (var background in availableBackgrounds)
		{
            var backgroundJson = JsonUtility.ToJson(background);
            File.WriteAllText($"{Application.persistentDataPath}/backgrounds/{background.defaultTheme.ThemeName}.json", backgroundJson);
        }

        var activeBgJson = JsonUtility.ToJson(availableBackgrounds[0]);
        File.WriteAllText($"{Application.persistentDataPath}/backgrounds/activeBackground.json", activeBgJson);
    }

	public void Start()
	{
        InstantiateBackgrounds();
        LoadActiveBackground();
	}

	public void InstantiateBackgrounds()
    {
        var backgroundPrefab = Resources.Load<GameObject>("Prefabs/Background");
        foreach (var background in availableBackgrounds)
        {
            var backgroundObj = Instantiate(backgroundPrefab, backgroundHolder).GetComponent<BackgroundObj>();
            backgroundObj.bg = background;
            backgroundObj.UpdateIcon();
        }
    }

    public void ToggleUpdateTheme (bool on)
	{
        updateTheme = on;
	}

    public void Toggle ()
	{
        cg.alpha = cg.alpha == 0 ? 1 : 0;
        cg.blocksRaycasts = !cg.blocksRaycasts;
	}

    public void SwapBackground (Background newBg)
	{
        activeBg = newBg;
        if (updateTheme)
            GradientManager.Instance.SwapTheme(newBg.defaultTheme);

        UpdateBg();
	}

    private void UpdateBg ()
	{
        background.sprite = activeBg.image;
	}

	private void Update()
	{
        if (backgroundIcon.sprite != background.sprite)
            backgroundIcon.sprite = background.sprite;
	}

    public void SaveActiveBackground()
    {
        var backgroundJson = JsonUtility.ToJson(activeBg);
        File.WriteAllText($"{Application.persistentDataPath}/backgrounds/activeBackground.json", backgroundJson);
    }

    public void LoadActiveBackground()
    {
        var activeBgJson = File.ReadAllText($"{Application.persistentDataPath}/backgrounds/activeBackground.json"); 
        activeBg = JsonUtility.FromJson<Background>(activeBgJson);
        SwapBackground(activeBg);
    }

}