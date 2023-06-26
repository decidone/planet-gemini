using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BuildingDataGet : MonoBehaviour
{
    Dictionary<string, Dictionary<int, BuildingData>> buildingDataDic;
    BuildingData buildingData;

    #region Singleton
    public static BuildingDataGet instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of recipeList found!");
            return;
        }

        instance = this;
        buildingDataDic = new Dictionary<string, Dictionary<int, BuildingData>>();
    }
    #endregion

    void Start()
    {
        string json = File.ReadAllText("Assets/Data/BuildingList.json");
        buildingDataDic = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<int, BuildingData>>>(json);
    }

    public BuildingData GetBuildingName(string str, int level)
    {
        if (buildingDataDic.ContainsKey(str))
        {
            Dictionary<int, BuildingData> innerDictionary = buildingDataDic[str];
            if (innerDictionary.ContainsKey(level))
            {
                return innerDictionary[level];
            }
        }

        // ã�� �� ���� ��� �Ǵ� ���� ó���� ���ϴ� ��쿡 ���� �⺻�� ��ȯ
        return null;
    }
}
