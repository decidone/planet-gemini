using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class TempScienceDb : MonoBehaviour
{
    public static TempScienceDb instance;
    public Dictionary<string, List<int>> scienceNameDb = new Dictionary<string, List<int>>();

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

    public void SaveSciDb(string sciName, int sciLv)
    {
        if (scienceNameDb.ContainsKey(sciName))
        {
            if (!IsLevelExists(sciName, sciLv))
            {            
                scienceNameDb[sciName].Add(sciLv);
            }
        }
        else
        {
            scienceNameDb.Add(sciName, new List<int>());
            scienceNameDb[sciName].Add(sciLv);
        }
    }

    public bool IsLevelExists(string sciName, int sciLv)
    {
        if (scienceNameDb.ContainsKey(sciName))
        {
            List<int> levels = scienceNameDb[sciName];
            return levels.Contains(sciLv);
        }

        return false;
    }
}
