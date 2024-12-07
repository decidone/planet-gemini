using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

// UTF-8 설정
public class ScienceDb : NetworkBehaviour
{
    public static ScienceDb instance;
    //public Dictionary<string, List<int>> scienceNameDb = new Dictionary<string, List<int>>();
    public Dictionary<string, Dictionary<int, int>> scienceNameDb = new Dictionary<string, Dictionary<int, int>>();
    public ScienceBtn[] scienceBtns;
    public int[] coreLevelUpgrade = new int[5] { 0, 0, 0, 0, 0 }; // 코어 레벨 당 업그레이드 개수
    public int coreLevel = 1;

    // 건물
    bool[] increasedStructure = new bool[4];
    // 0 생산속도, 1 Hp, 2 인풋아웃풋 속도, 3 에너지 소비량 감소

    // 유닛
    bool[] increasedUnit = new bool[4];
    // 0 Hp, 1 데미지, 2 공격속도, 3 방어력

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

        if (sciName == "StructureUpgrade")
        {
            increasedStructure[sciLv - 1] = true;
            EffectUpgrade(true);
        }
        else if (sciName == "UnitUpgrade")
        {
            increasedUnit[sciLv - 1] = true;
            EffectUpgrade(false);
        }

        if (!isLoad)
        {
            WarningWindowSetServerRpc(sciName);
        }
    }

    void EffectUpgrade(bool isStr)
    {
        if (isStr)
        {
            Structure[] scripts = FindObjectsOfType<Structure>();
            foreach (Structure script in scripts)
            {
                script.onEffectUpgradeCheck.Invoke();
            }
        }
        else
        {
            UnitAi[] scripts = FindObjectsOfType<UnitAi>();
            foreach (UnitAi script in scripts)
            {
                script.onEffectUpgradeCheck.Invoke();
            }
        }
    }
        
    public bool[] IncreasedStructureCheck(bool isStr)
    {
        if (isStr)
        {
            return increasedStructure;
        }
        else
        {
            return increasedUnit;
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
        LoadData(data);
    }
}
