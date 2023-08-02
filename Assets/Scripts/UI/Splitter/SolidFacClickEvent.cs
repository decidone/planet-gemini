using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SolidFacClickEvent : MonoBehaviour
{
    [SerializeField]
    GameObject spilterInfoUI;
    [SerializeField]
    SplitterFilterManager sFilterManager;

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
                spilterInfoUI = list;
                splittercloseBtn = spilterInfoUI.transform.Find("CloseButton").gameObject.GetComponent<Button>();
                splittercloseBtn.onClick.AddListener(CloseUI);
            }
        }
        sFilterManager = canvas.GetComponent<SplitterFilterManager>();
    }

    public void OpenUI()
    {
        if (solidFactory.TryGetComponent(out SplitterCtrl splitter))
        {
            sFilterManager.SetSplitter(splitter);
            sFilterManager.OpenUI();
        }
    }

    public void CloseUI()
    {
        if (solidFactory.GetComponent<SplitterCtrl>())
        {
            sFilterManager.ReleaseInven();
            sFilterManager.CloseUI();
        }
    }
}
