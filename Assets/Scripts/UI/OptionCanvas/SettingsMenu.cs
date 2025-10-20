using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SettingsMenu : MonoBehaviour
{
    public int fixedWidth = 1920;
    public int fixedHeight = 1080;

    [SerializeField]
    GameObject settingsPanel;
    [SerializeField]
    Dropdown windowModeDropdown;
    [SerializeField]
    Button saveBtn;
    [SerializeField]
    GameObject keyBindingsPanel;
    [SerializeField]
    GameObject keyBindingPanel;
    [SerializeField]
    Button resetBtn;
    List<KeyBindingsBtn> keyBindingsBtn = new List<KeyBindingsBtn>();
    Dictionary<int, (string, int, int, bool, bool)> windowSize = new Dictionary<int, (string, int, int, bool, bool)>();
    public Dictionary<string, InputAction> inputActions = new Dictionary<string, InputAction>();
    [SerializeField]
    public Toggle tutorialQuestToggle;
    int windowSizeIndex;
    int tempWindowSizeIndex;
    bool gameStartFirstSet;

    public int autoSaveInterval;
    int autoSaveValue;
    [SerializeField]
    InputField autoSaveInput;
    [SerializeField]
    public Slider autoSaveSlider;
    bool isInputChanging = false;
    bool isSliderChanging = false;
    SoundManager soundManager;

    #region Singleton
    public static SettingsMenu instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        gameStartFirstSet = true;
    }
    #endregion

    void Start()
    {
        soundManager = SoundManager.instance;
        saveBtn.onClick.AddListener(() => SaveBtnFunc());
        resetBtn.onClick.AddListener(() => ResetBtnFunc());
        tutorialQuestToggle.onValueChanged.AddListener(delegate { TutorialQuestToggleValue(); });
        autoSaveInput.onValueChanged.AddListener(delegate { AutoSaveInputFunc(); });
        autoSaveSlider.onValueChanged.AddListener(delegate { AutoSaveSliderFunc(); });
    }

    public void MenuOpen()
    {
        settingsPanel.SetActive(true);

        if (GameManager.instance != null)
            GameManager.instance.onUIChangedCallback?.Invoke(settingsPanel);
        else
            MainManager.instance.OpenedUISet(settingsPanel);
    }

    public void MenuClose()
    {
        OptionDataManager.instance.Save();
        settingsPanel.SetActive(false);
        if (GameManager.instance != null)
        {
            GameManager.instance.onUIChangedCallback?.Invoke(settingsPanel);
            BasicUIBtns.instance.KeyValueSet();
        }
        else
            MainManager.instance.ClosedUISet();
    }

    #region WindowMode
    void WindowModeDropdownSet()
    {
        windowModeDropdown.ClearOptions();
        foreach (var winSize in windowSize)
        {
            windowModeDropdown.options.Add(new Dropdown.OptionData(winSize.Value.Item1));
        }
        windowModeDropdown.onValueChanged.AddListener(delegate { WindowModeDropdownFunc(windowModeDropdown); });
    }

    void WindowModeDropdownFunc(Dropdown dropdown)
    {
        WindowSet(dropdown.value);
        if (gameStartFirstSet)
        {
            gameStartFirstSet = false;
            return;
        }

        ConfirmPanel.instance.WindowSizeCallConfirm();
    }

    void WindowSet(int dropdownIndex)
    {
        var data = windowSize[dropdownIndex];
        if (data.Item4)
        {
            Screen.SetResolution(data.Item2, data.Item3, FullScreenMode.FullScreenWindow);
            Screen.fullScreen = true;
        }
        else
        {
            Screen.SetResolution(data.Item2, data.Item3, FullScreenMode.Windowed);
            Screen.fullScreen = false;
        }
        if (data.Item5)
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }

        if (GameManager.instance != null)
            CameraController.instance.WindowSizeSet(CameraController.instance.zoomLevel, data.Item2, data.Item3);

        fixedWidth = data.Item2;
        fixedHeight = data.Item3;
        tempWindowSizeIndex = dropdownIndex;
    }

    void TutorialQuestToggleValue()
    {
        if (GameManager.instance != null)
        {
            if (tutorialQuestToggle.isOn)
            {
                QuestManager.instance.UIOpen();
            }
            else
            {
                QuestManager.instance.UIClose();
            }
        }
        soundManager.PlayUISFX("ButtonClick");
    }
    #endregion

    #region KeyBindings
    void KeyBindingPanelSet()
    {
        foreach (var action in inputActions)
        {
            GameObject keyBindPanel = Instantiate(keyBindingPanel);
            keyBindPanel.transform.SetParent(keyBindingsPanel.transform, false);
            KeyBindingsBtn btn = keyBindPanel.GetComponentInChildren<KeyBindingsBtn>();
            keyBindingsBtn.Add(btn);
            string key = inputActionSet(action.Value);
            btn.BtnSetting(action.Value, action.Key, key);
        }
    }

    string inputActionSet(InputAction input)
    {
        var playerInvenAction = input.bindings[0].effectivePath;
        string key = InputControlPath.ToHumanReadableString(playerInvenAction, InputControlPath.HumanReadableStringOptions.OmitDevice);
        return key;
    }

    private void LoadRebindings()
    {
        foreach (var inputAction in inputActions)
        {
            InputAction action = inputAction.Value;
            string name = inputAction.Key;
            // 저장된 리바인딩 데이터를 로드
            if (PlayerPrefs.HasKey(name))
            {
                string rebinds = PlayerPrefs.GetString(name);
                action.LoadBindingOverridesFromJson(rebinds);
                Debug.Log("Rebindings loaded.");
            }
        }

        foreach (var btn in keyBindingsBtn)
        {
            btn.InputTextSet();
        }
    }

    void ResetBtnFunc()
    {
        EventSystem.current.SetSelectedGameObject(null);

        ConfirmPanel.instance.KeyBindingResetConfirm();
        soundManager.PlayUISFX("ButtonClick");
    }

    public void ResetToDefault()
    {
        foreach (var btn in keyBindingsBtn)
        {
            btn.ResetToDefault();
        }
    }
    #endregion

    void SaveBtnFunc()
    {
        MenuClose();
        soundManager.PlayUISFX("ButtonClick");
    }

    public void SaveData()
    {
        PlayerPrefs.SetInt("WindowIndex", windowSizeIndex);
        PlayerPrefs.SetInt("TutorialQuest", tutorialQuestToggle.isOn ? 0 : 1);
        PlayerPrefs.SetInt("AutoSaveTime", autoSaveValue);
        if (GameManager.instance != null)
        {
            GameManager.instance.AutoSaveTimeIntervalSet(autoSaveInterval);
        }
        PlayerPrefs.Save();
    }

    public void LoadData()
    {
        WindowModeDropdownSet();
        KeyBindingPanelSet();

        windowSizeIndex = PlayerPrefs.GetInt("WindowIndex", 0);
        tutorialQuestToggle.isOn = PlayerPrefs.GetInt("TutorialQuest", 0) == 0;
        autoSaveSlider.value = PlayerPrefs.GetInt("AutoSaveTime", 10);
        autoSaveValue = (int)autoSaveSlider.value;
        autoSaveInput.text = autoSaveValue.ToString();
        autoSaveInterval = autoSaveValue * 60;

        WindowSet(windowSizeIndex);
        if (windowModeDropdown.value == windowSizeIndex)
        {
            gameStartFirstSet = false;
        }
        windowModeDropdown.value = windowSizeIndex;
        LoadRebindings();
    }

    public void GetSettingData(Dictionary<int, (string, int, int, bool, bool)> winSize, Dictionary<string, InputAction> inputs)
    {
        windowSize = new Dictionary<int, (string, int, int, bool, bool)>(winSize);
        inputActions = new Dictionary<string, InputAction>(inputs);
    }

    public void WindowSizeConfirm(bool isOk)
    {
        Debug.Log(isOk);
        if(isOk)
        {
            windowSizeIndex = tempWindowSizeIndex;
        }
        else
        {
            WindowSet(windowSizeIndex);
            windowModeDropdown.value = windowSizeIndex;
        }

        SaveData();
    }

    void AutoSaveInputFunc()
    {
        if (isSliderChanging) return;

        isInputChanging = true;

        if (!int.TryParse(autoSaveInput.text, out int textInt))
        {
            autoSaveSlider.value = autoSaveSlider.minValue;
            autoSaveInput.text = autoSaveSlider.minValue.ToString();
        }
        else if (textInt < autoSaveSlider.minValue)
        {
            autoSaveSlider.value = autoSaveSlider.minValue;
            autoSaveInput.text = autoSaveSlider.minValue.ToString();
        }
        else if (textInt > autoSaveSlider.maxValue)
        {
            autoSaveSlider.value = autoSaveSlider.maxValue;
            autoSaveInput.text = autoSaveSlider.maxValue.ToString();
        }
        else
        {
            autoSaveSlider.value = textInt;
        }

        autoSaveValue = (int)autoSaveSlider.value;
        autoSaveInterval = autoSaveValue * 60;
        isInputChanging = false;
    }

    void AutoSaveSliderFunc()
    {
        if (isInputChanging) return;

        isSliderChanging = true;

        autoSaveValue = (int)autoSaveSlider.value;
        autoSaveInput.text = autoSaveValue.ToString();
        autoSaveInterval = autoSaveValue * 60;

        isSliderChanging = false;
    }
}
