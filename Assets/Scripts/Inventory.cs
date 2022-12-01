
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
        if(items.Count >= space)
        {
            if (!items.ContainsValue(item))
            {
                Debug.Log("Not enough space");
                return false;
            }
            else
            {
                // �κ��丮 Ǯ && �������� �̹� �κ��丮�� �ִ� ���
                // ������ ���� üũ �� �κ��丮�� ���� �� �ִ��� �Ǵ�
            }
        }
        else
        {
            if (!items.ContainsValue(item))
            {
                items.Add(items.Count, item);
                amounts.Add(items.Count, amount);
            }
            else
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if(items[i] == item)
                    {
                        amounts[i] += amount;
                        Debug.Log(items[i] + ", " + amounts[i]);
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
