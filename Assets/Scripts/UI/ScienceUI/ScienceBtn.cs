using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// UTF-8 설정
public class ScienceBtn : MonoBehaviour
{
    public int btnIndex;
    public string sciName;
    public string gameName;
    public int coreLevel;
    public int level;
    public float upgradeTime;
    GameObject lockUI;
    Image btnImage;
    public Image upgradeImg; 
    Button scBtn;
    [SerializeField]
    bool isLock = true;
    [HideInInspector]
    public bool upgradeStart = false;
    [HideInInspector]
    public bool upgrade = false;
    [HideInInspector]
    public bool isCore = false;
    public bool isMain;
    ScienceManager scienceManager;
    SciUpgradeFunc upgradeFunc;
    public ScienceInfoData scienceInfoData;
    public List<(int, int)> itemAmountList = new List<(int, int)>();    // 저장량 / 최대량
    //public ScienceBtn othCoreBtn;
    bool basicScience;
    SoundManager soundManager;
    private void Awake()
    {
        soundManager = SoundManager.instance;
        scienceManager = GameManager.instance.inventoryUiCanvas.GetComponent<ScienceManager>();
        scBtn = GetComponent<Button>();
    }
    void Start()
    {
        if (scBtn != null)
            scBtn.onClick.AddListener(ButtonFunc);
    }

    public void UiSetting()
    {
        upgradeFunc = SciUpgradeFunc.instance;

        if (isCore)
            lockUI = transform.parent.Find("LockUi").gameObject;
        else
            lockUI = transform.Find("LockUi").gameObject;
        upgradeImg = lockUI.transform.Find("Upgrade").gameObject.GetComponent<Image>();
        btnImage = GetComponent<Image>();
        if (basicScience)
        {
            Invoke(nameof(UpgradeFuncInvoke), 0.2f);
        }
    }

    void UpgradeFuncInvoke()
    {
        UpgradeFunc(true);
    }

    void ButtonFunc()
    {
        if (isCore && !scienceManager.CoreUpgradeCheck(coreLevel))
        {
            scienceManager.CoreUpgradeWarningWindow(coreLevel);
        }
        else
        {
            if (scienceInfoData.coreLv > ScienceDb.instance.coreLevel)
            {
                scienceManager.coreCtrl[coreLevel - 1].StartBlink();
            }
            else
            {
                if (ItemFullCheck())
                {
                    if (!upgradeStart && !upgrade && isLock)
                    {
                        scienceManager.OpenUpgradeWindow();
                    }
                }
                else
                {
                    if (!upgradeStart && !upgrade && isLock)
                    {
                        scienceManager.OpenItemSetWindow();
                    }
                }
            }
        }
    }

    public void UpgradeFunc(bool isLoad)
    {
        if(scienceManager == null)
            scienceManager = GameManager.instance.inventoryUiCanvas.GetComponent<ScienceManager>();

        scienceManager.SciUpgradeEnd(sciName, level, coreLevel, isLoad);
        LockUiActiveFalse();
        upgrade = true;
        btnImage.color = new Color(255, 255, 255);
    }

    public void LockUiActiveFalse()
    {
        lockUI.SetActive(false);
        isLock = false;
    }

    public void SetInfo(string name, int lv, int coreLv, float time, bool core, string _gameName, bool _basicScience)
    {
        sciName = name;
        gameName = _gameName;
        level = lv;
        coreLevel = coreLv;
        upgradeTime = time;
        isCore = core;
        scienceInfoData = new ScienceInfoData();
        scienceInfoData = ScienceInfoGet.instance.GetBuildingName(sciName, level);

        foreach(var data in scienceInfoData.amounts)
        {
            itemAmountList.Add((0, data));
        }

        basicScience = _basicScience;
    }

    public void ItemAddAmount(int index, int amount)
    {
        if (amount == 0)
            return;


        if (!scienceManager)
        {
            scienceManager = GameManager.instance.inventoryUiCanvas.GetComponent<ScienceManager>();
        }

        scienceManager.SyncSciBtnItem(btnIndex, index, amount);
    }

    public void SyncItemAddAmount(int index, int amount)
    {
        itemAmountList[index] = (itemAmountList[index].Item1 + amount, itemAmountList[index].Item2);

        if (ItemFullCheck())
        {
            btnImage.color = new Color(0, 255, 100);
        }
    }

    public bool ItemFullCheck()
    {
        foreach (var itemAmount in itemAmountList)
        {
            if (itemAmount.Item1 != itemAmount.Item2)
            {
                return false;
            }
        }

        return true;
    }

    public void ItemSaveEnd()
    {
        if (!ItemFullCheck())
            return;

        upgradeStart = true;

        upgradeImg.enabled = true;
        upgradeFunc.CoroutineSet(this, upgradeTime);
    }

    public void LoadItemAddAmount(int index, int amount)
    {
        if (amount == 0)
            return;

        if (scienceManager == null)
            scienceManager = GameManager.instance.inventoryUiCanvas.GetComponent<ScienceManager>();

        itemAmountList[index] = (itemAmountList[index].Item1 + amount, itemAmountList[index].Item2);
    }

    public void LoadEnd(float upgradeState, bool isLockCheck, float upgradeTimeSet)
    {
        if (upgradeState == 0)
        {
            if (ItemFullCheck())
            {
                btnImage.color = new Color(0, 255, 100);
            }
        }
        else if (upgradeState == 1)
        {
            upgradeImg.enabled = true;
            upgradeStart = true;
            upgradeFunc.LoadCoroutineSet(this, upgradeTime, upgradeTimeSet);
            btnImage.color = new Color(0, 255, 100);
        }
        else if (upgradeState == 2)
        {
            upgradeStart = true;
            UpgradeFunc(true);
        }
        isLock = isLockCheck;
    }

    public ScienceData SaveData()
    {
        ScienceData data = new ScienceData();

        foreach (var itemAmount in itemAmountList)
        {
            data.saveItemCount.Add(itemAmount.Item1); 
        }

        int upgradeState = 0;

        if(upgradeStart)
        {
            if (upgrade)
            {
                upgradeState = 2;
            }
            else
            {
                upgradeState = 1;
                data.upgradeTime = upgradeFunc.UpgradeTimeReturn(this);
            }
        }

        data.upgradeState = upgradeState;
        data.lockCheck = isLock;

        return data;
    }
}
