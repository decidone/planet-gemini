using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem;

public class SettingsMenu : MonoBehaviour
{
    int fixedWidth = 1920;
    int fixedHeight = 1080;

    [SerializeField]
    GameObject settingsPanel;
    [SerializeField]
    Dropdown windowModeDropdown;
    [SerializeField]
    Button backBtn;
    [SerializeField]
    GameObject keyBindingsPanel;
    [SerializeField]
    GameObject keyBindingPanel;
    [SerializeField]
    Button resetBtn;
    List<KeyBindingsBtn> keyBindingsBtn = new List<KeyBindingsBtn>();
    Dictionary<int, (string, int, int, bool, bool)> windowSize = new Dictionary<int, (string, int, int, bool, bool)>();
    Dictionary<string, (InputAction, string)> inputActions = new Dictionary<string, (InputAction, string)>();

    int windowSizeIndex;

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
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        backBtn.onClick.AddListener(() => BackBtnFunc());
        resetBtn.onClick.AddListener(()=> ResetToDefault());
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
            GameManager.instance.onUIChangedCallback?.Invoke(settingsPanel);
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
        WindowModeDropdownFunc(windowModeDropdown);
        windowModeDropdown.onValueChanged.AddListener(delegate { WindowModeDropdownFunc(windowModeDropdown); });
    }

    void WindowModeDropdownFunc(Dropdown dropdown)
    {
        WindowSet(dropdown.value);
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
        windowSizeIndex = dropdownIndex;
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
            btn.BtnSetting(action.Value.Item1, action.Key, action.Value.Item2); // 현제는 초기값만 넣게 하는데 저장 기능 추가되면 저장된 값을 불러오도록(덮어씌우기 하도록)
        }
    }

    private void LoadRebindings()
    {
        foreach (var inputAction in inputActions)
        {
            InputAction action = inputAction.Value.Item1;
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

    void ResetToDefault()
    {
        foreach (var btn in keyBindingsBtn)
        {
            btn.ResetToDefault();
        }
    }

    #endregion

    void BackBtnFunc()
    {
        MenuClose();
    }

    public void SaveData()
    {
        PlayerPrefs.SetInt("WindowIndex", windowSizeIndex);
        PlayerPrefs.Save();
    }

    public void LoadData()
    {
        WindowModeDropdownSet();
        KeyBindingPanelSet();

        int winIndex = PlayerPrefs.GetInt("WindowIndex", 0);
        WindowSet(winIndex);
        windowModeDropdown.value = winIndex;

        LoadRebindings();
    }

    public void GetSettingData(Dictionary<int, (string, int, int, bool, bool)> winSize, Dictionary<string, (InputAction, string)> inputs)
    {
        windowSize = new Dictionary<int, (string, int, int, bool, bool)>(winSize);
        inputActions = new Dictionary<string, (InputAction, string)>(inputs);
    }
}
