using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class SaveLoadMenu : MonoBehaviour
{
    public string path;
    [SerializeField]
    Text optionName;
    [SerializeField]
    GameObject saveLoadPanel;
    [SerializeField]
    RectTransform saveLoadPanelsRectTr;
    [SerializeField]
    GameObject autoSavePanel;
    [SerializeField]
    SaveLoadBtn autoSaveBtn;
    [SerializeField]
    GameObject content;
    [SerializeField]
    GameObject btnObj;
    [SerializeField]
    Button backBtn;

    SaveLoadBtn[] buttons;
    int saveCount = 21;

    #region Singleton
    public static SaveLoadMenu instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        path = Application.persistentDataPath + "/save";
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        buttons = new SaveLoadBtn[saveCount];
        LoadSaveData();
        backBtn.onClick.AddListener(() => BackBtnFunc());
    }

    void LoadSaveData()
    {
        SaveData saveData;
        InGameData inGameData;

        buttons[0] = autoSaveBtn;
        var data = GetJsonFromFile(0);
        if (data.Item1)
        {
            saveData = JsonConvert.DeserializeObject<SaveData>(data.Item2);
            inGameData = saveData.InGameData;
            buttons[0].SetSlotData(0, inGameData.saveDate);
        }
        else
        {
            buttons[0].SetSlotData(0, null);
        }

        for (int i = 1; i < saveCount; i++)
        {
            GameObject btn = Instantiate(btnObj);
            btn.transform.SetParent(content.transform, false);
            buttons[i] = btn.GetComponent<SaveLoadBtn>();
            data = GetJsonFromFile(i);
            if (data.Item1)
            {
                saveData = JsonConvert.DeserializeObject<SaveData>(data.Item2);
                inGameData = saveData.InGameData;
                buttons[i].SetSlotData(i, inGameData.saveDate, inGameData.fileName, inGameData.mapSizeIndex, inGameData.difficultyLevel);
            }
            else
            {
                buttons[i].SetSlotData(i, null, null, -1, -1);
            }
        }
    }

    public void MenuOpen(bool state) // true : save 버튼, false : load 버튼
    {
        if (state)
        {
            optionName.text = "Save";
            autoSavePanel.SetActive(false);
            saveLoadPanelsRectTr.anchoredPosition = new Vector2(0, 0f);
            saveLoadPanelsRectTr.sizeDelta = new Vector2(870, 900);
        }
        else
        {
            optionName.text = "Load";
            autoSavePanel.SetActive(true);
            saveLoadPanelsRectTr.anchoredPosition = new Vector2(0, -60);
            saveLoadPanelsRectTr.sizeDelta = new Vector2(870, 780);
        }

        saveLoadPanel.SetActive(true);
        foreach (SaveLoadBtn btn in buttons)
        {
            btn.BtnStateSet(state);
        }
        if(GameManager.instance != null)
            GameManager.instance.onUIChangedCallback?.Invoke(saveLoadPanel);
        else
            MainManager.instance.OpenedUISet(saveLoadPanel);
    }

    public void MenuClose()
    {
        saveLoadPanel.SetActive(false);
        if (GameManager.instance != null)
            GameManager.instance.onUIChangedCallback?.Invoke(saveLoadPanel);
        else
            MainManager.instance.ClosedUISet();
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
        InGameData inGameData = saveData.InGameData;
        buttons[slotNum].SetSlotData(slotNum, inGameData.saveDate, inGameData.fileName, inGameData.mapSizeIndex, inGameData.difficultyLevel);
    }

    public void Load(int slotNum) // 인게임에서 로드와 로비씬에서 로드를 구분해야함
    {
        DataManager.instance.Load(slotNum);
        Debug.Log(slotNum + " : Load");
    }

    public void LoadConfirm(int slotNum)
    {
        MainGameSetting.instance.NewGameState(false);
        MainGameSetting.instance.LoadDataIndexSet(slotNum);
        var data = GetJsonFromFile(slotNum);
        SaveData saveData = JsonConvert.DeserializeObject<SaveData>(data.Item2);
        InGameData inGameData = saveData.InGameData;
        MainGameSetting.instance.MapSizeSet(inGameData.mapSizeIndex);
        MainGameSetting.instance.RandomSeedValue(inGameData.seed);
        //MainGameSetting.instance.LoadMapData(saveData.mapData);

        if (GameManager.instance != null)
        {
            LoadingUICtrl.Instance.LoadScene("GameScene");
            // NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        }
        else
        {
            //SteamManager.instance.HostLobby();
            NetworkManager.Singleton.StartHost();
            LoadingUICtrl.Instance.LoadScene("GameScene", true);
        }
    }

    public void Delete(int slotNum)
    {
        string filePath = path + slotNum.ToString() + ".json";

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    void BackBtnFunc()
    {
        MenuClose();
    }
}
