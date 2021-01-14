using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WardrobeSlot : Slot
{
	public override void FillItem(Item newItem)
	{
        ClearChildren(imageParent.transform);
        currentHoldingItem = newItem;

        if (flatItemImagePrefab == null) flatItemImagePrefab = Resources.Load<GameObject>("Prefabs/Slot/2D");
        InstantiateModel(Shader.Find("Custom/StencilObject"));
    }
}
