using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClickEvent : MonoBehaviour
{
    public GameObject structureInfoUI;
    Inventory inventory;
    StructureInvenManager sInvenManager;
    GameObject storage;
    GameObject miner;
    GameObject furnace;
    Button closeBtn;
    GameManager gameManager;
    string recipeUI;

    void Start()
    {
        gameManager = GameManager.instance;
        storage = structureInfoUI.transform.Find("Storage").gameObject;
        miner = storage.transform.Find("Miner").gameObject;
        furnace = storage.transform.Find("Furnace").gameObject;
        closeBtn = structureInfoUI.transform.Find("CloseButton").gameObject.GetComponent<Button>();
        closeBtn.onClick.AddListener(CloseUI);
        sInvenManager = structureInfoUI.GetComponent<StructureInvenManager>();
    }

    public void OpenUI()
    {
        structureInfoUI.SetActive(true);
        if (gameManager.onUIChangedCallback != null)
            gameManager.onUIChangedCallback.Invoke(structureInfoUI);

        // 이거 메서드로 떼서 사용 할 것
        if (this.transform.GetComponent<Miner>())
        {
            Miner _miner = this.transform.GetComponent<Miner>();
            recipeUI = _miner.recipeUI;
            inventory = _miner.transform.GetComponent<Inventory>();
            sInvenManager.SetInven(inventory, miner);
        }
        else if (this.transform.GetComponent<Furnace>())
        {
            Furnace _furnace = this.transform.GetComponent<Furnace>();
            recipeUI = _furnace.recipeUI;
            inventory = _furnace.transform.GetComponent<Inventory>();
            sInvenManager.SetInven(inventory, furnace);
        }

        // 이거 떼서 건물쪽 스크립트로 옮기기
        switch (recipeUI)
        {
            case "Miner":
                miner.SetActive(true);
                sInvenManager.slots[0].outputSlot = true;
                break;

            case "Furnace":
                furnace.SetActive(true);
                sInvenManager.slots[0].SetInputItem(ItemList.instance.itemDic["Coal"]);
                sInvenManager.slots[2].outputSlot = true;
                break;

            default:
                Debug.Log("no recipe detected");
                break;
        }
    }

    public void CloseUI()
    {
        structureInfoUI.SetActive(false);
        if (gameManager.onUIChangedCallback != null)
            gameManager.onUIChangedCallback.Invoke(structureInfoUI);

        int childAmount = storage.transform.childCount;
        for(int i = 0; i < childAmount; i++)
        {
            storage.transform.GetChild(i).gameObject.SetActive(false);
        }
    }
}
