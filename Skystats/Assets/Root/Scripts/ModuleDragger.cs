using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Helper;

public class ModuleDragger : UIBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public RectTransform windowToDrag;
    public List<GameObject> objectDisableOnMinimize;
    public List<Behaviour> componentDisableOnMinimize;
    public bool clampInsideParentRect = true, isMinimized = false;
    public float originalHeight;

    private Vector2 currentPos, lastPos;
    public ModuleHandler mh = null;

    private void Awake()
    {
        mh = Global.FindParentWithComponent<ModuleHandler>(transform.parent);
        if (!windowToDrag)
            windowToDrag = transform as RectTransform;
    }

	public virtual void Start()
	{
        Minimize(false);
    }

    public void RemoveModule ()
	{
        mh.entryTransform = windowToDrag;
        mh.RemoveEntry();

        mh.UpdatePositions();
        mh.SaveModules();
	}

    public void Minimize (bool save)
	{
        if (isMinimized == true)
            originalHeight = windowToDrag.sizeDelta.y;

        isMinimized = !isMinimized;

        foreach (var comp in componentDisableOnMinimize)
            comp.enabled = isMinimized;

        var size = isMinimized ? new Vector2(windowToDrag.sizeDelta.x, originalHeight)
                             : new Vector2(windowToDrag.sizeDelta.x, GetComponent<RectTransform>().sizeDelta.y);

        windowToDrag.sizeDelta = size;

        foreach (var obj in objectDisableOnMinimize)
            obj.SetActive(isMinimized);

        if (save)
            mh.UpdateModule(windowToDrag);
		for (int i = 0; i < 5; i++)
            Global.UpdateCanvasElement(windowToDrag);
    }

	public virtual void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(windowToDrag.parent as RectTransform, eventData.position, eventData.pressEventCamera, out Vector2 result))
        {
            currentPos += result - lastPos;
            lastPos = result;
            windowToDrag.anchoredPosition = ClampPosition(currentPos);
        }
    }

    public float Round (float orig)
	{
        return ((int)Mathf.Round(orig / 10f)) * 10;
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        mh.UpdateModule(windowToDrag);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(windowToDrag.parent as RectTransform, eventData.position, eventData.pressEventCamera, out lastPos);
        currentPos = windowToDrag.anchoredPosition;
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        mh.UpdateModule(windowToDrag);
    }

    private Vector2 ClampPosition(Vector2 position)
    {
        if (!clampInsideParentRect)
            return position;

        Vector2 size = windowToDrag.rect.size;
        size = Vector2.Scale(size, windowToDrag.localScale);
        Rect area = (windowToDrag.parent as RectTransform).rect;

        // Offset center to accomidate to our anchor position
        Vector2 anchorCenter = new Vector2(0.5f, 0.5f) - ((windowToDrag.anchorMin + windowToDrag.anchorMax) * 0.5f);
        area.center = Vector2.Scale(area.size, anchorCenter);

        // Clamp the position to the area of the parent of the WindowTodrag
        var newPos = new Vector2(Mathf.Clamp(position.x, area.xMin + (size.x * windowToDrag.pivot.x), area.xMax - (size.x * (1f - windowToDrag.pivot.x))),
            Mathf.Clamp(position.y, area.yMin + (size.y * windowToDrag.pivot.y), area.yMax - (size.y * (1f - windowToDrag.pivot.y))));

        return mh.round ? new Vector2(Round(newPos.x), Round(newPos.y)) : new Vector2(newPos.x, newPos.y);
    }

    public void LoadPosition(Vector3 newPos, bool minimized)
    {
        if (mh.loadSavedPosition)
            windowToDrag.anchoredPosition = newPos;

        if (minimized) 
            Minimize(false);
    }
}
