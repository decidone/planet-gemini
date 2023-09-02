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
        SetInven(inventory, inventoryUI);
    }

    protected override void InputCheck()
    {
        base.InputCheck();

        if (sManager.isOpened)
        {
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
            {
                if (focusedSlot != null)
                {
                    if (focusedSlot.item != null)
                    {
                        int amount = sManager.InsertItem(focusedSlot.item, focusedSlot.amount);
                        if (amount > 0)
                            inventory.Sub(focusedSlot.slotNum, amount);
                    }
                }
            }
        }
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
        gameManager.onUIChangedCallback?.Invoke(inventoryUI);
    }

    public override void CloseUI()
    {
        inventoryUI.SetActive(false);
        gameManager.onUIChangedCallback?.Invoke(inventoryUI);
    }
}
