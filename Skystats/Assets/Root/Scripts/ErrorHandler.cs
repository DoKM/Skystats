using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ErrorHandler : MonoBehaviour
{
	#region Singleton
	public static ErrorHandler Instance;
	public void OnEnable()
	{
		if (!Instance) Instance = this;
		else Destroy(gameObject);
	}
	#endregion

	public float errorDisplayTime;

	private Queue<Error> errorQueue = new Queue<Error>();
	private GameObject currentDisplayingError;
	private GameObject errorPrefab;

	public bool processing;

	private void Start()
	{
		errorPrefab = Resources.Load<GameObject>("Prefabs/Error");
	}

	private void Update()
	{
		if (processing == false && errorQueue.Count > 0)
			StartCoroutine(ProcessErrors());
	}

	public void Push (Error error)
	{
		errorQueue.Enqueue(error);
	}

	public void InstantiateError (Error error)
	{
		currentDisplayingError = Instantiate(errorPrefab, transform);
		currentDisplayingError.transform.Find("Message").GetComponent<TMP_Text>().text = error.ErrorMessage;

		var headerText = $"{error.ErrorHeader} (#{error.ErrorCode})";
		currentDisplayingError.transform.Find("Background").Find("Header").GetComponent<TMP_Text>().text = headerText;
	}

	public IEnumerator ProcessErrors ()
	{
		processing = true;

		for (int i = 0; i < errorQueue.Count; i++)
		{
			yield return new WaitForSeconds(1);

			var currentError = errorQueue.Dequeue();
			InstantiateError(currentError);

			Debug.Log($"Processing error #{currentError.ErrorCode}");

			yield return new WaitForSeconds(errorDisplayTime);
			Destroy(currentDisplayingError);
		}

		processing = false;
	}

}

[Serializable]
public class Error
{
	public int ErrorCode { get; set; }
	public string ErrorMessage { get; set; }
	public string ErrorHeader { get; set; }
}


