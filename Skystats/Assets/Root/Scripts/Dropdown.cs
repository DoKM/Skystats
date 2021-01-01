using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Helper;

public class Dropdown : MonoBehaviour
{
    public void Trigger ()
	{
		Global.UpdateScrollView();
		GetComponent<Animator>().SetBool("Open", !GetComponent<Animator>().GetBool("Open"));
		Global.UpdateScrollView();
	}
}
