using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SolidFacClickEvent : MonoBehaviour
{
    [SerializeField]
    GameObject solidFacUI;
    [SerializeField]
    SplitterFilterManager sFilterManager;    
    [SerializeField]
    ItemSpManager itemSpManager;

    Button closeBtn;
    Button splittercloseBtn;
    GameManager gameManager;

    SolidFactoryCtrl solidFactory;

    public void SolidFacCheck()
    {
        gameManager = GameManager.instance;
        GameObject canvas = gameManager.GetComponent<GameManager>().inventoryUiCanvas;
        InventoryList inventoryList = canvas.GetComponent<InventoryList>();

        solidFactory = GetComponent<SolidFactoryCtrl>();

        foreach (GameObject list in inventoryList.InventoryArr)
        {
            if (solidFactory.GetComponent<SplitterCtrl>() && list.name == "SplitterMenu")
            {
                solidFacUI = list;
                splittercloseBtn = solidFacUI.transform.Find("CloseButton").gameObject.GetComponent<Button>();
                splittercloseBtn.onClick.AddListener(CloseUI);
                sFilterManager = canvas.GetComponent<SplitterFilterManager>();
            }
            else if (solidFactory.GetComponent<ItemSpawner>() && list.name == "ItemSpwanerFilter")
            {
                solidFacUI = list;
                splittercloseBtn = solidFacUI.transform.Find("CloseButton").gameObject.GetComponent<Button>();
                splittercloseBtn.onClick.AddListener(CloseUI);
                itemSpManager = canvas.GetComponent<ItemSpManager>();
            }
        }
    }

    public void OpenUI()
    {
        if (solidFactory.TryGetComponent(out SplitterCtrl splitter))
        {
            sFilterManager.SetSplitter(splitter);
            sFilterManager.OpenUI();
        }
        else if (solidFactory.TryGetComponent(out ItemSpawner itemSpawner))
        {
            itemSpManager.SetItemSp(itemSpawner);
            itemSpManager.OpenUI();
        }
    }

    public void CloseUI()
    {
        if (solidFactory.GetComponent<SplitterCtrl>())
        {
            sFilterManager.ReleaseInven();
            sFilterManager.CloseUI();
        }
        else if (solidFactory.GetComponent<ItemSpawner>())
        {
            itemSpManager.ReleaseInven();
            itemSpManager.CloseUI();
        }
    }
}
