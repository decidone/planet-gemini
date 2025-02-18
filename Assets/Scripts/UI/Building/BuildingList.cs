using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class BuildingList : MonoBehaviour
{
    public BuildingListSO buildingListSO;
    public Dictionary<string, (int, Building)> itemDic = new Dictionary<string, (int, Building)>();

    #region Singleton
    public static BuildingList instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        int dicIndex = 0;
        foreach (Building item in buildingListSO.buildingSOList)
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

    public Building FindBuildingData(int index)
    {
        return buildingListSO.buildingSOList[index];
    }

    public GameObject FindBuildingListObj(int index)
    {
        return buildingListSO.buildingSOList[index].gameObj;
    }

    public GameObject FindSideBuildingListObj(int index)
    {
        return buildingListSO.buildingSOList[index].sideObj;
    }
}