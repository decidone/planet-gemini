using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogisticsClickEvent : MonoBehaviour
{
    public GameObject LogisticsUI;
    public SplitterFilterManager sFilterManager;    
    public ItemSpManager itemSpManager;

    Button closeBtn;
    Button splittercloseBtn;
    GameManager gameManager;

    LogisticsCtrl logisticsCtrl;

    public void LogisticsCheck()
    {
        gameManager = GameManager.instance;
        GameObject canvas = gameManager.GetComponent<GameManager>().inventoryUiCanvas;
        InventoryList inventoryList = canvas.GetComponent<InventoryList>();

        logisticsCtrl = GetComponent<LogisticsCtrl>();

        foreach (GameObject list in inventoryList.InventoryArr)
        {
            if (logisticsCtrl.GetComponent<SplitterCtrl>() && list.name == "SplitterMenu")
            {
                LogisticsUI = list;
                splittercloseBtn = LogisticsUI.transform.Find("CloseButton").gameObject.GetComponent<Button>();
                splittercloseBtn.onClick.AddListener(CloseUI);
                sFilterManager = canvas.GetComponent<SplitterFilterManager>();
            }
            else if (logisticsCtrl.GetComponent<ItemSpawner>() && list.name == "ItemSpwanerFilter")
            {
                LogisticsUI = list;
                splittercloseBtn = LogisticsUI.transform.Find("CloseButton").gameObject.GetComponent<Button>();
                splittercloseBtn.onClick.AddListener(CloseUI);
                itemSpManager = canvas.GetComponent<ItemSpManager>();
            }
        }
    }

    public void OpenUI()
    {
        if (logisticsCtrl.TryGetComponent(out SplitterCtrl splitter))
        {
            sFilterManager.SetSplitter(splitter);
            sFilterManager.OpenUI();
        }
        else if (logisticsCtrl.TryGetComponent(out ItemSpawner itemSpawner))
        {
            itemSpManager.SetItemSp(itemSpawner);
            itemSpManager.OpenUI();
        }
    }

    public void CloseUI()
    {
        if (logisticsCtrl.GetComponent<SplitterCtrl>())
        {
            sFilterManager.ReleaseInven();
            sFilterManager.CloseUI();
        }
        else if (logisticsCtrl.GetComponent<ItemSpawner>())
        {
            itemSpManager.ReleaseInven();
            itemSpManager.CloseUI();
        }
    }
}
