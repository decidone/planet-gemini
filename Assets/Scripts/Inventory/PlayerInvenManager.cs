using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInvenManager : InventoryManager
{
    protected override void Start()
    {
        base.Start();
    }

    public void SortBtn()
    {
        if (dragSlot.slot.item == null)
        {
            inventory.Sort();
        }
    }

    public void OpenUI()
    {
        inventoryUI.SetActive(true);
        if (gameManager.onUIChangedCallback != null)
            gameManager.onUIChangedCallback.Invoke(inventoryUI);
    }

    public void CloseUI()
    {
        inventoryUI.SetActive(false);
        if (gameManager.onUIChangedCallback != null)
            gameManager.onUIChangedCallback.Invoke(inventoryUI);
    }
}
