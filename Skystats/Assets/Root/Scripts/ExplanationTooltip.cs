using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

public class ExplanationTooltip : MonoBehaviour
{
    [ResizableTextArea]
    public string tooltipText;

    private GameObject tooltip, tooltipHolder, backpackHolder;

	private void Awake()
	{
        tooltip = GameObject.FindGameObjectWithTag("Tooltip");
        tooltipHolder = tooltip.transform.GetChild(0).gameObject;
        backpackHolder = tooltipHolder.transform.Find("Backpack Preview IGNORE").GetChild(0).gameObject;
    }

    public void OnMouseEnter()
    {
        if (Main.Instance.helpTooltips)
		{
            ClearTooltip();
            ActivateTooltip(true);
        }
    }

    public void OnMouseExit()
    {
        if (Main.Instance.helpTooltips)
            ActivateTooltip(false);
    }

    public virtual void ActivateTooltip(bool active)
    {
        if (active)
        {
            var newObj = Instantiate(Resources.Load<GameObject>("Description line"), tooltipHolder.transform);
            newObj.transform.SetAsFirstSibling();

            newObj.GetComponent<Text>().text = tooltipText;
            tooltip.GetComponent<Tooltip>().update = true;

            ClearBackpack();
            tooltipHolder.SetActive(true);
        }
        else
            tooltipHolder.SetActive(false);

    }

    public void ClearBackpack()
    {
        if (backpackHolder != null)
        {
            Main.Instance.ClearChildren(backpackHolder.transform);
            tooltip.GetComponent<Tooltip>().UpdateTooltipRect();
            backpackHolder.transform.parent.gameObject.SetActive(false);
        }
    }

    public void ClearTooltip()
    {
        for (int i = 0; i < tooltipHolder.transform.childCount; i++)
        {
            if (!tooltipHolder.transform.GetChild(i).name.Contains("IGNORE"))
            {
                Destroy(tooltipHolder.transform.GetChild(i).gameObject);
                tooltip.GetComponent<Tooltip>().UpdateTooltipRect();
            }
        }
    }

}
