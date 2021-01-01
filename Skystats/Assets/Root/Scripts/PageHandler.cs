using Helper;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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

    public RectTransform activePage;
    public List<RectTransform> pages;

    public int activePageIndex = 0;
    public RectTransform page;

	private void Awake()
	{
        LoadPages();
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

    [Button]
    public void AddPage()
    {
        var newPage = Instantiate(Resources.Load<GameObject>("Prefabs/Page"), transform);
        newPage.GetComponent<Canvas>().worldCamera = Camera.main;
        pages.Add(newPage.transform as RectTransform);
        newPage.name = GetNewPageName();
    }

    private string GetNewPageName ()
	{
        var value = "page ";

		for (int i = 0; i < pages.Count; i++)
		{
            if (!pages[i].name.Contains((i + 1).ToString()))
			{
                value += (i + 1);
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
        pages.Clear();
		for (int i = 0; i < transform.childCount; i++)
            pages.Add(transform.GetChild(i) as RectTransform);

		foreach (var page in pages)
            page.gameObject.SetActive(page.GetSiblingIndex() == pageIndex);

        activePageIndex = pageIndex;
        activePage = pages[activePageIndex];
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

        for (int i = 0; i < pages.Count; i++)
            AddPage(pages[i]);

        Invoke("SwitchPage", 0.1f);
    }
}
