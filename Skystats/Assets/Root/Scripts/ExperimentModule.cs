using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Helper;

public class ExperimentModule : MonoBehaviour
{
    public GameObject experimentPrefab;
    public Transform parent, title;

	private void Start()
    {
        Main.Instance.OnLoadProfile += InstantiateModule;

        if (Main.Instance.currentProfile != null && !string.IsNullOrEmpty(Main.Instance.currentProfile.FormattedUsername))
            InstantiateModule(Main.Instance, new OnLoadProfileEventArgs { profile = Main.Instance.currentProfile });
    }

    // Update is called once per frame
    public void InstantiateModule(object sender, OnLoadProfileEventArgs e)
    {
        if (e.profile.Experiments == null) return;

        Main.Instance.ClearChildren(parent);

		foreach (var experiment in e.profile.Experiments)
            Instantiate(experimentPrefab, parent).GetComponent<ExperimentItem>().currentExperiment = experiment;

		for (int i = 0; i < 6; i++)
		    Global.UpdateCanvasElement(parent as RectTransform);

        for (int i = 0; i < 6; i++)
            if (transform != null)
                Global.UpdateCanvasElement(transform as RectTransform);
    }

    
}
