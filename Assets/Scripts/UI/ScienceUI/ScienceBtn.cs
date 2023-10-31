using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// UTF-8 설정
public class ScienceBtn : MonoBehaviour
{
    public string sciName;
    public int level;
    GameObject lockUI;
    Button scBtn;
    bool isLock = true;
    public bool isCore = false;

    void Start()
    {
        scBtn = this.GetComponent<Button>();
        if(isCore)
            lockUI = this.transform.parent.Find("LockUi").gameObject;
        else
            lockUI = this.transform.Find("LockUi").gameObject;

        if (scBtn != null)
            scBtn.onClick.AddListener(ButtonFunc);
    }

    void ButtonFunc()
    {
        if (InfoWindow.instance != null && sciName != "")
        { 
            if (isLock && InfoWindow.instance.enabled)
            {
                if (InfoWindow.instance.totalAmountsEnough)
                {
                    InfoWindow.instance.SciUpgradeEnd();
                    LockUiActiveFalse();
                }
            }
        }
    }

    public void LockUiActiveFalse()
    {
        if(lockUI == null)
        {
            if (isCore)
                lockUI = this.transform.parent.Find("LockUi").gameObject;
            else
                lockUI = this.transform.Find("LockUi").gameObject;
        }

        lockUI.SetActive(false);
        isLock = false;
    }

    public void SetInfo(string name, int coreLv, bool core)
    {
        sciName = name;
        level = coreLv;
        isCore = core;
    }
}
