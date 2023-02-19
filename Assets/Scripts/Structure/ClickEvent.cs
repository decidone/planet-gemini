using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClickEvent : MonoBehaviour
{
    [SerializeField]
    GameObject structureInfoUI;
    Button closeBtn;
    Inventory inventory;
    string recipeUI;
    GameObject info;
    GameObject oneStorage;
    GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.instance;
        info = structureInfoUI.transform.Find("Info").gameObject;
        oneStorage = info.transform.Find("OneStorage").gameObject;
        closeBtn = structureInfoUI.transform.Find("CloseButton").gameObject.GetComponent<Button>();
        closeBtn.onClick.AddListener(CloseUI);
    }

    public void OpenUI()
    {
        structureInfoUI.SetActive(true);

        if (gameManager.OpenedInvenCheck())
        {
            gameManager.dragSlot.SetActive(true);
        }

        // 이거 메서드로 떼서 사용 할 것
        if (this.transform.GetComponent<Miner>())
        {
            Miner miner = (Miner)this.transform.GetComponent<Miner>();
            recipeUI = miner.recipeUI;
            inventory = miner.transform.GetComponent<Inventory>();
            InventoryUI ui = info.GetComponent<InventoryUI>();
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

        if (!gameManager.OpenedInvenCheck())
        {
            gameManager.dragSlot.SetActive(false);
        }

        int childAmount = info.transform.childCount;
        for(int i = 0; i < childAmount; i++)
        {
            info.transform.GetChild(i).gameObject.SetActive(false);
        }
    }
}
