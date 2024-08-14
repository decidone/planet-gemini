using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// UTF-8 설정
public class BuildingDataGet : MonoBehaviour
{
    Dictionary<string, Dictionary<int, BuildingData>> buildingDataDic;

    #region Singleton
    public static BuildingDataGet instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        buildingDataDic = new Dictionary<string, Dictionary<int, BuildingData>>();
    }
    #endregion

    void Start()
    {
        string json = Resources.Load<TextAsset>("BuildingList").ToString();
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

        // 찾을 수 없는 경우 또는 예외 처리를 원하는 경우에 대한 기본값 반환
        return null;
    }
}
