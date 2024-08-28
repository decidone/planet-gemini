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
    float countdownTimer;
    float countdownInterval;

    float windowTimer = 16;

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
        CanelBtn.onClick.AddListener(() => CanelBtnFunc());
        inputManager = InputManager.instance;
    }

    void Update()
    {
        if (windowSetting)
        {
            countdownTimer -= Time.deltaTime;
            countdownText.text = ((int)countdownTimer).ToString();
            if (countdownTimer < countdownInterval)
            {
                CanelBtnFunc();
            }
        }
    }

    public void CallConfirm(SaveLoadBtn btn, bool saveLoadState, int slotNum)
    {
        OkBtn.gameObject.SetActive(true);
        CanelBtn.gameObject.SetActive(true);
        confirmPanel.SetActive(true);
        saveLoadBtn = btn;
        if (saveLoadState)
        {
            contentText.text = "Do you want to save to slot " + slotNum + "?";
            inputObj.SetActive(true);
            InputManager.instance.OpenChat();
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

    public void WindowSizeCallConfirm()
    {
        OkBtn.gameObject.SetActive(true);
        CanelBtn.gameObject.SetActive(true);
        confirmPanel.SetActive(true);
        windowSetting = true;
        countdownTimer = windowTimer;
        countdownText.gameObject.SetActive(true);
        contentText.text = "Do you want to apply resolution?";

        if (GameManager.instance != null)
            GameManager.instance.onUIChangedCallback?.Invoke(confirmPanel);
        else
            MainManager.instance.OpenedUISet(confirmPanel);
    }

    public void KeyBindingDuplication()
    {
        contentText.text = "The key is already assigned" + System.Environment.NewLine + "Press ESC to cancel";
    }

    public void KeyBindingCallConfirm()
    {
        OkBtn.gameObject.SetActive(false);
        CanelBtn.gameObject.SetActive(false);
        confirmPanel.SetActive(true);
        contentText.text = "Waiting For Input" + System.Environment.NewLine + "Press ESC to cancel";

        if (GameManager.instance != null)
            GameManager.instance.onUIChangedCallback?.Invoke(confirmPanel);
        else
            MainManager.instance.OpenedUISet(confirmPanel);
    }

    void OkBtnFunc()
    {
        if(saveLoadBtn)
        {
            saveLoadBtn.BtnConfirm(true, inputField.text);
        }
        else if (windowSetting)
        {
            SettingsMenu.instance.WindowSizeConfirm(true);
        }
        UIClose();
    }

    public void CanelBtnFunc()
    {
        if (saveLoadBtn)
        {
            saveLoadBtn.BtnConfirm(false, inputField.text);
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
        InputManager.instance.CloseChat();

        if (GameManager.instance != null)
            GameManager.instance.onUIChangedCallback?.Invoke(confirmPanel);
        else
            MainManager.instance.ClosedUISet(); 

        saveLoadBtn = null;
        windowSetting = false;
        countdownText.gameObject.SetActive(false);
    }
}
