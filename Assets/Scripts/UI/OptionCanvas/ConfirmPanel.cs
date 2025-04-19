using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ConfirmPanel : MonoBehaviour
{
    [SerializeField]
    GameObject confirmPanel;
    [SerializeField]
    Text contentText;
    [SerializeField]
    Button OkBtn;
    [SerializeField]
    Button CanelBtn;
    [SerializeField]
    GameObject inputObj;
    [SerializeField]
    InputField inputField;
    [SerializeField]
    Text countdownText;
    SaveLoadBtn saveLoadBtn;
    InputManager inputManager;

    bool windowSetting;
    bool keyBindingReset;
    bool hostGameQuit;
    float countdownTimer;
    float countdownInterval;

    float windowTimer = 16;
    bool isSaveLoadDelete;

    #region Singleton
    public static ConfirmPanel instance;

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
        OkBtn.onClick.AddListener(() => OkBtnFunc());
        CanelBtn.onClick.AddListener(() => CancelBtnFunc());
        inputManager = InputManager.instance;
    }

    void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        if (windowSetting)
        {
            countdownTimer -= Time.deltaTime;
            countdownText.text = ((int)countdownTimer).ToString();
            if (countdownTimer < countdownInterval)
            {
                CancelBtnFunc();
            }
        }
    }

    void PanelSetBtn(bool btnOn)
    {
        OkBtn.gameObject.SetActive(btnOn);
        CanelBtn.gameObject.SetActive(btnOn);
        confirmPanel.SetActive(true);
    }

    public void CallConfirm(SaveLoadBtn btn, bool saveLoadState, int slotNum)
    {
        PanelSetBtn(true);
        saveLoadBtn = btn;
        isSaveLoadDelete = false;
        if (saveLoadState)
        {
            contentText.text = "Do you want to save to slot " + slotNum + "?";
            inputObj.SetActive(true);
            //InputManager.instance.OpenChat();
        }
        else
        {
            contentText.text = "Do you want to load to slot " + slotNum + "?";
        }
        if (GameManager.instance != null)
            GameManager.instance.onUIChangedCallback?.Invoke(confirmPanel);
        else
            MainManager.instance.OpenedUISet(confirmPanel);
    }

    public void CallDeleteConfirm(SaveLoadBtn btn, int slotNum)
    {
        PanelSetBtn(true);
        saveLoadBtn = btn;
        isSaveLoadDelete = true;
        contentText.text = "Do you really want to delete saved data at slot " + slotNum + "?";
        
        if (GameManager.instance != null)
            GameManager.instance.onUIChangedCallback?.Invoke(confirmPanel);
        else
            MainManager.instance.OpenedUISet(confirmPanel);
    }

    public void WindowSizeCallConfirm()
    {
        PanelSetBtn(true);
        windowSetting = true;
        countdownTimer = windowTimer;
        countdownText.gameObject.SetActive(true);
        contentText.text = "Do you want to apply resolution?";

        if (GameManager.instance != null)
            GameManager.instance.onUIChangedCallback?.Invoke(confirmPanel);
        else
            MainManager.instance.OpenedUISet(confirmPanel);
    }

    public void KeyBindingResetConfirm()
    {
        PanelSetBtn(true);
        keyBindingReset = true;
        contentText.text = "Do you want to reset" + System.Environment.NewLine + "all key bindings?";

        if (GameManager.instance != null)
            GameManager.instance.onUIChangedCallback?.Invoke(confirmPanel);
        else
            MainManager.instance.OpenedUISet(confirmPanel);
    }

    public void KeyBindingDuplication()
    {
        contentText.text = "The key is already assigned" + System.Environment.NewLine + "Press ESC to cancel.";
    }

    public void KeyBindingCallConfirm()
    {
        PanelSetBtn(false);
        contentText.text = "Waiting For Input" + System.Environment.NewLine + "Press ESC to cancel.";

        if (GameManager.instance != null)
            GameManager.instance.onUIChangedCallback?.Invoke(confirmPanel);
        else
            MainManager.instance.OpenedUISet(confirmPanel);
    }

    public void HostQuitGameCallConfirm()
    {
        PanelSetBtn(true);
        hostGameQuit = true;
        contentText.text = "Return to the Main Menu?" + System.Environment.NewLine + "Unsaved progress will be lost.";

        if (GameManager.instance != null)
            GameManager.instance.onUIChangedCallback?.Invoke(confirmPanel);
        else
            MainManager.instance.OpenedUISet(confirmPanel);
    }

    void OkBtnFunc()
    {
        if(saveLoadBtn)
        {
            if (isSaveLoadDelete)
            {
                saveLoadBtn.DeleteBtnConfirm(true);
            }
            else
            {
                saveLoadBtn.BtnConfirm(true, inputField.text);
            }
        }
        else if (windowSetting)
        {
            SettingsMenu.instance.WindowSizeConfirm(true);
        }
        else if (keyBindingReset)
        {
            SettingsMenu.instance.ResetToDefault();
        }
        else if (hostGameQuit)
        {
            OptionCanvas.instance.QuitFunc();
        }
        UIClose();
    }

    public void CancelBtnFunc()
    {
        if (saveLoadBtn)
        {
            if (isSaveLoadDelete)
            {
                saveLoadBtn.DeleteBtnConfirm(false);
            }
            else
            {
                saveLoadBtn.BtnConfirm(false, inputField.text);
            }
        }
        else if (windowSetting)
        {
            SettingsMenu.instance.WindowSizeConfirm(false);
        }
        UIClose();
    }

    public void UIClose()
    {
        confirmPanel.SetActive(false);
        inputObj.SetActive(false);
        inputField.text = string.Empty;
        //InputManager.instance.CloseChat();

        if (GameManager.instance != null)
            GameManager.instance.onUIChangedCallback?.Invoke(confirmPanel);
        else
            MainManager.instance.ClosedUISet(); 

        saveLoadBtn = null;
        windowSetting = false;
        keyBindingReset = false;
        hostGameQuit = false;
        countdownText.gameObject.SetActive(false);
    }
}
