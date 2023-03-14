using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureInvenManager : InventoryManager
{
    [SerializeField]
    Inventory playerInven;

    protected override void Start()
    {
        base.Start();
        inventory.Refresh();
    }

    protected override void InputCheck()
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
