using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// UTF-8 설정
public class PlayerInvenManager : InventoryManager
{
    [SerializeField]
    StructureInvenManager sManager;
    [SerializeField]
    Text title;
    [SerializeField]
    PlayerController player;

    protected override void Start()
    {
        base.Start();
        inventory = GameManager.instance.inventory;
        SetInven(inventory, inventoryUI);
    }

    void OnEnable()
    {
        inputManager = InputManager.instance;
        inputManager.controls.Inventory.SlotLeftClick.performed += SlotLeftClick;
        inputManager.controls.Inventory.SlotRightClickHold.performed += SlotRightClickHold;
        inputManager.controls.Inventory.SlotLeftClick.performed += SlotShiftClick;
    }

    void OnDisable()
    {
        inputManager.controls.Inventory.SlotLeftClick.performed -= SlotLeftClick;
        inputManager.controls.Inventory.SlotRightClickHold.performed -= SlotRightClickHold;
        inputManager.controls.Inventory.SlotLeftClick.performed -= SlotShiftClick;
    }

    void SlotShiftClick(InputAction.CallbackContext ctx)
    {
        if (!sManager.isOpened) return;
        if (!inputManager.shift) return;

        if (focusedSlot != null)
        {
            if (focusedSlot.item != null)
            {
                sManager.PlayerToStrInven(focusedSlot.slotNum, focusedSlot.item, focusedSlot.amount);
                soundManager.PlayUISFX("ItemSelect");
            }
        }
    }

    public void SortBtn()
    {
        if (!ItemDragManager.instance.isDrag)
        {
            inventory.SortServerRpc();
        }
        soundManager.PlayUISFX("ButtonClick");
    }

    public override void OpenUI()
    {
        inventoryUI.SetActive(true);
        if (inventory == GameManager.instance.hostMapInven)
        {
            title.text = "Castor";
        }
        else
        {
            title.text = "Pollux";
        }

        gameManager.onUIChangedCallback?.Invoke(inventoryUI);
    }

    public override void CloseUI()
    {
        focusedSlot = null;
        inventoryUI.SetActive(false);
        itemInfoWindow.CloseWindow();
        soundManager.PlayUISFX("CloseUI");

        gameManager.onUIChangedCallback?.Invoke(inventoryUI);
    }
}
