using Helper;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Michsky.UI.ModernUIPack;

public class PageHandler : MonoBehaviour
{
    #region Singleton
    public static PageHandler Instance;
    private void OnEnable()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    #endregion

    public TMP_Text pageNumber;
    public RectTransform activePage;
    public List<RectTransform> pages;

    public int activePageIndex = 0;
    public RectTransform page;
    public List<Button> buttonsDisableSinglePage;

	private void Awake()
	{
        LoadPages();
	}

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
		{
            if (Input.GetKeyDown(KeyCode.X))
                NextPage();
            else if (Input.GetKeyDown(KeyCode.Z))
                PreviousPage();
            else if (Input.GetKeyDown(KeyCode.Alpha1))
                SwitchPage(0);
        }

        if (pages.Count == 1)
            foreach (var button in buttonsDisableSinglePage)
                DisableButton(button, false);
        else if (pages.Count > 1)
            foreach (var button in buttonsDisableSinglePage)
                DisableButton(button, true);

        if (buttonsDisableSinglePage.Any(x => x.gameObject.name == "Remove"))
		{
            var button = buttonsDisableSinglePage.First(x => x.gameObject.name == "Remove");
            var interactable = activePage != pages[0];

            DisableButton(button, interactable);
        }
    }

    private void DisableButton (Button button, bool interactable)
	{
        button.interactable = interactable;

        var newGrad = interactable == true ? GradientType.uiColor : GradientType.disabledColor;
        button.transform.GetChild(0).GetComponent<StaticGradient>().enabledGradient = newGrad;

        var newIconAlpha = interactable == true ? 1 : 0;
        var icon = button.transform.GetChild(1).GetComponent<Image>();
        icon.color = new Color(icon.color.r, icon.color.g, icon.color.b, newIconAlpha);
    }

	[Button]
    public void DeletePage ()
	{
        if (pages.Contains(page))
		{
            pages.Remove(page);
            page.GetComponent<ModuleHandler>().Remove();
            Destroy(page.gameObject);
		}
    }

    public void DeleteCurrentPage ()
	{
        page = activePage;
        DeletePage();
        PreviousPage();
    }

    [Button]
    public void AddPage()
    {
        var newPage = Instantiate(Resources.Load<GameObject>("Prefabs/Page"), transform);
        newPage.GetComponent<Canvas>().worldCamera = Camera.main;
        pages.Add(newPage.transform as RectTransform);
        newPage.name = GetNewPageName();

        SwitchPage(pages.Count - 1);
    }

    private string GetNewPageName ()
	{
        var value = "page ";

		for (int i = 0; i < pages.Count; i++)
		{
            if (!pages[i].name.Contains((i + 1).ToString()))
			{
                value += i + 1;
                break;
            }
		}

        return value;
	}

    public void AddPage(string pageName)
    {
        var newPage = Instantiate(Resources.Load<GameObject>("Prefabs/Page"), transform);
        newPage.GetComponent<Canvas>().worldCamera = Camera.main;
        pages.Add(newPage.transform as RectTransform);
        newPage.name = pageName;
    }

    [Button]
    public void NextPage ()
	{
        if (pages.Count >= activePageIndex + 2)
            SwitchPage(activePageIndex + 1);
        else
            SwitchPage(0);
    }

    [Button]
    public void PreviousPage ()
	{
        if (activePageIndex - 1 >= 0)
            SwitchPage(activePageIndex - 1);
        else
            SwitchPage(pages.Count - 1);
    }

	public void SwitchPage (int pageIndex)
	{
		foreach (var page in pages)
            page.gameObject.SetActive(page.GetSiblingIndex() == pageIndex);

        activePageIndex = pageIndex;
        activePage = pages[activePageIndex];

        activePage.ForceUpdateRectTransforms();
        activePage.GetComponent<ModuleHandler>().UpdateModules();

        pageNumber.text = (activePageIndex + 1).ToString();
    }

    public void SwitchPage ()
	{
        SwitchPage(activePageIndex);
	}

    private void LoadPages ()
	{
        var pages = new List<string>();
        var drInfo = new DirectoryInfo($"{Application.persistentDataPath}/modules");
        var dirs = drInfo.GetDirectories();

        for (int i = 0; i < dirs.Length; i++)
        {
            if (dirs[i].Attributes.HasFlag(FileAttributes.Directory))
                pages.Add(dirs[i].Name);
        }

        // Defaults
        if (pages.Count == 0)
		{
            pages.Add("page 1");
            pages.Add("page 2");
        }

        for (int i = 0; i < pages.Count; i++)
            AddPage(pages[i]);

        Invoke("SwitchPage", 0.1f);
    }
}
