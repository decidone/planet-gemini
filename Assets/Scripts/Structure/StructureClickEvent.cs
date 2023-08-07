using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructureClickEvent : MonoBehaviour
{
    public GameObject structureInfoUI;
    public StructureInvenManager sInvenManager;
    Button closeBtn;
    Production prod;

    GameManager gameManager;

    //void Start()
    //{
    //    closeBtn = structureInfoUI.transform.Find("CloseButton").gameObject.GetComponent<Button>();
    //    closeBtn.onClick.AddListener(CloseUI);

    //    prod = this.transform.GetComponent<Production>();
    //}

    public void StructureClick()
    {
        gameManager = GameManager.instance;
        GameObject canvas = gameManager.GetComponent<GameManager>().inventoryUiCanvas;
        InventoryList inventoryList = canvas.GetComponent<InventoryList>();

        prod = this.transform.GetComponent<Production>();

        foreach (GameObject list in inventoryList.InventoryArr)
        {
            if (list.name == "StructureInfo")
            {
                structureInfoUI = list;
                closeBtn = structureInfoUI.transform.Find("CloseButton").gameObject.GetComponent<Button>();
                closeBtn.onClick.AddListener(CloseUI);
            }
        }
        sInvenManager = canvas.GetComponent<StructureInvenManager>();
    }

    public void OpenUI()
    {
        prod.OpenUI();
        sInvenManager.OpenUI();
    }

    public void CloseUI()
    {
        prod.CloseUI();
        sInvenManager.CloseUI();
    }
}
