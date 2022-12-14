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
            Debug.LogWarning("More than one instance of inventory foune!");
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
        Debug.Log("total amount : " + invenItemAmount + tempAmount);

        // 1. 빈 칸 계산 후 인벤에 안들어가는 만큼 버리기
        int totalAmount = invenItemAmount + tempAmount;
        int usableSlot = unoccupiedSlot + occupiedSlot;

        if (totalAmount > usableSlot * space)
        {
            int dropAmount = totalAmount - (usableSlot * space);
            tempAmount -= dropAmount;

            // 인벤토리 공간이 아예 없을 때
            if (tempAmount == 0)
            {
                Debug.Log("Not enough space");
                return false;
            }
            else
            {
                // 아이템 드랍
                Debug.Log("Drop : " + dropAmount);
                GameObject dropItem = Instantiate(itemPref);
                SpriteRenderer sprite = dropItem.GetComponent<SpriteRenderer>();
                sprite.sprite = item.icon;
                ItemProps itemProps = dropItem.GetComponent<ItemProps>();
                itemProps.item = item;
                itemProps.amount = dropAmount;
                dropItem.transform.position = player.transform.position;
                dropItem.transform.position += Vector3.down * 1.5f;
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
                        amounts[i] = maxAmount;
                        tempAmount -= (maxAmount - amounts[i]);
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

    public void Split(Item item, int slotNum, int splitAmount)
    {
        if (items.ContainsKey(slotNum))
        {
            if (amounts[slotNum] > splitAmount && items.Count < space)
            {
                for (int i = 0; i < space; i++)
                {
                    if (!items.ContainsKey(i))
                    {
                        items[i] = item;
                        amounts[i] = splitAmount;

                        amounts[slotNum] -= splitAmount;

                        if (onItemChangedCallback != null)
                            onItemChangedCallback.Invoke();
                        break;
                    }
                }
            }
        }
    }


    public void Remove(Item item, int amount)
    {
        // items.Remove(item);

        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }
}
