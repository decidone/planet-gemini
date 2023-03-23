using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInvenManager : InventoryManager
{
    protected override void Start()
    {
        base.Start();
        SetInven(inventory, inventoryUI);
    }

    public void SortBtn()
    {
        if (dragSlot.slot.item == null)
        {
            inventory.Sort();
        }
    }

    public override void OpenUI()
    {
        inventoryUI.SetActive(true);
        if (gameManager.onUIChangedCallback != null)
            gameManager.onUIChangedCallback.Invoke(inventoryUI);
    }

    public override void CloseUI()
    {
        inventoryUI.SetActive(false);
        if (gameManager.onUIChangedCallback != null)
            gameManager.onUIChangedCallback.Invoke(inventoryUI);
    }
}
