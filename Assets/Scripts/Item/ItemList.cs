using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemList : MonoBehaviour
{
    // ��ũ��Ʈ���� ������ ��� �� �κ��丮 ������ ���Ŀ� ���
    // ������ ����� ���� <string, Item>���� Document ���� �ʿ䰡 ����
    public List<Item> itemList = new List<Item>();
    public Dictionary<string, Item> itemDic = new Dictionary<string, Item>();

    #region Singleton
    public static ItemList instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of itemList found!");
            return;
        }

        instance = this;

        foreach (Item item in itemList)
        {
            itemDic.Add(item.name, item);
        }
    }
    #endregion
}
