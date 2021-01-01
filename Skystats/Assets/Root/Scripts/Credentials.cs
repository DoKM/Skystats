using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Credentials
{
    private static string m_key = "";

    public static string key { 
        get 
        { 
            return m_key; 
        } 
        set 
        { 
            m_key = value; 
        } 
    }
}
