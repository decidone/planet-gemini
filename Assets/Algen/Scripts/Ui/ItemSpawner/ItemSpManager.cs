using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class ItemSpManager : InventoryManager
{
    List<Item> itemsList;
    ItemSpawner itemSpawner = null;
    //int slotIndex = -1;

    // Start is called before the first frame update
    protected override void Start()
    {
        gameManager = GameManager.instance;
        itemsList = gameManager.GetComponent<ItemList>().itemList;

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].amountText.gameObject.SetActive(false);
        }
    }
    protected override void InputCheck()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (focusedSlot != null)
            {
                if (focusedSlot.item != null)
                {
                    SetItem(focusedSlot.item);
                    focusedSlot = null;
                }
            }
        }
    }

    public void SetItemSp(ItemSpawner _itemSp)
    {
        itemSpawner = _itemSp;
    }
    public void ReleaseInven()
    {
        ResetInvenOption();
        itemSpawner = null;
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

    public void SetItem(Item _item)
    {
        itemSpawner.itemData = _item;
    }

    void SetItemList()
    {
        inventory.ResetInven();
        for (int i = 0; i < itemsList.Count; i++)
        {
            if (itemsList[i].name == "FullFilter")
                continue;
            inventory.Add(itemsList[i], 1);
        }
        SetInven(inventory, inventoryUI);
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
    }

}
