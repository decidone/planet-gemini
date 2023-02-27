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
    GameObject oneStorage;
    Button closeBtn;
    GameManager gameManager;
    DragSlot dragSlot;
    string recipeUI;

    void Start()
    {
        gameManager = GameManager.instance;
        storage = structureInfoUI.transform.Find("Storage").gameObject;
        oneStorage = storage.transform.Find("OneStorage").gameObject;
        closeBtn = structureInfoUI.transform.Find("CloseButton").gameObject.GetComponent<Button>();
        closeBtn.onClick.AddListener(CloseUI);
        ui = structureInfoUI.GetComponent<StructureInvenUI>();
        dragSlot = DragSlot.instance;
    }

    public void OpenUI()
    {
        structureInfoUI.SetActive(true);
        if (gameManager.onUIChangedCallback != null)
            gameManager.onUIChangedCallback.Invoke(structureInfoUI);

        // �̰� �޼���� ���� ��� �� ��
        if (this.transform.GetComponent<Miner>())
        {
            Miner miner = (Miner)this.transform.GetComponent<Miner>();
            recipeUI = miner.recipeUI;
            inventory = miner.transform.GetComponent<Inventory>();
            ui.inventory = inventory;
        }
        switch (recipeUI)
        {
            case "OneStorage":
                oneStorage.SetActive(true);
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
