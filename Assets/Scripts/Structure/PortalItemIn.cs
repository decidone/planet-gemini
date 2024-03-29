using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalItemIn : PortalObj
{
    PortalItemOut portalItemOut;
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
            if (portalItemOut != null)
            {
                prodTimer += Time.deltaTime;
                if (prodTimer > cooldown)
                {
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
        inventory.Add(itemProps.item, itemProps.amount);
        itemProps.itemPool.Release(itemProps.gameObject);
    }

    public override void OnFactoryItem(Item item)
    {
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

    public override void ConnectObj(GameObject othObj)
    {
        portalItemOut = othObj.GetComponent<PortalItemOut>();
    }
}
