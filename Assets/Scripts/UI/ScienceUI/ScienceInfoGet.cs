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
            Destroy(gameObject);
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

    public SortedDictionary<int ,(string, int, int, float, bool)> GetSciLevelData(int desiredLevel)
    {
        SortedDictionary<int, (string, int, int, float, bool)> data = new SortedDictionary<int, (string, int, int, float, bool)>();
        //List<string> name = new List<string>();
        //List<int> coreLevel = new List<int>();
        //List<int> level = new List<int>();
        //List<float> time = new List<float>();
        //List<bool> basicScience = new List<bool>();

        foreach (var scienceCategory in scienceInfoDataDic)
        {
            var categoryData = scienceCategory.Value;

            foreach (var classData in categoryData)
            {
                if (classData.Key != "Core")
                //if (classData.Key == desiredClass)
                {
                    var levelData = classData.Value;

                    foreach (var levelEntry in levelData)
                    {
                        var scienceInfo = levelEntry.Value;

                        if (scienceInfo.coreLv == desiredLevel)
                        {
                            data.Add(levelEntry.Value.sortIndex, (scienceCategory.Key, levelEntry.Key, levelEntry.Value.coreLv, levelEntry.Value.time, levelEntry.Value.basicScience));
                            //name.Add(scienceCategory.Key);
                            //level.Add(levelEntry.Key);
                            //coreLevel.Add(levelEntry.Value.coreLv);
                            //time.Add(levelEntry.Value.time);
                            //basicScience.Add(levelEntry.Value.basicScience);
                        }
                    }
                }
            }
        }

        //return (name, level, coreLevel, time, basicScience);
        return data;
    }

    public float CoreUpgradeTime(int level)
    {
        float time = 0;
        foreach (var scienceCategory in scienceInfoDataDic)
        {
            if(scienceCategory.Key == "Core")
            {
                var categoryData = scienceCategory.Value;

                foreach (var classData in categoryData)
                {
                    var levelData = classData.Value;
                    foreach (var levelEntry in levelData)
                    {
                        if(levelEntry.Key == level)
                        {
                            time = levelEntry.Value.time;
                        }
                    }
                }
            }
        }

        return time;
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
