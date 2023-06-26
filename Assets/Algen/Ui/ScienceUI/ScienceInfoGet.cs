using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ScienceInfoGet : MonoBehaviour
{
    Dictionary<string, Dictionary<int, ScienceInfoData>> scienceInfoDataDic;
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
        scienceInfoDataDic = new Dictionary<string, Dictionary<int, ScienceInfoData>>();
    }
    #endregion

    void Start()
    {
        string json = File.ReadAllText("Assets/Data/ScienceInfo.json");
        scienceInfoDataDic = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<int, ScienceInfoData>>>(json);
    }

    public ScienceInfoData GetBuildingName(string str, int level)
    {
        if (scienceInfoDataDic.ContainsKey(str))
        {
            Dictionary<int, ScienceInfoData> innerDictionary = scienceInfoDataDic[str];
            if (innerDictionary.ContainsKey(level))
            {
                return innerDictionary[level];
            }
        }

        // ã�� �� ���� ��� �Ǵ� ���� ó���� ���ϴ� ��쿡 ���� �⺻�� ��ȯ
        return null;
    }
}
