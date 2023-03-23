using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemList : MonoBehaviour
{
    // 스크립트에서 아이템 사용 및 인벤토리 아이템 정렬에 사용
    // 아이템 사용을 위해 <string, Item>으로 Document 만들 필요가 있음
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
