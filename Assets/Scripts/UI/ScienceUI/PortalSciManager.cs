using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalSciManager : MonoBehaviour
{
    TempScienceDb scienceDb;
    public string[] portalSciName;
    public Dictionary<string, bool> portalSciDic = new Dictionary<string, bool>();
    GameManager gameManager;
    GameObject canvas;
    public Dictionary<string, PortalUIBtn> UIBtnData = new Dictionary<string, PortalUIBtn>();

    #region Singleton
    public static PortalSciManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of GameManager found!");
            return;
        }

        instance = this;
    }
    #endregion

    private void Start()
    {
        scienceDb = TempScienceDb.instance;
        gameManager = GameManager.instance;
        canvas = gameManager.inventoryUiCanvas;
        GetUIFunc();
        portalSciName = new string[] { "PortalItemIn", "PortalItemOut", "PortalUnitIn", "PortalUnitOut" };

        for (int i = 0; i < portalSciName.Length; i++)
        {
            if (scienceDb.scienceNameDb.ContainsKey(portalSciName[i]))
            {
                portalSciDic.Add(portalSciName[i], true);
                if(UIBtnData.TryGetValue(portalSciName[i], out PortalUIBtn btn))
                {
                    btn.SciUpgradeCheck();
                }
            }
            else
            {
                portalSciDic.Add(portalSciName[i], false);
            }
        }
    }

    public void PortalSciUpgrade(string sciName)
    {
        if (portalSciDic.ContainsKey(sciName))
        {
            portalSciDic[sciName] = true;
            if (UIBtnData.TryGetValue(sciName, out PortalUIBtn btn))
            {
                btn.SciUpgradeCheck();
            }
        }
    }

    void GetUIFunc()
    {
        InventoryList inventoryList = canvas.GetComponent<InventoryList>();
        GameObject portalUI;
        PortalUIBtn[] portalObjBtn = null;

        foreach (GameObject list in inventoryList.StructureStorageArr)
        {
            if (list.name == "Portal")
            {
                portalUI = list;
                portalObjBtn = portalUI.GetComponentsInChildren<PortalUIBtn>();
                foreach (PortalUIBtn btn in portalObjBtn)
                {
                    btn.SetData();
                }
            }
        }

        for (int i = 0; i < portalObjBtn.Length; i++) 
        {
            UIBtnData.Add(portalObjBtn[i].objName, portalObjBtn[i]);
        }
    }
}
