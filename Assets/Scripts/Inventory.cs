
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
    public Dictionary<int, Item> items = new Dictionary<int, Item>();
    public Dictionary<int, int> amounts = new Dictionary<int, int>();

    public bool Add(Item item, int amount)
    {
        // 스왑기능 넣을 땐 미리 Dictionary 공간을 할당해둘 것
        if(items.Count >= space)
        {
            if (!items.ContainsValue(item))
            {
                Debug.Log("Not enough space");
                return false;
            }
            else
            {
                // 인벤토리 풀 && 아이템이 이미 인벤토리에 있는 경우
                // 슬롯 당 아이템 수량 제한 기능이 추가되면 사용하는 코드
                // 아이템 수량 체크 후 인벤토리에 넣을 수 있는지 판단
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i] == item)
                    {
                        // 나중에 슬롯당 아이템 수량 처리도 넣을 것
                        amounts[i] += amount;
                    }
                }
            }
        }
        else
        {
            if (!items.ContainsValue(item))
            {
                int count = items.Count;
                items.Add(count, item);
                amounts.Add(count, amount);
            }
            else
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if(items[i] == item)
                    {
                        // 나중에 슬롯당 아이템 수량 처리도 넣을 것
                        amounts[i] += amount;
                    }
                }
            }
        }
        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();

        return true;
    }

    public void Remove(Item item, int amount)
    {
        // items.Remove(item);

        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }
}
