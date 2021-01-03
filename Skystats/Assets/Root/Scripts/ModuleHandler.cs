using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Helper;
using System.IO;
using Newtonsoft.Json;

[Serializable]
public class ModuleList
{
	public List<Module> Modules { get; set; }
}

[Serializable]
public class Module
{
	public string Name;
	public int Index;
	public bool IsMinimized;
	public Vector3 SavedPosition;
}

public class ModuleHandler : MonoBehaviour
{
	public bool loadSavedPosition;
	public RectTransform currentModule;

	public List<RectTransform> modules;
	public ModuleList moduleList;

	public RectTransform entryTransform;
	public Module entryModule;

	public void Start()
	{
		DirectoryInfo dir = new DirectoryInfo($"{Application.persistentDataPath}/modules/{name.ToLower()}");
		if (!dir.Exists) dir.Create();

		LoadModules();
	}

	[Button]
	public void RemoveEntry ()
	{
		if (modules.Contains(entryTransform))
		{
			moduleList.Modules.Remove(moduleList.Modules.Find(x => x.Index == modules.IndexOf(entryTransform)));
			modules.Remove(entryTransform);
			Destroy(entryTransform.gameObject);
		}
	}

	[Button]
	public void AddEntry ()
	{
		CreateModule(entryModule);
	}

	public GameObject CreateModule (Module module)
	{
		var prefab = Resources.Load<GameObject>($"Prefabs/Modules/{module.Name}");
		if (prefab)
		{
			var moduleObj = Instantiate(prefab, transform);
			moduleObj.name = module.Name;

			var md = Global.FindObjectsOfTypeInTranform<ModuleDragger>(moduleObj.transform);
			md[0].LoadPosition(module.SavedPosition, module.IsMinimized);

			modules.Add(moduleObj.transform as RectTransform);
			return moduleObj;
		}

		return null;
	}

	public void UpdateModule (RectTransform newModule)
	{
		currentModule = newModule;
		SetIndex(currentModule, 0);
		UpdatePositions();

		SaveModules();
	}

	public void UpdateModules ()
	{
		foreach (var module in modules)
			for (int i = 0; i < 4; i++)
				Global.UpdateCanvasElement(module);
	}

	[Button]
	public void UpdatePositions ()
	{
		var count = -300 * modules.Count;
		foreach (var module in modules)
		{
			module.localPosition = new Vector3(module.localPosition.x, module.localPosition.y, count);
			module.SetAsFirstSibling();
			count += 300;
		}

		moduleList.Modules = new List<Module>();
		foreach (var module in modules)
			moduleList.Modules.Add(
				new Module { Name = module.name, 
							 Index = modules.IndexOf(module), 
							 SavedPosition = module.anchoredPosition, 
							 IsMinimized = !Global.FindObjectsOfTypeInTranform<ModuleDragger>(module)[0].isMinimized 
						   }
				);
	}

	public void SetIndex (RectTransform module, int index)
	{
		modules.Remove(module);
		modules.Insert(index, module);
	}

	public void LoadModules ()
	{
		var text = File.Exists($"{Application.persistentDataPath}/modules/{name.ToLower()}/modules.txt")
				   ? File.ReadAllText($"{Application.persistentDataPath}/modules/{name.ToLower()}/modules.txt")
				   : Resources.Load<TextAsset>($"Defaults/{name.ToLower()}/modules") != null 
							? Resources.Load<TextAsset>($"Defaults/{name.ToLower()}/modules").text
							: Resources.Load<TextAsset>("Defaults/modules").text;
		moduleList = JsonConvert.DeserializeObject<ModuleList>(text);

		if (moduleList.Modules != null)
		{
			var indexes = new Dictionary<RectTransform, int>();

			foreach (var module in moduleList.Modules)
				indexes.Add(CreateModule(module).transform as RectTransform, module.Index);

			foreach (var item in indexes)
				SetIndex(item.Key, item.Value);

			UpdatePositions();
		}
	}

	[Button]
	public void SaveModules()
	{
		var moduleData = JsonConvert.SerializeObject(moduleList);
		File.WriteAllText($"{Application.persistentDataPath}/modules/{name.ToLower()}/modules.txt", moduleData);
	}

	public void Remove ()
	{
		DirectoryInfo dir = new DirectoryInfo($"{Application.persistentDataPath}/modules/{name.ToLower()}");
		if (dir.Exists)
			Directory.Delete(dir.FullName, true);
	}
}
