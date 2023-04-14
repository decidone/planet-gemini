using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructureInvenManager : InventoryManager
{
    [SerializeField]
    Inventory playerInven;
    [SerializeField]
    GameObject structureInfoUI;
    public ProgressBar progressBar;
    public ProgressBar energyBar;
    Production prod;
    bool isOpened;

    protected override void Update()
    {
        base.Update();
        if (isOpened)
        {
            progressBar.SetProgress(prod.GetProgress());
            if (prod.GetType().Name == "Furnace")
            {
                energyBar.SetProgress(prod.GetFuel());
            }
        }
    }

    protected override void InputCheck()
    {
        if (inventory != null)
        {
            base.InputCheck();

            if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
            {
                if (focusedSlot != null)
                {
                    if (focusedSlot.item != null)
                    {
                        int containableAmount = playerInven.SpaceCheck(focusedSlot.item);
                        if (focusedSlot.amount <= containableAmount)
                        {
                            playerInven.Add(focusedSlot.item, focusedSlot.amount);
                            inventory.Remove(focusedSlot);
                        }
                        else if (containableAmount != 0)
                        {
                            playerInven.Add(focusedSlot.item, containableAmount);
                            inventory.Sub(focusedSlot.slotNum, containableAmount);
                        }
                        else
                        {
                            Debug.Log("not enough space");
                        }
                    }
                }
            }
        }
    }

    public void ReleaseInven()
    {
        ResetInvenOption();
        prod = null;
        progressBar.SetMaxProgress(1);
        energyBar.SetMaxProgress(1);
    }

    public void ResetInvenOption()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            Slot slot = slots[i];
            slot.ResetOption();
        }
    }

    public void InvenInit()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            Slot slot = slots[i];
            slot.outputSlot = true;
        }
    }

    public void SetProd(Production _prod)
    {
        prod = _prod;
    }

    public override void OpenUI()
    {
        structureInfoUI.SetActive(true);
        inventoryUI.SetActive(true);
        gameManager.onUIChangedCallback?.Invoke(structureInfoUI);
        isOpened = true;
    }

    public override void CloseUI()
    {
        structureInfoUI.SetActive(false);
        inventoryUI.SetActive(false);
        gameManager.onUIChangedCallback?.Invoke(structureInfoUI);
        isOpened = false;
    }
}
