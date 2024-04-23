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
        saveData = new SaveData();

        // 플레이어
        PlayerSaveData playerData = new PlayerSaveData();
        playerData.isHostPlayer = true; // 임시
        saveData.playerDataList.Add(playerData);

        // 행성 인벤토리
        InventorySaveData hostMapInventoryData = GameManager.instance.hostMapInven.SaveData();
        saveData.HostMapInvenData = hostMapInventoryData;
        InventorySaveData clientMapInventoryData = GameManager.instance.clientMapInven.SaveData();
        saveData.ClientMapInvenData = clientMapInventoryData;

        // Json 저장
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
        // 호스트가 파일로부터 json을 불러와서 동기화
        string json = GetJsonFromFile();
        saveData = JsonConvert.DeserializeObject<SaveData>(json);
        LoadData(saveData);
    }

    public void Load(string json)
    {
        // 클라이언트가 접속 시 호스트로부터 json을 받아서 동기화
        // 네트워크 오브젝트라서 스폰을 시킬 필요가 없는 경우 등등 호스트가 파일을 불러와서 동기화 하는 과정과는 좀 달라질 예정
        saveData = JsonConvert.DeserializeObject<SaveData>(json);
        LoadData(saveData);
    }

    public void LoadData(SaveData saveData)
    {
        // 행성 인벤토리
        GameManager.instance.hostMapInven.LoadData(saveData.HostMapInvenData);
        GameManager.instance.clientMapInven.LoadData(saveData.ClientMapInvenData);
    }

    public void Clear()
    {
        //selectedSlot = -1;
        saveData = new SaveData();
    }
}
