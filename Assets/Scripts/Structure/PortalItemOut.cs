using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalItemOut : PortalObj
{
    protected override void Start()
    {
        base.Start();
        isPortalBuild = true;
        isStorageBuilding = true;
    }

    public override void OpenUI()
    {
        sInvenManager.SetInven(inventory, ui);
        sInvenManager.SetProd(this);
        sInvenManager.progressBar.gameObject.SetActive(false);
        sInvenManager.energyBar.gameObject.SetActive(false);
    }

    public override void CloseUI()
    {
        sInvenManager.progressBar.gameObject.SetActive(true);
        sInvenManager.energyBar.gameObject.SetActive(true);
        sInvenManager.ReleaseInven();
    }
    public override bool CanTakeItem(Item item)
    {
        bool canTake = false;
        int containableAmount = inventory.SpaceCheck(item);

        if (1 <= containableAmount)
        {
            canTake = true;
        }
        else if (containableAmount != 0)
        {
            canTake = true;
        }
        else
        {
            canTake = false;
        }

        return canTake;
    }

    public override void OnFactoryItem(ItemProps itemProps)
    {
        if(IsServer)
            inventory.StorageAdd(itemProps.item, itemProps.amount);
        itemProps.itemPool.Release(itemProps.gameObject);
    }

    public override void OnFactoryItem(Item item)
    {
        if (IsServer)
            inventory.StorageAdd(item, 1);
    }

    public override void GetUIFunc()
    {
        InventoryList inventoryList = canvas.GetComponent<InventoryList>();

        foreach (GameObject list in inventoryList.StructureStorageArr)
        {
            if (list.name == "PortalItem")
            {
                ui = list;
            }
        }
    }

    public Dictionary<Item, int> TakeItemDic(Dictionary<Item, int> invItemCheckDic)
    {
        Dictionary<Item, int> getData = new Dictionary<Item, int>(invItemCheckDic);
        Dictionary<Item, int> overGetItem = new Dictionary<Item, int>();

        foreach (var itemData in getData)
        {
            int containableAmount = inventory.SpaceCheck(itemData.Key);
            if (itemData.Value <= containableAmount)
            {
                inventory.Add(itemData.Key, itemData.Value);
            }
            else if (containableAmount != 0)
            {
                inventory.Add(itemData.Key, containableAmount);
                overGetItem.Add(itemData.Key, itemData.Value - containableAmount);
            }
            else
            {
                overGetItem.Add(itemData.Key, itemData.Value);
                Debug.Log("not enough space");
            }
        }

        return overGetItem;
    }
}
