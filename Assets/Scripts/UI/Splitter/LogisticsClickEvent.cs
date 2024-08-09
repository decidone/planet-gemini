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
            if (logisticsCtrl.TryGetComponent(out SplitterCtrl splitterCtrl) && splitterCtrl.level > 0 && obj.name == "LogisticsMenu")
            {
                LogisticsUI = obj;
                logisticsCloseBtn = LogisticsUI.transform.Find("CloseButton").gameObject.GetComponent<Button>();
                logisticsCloseBtn.onClick.AddListener(CloseUI);
                sFilterManager = canvas.GetComponent<SplitterFilterManager>();
                inventoryList.LogisticsArr[0].gameObject.SetActive(true);
                canOpen = true;
            }
            else if (logisticsCtrl.TryGetComponent(out Unloader unloader) && obj.name == "LogisticsMenu")
            {
                LogisticsUI = obj;
                logisticsCloseBtn = LogisticsUI.transform.Find("CloseButton").gameObject.GetComponent<Button>();
                logisticsCloseBtn.onClick.AddListener(CloseUI);
                unloaderManager = canvas.GetComponent<UnloaderManager>();
                inventoryList.LogisticsArr[1].gameObject.SetActive(true);
                canOpen = true;
            }
            else if (logisticsCtrl.GetComponent<ItemSpawner>() && obj.name == "ItemSpwanerFilter")
            {
                LogisticsUI = obj;
                logisticsCloseBtn = LogisticsUI.transform.Find("CloseButton").gameObject.GetComponent<Button>();
                logisticsCloseBtn.onClick.AddListener(CloseUI);
                itemSpManager = canvas.GetComponent<ItemSpManager>();
                canOpen = true;
            }
        }

        return canOpen;
    }

    public void OpenUI()
    {
        if (logisticsCtrl.TryGetComponent(out SplitterCtrl splitter))
        {
            sFilterManager.SetSplitter(splitter);
            inventoryList.LogisticsArr[0].gameObject.SetActive(true);
            gameManager.SelectPointSpawn(splitter.gameObject);
            sFilterManager.OpenUI();
        }
        else if (logisticsCtrl.TryGetComponent(out Unloader unloader))
        {
            unloaderManager.SetUnloader(unloader);
            inventoryList.LogisticsArr[1].gameObject.SetActive(true);
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
        if (logisticsCtrl.GetComponent<SplitterCtrl>())
        {
            sFilterManager.ReleaseInven();
            inventoryList.LogisticsArr[0].gameObject.SetActive(false);
            gameManager.SelectPointRemove();
            sFilterManager.CloseUI();
        }
        else if (logisticsCtrl.GetComponent<Unloader>())
        {
            unloaderManager.ReleaseInven();
            inventoryList.LogisticsArr[1].gameObject.SetActive(false);
            gameManager.SelectPointRemove();
            unloaderManager.CloseUI();
        }
        else if (logisticsCtrl.GetComponent<ItemSpawner>())
        {
            itemSpManager.ReleaseInven();
            gameManager.SelectPointRemove();
            itemSpManager.CloseUI();
        }
        soundManager.PlayUISFX("CloseUI");
    }
}
