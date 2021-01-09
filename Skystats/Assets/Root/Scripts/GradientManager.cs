using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public enum GradientType
{
    enchantColor,
    maxColor,
    normalColor,
    uiColor,
    disabledColor
}

public class GradientManager : MonoBehaviour
{
    #region Singleton
    public static GradientManager Instance;
    private void OnEnable()
    {
        if (!Instance) Instance = this;
        else Destroy(this);
    }
    #endregion

    public Theme currentTheme;
    private string savePath = "/gradients/main theme";

    private void Awake()
    {
        DirectoryInfo gradientDir = new DirectoryInfo(Application.persistentDataPath + savePath);
        if (!gradientDir.Exists)
            gradientDir.Create();

        currentTheme = ReadTheme("mainTheme");
    }

    public void SwapTheme (Theme newTheme)
	{
        currentTheme = newTheme;
	}

    public void SaveMainTheme()
    {
        var themeJson = JsonUtility.ToJson(currentTheme);
        File.WriteAllText($"{Application.persistentDataPath}{savePath}/mainTheme.json", themeJson);
    }

    public void SaveDefaultTheme()
    {
        string path = null;
#if UNITY_EDITOR
        path = "Assets/Resources/Themes/defaultTheme.json";
#endif
        string str = JsonUtility.ToJson(currentTheme);
        using (FileStream fs = new FileStream(path, FileMode.Create))
        {
            using (StreamWriter writer = new StreamWriter(fs))
            {
                writer.Write(str);
            }
        }
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    public void ReadMainTheme()
    {
        ReadTheme("mainTheme");
    }

    public ThemeGradient GetCurrentThemeColor(GradientType type)
    {
        if (currentTheme.ThemeColors.FindIndex((x) => x.Type == type) >= 0)
            return currentTheme.ThemeColors.Find((x) => x.Type == type);
        else
            return null;
    }

    public void SaveTheme(Theme theme)
    {
        var themeJson = JsonUtility.ToJson(theme);
        File.WriteAllText($"{Application.persistentDataPath}{savePath}/{theme.ThemeName}.json", themeJson);
    }

    public Theme ReadTheme(string themeName)
    {
        var themeJson = Resources.Load<TextAsset>("Themes/defaultTheme").text;

        if (File.Exists($"{Application.persistentDataPath}{savePath}/{themeName}.json"))
            themeJson = File.ReadAllText($"{Application.persistentDataPath}{savePath}/{themeName}.json");

        var theme = JsonUtility.FromJson<Theme>(themeJson);
        return theme;
    }

    public void ToggleMenu()
    {
        var cg = GetComponent<CanvasGroup>();
        _ = cg.alpha == 0 ? cg.alpha = 1 : cg.alpha = 0;

        cg.blocksRaycasts = !cg.blocksRaycasts;
    }

    private void OnApplicationFocus()
    {
        SaveMainTheme();
    }

}

[Serializable]
public struct Theme
{
    public string ThemeName;
    public List<ThemeGradient> ThemeColors;
}

[Serializable]
public class ThemeGradient
{
    public string GradientName;
    public Gradient Color;
    public GradientType Type;
}
