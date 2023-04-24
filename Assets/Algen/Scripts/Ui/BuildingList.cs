using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingList : MonoBehaviour
{
    // ��ũ��Ʈ���� ������ ��� �� �κ��丮 ������ ���Ŀ� ���
    // ������ ����� ���� <string, Item>���� Document ���� �ʿ䰡 ����
    public List<Building> itemList = new List<Building>();
    //public List<Item> itemList = new List<Item>();
    public Dictionary<string, Building> itemDic = new Dictionary<string, Building>();
    //public Dictionary<string, Item> itemDic = new Dictionary<string, Item>();

    #region Singleton
    public static BuildingList instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of itemList found!");
            return;
        }

        instance = this;

        //foreach (Item item in itemList)
        //{
        //    itemDic.Add(item.name, item);
        //}
        foreach (Building item in itemList)
        {
            itemDic.Add(item.name, item);
        }
    }
    #endregion
}
