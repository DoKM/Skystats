using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientID : MonoBehaviour
{
    private static long _clientID = 801819794587254825;

    public static long ID
    {
        get
        {
            return _clientID;
        }
        set
        {
            _clientID = value;
        }
    }
}
