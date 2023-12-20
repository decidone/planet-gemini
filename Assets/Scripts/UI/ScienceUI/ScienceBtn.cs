using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// UTF-8 설정
public class ScienceBtn : MonoBehaviour
{
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
        if (!upgradeStart && !upgrade)
        {
            if (isLock && InfoWindow.instance.enabled)
            {
                if (InfoWindow.instance.totalAmountsEnough)
                {
                    upgradeStart = true;
                    InfoWindow.instance.SciUpgradeStart();
                    upgradeImg.enabled = true;
                    upgradeFunc.CoroutineSet(this, upgradeTime);
                }
            }
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
    }
}
