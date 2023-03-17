using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureInvenManager : InventoryManager
{
    [SerializeField]
    Inventory playerInven;
    [SerializeField]
    GameObject structureInfoUI;

    protected override void InputCheck()
    {
        if (inventory != null)
        {
            base.InputCheck();

            if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
            {
                if (inventory != playerInven)
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
    }

    public override void OpenUI()
    {
        inventoryUI.SetActive(true);

        structureInfoUI.SetActive(true);
        if (gameManager.onUIChangedCallback != null)
            gameManager.onUIChangedCallback.Invoke(structureInfoUI);
    }

    public override void CloseUI()
    {
        inventoryUI.SetActive(false);

        structureInfoUI.SetActive(false);
        if (gameManager.onUIChangedCallback != null)
            gameManager.onUIChangedCallback.Invoke(structureInfoUI);
    }
}
