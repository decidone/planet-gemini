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
    Dictionary<string, InputAction> inputActions = new Dictionary<string, InputAction>();

    #region Singleton
    public static OptionDataManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        instance = this;
    }
    #endregion

    void Start()
    {
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
    }

    void KeyBindingSetting()
    {
        inputActions.Add("Inventory", InputManager.instance.controls.Inventory.PlayerInven);
        inputActions.Add("Loot", InputManager.instance.controls.Player.Loot);
        inputActions.Add("Interaction", InputManager.instance.controls.Player.Interaction);
        inputActions.Add("Map", InputManager.instance.controls.State.ToggleMap);
        inputActions.Add("Science Tree", InputManager.instance.controls.HotKey.ScienceTree);
        inputActions.Add("Overall", InputManager.instance.controls.HotKey.Overall);
        inputActions.Add("Rotate", InputManager.instance.controls.Building.Rotate);
        inputActions.Add("Unit Attack", InputManager.instance.controls.Unit.Attack);
        inputActions.Add("Unit Patrol", InputManager.instance.controls.Unit.Patrol);
        inputActions.Add("Unit Hold", InputManager.instance.controls.Unit.Hold);
        inputActions.Add("Tank Inventory", InputManager.instance.controls.Player.TankInven);
        inputActions.Add("Tank Attack", InputManager.instance.controls.Player.TankAttack);
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
