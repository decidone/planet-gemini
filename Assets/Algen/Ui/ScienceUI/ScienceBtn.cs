using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScienceBtn : MonoBehaviour
{
    public string sciName = null;
    public int level = 0;
    GameObject lockUI = null;
    Button scBtn = null;
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
            if (isLock == true && InfoWindow.instance.enabled)
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
}
