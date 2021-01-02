using Helper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ModuleAdding : ModuleDragger
{
	public Button minimize, remove;
	private ModuleDragger latestSpawn;

	public override void Start()
	{
		Minimize(false);
		Minimize(false);

		for (int i = 0; i < 3; i++)
			if (windowToDrag.GetChild(0).childCount > 1)
				Global.UpdateCanvasElement(windowToDrag.GetChild(0).GetChild(1) as RectTransform);
	}

	public override void OnEndDrag(PointerEventData eventData)
	{
		latestSpawn.OnEndDrag(eventData);
	}

	public override void OnDrag(PointerEventData eventData)
	{
		latestSpawn.OnDrag(eventData);
	}

	public override void OnBeginDrag(PointerEventData eventData)
	{
		var dragObj = Instantiate(Resources.Load<GameObject>($"Prefabs/Modules/{windowToDrag.name}"), eventData.pointerPressRaycast.worldPosition, transform.rotation, PageHandler.Instance.activePage);
		dragObj.name = windowToDrag.name;
		dragObj.transform.SetSiblingIndex(windowToDrag.GetSiblingIndex());

		var activePageMh = PageHandler.Instance.activePage.GetComponent<ModuleHandler>();
		activePageMh.modules.Insert(0, dragObj.transform as RectTransform);

		if (activePageMh.moduleList.Modules == null) activePageMh.moduleList.Modules = new List<Module>();
		activePageMh.moduleList.Modules.Add(new Module
		{
			Index = 0,
			IsMinimized = false,
			Name = windowToDrag.name,
			SavedPosition = windowToDrag.position
		});

		var drag = Global.FindObjectsOfTypeInTranform<ModuleDragger>(dragObj.transform)[0];

		drag.mh = activePageMh;
		drag.originalHeight = originalHeight;
		drag.windowToDrag = dragObj.transform as RectTransform;
		drag.componentDisableOnMinimize = componentDisableOnMinimize;
		drag.objectDisableOnMinimize = objectDisableOnMinimize;

		latestSpawn = drag;
		drag.OnBeginDrag(eventData);
	}
}
