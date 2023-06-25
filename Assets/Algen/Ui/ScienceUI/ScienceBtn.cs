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

    // Start is called before the first frame update
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

    //Update is called once per frame
    void Update()
    {

    }

    void ButtonFunc()
    {
        if (InfoWindow.instance != null)
        { 
            if (isLock == true && InfoWindow.instance.enabled)
            {
                if (InfoWindow.instance.totalAmountsEnough)
                {
                    lockUI.SetActive(false);
                    InfoWindow.instance.SciUpgradeEnd();
                    isLock = false;
                }
            }
        }
    }
}
