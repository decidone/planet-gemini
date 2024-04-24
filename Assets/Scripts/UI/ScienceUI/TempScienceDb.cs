using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

// UTF-8 설정
public class TempScienceDb : NetworkBehaviour
{
    public static TempScienceDb instance;
    public Dictionary<string, List<int>> scienceNameDb = new Dictionary<string, List<int>>();
    public ScienceBtn[] scienceBtns;

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

    public void ScienceBtnArrGet(ScienceBtn[] arr)
    {
        scienceBtns = arr;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncSciBtnItemServerRpc(int btnIndex, int index, int amount)
    {
        SyncSciBtnItemClientRpc(btnIndex, index, amount);
    }

    [ClientRpc]
    public void SyncSciBtnItemClientRpc(int btnIndex, int index, int itemAmount)
    {
        scienceBtns[btnIndex].SyncItemAddAmount(index, itemAmount);
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
