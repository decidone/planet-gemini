using Steamworks.ServerList;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// UTF-8 설정
public class LogisticsClickEvent : MonoBehaviour
{
    public GameObject LogisticsUI;
    public SplitterFilterManager sFilterManager;
    public UnloaderManager unloaderManager;
    public ItemSpManager itemSpManager;

    Button logisticsCloseBtn;
    GameManager gameManager;

    LogisticsCtrl logisticsCtrl;
    InventoryList inventoryList;

    SoundManager soundManager;

    public bool openUI;

    private void Start()
    {
        soundManager = SoundManager.instance;
    }


    public bool LogisticsCheck()
    {
        bool canOpen = false;
        gameManager = GameManager.instance;
        GameObject canvas = gameManager.GetComponent<GameManager>().inventoryUiCanvas;
        inventoryList = canvas.GetComponent<InventoryList>();
        logisticsCtrl = GetComponent<LogisticsCtrl>();

        foreach (GameObject obj in inventoryList.InventoryArr)
        {
            if (logisticsCtrl.TryGetComponent(out SplitterCtrl splitterCtrl) && obj.name == "LogisticsMenu")
            {
                LogisticsUI = obj;
                logisticsCloseBtn = LogisticsUI.transform.Find("CloseButton").gameObject.GetComponent<Button>();
                logisticsCloseBtn.onClick.RemoveAllListeners();
                logisticsCloseBtn.onClick.AddListener(CloseUI);
                sFilterManager = canvas.GetComponent<SplitterFilterManager>();

                canOpen = true;
            }
            else if (logisticsCtrl.TryGetComponent(out Unloader unloader) && obj.name == "LogisticsMenu")
            {
                LogisticsUI = obj;
                logisticsCloseBtn = LogisticsUI.transform.Find("CloseButton").gameObject.GetComponent<Button>();
                logisticsCloseBtn.onClick.RemoveAllListeners();
                logisticsCloseBtn.onClick.AddListener(CloseUI);
                unloaderManager = canvas.GetComponent<UnloaderManager>();

                canOpen = true;
            }
            else if (logisticsCtrl.GetComponent<ItemSpawner>() && obj.name == "ItemSpwanerFilter")
            {
                LogisticsUI = obj;
                logisticsCloseBtn = LogisticsUI.transform.Find("CloseButton").gameObject.GetComponent<Button>();
                logisticsCloseBtn.onClick.RemoveAllListeners();
                logisticsCloseBtn.onClick.AddListener(CloseUI);
                itemSpManager = canvas.GetComponent<ItemSpManager>();
                canOpen = true;
            }
        }

        return canOpen;
    }

    public void OpenUI()
    {
        openUI = true;
        if (logisticsCtrl.TryGetComponent(out SplitterCtrl splitter))
        {
            sFilterManager.SetSplitter(splitter);

            if (splitter.level == 1) // 스마트 스플리터
            {
                var uiList = inventoryList.LogisticsArr[0].GetComponent<SplitterMenu>().SetMenu(splitter.dirNum, splitter);
                sFilterManager.GetObjArr(uiList.Item1, uiList.Item2, uiList.Item3, uiList.Item4, true);
                inventoryList.LogisticsArr[0].gameObject.SetActive(true);
            }
            else if (splitter.level == 0) // 스플리터
            {
                var uiList = inventoryList.LogisticsArr[1].GetComponent<SplitterMenu>().SetMenu(splitter.dirNum, splitter);
                sFilterManager.GetObjArr(uiList.Item1, uiList.Item2, uiList.Item3, uiList.Item4, false);
                inventoryList.LogisticsArr[1].gameObject.SetActive(true);
            }

            //inventoryList.LogisticsArr[0].gameObject.SetActive(true);
            gameManager.SelectPointSpawn(splitter.gameObject);
            sFilterManager.OpenUI();
        }
        else if (logisticsCtrl.TryGetComponent(out Unloader unloader))
        {
            unloaderManager.SetUnloader(unloader);
            inventoryList.LogisticsArr[2].gameObject.SetActive(true);
            gameManager.SelectPointSpawn(unloader.gameObject);
            unloaderManager.OpenUI();
        }
        else if (logisticsCtrl.TryGetComponent(out ItemSpawner itemSpawner))
        {
            itemSpManager.SetItemSp(itemSpawner);
            gameManager.SelectPointSpawn(itemSpawner.gameObject);
            itemSpManager.OpenUI();
        }
    }

    public void CloseUI()
    {
        openUI = false;
        if (logisticsCtrl.TryGetComponent(out SplitterCtrl splitter))
        {
            sFilterManager.ReleaseInven();

            if (splitter.level == 1) // 스마트 스플리터
            {
                inventoryList.LogisticsArr[0].gameObject.SetActive(false);
                splitter.onFilterChangedCallback = null;
            }
            else if (splitter.level == 0) // 스플리터
            {
                inventoryList.LogisticsArr[1].gameObject.SetActive(false);
                splitter.onFilterChangedCallback = null;
            }

            //inventoryList.LogisticsArr[0].gameObject.SetActive(false);
            gameManager.SelectPointRemove();
            sFilterManager.CloseUI();
        }
        else if (logisticsCtrl.GetComponent<Unloader>())
        {
            unloaderManager.ReleaseInven();
            inventoryList.LogisticsArr[2].gameObject.SetActive(false);
            gameManager.SelectPointRemove();
            unloaderManager.CloseUI();
        }
        else if (logisticsCtrl.GetComponent<ItemSpawner>())
        {
            itemSpManager.ReleaseInven();
            gameManager.SelectPointRemove();
            itemSpManager.CloseUI();
        }
        GameManager.instance.CheckAndCancelFocus(logisticsCtrl);

        soundManager.PlayUISFX("CloseUI");
    }

    //public void CloseTest()
    //{
    //    openUI = false;
    //    if (logisticsCtrl.GetComponent<SplitterCtrl>())
    //    {
    //        sFilterManager.ReleaseInven();
    //        inventoryList.LogisticsArr[0].gameObject.SetActive(false);
    //        gameManager.SelectPointRemove();
    //    }
    //    else if (logisticsCtrl.GetComponent<Unloader>())
    //    {
    //        unloaderManager.ReleaseInven();
    //        inventoryList.LogisticsArr[1].gameObject.SetActive(false);
    //        gameManager.SelectPointRemove();
    //    }
    //    else if (logisticsCtrl.GetComponent<ItemSpawner>())
    //    {
    //        itemSpManager.ReleaseInven();
    //        gameManager.SelectPointRemove();
    //    }
    //    soundManager.PlayUISFX("CloseUI");
    //}
}
