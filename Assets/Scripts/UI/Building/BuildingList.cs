using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class BuildingList : MonoBehaviour
{
    public List<Building> buildingDataList = new List<Building>();
    public Dictionary<string, (int, Building)> itemDic = new Dictionary<string, (int, Building)>();

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

        int dicIndex = 0;
        foreach (Building item in buildingDataList)
        {
            itemDic.Add(item.name, (dicIndex, item));
            dicIndex++;
        }
    }
    #endregion

    public int FindBuildingListIndex(string name)
    {
        int index = -1;

        if(itemDic.TryGetValue(name, out (int, Building) data))
        {
            return data.Item1;
        }

        return index;
    }

    public GameObject FindBuildingListObj(int index)
    {
        return buildingDataList[index].gameObj;
    }
}