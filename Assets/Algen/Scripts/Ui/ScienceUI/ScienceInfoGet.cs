using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class ScienceInfoGet : MonoBehaviour
{
    Dictionary<string, Dictionary<string, Dictionary<int, ScienceInfoData>>> scienceInfoDataDic;

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
        string json = Resources.Load<TextAsset>("ScienceInfo").ToString();
        scienceInfoDataDic = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<int, ScienceInfoData>>>>(json);
    }
    #endregion

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
        return null;
    }

    public Dictionary<string, int> GetSciLevelData(string desiredClass, int desiredLevel)
    {
        Dictionary<string, int> matchingData = new Dictionary<string, int>();

        foreach (var scienceCategory in scienceInfoDataDic)
        {
            var categoryData = scienceCategory.Value;

            foreach (var classData in categoryData)
            {
                if (classData.Key == desiredClass)
                {
                    var levelData = classData.Value;

                    foreach (var levelEntry in levelData)
                    {
                        var scienceInfo = levelEntry.Value;

                        if (scienceInfo.coreLv == desiredLevel)
                        {
                            matchingData.Add(scienceCategory.Key, levelEntry.Key);
                        }
                    }
                }
            }
        }

        return matchingData;
    }
}
