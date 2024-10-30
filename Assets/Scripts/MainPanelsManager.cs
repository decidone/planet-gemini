using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainPanelsManager : MonoBehaviour
{
    MainGameSetting gameSetting;

    [SerializeField]
    GameObject newGamePanel;
    [SerializeField]
    Dropdown mapSizeDropdown;
    [SerializeField]
    Dropdown difficultyLevelDropdown;
    [SerializeField]
    Button gameStartBtn;
    [SerializeField]
    Button backBtn;
    [SerializeField]
    Toggle randomSeedToggle;
    [SerializeField]
    GameObject seedPanel;
    [SerializeField]
    InputField seedInputField;
    SaveLoadMenu saveLoadPanel;

    // Start is called before the first frame update
    void Start()
    {
        gameSetting = MainGameSetting.instance;
        saveLoadPanel = SaveLoadMenu.instance;
        gameStartBtn.onClick.AddListener(() => GameStartBtnFunc());
        backBtn.onClick.AddListener(() => NewGamePanelSet(false));
        mapSizeDropdown.onValueChanged.AddListener(delegate { MapSizeDropdownFunc(mapSizeDropdown); });
        difficultyLevelDropdown.onValueChanged.AddListener(delegate { DifficultyLevelDropdownFunc(difficultyLevelDropdown); });
        randomSeedToggle.onValueChanged.AddListener(delegate { RandomSeedToggleValue(); });
    }

    public void NewGamePanelSet(bool state)
    {
        newGamePanel.SetActive(state);
        if (state)
            MainManager.instance.OpenedUISet(gameObject);
        else
            MainManager.instance.ClosedUISet();
    }

    void GameStartBtnFunc()
    {
        if (seedInputField.text == "")
        {
            SetRandomSeed();
        }
        else
        {
            int inputValue;
            int.TryParse(seedInputField.text, out inputValue);
            gameSetting.RandomSeedValue(inputValue);
        }
        gameSetting.NewGameState(true);
        NetworkManager.Singleton.StartHost();
        LoadingUICtrl.Instance.LoadScene("GameScene", true);
    }

    void MapSizeDropdownFunc(Dropdown dropdown)
    {
        gameSetting.MapSizeSet(dropdown.value);
    }

    void DifficultyLevelDropdownFunc(Dropdown dropdown)
    {
        gameSetting.DifficultylevelSet(dropdown.value);
    }

    public void SaveLoadPanelSet()
    {
        saveLoadPanel.MenuOpen(false);
    }

    void RandomSeedToggleValue()
    {
        if (randomSeedToggle.isOn)
        {
            seedPanel.SetActive(false);
        }
        else
        {
            seedPanel.SetActive(true);
        }

        SetRandomSeed();
    }

    void SetRandomSeed()
    {
        gameSetting.RandomSeedValue(UnityEngine.Random.Range(int.MinValue, int.MaxValue));
    }
}
