using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

// UTF-8 설정
public class ScienceBtn : MonoBehaviour
{
    public int btnIndex;
    public string sciName;
    public int level;
    public float upgradeTime;
    GameObject lockUI;
    public Image upgradeImg; 
    Button scBtn;
    bool isLock = true;
    bool upgradeStart = false;
    bool upgrade = false;
    public bool isCore = false;
    ScienceManager scienceManager;
    SciUpgradeFunc upgradeFunc;
    public ScienceInfoData scienceInfoData;
    public List<(int, int)> itemAmountList = new List<(int, int)>();    // 저장량 / 최대량

    void Start()
    {
        scienceManager = GameManager.instance.inventoryUiCanvas.GetComponent<ScienceManager>();
        upgradeFunc = SciUpgradeFunc.instance;
        scBtn = GetComponent<Button>();
        if (isCore)
            lockUI = transform.parent.Find("LockUi").gameObject;        
        else
            lockUI = transform.Find("LockUi").gameObject;
        
        upgradeImg = lockUI.transform.Find("Upgrade").gameObject.GetComponent<Image>();

        if (scBtn != null)
            scBtn.onClick.AddListener(ButtonFunc);
    }

    void ButtonFunc()
    {
        if (!upgradeStart && !upgrade && isLock)
        {
            scienceManager.OpenItemSetWindow();
        }
    }

    public void UpgradeFunc()
    {
        scienceManager.SciUpgradeEnd(sciName, level);
        LockUiActiveFalse();
        upgrade = true;
    }

    public void LockUiActiveFalse()
    {
        if (lockUI == null)
        {
            if (isCore)
                lockUI = transform.parent.Find("LockUi").gameObject;
            else
                lockUI = transform.Find("LockUi").gameObject;

            upgradeImg = lockUI.transform.Find("Upgrade").gameObject.GetComponent<Image>();
        }

        lockUI.SetActive(false);
        isLock = false;
    }

    public void SetInfo(string name, int coreLv, float time, bool core)
    {
        sciName = name;
        level = coreLv;
        upgradeTime = time;
        isCore = core;
        scienceInfoData = new ScienceInfoData();
        scienceInfoData = ScienceInfoGet.instance.GetBuildingName(sciName, level);

        foreach(var data in scienceInfoData.amounts)
        {
            itemAmountList.Add((0, data));
        }
    }

    public void ItemAddAmount(int index, int amount)
    {
        if (amount == 0)
            return;
        scienceManager.SyncSciBtnItem(btnIndex, index, amount);
    }

    public void SyncItemAddAmount(int index, int amount)
    {
        itemAmountList[index] = (itemAmountList[index].Item1 + amount, itemAmountList[index].Item2);
        ItemSaveEnd();
    }

    public void ItemSaveEnd()
    {
        foreach(var itemAmount in itemAmountList)
        {
            if(itemAmount.Item1 != itemAmount.Item2)
            {
                return;
            }
        }

        upgradeStart = true;
        upgradeImg.enabled = true;
        upgradeFunc.CoroutineSet(this, upgradeTime);
    }
}
