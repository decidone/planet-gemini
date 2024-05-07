using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalItemIn : PortalObj
{
    public PortalItemOut portalItemOut;
    int maxSendAmount;

    protected override void Start()
    {
        base.Start();
        isPortalBuild = true;
        maxFuel = 100;
        maxSendAmount = 99;
        isStorageBuilding = true;
    }

    protected override void Update()
    {
        base.Update();
        if (!isPreBuilding)
        {
            if (CheckSendableItemExists())
            {
                prodTimer += Time.deltaTime;
                if (prodTimer > cooldown)
                {
                    if(IsServer)
                        SendItemDicCheck(portalItemOut);
                    prodTimer = 0;
                }
            }
            else
                prodTimer = 0;
        }
    }

    public override void OpenUI()
    {
        sInvenManager.SetInven(inventory, ui);
        sInvenManager.SetProd(this);
        sInvenManager.progressBar.SetMaxProgress(cooldown);
    }

    public override void CloseUI()
    {
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
            inventory.Add(itemProps.item, itemProps.amount);
        itemProps.itemPool.Release(itemProps.gameObject);
    }

    public override void OnFactoryItem(Item item)
    {
        if (IsServer)
            inventory.Add(item, 1);
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

    void SendItemDicCheck(PortalItemOut portalItemOut)
    {
        int Sendcalculate = 0;
        Dictionary<Item, int> invItemCheckDic = new Dictionary<Item, int>();

        for (int i = 0; i < 18; i++)
        {
            var invenItem = inventory.SlotCheck(i);

            if (invenItem.item != null)
            {
                int availableAmount = Mathf.Min(invenItem.amount, maxSendAmount - Sendcalculate);

                if (!invItemCheckDic.ContainsKey(invenItem.item))
                {
                    invItemCheckDic.Add(invenItem.item, availableAmount);
                }
                else
                {
                    invItemCheckDic[invenItem.item] += availableAmount;
                }

                Sendcalculate += availableAmount;

                if (Sendcalculate >= maxSendAmount)
                    break;
            }
        }

        Dictionary<Item, int> overGetItem = portalItemOut.TakeItemDic(invItemCheckDic);

        foreach (var dicData in invItemCheckDic)
        {
            if(overGetItem.TryGetValue(dicData.Key, out int overCount))
            {
                inventory.Sub(dicData.Key, dicData.Value - overCount);
            }
            else
                inventory.Sub(dicData.Key, dicData.Value);
        }
    }

    bool CheckSendableItemExists()
    {
        bool exists = false;

        if (portalItemOut == null)
            return false;

        for (int i = 0; i < 18; i++)
        {
            var invenItem = inventory.SlotCheck(i);
            if (invenItem.item != null && invenItem.amount > 0)
            {
                exists = true;
                return exists;
            }
        }

        return exists;
    }

    public override void ConnectObj(GameObject othObj)
    {
        portalItemOut = othObj.GetComponent<PortalItemOut>();
    }
}
