using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScienceBtn : MonoBehaviour
{
    public string iconName = null;
    GameObject lockUI = null;

    Button scBtn = null;
    bool isLock = true;
    // Start is called before the first frame update
    void Start()
    {
        scBtn = this.GetComponent<Button>();
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
