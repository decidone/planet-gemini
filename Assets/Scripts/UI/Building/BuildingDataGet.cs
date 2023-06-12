using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BuildingDataGet : MonoBehaviour
{
    Dictionary<string, BuildingData> buildingDataDic;
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
        buildingDataDic = new Dictionary<string, BuildingData>();
    }
    #endregion

    void Start()
    {
        string json = File.ReadAllText("Assets/Data/BuildingList.json");
        buildingDataDic = JsonConvert.DeserializeObject<Dictionary<string, BuildingData>>(json);
    }

    public BuildingData GetBuildingName(string str)
    {
        buildingData = buildingDataDic[str];
        return buildingData;
    }
}
