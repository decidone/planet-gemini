using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClickEvent : MonoBehaviour
{
    public GameObject structureInfoUI;
    Inventory inventory;
    StructureInvenUI ui;
    GameObject storage;
    GameObject miner;
    Button closeBtn;
    GameManager gameManager;
    string recipeUI;

    void Start()
    {
        gameManager = GameManager.instance;
        storage = structureInfoUI.transform.Find("Storage").gameObject;
        miner = storage.transform.Find("Miner").gameObject;
        closeBtn = structureInfoUI.transform.Find("CloseButton").gameObject.GetComponent<Button>();
        closeBtn.onClick.AddListener(CloseUI);
        ui = structureInfoUI.GetComponent<StructureInvenUI>();
    }

    public void OpenUI()
    {
        structureInfoUI.SetActive(true);
        if (gameManager.onUIChangedCallback != null)
            gameManager.onUIChangedCallback.Invoke(structureInfoUI);

        // 이거 메서드로 떼서 사용 할 것
        if (this.transform.GetComponent<Miner>())
        {
            Miner miner = (Miner)this.transform.GetComponent<Miner>();
            recipeUI = miner.recipeUI;
            inventory = miner.transform.GetComponent<Inventory>();
            ui.inventory = inventory;
        }
        switch (recipeUI)
        {
            case "Miner":
                miner.SetActive(true);
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
