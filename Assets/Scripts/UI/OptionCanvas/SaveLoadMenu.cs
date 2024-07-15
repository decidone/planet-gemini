using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoadMenu : MonoBehaviour
{
    public string path;
    [SerializeField]
    Text optionName;
    [SerializeField]
    GameObject saveLoadPanel;
    [SerializeField]
    SaveLoadBtn autoSaveBtn;
    [SerializeField]
    GameObject content;
    [SerializeField]
    GameObject btnObj;
    [SerializeField]
    SaveLoadBtn[] buttons;
    [SerializeField]
    Button backBtn;

    #region Singleton
    public static SaveLoadMenu instance;

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

    // Start is called before the first frame update
    void Start()
    {
        path = Application.persistentDataPath + "/save";
        LoadSaveData();
        backBtn.onClick.AddListener(() => BackBtnFunc());
    }

    void LoadSaveData()
    { 
        SaveData saveData;
        buttons[0] = autoSaveBtn;
        var data = GetJsonFromFile(0);
        if (data.Item1)
        {
            saveData = JsonConvert.DeserializeObject<SaveData>(data.Item2);
            buttons[0].SetSlotData(0, saveData.saveDate);
        }
        else
        {
            buttons[0].SetSlotData(0, null);
        }

        for (int i = 1; i < 11; i++)
        {
            GameObject btn = Instantiate(btnObj);
            btn.transform.SetParent(content.transform, false);
            buttons[i] = btn.GetComponent<SaveLoadBtn>();
            data = GetJsonFromFile(i);
            if (data.Item1)
            {
                saveData = JsonConvert.DeserializeObject<SaveData>(data.Item2);
                buttons[i].SetSlotData(i, saveData.saveDate, saveData.fileName);
            }
            else
            {
                buttons[i].SetSlotData(i, null, null);
            }
        }
    }

    public void MenuOpen(bool state) // true : save 버튼, false : load 버튼
    {
        if (state)
            optionName.text = "Save";
        else
            optionName.text = "Load";
        saveLoadPanel.SetActive(true);
        foreach (SaveLoadBtn btn in buttons)
        {
            btn.BtnStateSet(state);
        }
        GameManager.instance.onUIChangedCallback?.Invoke(saveLoadPanel);

    }

    public void MenuClose()
    {
        saveLoadPanel.SetActive(false);
        GameManager.instance.onUIChangedCallback?.Invoke(saveLoadPanel);
    }

    public (bool, string) GetJsonFromFile(int saveSlotNum)
    {
        string filePath = path + saveSlotNum.ToString() + ".json";

        try
        {
            string json = File.ReadAllText(filePath);
            return (true, json);
        }
        catch (FileNotFoundException e)
        {
            // 파일이 없을 때의 처리
            return (false, string.Empty);
        }
    }

    public void Save(int slotNum, string fileName)
    {
        DataManager.instance.Save(slotNum, fileName);
        var data = GetJsonFromFile(slotNum);
        SaveData saveData = JsonConvert.DeserializeObject<SaveData>(data.Item2);
        buttons[slotNum].SetSlotData(slotNum, saveData.saveDate, saveData.fileName);
    }

    public void Load(int slotNum) // 인게임에서 로드와 로비씬에서 로드를 구분해야함
    {
        DataManager.instance.Load(slotNum);
        Debug.Log(slotNum + " : Load");
    }

    void BackBtnFunc()
    {
        MenuClose();
    }
}
