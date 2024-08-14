using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    [SerializeField]
    GameObject mainBtns;
    [SerializeField]
    Button hostBtn;
    [SerializeField]
    Button joinBtn;
    [SerializeField]
    Button settingsBtn;
    [SerializeField]
    Button quitBtn;

    [SerializeField]
    GameObject hostBtns;
    [SerializeField]
    Button newGameBtn;
    [SerializeField]
    Button loadBtn;
    [SerializeField]
    Button backBtn;

    [SerializeField]
    MainPanelsManager panelsManager;

    InputManager inputManager;
    GameObject openedUI;

    #region Singleton
    public static MainManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of MainManager found!");
            return;
        }

        instance = this;
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        hostBtn.onClick.AddListener(() => HostBtnFunc());
        joinBtn.onClick.AddListener(() => JoinBtnFunc());
        settingsBtn.onClick.AddListener(() => SettingsBtnFunc());
        quitBtn.onClick.AddListener(() => QuitBtnFunc());
        newGameBtn.onClick.AddListener(() => NewGameBtnFunc());
        loadBtn.onClick.AddListener(() => LoadBtnFunc());
        backBtn.onClick.AddListener(() => BackBtnFunc());

        Button[] mainBtnArr = mainBtns.GetComponentsInChildren<Button>();
        Button[] hostBtnsArr = hostBtns.GetComponentsInChildren<Button>();

        foreach (Button btn in mainBtnArr)
        {
            AddEvent(btn, EventTriggerType.PointerEnter, delegate { OnEnter(btn); });
            AddEvent(btn, EventTriggerType.PointerExit, delegate { OnExit(btn); });
            AddEvent(btn, EventTriggerType.PointerClick, delegate { OnExit(btn); });
        }

        foreach (Button btn in hostBtnsArr)
        {
            AddEvent(btn, EventTriggerType.PointerEnter, delegate { OnEnter(btn); });
            AddEvent(btn, EventTriggerType.PointerExit, delegate { OnExit(btn); });
            AddEvent(btn, EventTriggerType.PointerClick, delegate { OnExit(btn); });
        }
        inputManager = InputManager.instance;
        inputManager.controls.HotKey.Escape.performed += Escape;
    }

    void OnDisable()
    {
        inputManager.controls.HotKey.Escape.performed -= Escape;
    }


    void HostBtnFunc()
    {
        mainBtns.SetActive(false);
        hostBtns.SetActive(true);
        OpenedUISet(hostBtns);
    }

    void JoinBtnFunc()
    {
        SteamManager.instance.GetLobbiesList();
        //LoadingSceneManager.LoadScene("LobbyScene");
    }

    void SettingsBtnFunc()
    {
        OptionCanvas.instance.SettingsBtnFunc();
    }

    void QuitBtnFunc()
    {
        Application.Quit();
    }

    void NewGameBtnFunc()
    {
        panelsManager.NewGamePanelSet(true);
    }

    void LoadBtnFunc()
    {
        panelsManager.SaveLoadPanelSet();
    }

    void BackBtnFunc()
    {
        mainBtns.SetActive(true);
        hostBtns.SetActive(false);
        ClosedUISet();
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

    public void OpenedUISet(GameObject obj)
    {
        openedUI = obj;
    }

    public void ClosedUISet()
    {
        openedUI = null;
    }

    void Escape(InputAction.CallbackContext ctx)
    {
        if (openedUI != null)
        {
            switch (openedUI.gameObject.name)
            {
                case "HostBtns":
                    BackBtnFunc();
                    break;
                case "NewGame":
                    panelsManager.NewGamePanelSet(false);
                    break;
                case "LobbyMenu":
                    LobbiesListManager.instance.CloseUI();
                    break;
                case "SettingsPanel":
                    SettingsMenu.instance.MenuClose();
                    break;
                case "SaveLoadPanel":
                    SaveLoadMenu.instance.MenuClose();
                    break;
            }
        }
    }
}
