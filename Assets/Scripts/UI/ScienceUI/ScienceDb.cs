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
    // 0 생산속도, 1 Hp, 2 소비량 감소, 3 방어력

    // 타워
    bool[] increasedTower = new bool[2];
    // 0 공격력, 1 공격 속도

    // 유닛
    bool[] increasedUnit = new bool[4];
    // 0 Hp, 1 데미지, 2 공격속도, 3 방어력

    // 유닛최대치
    int[] increasedUnitExpansion = new int[] { 12, 8, 5 };
    // 0 10증가, 1 10증가, 2 10증가

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
            EffectUpgrade(0);
        }
        else if (sciName == "TowerUpgrade")
        {
            increasedTower[sciLv - 1] = true;
            EffectUpgrade(1);
        }
        else if (sciName == "UnitUpgrade")
        {
            increasedUnit[sciLv - 1] = true;
            EffectUpgrade(2);
        }
        else if (sciName == "AICommandExpansion")
        {
            GameManager.instance.playerUnitLimit += increasedUnitExpansion[sciLv - 1];
            GameManager.instance.PlayerUnitCount(0);
        }

        if (!isLoad)
        {
            WarningWindowSetServerRpc(sciName);
        }
    }

    void EffectUpgrade(int index)   // index : 0 Structure, 1 Tower, 2 Unit
    {
        switch (index)
        {
            case 0:
                {
                    Structure[] scripts = Object.FindObjectsByType<Structure>(FindObjectsSortMode.None);
                    foreach (Structure script in scripts)
                    {
                        script.onEffectUpgradeCheck.Invoke();
                    }
                    break;
                }
            case 1:
                {
                    TowerAi[] scripts = Object.FindObjectsByType<TowerAi>(FindObjectsSortMode.None);
                    foreach (TowerAi script in scripts)
                    {
                        script.onEffectUpgradeCheck.Invoke();
                    }
                    break;
                }
            case 2:
                {
                    UnitAi[] scripts = Object.FindObjectsByType<UnitAi>(FindObjectsSortMode.None);
                    foreach (UnitAi script in scripts)
                    {
                        script.onEffectUpgradeCheck.Invoke();
                    }
                    break;
                }
            default:
                break;
        }
    }
        
    public bool[] IncreasedStructureCheck(int index)
    {
        switch (index)
        {
            case 0:
                {
                    return increasedStructure;
                }
            case 1:
                {
                    return increasedTower;
                }
            case 2:
                {
                    return increasedUnit;
                }
            default:
                return null;
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

    public void ScienceWindowItemAdd(Item item, int scienceInfoDataIndex, int inputAmount, int btnIndex, bool isPlayerHostMap)
    {
        int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(item);
        ScienceWindowItemAddServerRpc(itemIndex, scienceInfoDataIndex, inputAmount, btnIndex, isPlayerHostMap);
    }

    [ServerRpc(RequireOwnership = false)]
    void ScienceWindowItemAddServerRpc(int itemIndex, int scienceInfoDataIndex, int inputAmount, int btnIndex, bool isPlayerHostMap)
    {
        Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
        ScienceBtn btn = ScienceManager.instance.scienceBtns[btnIndex];

        int maxInputItemAmount = btn.itemAmountList[scienceInfoDataIndex].Item2 - btn.itemAmountList[scienceInfoDataIndex].Item1;
        Inventory inven; 
            if (isPlayerHostMap)
            inven = GameManager.instance.hostMapInven;
        else
            inven = GameManager.instance.clientMapInven;

        int invenItemAmount = inven.totalItems[item];

        if (inputAmount > invenItemAmount)   // 인벤 아이템보다 요청이 많은 경우
        {
            inputAmount = invenItemAmount;
        }

        if (inputAmount == 0 || maxInputItemAmount == 0)
            return;

        Overall.instance.OverallConsumption(item, maxInputItemAmount);
        inven.Sub(item, inputAmount);
        btn.ItemAddAmount(scienceInfoDataIndex, inputAmount);
    }

    public void LoadData(List<ScienceData> data)
    {
        for (int i = 0; i < scienceBtns.Length; i++)
        {
            //if (scienceBtns[i].isCore)
            ////if (scienceBtns[i].isCore && ScienceManager.instance.CoreSaveCheck(scienceBtns[i]))
            //    continue;
            for (int j = 0; j < data[i].saveItemCount.Count; j++)
            {
                scienceBtns[i].LoadItemAddAmount(j, data[i].saveItemCount[j]);
            }

            scienceBtns[i].LoadEnd(data[i].upgradeState, data[i].lockCheck, data[i].upgradeTime);

            if (data[i].upgradeState == 2 && !scienceBtns[i].scienceInfoData.basicScience)
                ScienceManager.instance.isAnyUpgradeCompleted = true;
        }
    }

    public void LoadSet(List<ScienceData> data)
    {
        LoadData(data);
    }
}
