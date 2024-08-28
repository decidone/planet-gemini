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
    [SerializeField]
    bool isPortalObj;

    bool isLock;

    public PreBuilding preBuilding;

    GameManager gameManager;
    [SerializeField]
    GameObject confirmPanel;
    [SerializeField]
    Button okBtn;
    [SerializeField]
    Button canselBtn;


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
            if (isPortalObj)
            {
                preBuilding.SetPortalImage(building, portal, gameManager.isPlayerInHostMap, isPortalObj);
                preBuilding.isEnough = true;
            }
            else
            {
                confirmPanel.SetActive(true);
            }
        }
    }

    public void OkBtnFunc()
    {
        confirmPanel.SetActive(false);
        if (!gameManager.scienceBuildingSet)
        {
            portal.SetScienceBuildingServerRpc();
        }
    }

    public void CanselBtnFunc()
    {
        confirmPanel.SetActive(false);
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
