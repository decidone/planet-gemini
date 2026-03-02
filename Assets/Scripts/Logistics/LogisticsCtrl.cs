using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Unity.Netcode;

// UTF-8 설정
public class LogisticsCtrl : Structure
{
    public void BeltGroupSendItem(ItemProps itemObj)
    {
        itemObjList.Add(itemObj);
        itemObj.setOnBelt = Get<BeltCtrl>();
        if (itemObjList.Count >= structureData.MaxItemStorageLimit)
            isFull = true;
        else
            isFull = false;
    }

    public override void OnFactoryItem(ItemProps itemProps)
    {
        if (IsServer)
        {
            int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(itemProps.item);
            OnFactoryItemServerRpc(itemIndex);
        }

        itemProps.itemPool.Release(itemProps.gameObject);
    }

    public override void OnFactoryItem(Item item)
    {
        if (IsServer)
        {
            int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(item);
            OnFactoryItemServerRpc(itemIndex);
        }
    }

    public override void ItemNumCheck()
    {
        if (Get<BeltCtrl>())
        {
            if (itemObjList.Count >= structureData.MaxItemStorageLimit)
            {
                isFull = true;
            }
            else
                isFull = false;
        }
        else
        {
            if (itemList.Count >= structureData.MaxItemStorageLimit)
            {
                isFull = true;
            }
            else
                isFull = false;
        }
    }

    public override void AddInvenItem()
    {
        base.AddInvenItem();
        if (Get<BeltCtrl>())
        {
            if (itemObjList.Count > 0)
            {
                ItemPoolReleaseServerRpc();
            }
        }
        else
        {
            if (itemList.Count > 0)
            {
                foreach (Item item in itemList)
                {
                    playerInven.Add(item, 1);
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void ItemPoolReleaseServerRpc()
    {
        ItemPoolReleaseClientRpc();
    }

    [ClientRpc]
    void ItemPoolReleaseClientRpc()
    {
        foreach (ItemProps itemProps in itemObjList)
        {
            if(IsServer)
                playerInven.Add(itemProps.item, itemProps.amount);
            itemProps.itemPool.Release(itemProps.gameObject);
        }
    }
    

    public override Dictionary<Item, int> PopUpItemCheck() 
    { 
        if(itemList.Count > 0)
        {
            Dictionary<Item, int> returnDic = new Dictionary<Item, int>();
            foreach (Item item in itemList)
            {
                if(!returnDic.ContainsKey(item))
                    returnDic.Add(item, 1);
                else
                {
                    int currentValue = returnDic[item];
                    int newValue = currentValue + 1;
                    returnDic[item] = newValue;
                }
            }

            return returnDic;
        }
        else
            return null; 
    }

    protected override void ItemDrop()
    {
        if(itemList.Count> 0)
        {
            foreach (Item item in itemList)
            {
                ItemToItemProps(item, 1);
            }
        }

        if (itemObjList.Count > 0)
        {
            foreach (ItemProps itemProps in itemObjList)
            {
                itemProps.ResetItemProps();
            }
        }
    }
}
