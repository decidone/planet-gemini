using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class BuildingList : MonoBehaviour
{
    public List<Building> buildingDataList = new List<Building>();
    public Dictionary<string, Building> itemDic = new Dictionary<string, Building>();

    #region Singleton
    public static BuildingList instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of buildingList found!");
            return;
        }

        instance = this;

        foreach (Building item in buildingDataList)
        {
            itemDic.Add(item.name, item);
        }
    }
    #endregion
}
