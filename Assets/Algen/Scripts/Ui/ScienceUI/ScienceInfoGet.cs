using Newtonsoft.Json;
using System.Collections;
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
        scienceInfoDataDic = new Dictionary<string, Dictionary<string, Dictionary<int, ScienceInfoData>>>();

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


    //string myLog;
    //Queue myLogQueue = new Queue();

    //void OnEnable()
    //{
    //    Application.logMessageReceived += HandleLog;
    //}

    //void OnDisable()
    //{
    //    Application.logMessageReceived -= HandleLog;
    //}

    //void HandleLog(string logString, string stackTrace, LogType type)
    //{
    //    myLog = logString;
    //    string newString = "\n [" + type + "] : " + myLog;
    //    myLogQueue.Enqueue(newString);
    //    if (type == LogType.Exception)
    //    {
    //        newString = "\n" + stackTrace;
    //        myLogQueue.Enqueue(newString);
    //    }
    //    myLog = string.Empty;
    //    foreach (string mylog in myLogQueue)
    //    {
    //        myLog += mylog;
    //    }
    //}

    //void OnGUI()
    //{
    //    GUILayout.Label(myLog);
    //}
}
