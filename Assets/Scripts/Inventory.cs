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

    public int space;
    public int maxAmount;
    public GameObject itemPref;
    public GameObject player;

    public Dictionary<int, Item> items = new Dictionary<int, Item>();
    public Dictionary<int, int> amounts = new Dictionary<int, int>();

    public List<Item> itemsList = new List<Item>();
    public Dictionary<Item, int> totalItems = new Dictionary<Item, int>();

    public bool Add(Item item, int amount)
    {
        int tempAmount = amount;
        int unoccupiedSlot = space - items.Count;
        int occupiedSlot = 0;
        int invenItemAmount = 0;

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

        Debug.Log("unoccupiedSlot : " + unoccupiedSlot);
        Debug.Log("slot : " + occupiedSlot);
        Debug.Log("amount : " + invenItemAmount);
        Debug.Log("total amount : " + (invenItemAmount + tempAmount));

        // 1. �� ĭ ��� �� �κ��� �ȵ��� ��ŭ ������
        int totalAmount = invenItemAmount + tempAmount;
        int usableSlot = unoccupiedSlot + occupiedSlot;

        if (totalAmount > usableSlot * space)
        {
            int dropAmount = totalAmount - (usableSlot * space);
            tempAmount -= dropAmount;

            // �κ��丮 ������ �ƿ� ���� ��
            if (tempAmount == 0)
            {
                return false;
            }
            else
            {
                Drop(item, dropAmount);
            }
        }

        // 2. �̹� �ִ� ĭ�� ���� ����
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

        // 3. 2�� ó���ϰ� ���� ������ŭ �� ĭ�� ����
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

    public void Split(InventorySlot slot, int splitAmount)
    {
        if (items.ContainsKey(slot.slotNum))
        {
            if (amounts[slot.slotNum] > splitAmount && items.Count < space)
            {
                for (int i = 0; i < space; i++)
                {
                    if (!items.ContainsKey(i))
                    {
                        items[i] = slot.item;
                        amounts[i] = splitAmount;

                        amounts[slot.slotNum] -= splitAmount;

                        if (onItemChangedCallback != null)
                            onItemChangedCallback.Invoke();
                        break;
                    }
                }
            }
        }
    }

    public void Swap(InventorySlot slot1, InventorySlot slot2)
    {
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

    public void Remove(Item item, int amount)
    {
        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }
}