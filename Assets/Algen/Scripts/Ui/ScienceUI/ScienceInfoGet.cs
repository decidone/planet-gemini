using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ScienceInfoGet : MonoBehaviour
{
    Dictionary<string, Dictionary<string, Dictionary<int, ScienceInfoData>>> scienceInfoDataDic;
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
        scienceInfoDataDic = new Dictionary<string, Dictionary<string, Dictionary<int, ScienceInfoData>>>();
    }
    #endregion

    void Start()
    {
        string json = Resources.Load<TextAsset>("ScienceInfo").ToString();
        scienceInfoDataDic = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<int, ScienceInfoData>>>>(json);
    }

    public ScienceInfoData GetBuildingName(string buildingName, int level)
    {
        foreach (var sciData in scienceInfoDataDic)
        {
            if(sciData.Key == buildingName)
            {
                var categoryDictionary = sciData.Value;
                foreach (Dictionary<int, ScienceInfoData> Value in categoryDictionary.Values)
                {
                    Dictionary<int, ScienceInfoData> innerDictionary = Value;
                    if (innerDictionary.ContainsKey(level))
                    {
                        return innerDictionary[level];
                    }
                }
            }
        }

        // 찾을 수 없는 경우 또는 예외 처리를 원하는 경우에 대한 기본값 반환
        return null;
    }

    public List<string> GetSciLevel(string category, int level)
    {
        List<string> sciName = new List<string>();

        foreach (var sciData in scienceInfoDataDic)
        {
            var categoryDictionary = sciData.Value;
            if (categoryDictionary.ContainsKey(category))
            {
                foreach (Dictionary<int, ScienceInfoData> Value in categoryDictionary.Values)
                {
                    Dictionary<int, ScienceInfoData> innerDictionary = Value;
                    if (innerDictionary.ContainsKey(level))
                    {
                        sciName.Add(sciData.Key);
                    }
                }
            }
        }

        if(sciName.Count > 0)
        {
            return sciName;
        }
        else
            return null;
    }
}
