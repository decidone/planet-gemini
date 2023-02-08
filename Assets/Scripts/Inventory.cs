using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    public int space;   // 아이템 슬롯 상한, 드래그용 슬롯 번호를 겸 함
    public int maxAmount;   // 한 슬롯 당 최대 수량
    public GameObject itemPref;
    public GameObject player;
    InventorySlot dragSlot;

    // 인벤토리에 표시되는 아이템
    public Dictionary<int, Item> items = new Dictionary<int, Item>();
    public Dictionary<int, int> amounts = new Dictionary<int, int>();

    // 아이템 총량 관리
    List<Item> itemsList;
    public Dictionary<Item, int> totalItems = new Dictionary<Item, int>();

    public void Start()
    {
        itemsList = ItemsList.instance.itemsList;
        foreach (Item item in itemsList)
        {
            totalItems.Add(item, 0);
        }
        dragSlot = DragSlot.instance.slot;
    }

    public bool Add(Item item, int amount, bool isCount)
    {
        int tempAmount = amount;
        int unoccupiedSlot = 0;
        int occupiedSlot = 0;
        int invenItemAmount = 0;
        if (isCount)
            totalItems[item] += amount;

        // 인벤토리의 빈 공간, 습득한 아이템과 같은 아이템이 차지하고 있는 공간을 체크
        for (int i = 0; i < space; i++)
        {
            if (items.ContainsKey(i))
            {
                if (items[i] == item)
                {
                    occupiedSlot++;
                    invenItemAmount += amounts[i];
                }
            }
            else
            {
                unoccupiedSlot++;
            }
        }

        // 1. 빈 칸 계산 후 인벤에 안들어가는 만큼 버리기
        int totalAmount = invenItemAmount + tempAmount;
        int usableSlot = unoccupiedSlot + occupiedSlot;
        if (totalAmount > usableSlot * maxAmount)
        {
            int dropAmount = totalAmount - (usableSlot * maxAmount);
            tempAmount -= dropAmount;

            if (tempAmount == 0)
            {
                // 인벤토리 공간이 아예 없을 때
                if (isCount)
                {
                    totalItems[item] -= dropAmount;
                    return false;
                }
            }

            Drop(item, dropAmount);
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
            if (tempAmount <= 0)
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

        return true;
    }

    public void Split(InventorySlot slot)
    {
        if (items.ContainsKey(slot.slotNum))
        {
            if (amounts[slot.slotNum] > 0)
            {
                if (dragSlot.amount < maxAmount)
                {
                    dragSlot.item = slot.item;
                    dragSlot.amount++;
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

    public void Swap(InventorySlot slot)
    {
        if (!items.ContainsKey(slot.slotNum))
        {
            // 타겟 슬롯이 비어있는 경우
            items.Add(slot.slotNum, dragSlot.item);
            amounts.Add(slot.slotNum, dragSlot.amount);
            dragSlot.ClearSlot();
        }else if (dragSlot.item == null)
        {
            // 드래그 슬롯이 비어있는 경우
            dragSlot.item = items[slot.slotNum];
            dragSlot.amount = amounts[slot.slotNum];

            items.Remove(slot.slotNum);
            amounts.Remove(slot.slotNum);
        }
        else
        {
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

    public void Merge(InventorySlot mergeSlot)
    {
        // 드래그 중인 슬롯이 첫 번째 인자
        int mergeAmount = dragSlot.amount + amounts[mergeSlot.slotNum];

        if (mergeAmount > maxAmount)
        {
            amounts[mergeSlot.slotNum] = maxAmount;
            dragSlot.amount = mergeAmount - maxAmount;
        }
        else
        {
            amounts[mergeSlot.slotNum] = mergeAmount;
            dragSlot.ClearSlot();
        }

        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }

    public void Remove(InventorySlot slot)
    {
        items.Remove(slot.slotNum);
        amounts.Remove(slot.slotNum);

        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }

    public void Drop()
    {
        Debug.Log("Drop : " + dragSlot.item.name + ", Amount : " + dragSlot.amount);
        //totalItems[items[slot.slotNum]] -= amounts[slot.slotNum];

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

    public void Drop(Item item, int dropAmount)
    {
        Debug.Log("Drop : " + item.name + ", Amount : " + dropAmount);
        totalItems[item] -= dropAmount;

        GameObject dropItem = Instantiate(itemPref);
        SpriteRenderer sprite = dropItem.GetComponent<SpriteRenderer>();
        sprite.sprite = item.icon;
        ItemProps itemProps = dropItem.GetComponent<ItemProps>();
        itemProps.item = item;
        itemProps.amount = dropAmount;
        dropItem.transform.position = player.transform.position;
        dropItem.transform.position += Vector3.down * 1.5f;
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

    public void CancelDrag()
    {
        Add(dragSlot.item, dragSlot.amount, false);
        dragSlot.ClearSlot();

        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }
}
