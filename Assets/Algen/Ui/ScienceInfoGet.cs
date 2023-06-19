using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ScienceInfoGet : MonoBehaviour
{
    Dictionary<string, ScienceInfoData> scienceInfoDataDic;
    ScienceInfoData scienceInfoData;

    #region Singleton
    public static ScienceInfoGet instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of recipeList found!");
            return;
        }

        instance = this;
        scienceInfoDataDic = new Dictionary<string, ScienceInfoData>();
    }
    #endregion

    void Start()
    {
        string json = File.ReadAllText("Assets/Data/ScienceInfo.json");
        scienceInfoDataDic = JsonConvert.DeserializeObject<Dictionary<string, ScienceInfoData>>(json);
    }

    public ScienceInfoData GetBuildingName(string str)
    {
        scienceInfoData = scienceInfoDataDic[str];
        return scienceInfoData;
    }
}
