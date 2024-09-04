using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

// UTF-8 설정
public class TempScienceDb : NetworkBehaviour
{
    public static TempScienceDb instance;
    //public Dictionary<string, List<int>> scienceNameDb = new Dictionary<string, List<int>>();
    public Dictionary<string, Dictionary<int, int>> scienceNameDb = new Dictionary<string, Dictionary<int, int>>();
    public ScienceBtn[] scienceBtns;
    public int[] coreLevelUpgrade = new int[5] { 0, 0, 0, 0, 0 }; // 코어 레벨 당 업그레이드 개수
    List<ScienceData> getData = new List<ScienceData>();
    bool loadDataSet = false;
    public int coreLevel = 1;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public void ScienceBtnArrGet(ScienceBtn[] arr)
    {
        scienceBtns = arr;
        if (loadDataSet)
        {
            LoadData(getData);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncSciBtnItemServerRpc(int btnIndex, int index, int amount)
    {
        SyncSciBtnItemClientRpc(btnIndex, index, amount);
    }

    [ClientRpc]
    void SyncSciBtnItemClientRpc(int btnIndex, int index, int itemAmount)
    {
        scienceBtns[btnIndex].SyncItemAddAmount(index, itemAmount);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SciBtnUpgradeServerRpc(int btnIndex)
    {
        SciBtnUpgradeClientRpc(btnIndex);
    }

    [ClientRpc]
    public void SciBtnUpgradeClientRpc(int btnIndex)
    {
        scienceBtns[btnIndex].ItemSaveEnd();
    }

    public void CoreLevelUpgradeCheck(int coreLevel)
    {
        coreLevelUpgrade[coreLevel -1] += 1;
    }

    public int CoreLevelUpgradeCount(int coreLv)
    {
        return coreLevelUpgrade[coreLv - 1];
    }

    public void SaveSciDb(string sciName, int sciLv, int coreLv, bool isLoad)
    {
        if (scienceNameDb.ContainsKey(sciName))
        {
            if (!IsLevelExists(sciName, sciLv))
            {
                scienceNameDb[sciName].Add(sciLv, coreLv);
            }
        }
        else
        {
            scienceNameDb.Add(sciName, new Dictionary<int, int>());
            scienceNameDb[sciName].Add(sciLv, coreLv);
        }

        if (sciName != "Core")
        {
            CoreLevelUpgradeCheck(coreLv);
        }

        if (!isLoad)
        {
            WarningWindowSetServerRpc(sciName);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    void WarningWindowSetServerRpc(string sciName)
    {
        WarningWindowSetClientRpc(sciName);
    }

    [ClientRpc]
    void WarningWindowSetClientRpc(string sciName)
    {
        WarningWindow.instance.WarningTextSet(sciName + " Upgrade Complete.");
    }


    public bool IsLevelExists(string sciName, int sciLv)
    {
        if (scienceNameDb.ContainsKey(sciName))
        {
            Dictionary<int, int> levels = scienceNameDb[sciName];
            return levels.ContainsKey(sciLv);
        }

        return false;
    }

    public void LoadData(List<ScienceData> data)
    {
        for (int i = 0; i < scienceBtns.Length; i++)
        {
            if (scienceBtns[i].isCore && ScienceManager.instance.CoreSaveCheck(scienceBtns[i]))
                continue;
            for (int j = 0; j < data[i].saveItemCount.Count; j++)
            {
                scienceBtns[i].LoadItemAddAmount(j, data[i].saveItemCount[j]);
            }

            scienceBtns[i].LoadEnd(data[i].upgradeState, data[i].lockCheck, data[i].upgradeTime);

            if (data[i].upgradeState == 2)
                ScienceManager.instance.isAnyUpgradeCompleted = true;
        }
    }

    public void LoadSet(List<ScienceData> data)
    {
        getData = data;
        loadDataSet = true;
    }
}
