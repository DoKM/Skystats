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

	public override void Start()
	{
		Minimize(false);
		Minimize(false);
	}

	public override void OnBeginDrag(PointerEventData eventData)
	{
		var self = Instantiate(windowToDrag.gameObject, windowToDrag.parent);
		self.transform.SetSiblingIndex(windowToDrag.GetSiblingIndex());
		self.name = windowToDrag.name;
		Global.FindObjectsOfTypeInTranform<ModuleDragger>(self.transform)[0].isMinimized = false;

		windowToDrag.SetParent(PageHandler.Instance.activePage);
		var activePageMh = PageHandler.Instance.activePage.GetComponent<ModuleHandler>();

		activePageMh.modules.Insert(0, windowToDrag);
		if (activePageMh.moduleList.Modules == null)
			activePageMh.moduleList.Modules = new List<Module>();
		activePageMh.moduleList.Modules.Add(new Module { Index = 0, IsMinimized = false, 
														 Name = windowToDrag.name, 
														 SavedPosition = windowToDrag.position 
													   });
		
		var drag = gameObject.AddComponent<ModuleDragger>();
		drag.mh = activePageMh;
		drag.originalHeight = originalHeight;
		drag.windowToDrag = windowToDrag;
		drag.componentDisableOnMinimize = componentDisableOnMinimize;
		drag.objectDisableOnMinimize = objectDisableOnMinimize;

		minimize.onClick.RemoveAllListeners();
		minimize.onClick.AddListener(delegate { drag.Minimize(true); });
		remove.onClick.AddListener(delegate { drag.RemoveModule(); });

		drag.OnBeginDrag(eventData);
		Destroy(this);
	}
}
