using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class PlayerInvenManager : InventoryManager
{
    [SerializeField]
    StructureInvenManager sManager;

    protected override void Start()
    {
        base.Start();
        inventory = GameManager.instance.inventory;
        SetInven(inventory, inventoryUI);
        inputManager.controls.Inventory.SlotLeftClick.performed += ctx => SlotShiftClick();
    }

    void SlotShiftClick()
    {
        if (!sManager.isOpened) return;
        if (!inputManager.shift) return;

        if (focusedSlot != null)
        {
            if (focusedSlot.item != null)
            {
                int amount = sManager.InsertItem(focusedSlot.item, focusedSlot.amount);
                if (amount > 0)
                {
                    inventory.SubServerRpc(focusedSlot.slotNum, amount);
                    soundManager.PlayUISFX("ItemSelect");
                }
            }
        }
    }

    public void SortBtn()
    {
        if (!ItemDragManager.instance.isDrag)
        {
            inventory.SortServerRpc();
        }
    }

    public override void OpenUI()
    {
        inventoryUI.SetActive(true);
        gameManager.onUIChangedCallback?.Invoke(inventoryUI);
    }

    public override void CloseUI()
    {
        inventoryUI.SetActive(false);
        itemInfoWindow.CloseWindow();
        soundManager.PlayUISFX("CloseUI");
        gameManager.onUIChangedCallback?.Invoke(inventoryUI);
    }
}
