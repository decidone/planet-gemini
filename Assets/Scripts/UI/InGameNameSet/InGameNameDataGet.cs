using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameNameDataGet : MonoBehaviour
{
    Dictionary<string, Dictionary<int, string>> objNameDic;

    #region Singleton
    public static InGameNameDataGet instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        string json = Resources.Load<TextAsset>("ObjName").ToString();
        objNameDic = new Dictionary<string, Dictionary<int, string>>();
        objNameDic = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<int, string>>>(json);
    }
    #endregion

    public string ReturnName(string dataName)
    {
        return ReturnName(1, dataName);
    }

    public string ReturnName(int index, string dataName)
    {
        if (objNameDic.ContainsKey(dataName))
        {
            Dictionary<int, string> data = objNameDic[dataName];
            if (data.ContainsKey(index))
            {
                return data[index];
            }
        }
        return "";
    }
}
