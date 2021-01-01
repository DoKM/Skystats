using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoMenu : MonoBehaviour
{
    public MenuType type;
    public Transform tab;

    public void Activate ()
	{
        Helper.Global.FindParentWithComponent<InformationList>(transform).ActivateMenu(type);
	}


}
