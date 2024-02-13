using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// UTF-8 설정
public class Inventory : NetworkBehaviour
{
    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    public int space;   // 아이템 슬롯 상한, 드래그용 슬롯 번호를 겸 함
    public int maxAmount;   // 한 슬롯 당 최대 수량
    [SerializeField]
    GameObject itemPref;

    // 인벤토리에 표시되는 아이템
    public Dictionary<int, Item> items = new Dictionary<int, Item>();
    public Dictionary<int, int> amounts = new Dictionary<int, int>();

    // 아이템 총량 관리
    List<Item> itemList;
    public Dictionary<Item, int> totalItems = new Dictionary<Item, int>();

    void Start()
    {
        itemList = ItemList.instance.itemList;
        foreach (Item item in itemList)
        {
            totalItems.Add(item, 0);
        }
    }

    public int SpaceCheck(Item item)
    {
        int containableAmount = 0;
        for (int i = 0; i < space; i++)
        {
            if (items.ContainsKey(i))
            {
                if (items[i] == item)
                {
                    containableAmount += (maxAmount - amounts[i]);
                }
            }
            else
            {
                containableAmount += maxAmount;
            }
        }

        return containableAmount;
    }

    public void Add(Item item, int amount)
    {
        int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(item);
        AddServerRpc(itemIndex, amount);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddServerRpc(int itemIndex, int amount)
    {
        Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
        int containableAmount = SpaceCheck(item);
        int tempAmount = amount;

        if (containableAmount < amount)
        {
            tempAmount = containableAmount;
            int dropAmount = amount - containableAmount;
            Drop(item, dropAmount);
        }
        //totalItems[item] += tempAmount;

        // 2. 이미 있던 칸에 수량 증가
        for (int i = 0; i < space; i++)
        {
            if (items.ContainsKey(i))
            {
                if (items[i] == item)
                {
                    if (amounts[i] + tempAmount <= maxAmount)
                    {
                        //amounts[i] += tempAmount;
                        SlotAdd(i, item, tempAmount);
                        tempAmount = 0;
                    }
                    else
                    {
                        SlotAdd(i, item, maxAmount - amounts[i]);
                        tempAmount -= (maxAmount - amounts[i]);

                        //amounts[i] = maxAmount;
                    }
                }
            }
            if (tempAmount == 0)
                break;
        }

        // 3. 2를 처리하고 남은 수량만큼 빈 칸에 배정
        if (tempAmount > 0)
        {
            for (int i = 0; i < space; i++)
            {
                if (!items.ContainsKey(i))
                {
                    if (tempAmount <= maxAmount)
                    {
                        //items[i] = item;
                        //amounts[i] = tempAmount;
                        SlotAdd(i, item, tempAmount);
                        tempAmount = 0;
                    }
                    else
                    {
                        //items[i] = item;
                        //amounts[i] = maxAmount;
                        SlotAdd(i, item, maxAmount);
                        tempAmount -= maxAmount;
                    }
                }
                if (tempAmount <= 0)
                    break;
            }

            if (tempAmount > 0)
            {
                Drop(item, tempAmount);
            }
        }

        onItemChangedCallback?.Invoke();
    }

    public void Swap(int slotNum)
    {
        SwapServerRpc(slotNum, GameManager.instance.isHost);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SwapServerRpc(int slotNum, bool isHost)
    {
        Item dragItem = ItemDragManager.instance.GetItem(isHost);
        int dragAmount = ItemDragManager.instance.GetAmount(isHost);

        if (!items.ContainsKey(slotNum))
        {
            // 타겟 슬롯이 비어있는 경우
            //items.Add(slotNum, dragItem);
            //amounts.Add(slotNum, dragAmount);
            //totalItems[dragItem] += dragAmount;
            SlotAdd(slotNum, dragItem, dragAmount);
            ItemDragManager.instance.Clear(isHost);
        }
        else if ((!ItemDragManager.instance.IsDragging(isHost) && isHost) || (!ItemDragManager.instance.IsDragging(isHost) && !isHost))
        {
            // 드래그 슬롯이 비어있는 경우
            ItemDragManager.instance.Add(items[slotNum], amounts[slotNum], isHost);
            RemoveServerRpc(slotNum);
            //totalItems[items[slotNum]] -= amounts[slotNum];
            //items.Remove(slotNum);
            //amounts.Remove(slotNum);
        }
        else
        {
            //totalItems[dragItem] += dragAmount;
            //totalItems[items[slotNum]] -= amounts[slotNum];
            Item tempItem = items[slotNum];
            int tempAmount = amounts[slotNum];
            //items[slotNum] = dragItem;
            //amounts[slotNum] = dragAmount;
            RemoveServerRpc(slotNum);
            SlotAdd(slotNum, dragItem, dragAmount);
            ItemDragManager.instance.Clear(isHost);
            ItemDragManager.instance.Add(tempItem, tempAmount, isHost);
        }

        onItemChangedCallback?.Invoke();
    }

    public void Merge(int slotNum)
    {
        MergeServerRpc(slotNum, GameManager.instance.isHost);
    }

    [ServerRpc(RequireOwnership = false)]
    public void MergeServerRpc(int slotNum, bool isHost)
    {
        // 드래그 중인 슬롯이 첫 번째 인자
        Item dragItem = ItemDragManager.instance.GetItem(isHost);
        int dragAmount = ItemDragManager.instance.GetAmount(isHost);
        int mergeAmount = dragAmount + amounts[slotNum];

        if (mergeAmount > maxAmount)
        {
            Debug.Log("merge " + mergeAmount);
            //totalItems[dragItem] += (maxAmount - amounts[slotNum]);
            //amounts[slotNum] = maxAmount;
            int tempAmount = amounts[slotNum];
            SlotAdd(slotNum, dragItem, maxAmount - amounts[slotNum]);
            ItemDragManager.instance.Sub(dragAmount - tempAmount, isHost);
        }
        else
        {
            //totalItems[dragItem] += dragAmount;
            //amounts[slotNum] = mergeAmount;
            SlotAdd(slotNum, dragItem, dragAmount);
            ItemDragManager.instance.Clear(isHost);
        }

        onItemChangedCallback?.Invoke();
    }

    public (Item item, int amount) SlotCheck(int slotNum)
    {
        Item item = null;
        int amount = 0;

        if (items.ContainsKey(slotNum))
        {
            item = items[slotNum];
            amount = amounts[slotNum];
        }

        return (item, amount);
    }

    public void SlotAdd(int slotNum, Item item, int amount)
    {
        int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(item);
        SlotAddServerRpc(slotNum, itemIndex, amount);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SlotAddServerRpc(int slotNum, int itemIndex, int amount)
    {
        SlotAddClientRpc(slotNum, itemIndex, amount);
    }

    [ClientRpc]
    public void SlotAddClientRpc(int slotNum, int itemIndex, int amount)
    {
        Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
        if (!items.ContainsKey(slotNum))
        {
            items.Add(slotNum, item);
            amounts.Add(slotNum, amount);
            totalItems[item] += amount;
        }
        else if (items[slotNum] == item)
        {
            amounts[slotNum] += amount;
            totalItems[items[slotNum]] += amount;
        }
        else
        {
            Debug.Log("SlotAdd Error");
        }

        onItemChangedCallback?.Invoke();
    }

    
    [ServerRpc(RequireOwnership = false)]
    public void SubServerRpc(int slotNum, int amount)
    {
        SubClientRpc(slotNum, amount);
    }

    [ClientRpc]
    public void SubClientRpc(int slotNum, int amount)
    {
        Debug.Log("Sub " + slotNum + amounts[slotNum] + "   " + amount);
        totalItems[items[slotNum]] -= amount;
        amounts[slotNum] -= amount;
        if (amounts[slotNum] == 0)
        {
            items.Remove(slotNum);
            amounts.Remove(slotNum);
        }

        onItemChangedCallback?.Invoke();
    }

    public void Sub(Item item, int amount)
    {
        int slotNum = FindItemSlot(item);
        if (slotNum != -1)
        {
            var slotData = SlotCheck(slotNum);
            if(slotData.amount >= amount)
            {
                SubServerRpc(slotNum, amount);
            }
            else
            {
                SubServerRpc(slotNum, slotData.amount);
                Sub(item, amount - slotData.amount);
            }
        }
    }

    int FindItemSlot(Item item)
    {
        for (int i = 0; i < space; i++)
        {
            if (items.ContainsKey(i))
            {
                if (items[i] == item)
                {
                    return i;
                }
            }
        }
        return -1;
    }

    public void Split(int slotNum)
    {
        SplitServerRpc(slotNum, GameManager.instance.isHost);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SplitServerRpc(int slotNum, bool isHost)
    {
        int dragAmount = ItemDragManager.instance.GetAmount(isHost);

        if (items.ContainsKey(slotNum))
        {
            if (amounts[slotNum] > 0)
            {
                if (dragAmount < maxAmount)
                {
                    ItemDragManager.instance.Add(items[slotNum], 1, isHost);
                    //totalItems[items[slotNum]]--;
                    //amounts[slotNum]--;
                    SubServerRpc(slotNum, 1);
                }

                if (amounts[slotNum] == 0)
                {
                    //items.Remove(slotNum);
                    //amounts.Remove(slotNum);
                    RemoveServerRpc(slotNum);
                }

                onItemChangedCallback?.Invoke();
            }
        }
    }

    public void Refresh()
    {
        onItemChangedCallback?.Invoke();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveServerRpc(int slotNum)
    {
        RemoveClientRpc(slotNum);
    }

    [ClientRpc]
    public void RemoveClientRpc(int slotNum)
    {
        totalItems[items[slotNum]] -= amounts[slotNum];
        items.Remove(slotNum);
        amounts.Remove(slotNum);

        onItemChangedCallback?.Invoke();
    }

    public void DragDrop()
    {
        Item dragItem = ItemDragManager.instance.GetItem(GameManager.instance.isHost);
        int dragAmount = ItemDragManager.instance.GetAmount(GameManager.instance.isHost);

        Drop(dragItem, dragAmount);
        ItemDragManager.instance.Clear(GameManager.instance.isHost);

        onItemChangedCallback?.Invoke();
    }

    public void Drop(Item item, int amount)
    {
        // 서버
        Debug.Log("Drop : " + item.name + ", Amount : " + amount);
        GameObject dropItem = Instantiate(itemPref);
        SpriteRenderer sprite = dropItem.GetComponent<SpriteRenderer>();
        sprite.sprite = item.icon;
        ItemProps itemProps = dropItem.GetComponent<ItemProps>();
        itemProps.item = item;
        itemProps.amount = amount;
        dropItem.transform.position = GameManager.instance.player.transform.position;
    }

    public void ResetInven()
    {
        // 얜 로컬에서만 쓰는 듯? 일단 회의때 다시 확인
        items.Clear();
        amounts.Clear();
        totalItems.Clear();
        foreach (Item item in itemList)
        {
            totalItems.Add(item, 0);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SortServerRpc()
    {
        SortClientRpc();
    }

    [ClientRpc]
    public void SortClientRpc()
    {
        items = new Dictionary<int, Item>();
        amounts = new Dictionary<int, int>();

        foreach (KeyValuePair<Item, int> item in totalItems)
        {
            if (item.Value > 0)
            {
                int tempAmount = item.Value;
                if (tempAmount > 0)
                {
                    for (int i = 0; i < space; i++)
                    {
                        if (!items.ContainsKey(i))
                        {
                            if (tempAmount <= maxAmount)
                            {
                                items[i] = item.Key;
                                amounts[i] = tempAmount;
                                tempAmount = 0;
                            }
                            else
                            {
                                items[i] = item.Key;
                                amounts[i] = maxAmount;
                                tempAmount -= maxAmount;
                            }
                        }
                        if (tempAmount <= 0)
                            break;
                    }
                }
            }
        }

        onItemChangedCallback?.Invoke();
    }

    public bool TotalItemsAmountLimitCheck(int amountLimit)
    {
        int amounts = 0;

        foreach (var itemDic in totalItems)
        {
            amounts += itemDic.Value;
            if (amounts > amountLimit)
            {
                return true;
            }
        }
        
        return false;
    }

    public bool HasItem()
    {
        return items.Count > 0;
    }
}
