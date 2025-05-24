using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class OptionCanvas : MonoBehaviour
{
    [SerializeField]
    Button EscapeBtn;
    [SerializeField]
    Button SettingsBtn;
    [SerializeField]
    Button SaveBtn;
    [SerializeField]
    Button LoadBtn;
    [SerializeField]
    Button quitBtn;

    public GameObject mainPanel;

    SoundManager soundManager;

    #region Singleton
    public static OptionCanvas instance;

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

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);

        EscapeBtn.onClick.AddListener(() => EscapeBtnFunc());
        SettingsBtn.onClick.AddListener(() => SettingsBtnFunc());
        SaveBtn.onClick.AddListener(() => SaveBtnFunc());
        LoadBtn.onClick.AddListener(() => LoadBtnFunc());
        quitBtn.onClick.AddListener(() => QuitBtnFunc());
        soundManager = SoundManager.instance;
        Button[] btnArr = new Button[] { EscapeBtn, SettingsBtn, SaveBtn, LoadBtn, quitBtn };

        foreach (Button btn in btnArr)
        {
            AddEvent(btn, EventTriggerType.PointerEnter, delegate { OnEnter(btn); });
            AddEvent(btn, EventTriggerType.PointerExit, delegate { OnExit(btn); });
            AddEvent(btn, EventTriggerType.PointerClick, delegate { OnExit(btn); });
            ButtonStateWatcher watcher = btn.gameObject.AddComponent<ButtonStateWatcher>();
            watcher.OnButtonDisabled += () => OnExit(btn);
        }
    }

    public void EscapeBtnFunc()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.PlayerEscapeFromStuck();
        }
        soundManager.PlayUISFX("ButtonClick");
    }

    public void SettingsBtnFunc()
    {
        SettingsMenu.instance.MenuOpen();
        soundManager.PlayUISFX("ButtonClick");
    }

    void SaveBtnFunc()
    {
        SaveLoadMenu.instance.MenuOpen(true);
        soundManager.PlayUISFX("ButtonClick");
    }

    void LoadBtnFunc()
    {
        SaveLoadMenu.instance.MenuOpen(false);
    }

    void QuitBtnFunc()
    {
        if (GameManager.instance.isHost)
        {
            ConfirmPanel.instance.HostQuitGameCallConfirm();
        }
        else
        {
            QuitFunc();
        }
        soundManager.PlayUISFX("ButtonClick");
    }

    public void QuitFunc()
    {
        MainPanelSet(false);
        SteamManager.instance.LeaveLobby();
        NetworkManager.Singleton.Shutdown();
        Destroy(NetworkManager.Singleton.gameObject);
        GameManager.instance.DestroyAllDontDestroyOnLoadObjects();
        SceneManager.LoadScene("MainMenuScene");
    }

    public void MainPanelSet(bool open)
    {
        mainPanel.SetActive(open);

        if (open)
        {
            InputManager.instance.OpenOption();
        }
        else
        {
            InputManager.instance.CloseOption();
        }
    }

    void AddEvent(Button btn, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry eventTrigger = new EventTrigger.Entry();
        eventTrigger.eventID = type;
        eventTrigger.callback.AddListener(action);

        EventTrigger trigger = btn.GetComponent<EventTrigger>();
        trigger.triggers.Add(eventTrigger);
    }

    void OnEnter(Button btn)
    {
        btn.TryGetComponent(out Image img);
        soundManager.PlayUISFX("MouseOnUI");
        Color newColor = img.color;
        newColor.a = 50 / 255f;
        img.color = newColor;
    }

    void OnExit(Button btn)
    {
        btn.TryGetComponent(out Image img);

        Color newColor = img.color;
        newColor.a = 0;
        img.color = newColor;
    }

    public void SaveBtnOnOff(bool on)
    {
        SaveBtn.gameObject.SetActive(on);
        RectTransform rect = quitBtn.gameObject.GetComponent<RectTransform>();
        if (on)
        {
            rect.anchoredPosition = new Vector2(0, -150);
        }
        else
        {
            rect.anchoredPosition = new Vector2(0, -50);
        }
    }
}
