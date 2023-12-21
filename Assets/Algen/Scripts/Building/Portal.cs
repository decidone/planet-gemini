using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : Production
{
    protected override void Awake()
    {
        myVision.SetActive(false);
        inventory = this.GetComponent<Inventory>();
    }

    protected override void Start()
    {
        GameManager gameManager = GameManager.instance;
        canvas = gameManager.GetComponent<GameManager>().inventoryUiCanvas;
        sInvenManager = canvas.GetComponent<StructureInvenManager>();
        GetUIFunc();
    }

    public override void OpenUI()
    {
        base.OpenUI();
        sInvenManager.SetInven(inventory, ui);
        sInvenManager.SetProd(this);

        sInvenManager.progressBar.gameObject.SetActive(false);
        sInvenManager.energyBar.gameObject.SetActive(false);
    }

    public override void CloseUI()
    {
        base.CloseUI();
        sInvenManager.progressBar.gameObject.SetActive(true);
        sInvenManager.energyBar.gameObject.SetActive(true);
        sInvenManager.ReleaseInven();
    }

    public override void GetUIFunc()
    {
        InventoryList inventoryList = canvas.GetComponent<InventoryList>();

        foreach (GameObject list in inventoryList.StructureStorageArr)
        {
            if (list.name == "Portal")
            {
                ui = list;
            }
        }
    }
}
