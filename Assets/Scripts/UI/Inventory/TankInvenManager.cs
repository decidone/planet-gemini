using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankInvenManager : InventoryManager
{
    PlayerController player;

    protected override void Start()
    {
        base.Start();
        inventory = GameManager.instance.inventory;
        SetInven(inventory, inventoryUI);
    }

    public override void OpenUI()
    {
        if (!player)
        {
            player = gameManager.player.GetComponent<PlayerController>();
        }

        if (player.onTankData)
        {
            inventoryUI.SetActive(true);
        }
        else
        {
            inventoryUI.SetActive(false);
        }
    }

    public override void CloseUI()
    {
        focusedSlot = null;
        inventoryUI.SetActive(false);
        itemInfoWindow.CloseWindow();
    }
}
