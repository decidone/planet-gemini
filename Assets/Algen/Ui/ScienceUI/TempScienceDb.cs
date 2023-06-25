using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempScienceDb : MonoBehaviour
{
    public static TempScienceDb instance;
    public List<string> scienceNameDb = new List<string>();
    public int coreLevel = 1;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of InfoWindow found!");
            return;
        }
        instance = this;
    }

    public void SaveSciDb(string sciName)
    {
        scienceNameDb.Add(sciName);
    }
}
