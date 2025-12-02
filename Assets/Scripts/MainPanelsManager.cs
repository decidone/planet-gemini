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
    [SerializeField]
    Toggle bloodMoonToggle; 
    SaveLoadMenu saveLoadPanel;
    SoundManager soundManager;

    int seed;
    void Start()
    {
        gameSetting = MainGameSetting.instance;
        saveLoadPanel = SaveLoadMenu.instance;
        soundManager = SoundManager.instance;
        gameStartBtn.onClick.AddListener(() => GameStartBtnFunc());
        backBtn.onClick.AddListener(() => NewGamePanelSet(false));
        //mapSizeDropdown.onValueChanged.AddListener(delegate { MapSizeDropdownFunc(mapSizeDropdown); });
        difficultyLevelDropdown.onValueChanged.AddListener(delegate { DifficultyLevelDropdownFunc(difficultyLevelDropdown); });
        randomSeedToggle.onValueChanged.AddListener(delegate { RandomSeedToggleValue(); });
        //bloodMoonToggle.onValueChanged.AddListener(delegate { BloodMoonToggleValue(); });
        seedInputField.onValueChanged.AddListener(delegate { SeedInputValueChanged(); });

        randomSeedToggle.isOn = true;
        bloodMoonToggle.isOn = true;
    }

    public void NewGamePanelSet(bool state)
    {
        newGamePanel.SetActive(state);
        if (state)
            MainManager.instance.OpenedUISet(gameObject);
        else
            MainManager.instance.ClosedUISet();
        soundManager.PlayUISFX("ButtonClick");
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

        gameSetting.MapSizeSet(mapSizeDropdown.value);
        gameSetting.DifficultylevelSet(difficultyLevelDropdown.value);
        gameSetting.BloodMoonState(bloodMoonToggle.isOn);
        gameSetting.RandomSeedValue(seed);

        gameSetting.NewGameState(true);
        NetworkManager.Singleton.StartHost();
        LoadingUICtrl.Instance.LoadScene("GameScene", true);
        soundManager.PlayUISFX("ButtonClick");
    }

    //void MapSizeDropdownFunc(Dropdown dropdown)
    //{
    //    gameSetting.MapSizeSet(dropdown.value);
    //}

    void DifficultyLevelDropdownFunc(Dropdown dropdown)
    {
        if(dropdown.value == 0)
        {
            bloodMoonToggle.isOn = false;
            bloodMoonToggle.interactable = false;
        }
        else
        {
            bloodMoonToggle.interactable = true;
        }
    }

    public void SaveLoadPanelSet()
    {
        saveLoadPanel.MenuOpen(false);
    }

    void RandomSeedToggleValue()
    {
        if (randomSeedToggle.isOn)
        {
            //seedPanel.SetActive(false);
            seedInputField.text = SetRandomSeed().ToString();
            seedInputField.interactable = false;
        }
        else
        {
            //seedPanel.SetActive(true);
            seedInputField.interactable = true;
        }
    }

    //void BloodMoonToggleValue()
    //{
    //    gameSetting.BloodMoonState(bloodMoonToggle.isOn);
    //}

    void SeedInputValueChanged()
    {
        if (seedInputField.text.Length > 0 && seedInputField.text[0] == '-')
            seedInputField.text = seedInputField.text.Remove(0, 1);
    }

    int SetRandomSeed()
    {
        seed = UnityEngine.Random.Range(0, 999999999);
        return seed;
    }
}
