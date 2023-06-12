using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitterFilterRecipe : InventoryManager
{
    List<Item> itemsList;
    public SplitterFilterManager splitter;

    int slotIndex = -1;

    // Start is called before the first frame update
    protected override void Start()
    {
        gameManager = GameManager.instance;
        itemsList = gameManager.GetComponent<ItemList>().itemList;
        //SetItemList();
    }

    protected override void InputCheck()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (focusedSlot != null)
            {
                if (focusedSlot.item != null)
                {
                    splitter.SetItem(focusedSlot.item, slotIndex);
                    focusedSlot = null;
                    CloseUI();
                }
            }
        }
    }

    void SetItemList()
    {
        inventory.ResetInven();
        for (int i = 0; i < itemsList.Count; i++)
        {
            inventory.Add(itemsList[i], 1);
        }
        SetInven(inventory, inventoryUI);
    }

    public void GetFillterNum(int buttonIndex)
    {
        slotIndex = buttonIndex;
    }

    public override void OpenUI()
    {
        SetItemList();

        inventoryUI.SetActive(true);
        gameManager.onUIChangedCallback?.Invoke(inventoryUI);
    }

    public override void CloseUI()
    {
        inventoryUI.SetActive(false);
        gameManager.onUIChangedCallback?.Invoke(inventoryUI);
        slotIndex = -1;
    }
}
