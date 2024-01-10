using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortalUIBtn : MonoBehaviour
{
    [SerializeField]
    GameObject obj;
    [SerializeField]
    Item objItem;
    [SerializeField]
    Image icon;
    [SerializeField]
    Button btn;
    [SerializeField]
    GameObject LockUi;
    public string objName;
    public Portal portal;

    bool isLock;

    GameObject preBuilding;

    private void Start()
    {
        btn.onClick.AddListener(() => ButtonFunc());
    }

    void ButtonFunc()
    {
        if (!isLock && !portal.PortalObjFind(objName))
        {
            preBuilding = GameManager.instance.preBuildingObj;
            preBuilding.SetActive(true);
            PreBuilding pre = preBuilding.GetComponent<PreBuilding>();
            pre.SetPortalImage(obj, portal);
            pre.isEnough = true;
        }
    }

    public void SetProduction(Production _prod)
    {
        portal = _prod.GetComponent<Portal>();
    }

    public void SciUpgradeCheck()
    {
        LockUi.SetActive(false);
        isLock = false;        
    }

    public void SetData()
    {
        objName = objItem.name;
        icon.sprite = objItem.icon;
        isLock = true;
    }
}
