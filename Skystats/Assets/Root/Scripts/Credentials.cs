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

    public static void UpdateKey(string newKey)
    {
        key = newKey;
        SaveKey(newKey);
    }

    public static void SaveKey(string key)
    {
        var aes = new AES();
        var encryptedKey = aes.Encrypt(key);
        PlayerPrefs.SetString("m_k", encryptedKey);
    }

    public static string LoadKey()
    {
        var aes = new AES();
        var key = PlayerPrefs.GetString("m_k", "");
        key = key != "" ? aes.Decrypt(key) : null;

        return key;
    }

}
