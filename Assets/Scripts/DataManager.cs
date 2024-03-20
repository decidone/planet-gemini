using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    InputManager inputManager;
    public SaveData saveData;
    public string path;
    public int selectedSlot;    // 저장 슬롯. 나중에 ui 넣을 때 지정

    #region Singleton
    public static DataManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of DataManager found!");
            return;
        }

        instance = this;
    }
    #endregion

    private void Start()
    {
        path = Application.persistentDataPath + "/save";
        saveData = new SaveData();
        selectedSlot = 0;

        inputManager = InputManager.instance;
        inputManager.controls.HotKey.Save.performed += ctx => Save();
        inputManager.controls.HotKey.Load.performed += ctx => Load();
    }

    public string Save()
    {
        //saveData = new SaveData();
        PlayerData playerData = new PlayerData();
        playerData.name = "test";
        saveData.playerDataList.Add(playerData);
        InventoryData hostMapInventoryData = GameManager.instance.hostMapInven.SaveData();
        saveData.HostMapInvenData = hostMapInventoryData;
        InventoryData clientMapInventoryData = GameManager.instance.clientMapInven.SaveData();
        saveData.ClientMapInvenData = clientMapInventoryData;

        Debug.Log("saved: " + path);
        string json = JsonConvert.SerializeObject(saveData);
        File.WriteAllText(path + selectedSlot.ToString() + ".json", json);

        return json;
    }

    public string GetJsonFromFile()
    {
        string json = File.ReadAllText(path + selectedSlot.ToString() + ".json");
        return json;
    }

    public void Load()
    {
        string json = GetJsonFromFile();
        saveData = JsonConvert.DeserializeObject<SaveData>(json);

        GameManager.instance.hostMapInven.LoadData(saveData.HostMapInvenData);
        GameManager.instance.clientMapInven.LoadData(saveData.ClientMapInvenData);
    }

    public void Load(string json)
    {
        saveData = JsonConvert.DeserializeObject<SaveData>(json);

        GameManager.instance.hostMapInven.LoadData(saveData.HostMapInvenData);
        GameManager.instance.clientMapInven.LoadData(saveData.ClientMapInvenData);
    }

    public void Clear()
    {
        selectedSlot = -1;
        saveData = new SaveData();
    }
}
