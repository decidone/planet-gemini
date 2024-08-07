using System;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class OptionDataManager : MonoBehaviour
{
    SettingsMenu settingsMenu;
    SoundManager soundManager;

    Dictionary<int, (string, int, int, bool, bool)> windowSize = new Dictionary<int, (string, int, int, bool, bool)>();  // 드롭다운인덱스, 이름, 가로, 높이, 전체화면, 커서 가두기
    Dictionary<string, (InputAction, string)> inputActions = new Dictionary<string, (InputAction, string)>();

    #region Singleton
    public static OptionDataManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of OptionDataManager found!");
            Destroy(gameObject);
            return;
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);

        settingsMenu = SettingsMenu.instance;
        soundManager = SoundManager.instance;
        WindowModeSetting();
        KeyBindingSetting();
        settingsMenu.GetSettingData(windowSize, inputActions);
        Load();
    }

    void WindowModeSetting()
    {
        windowSize.Add(0, ("FullScreen", 1920, 1080, true, true));
        windowSize.Add(1, ("BorderLess", 1920, 1080, true, false));
        windowSize.Add(2, ("Window : 1920 x 1080", 1920, 1080, false, false));
        windowSize.Add(3, ("Window : 1600 x 900", 1600, 900, false, false));
        windowSize.Add(4, ("Window : 1280 x 720", 1280, 720, false, false));
        windowSize.Add(5, ("Window : 960 x 540", 960, 540, false, false));
    }

    void KeyBindingSetting()
    {
        inputActions.Add("Inventory", (InputManager.instance.controls.Inventory.PlayerInven, "E"));
        inputActions.Add("Loot", (InputManager.instance.controls.Player.Loot, "C"));
        inputActions.Add("Interaction", (InputManager.instance.controls.Player.Interaction, "F"));
        inputActions.Add("Market", (InputManager.instance.controls.Player.Market, "V"));
        inputActions.Add("Map", (InputManager.instance.controls.State.ToggleMap, "M"));
        inputActions.Add("ScienceTree", (InputManager.instance.controls.HotKey.ScienceTree, "T"));
        inputActions.Add("Overall", (InputManager.instance.controls.HotKey.Overall, "O"));
        inputActions.Add("Rotate", (InputManager.instance.controls.Building.Rotate, "R"));
        inputActions.Add("Unit Attack", (InputManager.instance.controls.Unit.Attack, "A"));
        inputActions.Add("Unit Patrol", (InputManager.instance.controls.Unit.Patrol, "P"));
        inputActions.Add("Unit Hold", (InputManager.instance.controls.Unit.Hold, "H"));
    }

    public void Save()
    {
        settingsMenu.SaveData();
        soundManager.SaveData();
    }

    public void Load()
    {
        settingsMenu.LoadData();
        soundManager.LoadData();
    }
}
