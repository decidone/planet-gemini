using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    #region Singleton
    public static Inventory instance;
    
    private void Awake()
    {
        if(instance != null)
        {
            Debug.LogWarning("More than one instance of inventory found!");
            return;
        }

        instance = this;
    }
    #endregion

    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    public int space;   // 아이템 슬롯 상한, 드래그용 슬롯 번호를 겸 함
    public int maxAmount;   // 한 슬롯 당 최대 수량
    public GameObject itemPref;
    public GameObject player;

    // 인벤토리에 표시되는 아이템
    public Dictionary<int, Item> items = new Dictionary<int, Item>();
    public Dictionary<int, int> amounts = new Dictionary<int, int>();

    // 아이템 총량 관리
    public List<Item> itemsList = new List<Item>();
    public Dictionary<Item, int> totalItems = new Dictionary<Item, int>();

    public void Start()
    {
        foreach (Item item in itemsList)
        {
            totalItems.Add(item, 0);
        }
    }

    public bool Add(Item item, int amount)
    {
        int tempAmount = amount;
        int unoccupiedSlot = space - items.Count;
        int occupiedSlot = 0;
        int invenItemAmount = 0;
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
        }

        // 1. 빈 칸 계산 후 인벤에 안들어가는 만큼 버리기
        int totalAmount = invenItemAmount + tempAmount;
        int usableSlot = unoccupiedSlot + occupiedSlot;

        if (totalAmount > usableSlot * space)
        {
            int dropAmount = totalAmount - (usableSlot * space);
            tempAmount -= dropAmount;

            if (tempAmount == 0)
            {
                // 인벤토리 공간이 아예 없을 때
                totalItems[item] -= amount;

                return false;
            }
            else
            {
                Drop(item, dropAmount);
            }
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
                if (!items.ContainsKey(space))
                {
                    items.Add(space, slot.item);
                    amounts.Add(space, 1);
                    amounts[slot.slotNum]--;
                }
                else if (amounts[space] < maxAmount)
                {
                    items[space] = slot.item;
                    amounts[space]++;
                    amounts[slot.slotNum]--;
                }
                
                if (items.ContainsKey(slot.slotNum))
                {
                    if (amounts[slot.slotNum] <= 0)
                    {
                        items.Remove(slot.slotNum);
                        amounts.Remove(slot.slotNum);
                    }
                }
                
                if (onItemChangedCallback != null)
                    onItemChangedCallback.Invoke();
            }
        }
    }

    public void Swap(InventorySlot slot1, InventorySlot slot2)
    {
        // 빈 슬롯을 slot1에 넣으면 안 됨
        Item tempItem = items[slot1.slotNum];
        int tempAmount = amounts[slot1.slotNum];

        if (slot2.item != null)
        {
            items[slot1.slotNum] = items[slot2.slotNum];
            items[slot2.slotNum] = tempItem;

            amounts[slot1.slotNum] = amounts[slot2.slotNum];
            amounts[slot2.slotNum] = tempAmount;
        }
        else
        {
            items.Remove(slot1.slotNum);
            items.Add(slot2.slotNum, tempItem);

            amounts.Remove(slot1.slotNum);
            amounts.Add(slot2.slotNum, tempAmount);
        }

        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }

    public void Merge(InventorySlot dragSlot, InventorySlot mergeSlot)
    {
        // 드래그 중인 슬롯이 첫 번째 인자
        int mergeAmount = amounts[dragSlot.slotNum] + amounts[mergeSlot.slotNum];

        if (mergeAmount > maxAmount)
        {
            amounts[mergeSlot.slotNum] = maxAmount;
            amounts[dragSlot.slotNum] = mergeAmount - maxAmount;
        }
        else
        {
            amounts[mergeSlot.slotNum] = mergeAmount;
            items.Remove(dragSlot.slotNum);
            amounts.Remove(dragSlot.slotNum);
        }

        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }

    public void Drop(Item item, int dropAmount)
    {
        Debug.Log("Drop : " + item.name + "Amount : " + dropAmount);
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

    public void Drop(InventorySlot slot)
    {
        Debug.Log("Drop : " + items[slot.slotNum].name + "Amount : " + amounts[slot.slotNum]);
        totalItems[items[slot.slotNum]] -= amounts[slot.slotNum];

        GameObject dropItem = Instantiate(itemPref);
        SpriteRenderer sprite = dropItem.GetComponent<SpriteRenderer>();
        sprite.sprite = items[slot.slotNum].icon;
        ItemProps itemProps = dropItem.GetComponent<ItemProps>();
        itemProps.item = items[slot.slotNum];
        itemProps.amount = amounts[slot.slotNum];
        dropItem.transform.position = player.transform.position;
        dropItem.transform.position += Vector3.down * 1.5f;

        items.Remove(slot.slotNum);
        amounts.Remove(slot.slotNum);

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
