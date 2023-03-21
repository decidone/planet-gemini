using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    [SerializeField]
    int space;   // 아이템 슬롯 상한, 드래그용 슬롯 번호를 겸 함
    [SerializeField]
    int maxAmount;   // 한 슬롯 당 최대 수량
    [SerializeField]
    GameObject itemPref;
    [SerializeField]
    GameObject player;
    Slot dragSlot;

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
        dragSlot = DragSlot.instance.slot;
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
        int tempAmount = amount;
        totalItems[item] += amount;

        // 2. 이미 있던 칸에 수량 증가
        for (int i = 0; i < space; i++)
        {
            if (items.ContainsKey(i))
            {
                if (items[i] == item)
                {
                    if (amounts[i] + tempAmount <= maxAmount)
                    {
                        amounts[i] += tempAmount;
                        tempAmount = 0;
                    }
                    else
                    {
                        tempAmount -= (maxAmount - amounts[i]);
                        amounts[i] = maxAmount;
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
                        items[i] = item;
                        amounts[i] = tempAmount;
                        tempAmount = 0;
                    }
                    else
                    {
                        items[i] = item;
                        amounts[i] = maxAmount;
                        tempAmount -= maxAmount;
                    }
                }
                if (tempAmount <= 0)
                    break;
            }
        }

        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }

    public void Swap(Slot slot)
    {
        if (!items.ContainsKey(slot.slotNum))
        {
            // 타겟 슬롯이 비어있는 경우
            items.Add(slot.slotNum, dragSlot.item);
            amounts.Add(slot.slotNum, dragSlot.amount);
            totalItems[dragSlot.item] += dragSlot.amount;
            dragSlot.ClearSlot();
        }
        else if (dragSlot.item == null)
        {
            // 드래그 슬롯이 비어있는 경우
            dragSlot.item = items[slot.slotNum];
            dragSlot.amount = amounts[slot.slotNum];
            totalItems[dragSlot.item] -= dragSlot.amount;
            items.Remove(slot.slotNum);
            amounts.Remove(slot.slotNum);
        }
        else
        {
            totalItems[dragSlot.item] += dragSlot.amount;
            totalItems[items[slot.slotNum]] -= amounts[slot.slotNum];

            Item tempItem = items[slot.slotNum];
            int tempAmount = amounts[slot.slotNum];

            items[slot.slotNum] = dragSlot.item;
            dragSlot.item = tempItem;

            amounts[slot.slotNum] = dragSlot.amount;
            dragSlot.amount = tempAmount;
        }

        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }

    public void Merge(Slot mergeSlot)
    {
        // 드래그 중인 슬롯이 첫 번째 인자
        int mergeAmount = dragSlot.amount + amounts[mergeSlot.slotNum];

        if (mergeAmount > maxAmount)
        {
            totalItems[dragSlot.item] += (maxAmount - amounts[mergeSlot.slotNum]);
            amounts[mergeSlot.slotNum] = maxAmount;
            dragSlot.amount = mergeAmount - maxAmount;
        }
        else
        {
            totalItems[dragSlot.item] += dragSlot.amount;
            amounts[mergeSlot.slotNum] = mergeAmount;
            dragSlot.ClearSlot();
        }

        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
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

        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }

    public void Sub(int slotNum, int amount)
    {
        totalItems[items[slotNum]] -= amount;
        amounts[slotNum] -= amount;
        if (amounts[slotNum] == 0)
        {
            items.Remove(slotNum);
            amounts.Remove(slotNum);
        }

        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }

    public void Split(Slot slot)
    {
        if (items.ContainsKey(slot.slotNum))
        {
            if (amounts[slot.slotNum] > 0)
            {
                if (dragSlot.amount < maxAmount)
                {
                    dragSlot.item = slot.item;
                    dragSlot.amount++;
                    totalItems[items[slot.slotNum]]--;
                    amounts[slot.slotNum]--;
                }

                if (amounts[slot.slotNum] == 0)
                {
                    items.Remove(slot.slotNum);
                    amounts.Remove(slot.slotNum);
                }

                if (onItemChangedCallback != null)
                    onItemChangedCallback.Invoke();
            }
        }
    }

    public void Refresh()
    {
        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }

    public void Remove(Slot slot)
    {
        totalItems[items[slot.slotNum]] -= amounts[slot.slotNum];
        items.Remove(slot.slotNum);
        amounts.Remove(slot.slotNum);

        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }

    public void Drop()
    {
        Debug.Log("Drop : " + dragSlot.item.name + ", Amount : " + dragSlot.amount);
        GameObject dropItem = Instantiate(itemPref);
        SpriteRenderer sprite = dropItem.GetComponent<SpriteRenderer>();
        sprite.sprite = dragSlot.item.icon;
        ItemProps itemProps = dropItem.GetComponent<ItemProps>();
        itemProps.item = dragSlot.item;
        itemProps.amount = dragSlot.amount;
        dropItem.transform.position = player.transform.position;
        dropItem.transform.position += Vector3.down * 1.5f;
        dragSlot.ClearSlot();

        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }

    public void Sort()
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

        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }
}
