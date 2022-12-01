
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
        // ���ұ�� ���� �� �̸� Dictionary ������ �Ҵ��ص� ��
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
                // ���� �� ������ ���� ���� ����� �߰��Ǹ� ����ϴ� �ڵ�
                // ������ ���� üũ �� �κ��丮�� ���� �� �ִ��� �Ǵ�
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i] == item)
                    {
                        // ���߿� ���Դ� ������ ���� ó���� ���� ��
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
                        // ���߿� ���Դ� ������ ���� ó���� ���� ��
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
