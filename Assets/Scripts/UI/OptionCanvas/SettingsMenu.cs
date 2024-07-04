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
    List<KeyBindingsBtn> keyBindingsBtn = new List<KeyBindingsBtn>();
    Dictionary<InputAction, (string, string)> inputActions = new Dictionary<InputAction, (string, string)>();

    #region Singleton
    public static SettingsMenu instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of DataManager found!");
            return;
        }

        instance = this;

        //Camera camera = Camera.main;
        //Rect rect = camera.rect;
        //float scaleheight = ((float)Screen.width / Screen.height) / ((float)16 / 9); // (가로 / 세로)
        //float scalewidth = 1f / scaleheight;
        //if (scaleheight < 1)
        //{
        //    rect.height = scaleheight;
        //    fixedWidth = Screen.width;
        //    fixedHeight = fixedWidth * 9 / 16;
        //    rect.y = (1f - scaleheight) / 2f;
        //}
        //else
        //{
        //    rect.width = scalewidth;
        //    fixedHeight = Screen.height;
        //    fixedWidth = fixedHeight * 16 / 9;
        //    rect.x = (1f - scalewidth) / 2f;
        //}
        //camera.rect = rect;

        //Debug.Log(fixedWidth + " : " + fixedWidth); 
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        CameraController.instance.WindowSizeSet(1, fixedWidth, fixedHeight);

        backBtn.onClick.AddListener(() => BackBtnFunc());
        WindowModeDropdownSet();
        KeyBindingSetting();
        Screen.SetResolution(fixedWidth, fixedHeight, false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void MenuOpen()
    {
        settingsPanel.SetActive(true);
    }

    public void MenuClose()
    {
        settingsPanel.SetActive(false);
    }

    #region WindowMode
    void WindowModeDropdownSet()
    {
        windowModeDropdown.ClearOptions();
        windowModeDropdown.options.Add(new Dropdown.OptionData("FullScreen"));
        windowModeDropdown.options.Add(new Dropdown.OptionData("BorderLess"));
        windowModeDropdown.options.Add(new Dropdown.OptionData("Window : 960 x 540"));
        windowModeDropdown.options.Add(new Dropdown.OptionData("Window : 1100 x 540"));
        WindowModeDropdownFunc(windowModeDropdown);
        windowModeDropdown.onValueChanged.AddListener(delegate { WindowModeDropdownFunc(windowModeDropdown); });
    }

    void WindowModeDropdownFunc(Dropdown dropdown)
    {
        switch (dropdown.value)
        {
            case 0:
                FullScreenSet();
                break;
            case 1:
                BorderlessSet();
                break;
            case 2:
                WindowSettset(960, 540);
                break;
            case 3:
                WindowSettset(1100, 540);
                break;
            default:
                Debug.Log("Unhandled dropdown option");
                break;
        }
    }

    void FullScreenSet()
    {
        Screen.SetResolution(fixedWidth, fixedHeight, FullScreenMode.FullScreenWindow);
        Screen.fullScreen = true;
        Cursor.lockState = CursorLockMode.Confined;
        CameraController.instance.WindowSizeSet(CameraController.instance.zoomLevel, fixedWidth, fixedHeight);
        Debug.Log("FullScreenSet : " + fixedWidth + " : " + fixedHeight);
    }

    void BorderlessSet()
    {
        Screen.SetResolution(fixedWidth, fixedHeight, FullScreenMode.FullScreenWindow);
        Cursor.lockState = CursorLockMode.None;
        CameraController.instance.WindowSizeSet(CameraController.instance.zoomLevel, fixedWidth, fixedHeight);
        Debug.Log("BorderlessSet : " + fixedWidth + " : " + fixedHeight);
    }

    void WindowSettset(int width, int height)
    {
        Screen.SetResolution(width, height, FullScreenMode.Windowed);
        Screen.fullScreen = false;
        Cursor.lockState = CursorLockMode.None;
        CameraController.instance.WindowSizeSet(CameraController.instance.zoomLevel, 960, 540);// 삭제요망
        Debug.Log("WindowSet");
    }

    #endregion

    #region KeyBindings
    void KeyBindingSetting()
    {
        inputActions.Add(InputManager.instance.controls.Inventory.PlayerInven, ("Inventory", "E"));
        inputActions.Add(InputManager.instance.controls.Player.Loot, ("Loot", "C"));
        inputActions.Add(InputManager.instance.controls.Player.Interaction, ("Interaction", "F"));
        inputActions.Add(InputManager.instance.controls.Player.Market, ("Market", "V"));
        inputActions.Add(InputManager.instance.controls.State.ToggleMap, ("Map", "M"));
        inputActions.Add(InputManager.instance.controls.HotKey.ScienceTree, ("ScienceTree", "T"));
        inputActions.Add(InputManager.instance.controls.HotKey.Overall, ("Overall", "O"));
        inputActions.Add(InputManager.instance.controls.Building.Rotate, ("Rotate", "R"));
        inputActions.Add(InputManager.instance.controls.Unit.Patrol, ("Unit Patrol", "P"));
        inputActions.Add(InputManager.instance.controls.Unit.Hold, ("Unit Hold", "H"));

        KeyBindingPanelSet();
    }

    void KeyBindingPanelSet()
    {
        foreach (var action in inputActions)
        {
            GameObject keyBindPanel = Instantiate(keyBindingPanel);
            keyBindPanel.transform.SetParent(keyBindingsPanel.transform, false);
            KeyBindingsBtn btn = keyBindPanel.GetComponentInChildren<KeyBindingsBtn>();
            keyBindingsBtn.Add(btn);
            btn.BtnSetting(action.Key, action.Value.Item1, action.Value.Item2); // 현제는 초기값만 넣게 하는데 저장 기능 추가되면 저장된 값을 불러오도록(덮어씌우기 하도록)
        }
    }

    #endregion

    void BackBtnFunc()
    {
        MenuClose();
    }
}
