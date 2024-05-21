using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortalUIBtn : MonoBehaviour
{
    [SerializeField]
    Building building;
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

    public PreBuilding preBuilding;

    GameManager gameManager;

    private void Start()
    {
        preBuilding = PreBuilding.instance;
        gameManager = GameManager.instance;
        btn.onClick.AddListener(() => ButtonFunc());
    }

    void ButtonFunc()
    {
        if (!portal.PortalObjFind(objName))
        //if (!isLock && !portal.PortalObjFind(objName))
        {
            preBuilding.SetPortalImage(building, portal, gameManager.isPlayerInHostMap);
            preBuilding.isEnough = true;
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
