using System;
using System.Collections;
using System.Collections.Generic;
using ImaginationOverflow.UniversalDeepLinking;
using UnityEngine;

public class DeeplinkHandler : MonoBehaviour
{
    private void Awake()
    {
        DeepLinkManager.Instance.LinkActivated += InstanceOnLinkActivated;
    }

    private void InstanceOnLinkActivated(LinkActivation s)
    {
        StartCoroutine(LoadUser(s));
    }

    public IEnumerator LoadUser(LinkActivation s)
    {
        yield return new WaitForSeconds(1);
        var username = s.QueryString["u"];
        
        Main.Instance.username = username;
        Main.Instance.StartCoroutine(Main.Instance.IE_LoadProfiles());
    }

    private void OnDestroy()
    {
        DeepLinkManager.Instance.LinkActivated -= InstanceOnLinkActivated;
    }
}
