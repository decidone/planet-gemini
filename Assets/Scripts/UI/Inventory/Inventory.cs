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

    ulong[] singleTarget = new ulong[1];

    void Start()
    {
        if (totalItems.Count == 0)
        {        
            itemList = ItemList.instance.itemList;
            foreach (Item item in itemList)
            {
                totalItems.Add(item, 0);
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        //itemList = ItemList.instance.itemList;
        //foreach (Item item in itemList)
        //{
        //    totalItems.Add(item, 0);
        //}
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

    public bool MultipleSpaceCheck(Merch[] merchList)
    {
        // 각 상품들은 99개를 초과하지 않음
        bool containable;
        int emptySlots = space - items.Count;
        int emptySlotsRequirement = 0;

        foreach (Merch merch in merchList)
        {
            int merchItemSlotRemaining = 0;

            for (int i = 0; i < space; i++)
            {
                if (items.ContainsKey(i))
                {
                    if (items[i] == merch.item)
                    {
                        merchItemSlotRemaining += (maxAmount - amounts[i]);
                    }
                }
            }
            if (merch.amount > merchItemSlotRemaining)
            {
                emptySlotsRequirement++;
            }
        }

        if (emptySlots < emptySlotsRequirement)
            containable = false;
        else
            containable = true;
        
        return containable;
    }

    public void Add(Item item, int amount)
    {
        int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(item);
        AddServerRpc(itemIndex, amount);
    }

    public void StorageAdd(Item item, int amount)
    {
        int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(item);
        StorageAddServerRpc(itemIndex, amount);
    }

    public void RecipeInvenAdd(Item item, int amount)
    {
        int containableAmount = SpaceCheck(item);
        int tempAmount = amount;

        if (containableAmount < amount)
        {
            tempAmount = containableAmount;
        }

        // 2. 이미 있던 칸에 수량 증가
        for (int i = 0; i < space; i++)
        {
            if (items.ContainsKey(i))
            {
                if (items[i] == item)
                {
                    if (amounts[i] + tempAmount <= maxAmount)
                    {
                        RecipeSlotAdd(i, item, tempAmount);
                        tempAmount = 0;
                    }
                    else
                    {
                        RecipeSlotAdd(i, item, maxAmount - amounts[i]);
                        tempAmount -= (maxAmount - amounts[i]);
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
                        RecipeSlotAdd(i, item, tempAmount);
                        tempAmount = 0;
                    }
                    else
                    {
                        RecipeSlotAdd(i, item, maxAmount);
                        tempAmount -= maxAmount;
                    }
                }
                if (tempAmount <= 0)
                    break;
            }
        }

        onItemChangedCallback?.Invoke();
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddServerRpc(int itemIndex, int amount, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        bool isHost = clientId == 0;

        Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
        int containableAmount = SpaceCheck(item);
        int tempAmount = amount;

        if (containableAmount < amount)
        {
            tempAmount = containableAmount;
            int dropAmount = amount - containableAmount;
            Drop(item, dropAmount, isHost);
        }

        // 2. 이미 있던 칸에 수량 증가
        for (int i = 0; i < space; i++)
        {
            if (items.ContainsKey(i))
            {
                if (items[i] == item)
                {
                    if (amounts[i] + tempAmount <= maxAmount)
                    {
                        SlotAdd(i, item, tempAmount);
                        tempAmount = 0;
                    }
                    else
                    {
                        SlotAdd(i, item, maxAmount - amounts[i]);
                        tempAmount -= (maxAmount - amounts[i]);
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
                        SlotAdd(i, item, tempAmount);
                        tempAmount = 0;
                    }
                    else
                    {
                        SlotAdd(i, item, maxAmount);
                        tempAmount -= maxAmount;
                    }
                }
                if (tempAmount <= 0)
                    break;
            }

            if (tempAmount > 0)
            {
                Drop(item, tempAmount, isHost);
            }
        }

        onItemChangedCallback?.Invoke();
    }

    [ServerRpc(RequireOwnership = false)]
    public void StorageAddServerRpc(int itemIndex, int amount, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        bool isHost = clientId == 0;

        Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
        int tempAmount = amount;

        // 2. 이미 있던 칸에 수량 증가
        for (int i = 0; i < space; i++)
        {
            if (items.ContainsKey(i))
            {
                if (items[i] == item)
                {
                    if (amounts[i] + tempAmount <= maxAmount)
                    {
                        SlotAdd(i, item, tempAmount);
                        tempAmount = 0;
                    }
                    else
                    {
                        SlotAdd(i, item, maxAmount - amounts[i]);
                        tempAmount -= (maxAmount - amounts[i]);
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
                        SlotAdd(i, item, tempAmount);
                        tempAmount = 0;
                    }
                    else
                    {
                        SlotAdd(i, item, maxAmount);
                        tempAmount -= maxAmount;
                    }
                }
                if (tempAmount <= 0)
                    break;
            }
        }

        onItemChangedCallback?.Invoke();
    }

    [ClientRpc]
    public void DisplayLootInfoClientRpc(int itemIndex, int amount, ClientRpcParams rpcParams = default)
    {
        Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
        LootListManager.instance.DisplayLootInfo(item, amount);
    }

    public void Swap(int slotNum)
    {
        SwapServerRpc(slotNum);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SwapServerRpc(int slotNum, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        bool isHost = clientId == 0;

        Item dragItem = ItemDragManager.instance.GetItem(isHost);
        int dragAmount = ItemDragManager.instance.GetAmount(isHost);

        if (!items.ContainsKey(slotNum))
        {
            // 타겟 슬롯이 비어있는 경우
            SlotAdd(slotNum, dragItem, dragAmount);
            ItemDragManager.instance.Clear(isHost);
        }
        else if ((!ItemDragManager.instance.IsDragging(isHost) && isHost) || (!ItemDragManager.instance.IsDragging(isHost) && !isHost))
        {
            // 드래그 슬롯이 비어있는 경우
            ItemDragManager.instance.Add(items[slotNum], amounts[slotNum], isHost);
            RemoveServerRpc(slotNum);
        }
        else
        {
            Item tempItem = items[slotNum];
            int tempAmount = amounts[slotNum];
            RemoveServerRpc(slotNum);
            SlotAdd(slotNum, dragItem, dragAmount);
            ItemDragManager.instance.Clear(isHost);
            ItemDragManager.instance.Add(tempItem, tempAmount, isHost);
        }

        onItemChangedCallback?.Invoke();
    }

    public void Merge(int slotNum)
    {
        MergeServerRpc(slotNum);
    }

    [ServerRpc(RequireOwnership = false)]
    public void MergeServerRpc(int slotNum, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        bool isHost = clientId == 0;

        Item dragItem = ItemDragManager.instance.GetItem(isHost);
        int dragAmount = ItemDragManager.instance.GetAmount(isHost);
        int mergeAmount = dragAmount + amounts[slotNum];

        if (mergeAmount > maxAmount)
        {
            int tempAmount = amounts[slotNum];
            SlotAdd(slotNum, dragItem, maxAmount - amounts[slotNum]);
            ItemDragManager.instance.Sub(maxAmount - tempAmount, isHost);
        }
        else
        {
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
        if(TryGetComponent(out Structure structure))
        {
            if (!IsServer && !structure.settingEndCheck)
                return;
        }

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
            Debug.Log("Slot Add Error");
        }

        onItemChangedCallback?.Invoke();
    }

    public void RecipeSlotAdd(int slotNum, Item item, int amount)
    {
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
            Debug.Log("Slot Add Error");
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
        if (TryGetComponent(out Structure structure))
        {
            if (!IsServer && !structure.settingEndCheck)
                return;
        }

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
        SplitServerRpc(slotNum);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SplitServerRpc(int slotNum, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        bool isHost = clientId == 0;

        int dragAmount = ItemDragManager.instance.GetAmount(isHost);

        if (items.ContainsKey(slotNum))
        {
            if (amounts[slotNum] > 0)
            {
                if (dragAmount < maxAmount)
                {
                    ItemDragManager.instance.Add(items[slotNum], 1, isHost);
                    SubServerRpc(slotNum, 1);
                }

                onItemChangedCallback?.Invoke();
            }
        }
    }

    public void LootItem(GameObject itemObj)
    {
        LootItemServerRpc(itemObj.GetComponent<NetworkObject>());
    }

    [ServerRpc(RequireOwnership = false)]
    public void LootItemServerRpc(NetworkObjectReference itemObjNetworkObjectReference, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        itemObjNetworkObjectReference.TryGet(out NetworkObject itemObj);

        if (itemObj != null)
        {
            ItemProps itemProps = itemObj.GetComponent<ItemProps>();
            if (itemProps.waitingForDestroy)
                return;

            singleTarget[0] = clientId;
            ClientRpcParams rpcParams = default;
            rpcParams.Send.TargetClientIds = singleTarget;

            int containableAmount = SpaceCheck(itemProps.item);
            int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(itemProps.item);

            if (itemProps.amount <= containableAmount)
            {
                itemProps.waitingForDestroy = true;
                Add(itemProps.item, itemProps.amount);
                DisplayLootInfoClientRpc(itemIndex, itemProps.amount, rpcParams);
                GeminiNetworkManager.instance.DestroyItem(itemObj);
            }
            else if (containableAmount != 0)
            {
                Add(itemProps.item, containableAmount);
                DisplayLootInfoClientRpc(itemIndex, containableAmount, rpcParams);
                GeminiNetworkManager.instance.SetItemPropsClientRpc(itemObj, itemIndex, (itemProps.amount - containableAmount));
            }
            else
            {
                //Debug.Log("not enough space");
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
        if (items.ContainsKey(slotNum))
        {
            totalItems[items[slotNum]] -= amounts[slotNum];
            items.Remove(slotNum);
            amounts.Remove(slotNum);

            onItemChangedCallback?.Invoke();
        }
    }

    public void DragDrop()
    {
        if (GameManager.instance.isPlayerInMarket)
        {
            LootListManager.instance.DisplayInfoMessage("Can't drop items in market");
        }
        else
        {
            DragDropServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void DragDropServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        bool isHost = clientId == 0;
        if (!ItemDragManager.instance.IsDragging(isHost))
            return;

        Item dragItem = ItemDragManager.instance.GetItem(isHost);
        int dragAmount = ItemDragManager.instance.GetAmount(isHost);
        Drop(dragItem, dragAmount, isHost);
        ItemDragManager.instance.Clear(isHost);

        onItemChangedCallback?.Invoke();
    }

    public void Drop(Item item, int amount, bool isHost)
    {
        Vector3 spawnPos = GameManager.instance.GetPlayerPos(isHost);
        int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(item);
        DisplayDropInfoClientRpc(itemIndex, amount, isHost);
        GeminiNetworkManager.instance.ItemSpawnServerRpc(itemIndex, amount, spawnPos);
    }

    [ClientRpc]
    public void DisplayDropInfoClientRpc(int itemIndex, int amount, bool isHost)
    {
        if (isHost != GameManager.instance.isHost) return;

        Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
        LootListManager.instance.DisplayDropInfo(item, amount);
    }

    public void ResetInven()
    {
        Debug.Log("ResetInven");
        // 얜 로컬에서만 쓰는 듯? 일단 회의때 다시 확인
        items.Clear();
        amounts.Clear();
        totalItems.Clear();

        if (itemList == null)
        {
            itemList = ItemList.instance.itemList;
        }

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

    public int GetItemAmount(Item item)
    {
        return totalItems[item];
    }

    public InventorySaveData SaveData()
    {
        InventorySaveData data = new InventorySaveData();
        Dictionary<int, int> totalItemIndexes = new Dictionary<int, int>();
        Dictionary<int, int> itemIndexes = new Dictionary<int, int>();

        foreach (KeyValuePair<Item, int> kv in totalItems)
        {
            totalItemIndexes.Add(GeminiNetworkManager.instance.GetItemSOIndex(kv.Key), kv.Value);
        }
        foreach (KeyValuePair<int, Item> kv in items)
        {
            itemIndexes.Add(kv.Key, GeminiNetworkManager.instance.GetItemSOIndex(kv.Value));
        }

        data.totalItemIndexes = totalItemIndexes;
        data.itemIndexes = itemIndexes;
        data.amounts = amounts;

        return data;
    }

    public void LoadData(InventorySaveData data)
    {
        Dictionary<Item, int> tempTotalItems = new Dictionary<Item, int>();
        Dictionary<int, Item> tempItems = new Dictionary<int, Item>();
        
        foreach (KeyValuePair<int, int> kv in data.totalItemIndexes)
        {
            tempTotalItems.Add(GeminiNetworkManager.instance.GetItemSOFromIndex(kv.Key), kv.Value);
        }
        foreach (KeyValuePair<int, int> kv in data.itemIndexes)
        {
            tempItems.Add(kv.Key, GeminiNetworkManager.instance.GetItemSOFromIndex(kv.Value));
        }
        totalItems = tempTotalItems;
        items = tempItems;
        amounts = data.amounts;

        Refresh();
    }
}
