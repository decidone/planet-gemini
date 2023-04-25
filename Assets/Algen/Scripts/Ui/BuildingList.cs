using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingList : MonoBehaviour
{
    public List<Building> itemList = new List<Building>();
    public Dictionary<string, Building> itemDic = new Dictionary<string, Building>();

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

        foreach (Building item in itemList)
        {
            itemDic.Add(item.name, item);
        }
    }
    #endregion
}
