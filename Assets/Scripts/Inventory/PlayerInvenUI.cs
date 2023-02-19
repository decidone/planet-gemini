using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInvenUI : InventoryUI
{
    protected override void Start()
    {
        inventory = PlayerInventory.instance;
        base.Start();
    }

    protected override void InputCheck()
    {
        base.InputCheck();
        if (Input.GetButtonDown("Inventory"))
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);

            if (gameManager.OpenedInvenCheck())
            {
                gameManager.dragSlot.SetActive(true);
            }
            else
            {
                gameManager.dragSlot.SetActive(false);
            }
        }
    }

    protected override void Update()
    {
        base.Update();
        if (dragSlot.item != null)
        {
            dragSlot.GetComponent<RectTransform>().position = Input.mousePosition;
        }
    }
}
