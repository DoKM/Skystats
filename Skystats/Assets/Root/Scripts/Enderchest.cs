using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Helper;

public class Enderchest : MonoBehaviour
{
    public List<GameObject> enderchestPages = new List<GameObject>();
    public int selectedPage = 0;

    public Transform enderchestParent;
    private TMP_Text pageCounter;

    private void Start()
    {
        pageCounter = GameObject.FindGameObjectWithTag("EnderchestPageCounter").GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (enderchestParent != null && enderchestParent.gameObject.activeInHierarchy)
		{
            GetPages();
            UpdatePageCounter();
        }
    }

    private void UpdatePageCounter()
    {
        pageCounter.text = $"<color=#CCCCCC>Page</color> {selectedPage + 1}";
    }

    public void GetPages()
    {
        enderchestPages.Clear();

        for (int i = 1; i < enderchestParent.childCount; i++)
        {
            var newPage = enderchestParent.GetChild(i).gameObject;
            newPage.name = $"Page {i}";
            enderchestPages.Add(newPage);
        }
    }

    [Button]
    public void NextPage()
    {
        selectedPage++;
        SelectPage(selectedPage);
        Global.UpdateInfoList();
    }

    [Button]
    public void PreviousPage()
    {
        selectedPage--;
        SelectPage(selectedPage);
        Global.UpdateInfoList();
    }

    public void SelectPage(int pageIndex)
    {
        if (enderchestPages.Count > 0)
        {
            if (pageIndex > enderchestPages.Count - 1)
            {
                selectedPage = 0;
                ActivatePage(0);
            }
            else if (pageIndex < 0)
            {
                selectedPage = enderchestPages.Count - 1;
                ActivatePage(enderchestPages.Count - 1);
            }
            else
                ActivatePage(pageIndex);
        }
    }

    public void ActivatePage(int pageIndex)
    {
        for (int i = 0; i < enderchestPages.Count; i++)
        {
            if (i == pageIndex)
                enderchestPages[i].SetActive(true);
            else
                enderchestPages[i].SetActive(false);
        }
    }

}
