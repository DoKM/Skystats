using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class Tooltip : MonoBehaviour
{
    public float xOffset;
    public float yOffset;
    public float holderXOffset;
    [Space]
    public float scrollSens;
    [Space]
    public Camera camera;

    public Slot currentDisplayingSlot;
    public bool update;
    public bool resetOffset = true, scrolling = true;
    public Canvas mainCanvas;
    [HideInInspector] public RectTransform rct, backpackPreview, holder;
    

    public void ToggleOffset (bool on)
	{
        resetOffset = on;
	}

    public void ToggleScrolling (bool on)
	{
        scrolling = on;
	}

	private void Start()
	{
        rct = GetComponent<RectTransform>();
        holder = (RectTransform)transform.GetChild(0);
        backpackPreview = (RectTransform)holder.Find("Backpack Preview IGNORE");

        backpackPreview.gameObject.SetActive(false);
	}

	private void Update()
    {
        UpdateTooltipPos();

        if (!EventSystem.current.IsPointerOverGameObject()) 
            holder.gameObject.SetActive(false);
    }

    public void UpdateTooltipRect ()
	{
        if (resetOffset)
            holder.pivot = new Vector2(holder.pivot.x, 0.5f);
        LayoutRebuilder.ForceRebuildLayoutImmediate(holder.GetComponent<RectTransform>());
    }

    private void UpdateTooltipPos()
    {
        // Scroll pivot update
        if (holder.gameObject.activeSelf && update)
            UpdateTooltipRect();

        if (update) 
            update = !update;

        float newPos = holder.rect.size.x / 2 + holderXOffset;
        holder.transform.position = new Vector3(newPos, holder.transform.position.y, 0);

        if (holder.gameObject.activeSelf)
		{
            var screenPoint = new Vector3(xOffset, yOffset, 0);
            screenPoint += Input.mousePosition;

            if (scrolling)
			{
                var scrollInput = Input.mouseScrollDelta.y;
                holder.pivot = new Vector2(holder.pivot.x, holder.pivot.y + scrollInput * scrollSens);
            }
                
            var worldPoint = camera.ScreenToWorldPoint(screenPoint);
            worldPoint.z = 0;

            holder.position = worldPoint;
        }
    }
}
