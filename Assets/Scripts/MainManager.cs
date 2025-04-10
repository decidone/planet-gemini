using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    [SerializeField]
    GameObject[] btnArrs;
    int btnIndex;

    [SerializeField]
    Button hostBtn;
    [SerializeField]
    Button joinBtn;
    [SerializeField]
    Button settingsBtn;
    [SerializeField]
    Button quitBtn;

    [SerializeField]
    Button publicGameBtn;
    [SerializeField]
    Button privateBtn;
    [SerializeField]
    Text publicPrivatetext;
    [SerializeField]
    Button newGameBtn;
    [SerializeField]
    Button loadBtn;

    [SerializeField]
    Button[] backBtns;

    [SerializeField]
    MainPanelsManager panelsManager;

    InputManager inputManager;
    public List<GameObject> openedUI = new List<GameObject>();

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
        hostBtn.onClick.AddListener(() => OpenUI(1));
        joinBtn.onClick.AddListener(() => JoinBtnFunc());
        settingsBtn.onClick.AddListener(() => SettingsBtnFunc());
        quitBtn.onClick.AddListener(() => QuitBtnFunc());
        newGameBtn.onClick.AddListener(() => NewGameBtnFunc());
        loadBtn.onClick.AddListener(() => LoadBtnFunc());
        privateBtn.onClick.AddListener(() =>
        {
            MainGameSetting.instance.isPublic = false;
            publicPrivatetext.text = "Private Game";
            OpenUI(2);
        }); 
        publicGameBtn.onClick.AddListener(() =>
        {
            MainGameSetting.instance.isPublic = true;
            publicPrivatetext.text = "Public Game";
            OpenUI(2);
        });

        foreach (Button btn in backBtns)
        {
            btn.onClick.AddListener(BackBtnFunc);
        }

        foreach (GameObject btnObj in btnArrs)
        {
            Button[] btnArr = btnObj.GetComponentsInChildren<Button>();
            foreach (Button btn in btnArr)
            {
                AddEvent(btn, EventTriggerType.PointerEnter, delegate { OnEnter(btn); });
                AddEvent(btn, EventTriggerType.PointerExit, delegate { OnExit(btn); });
                ButtonStateWatcher watcher = btn.gameObject.AddComponent<ButtonStateWatcher>();
                watcher.OnButtonDisabled += () => OnExit(btn);
            }
        }
        OpenUI(0);

        inputManager = InputManager.instance;
        inputManager.controls.HotKey.Escape.performed += Escape;
    }

    void OnDisable()
    {
        inputManager.controls.HotKey.Escape.performed -= Escape;
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
        OpenUI(btnIndex - 1);
        ClosedUISet();
    }

    void OpenUI(int index)
    {
        btnIndex = index;
        for (int i = 0; i < btnArrs.Length; i++)
        {
            if (index == i)
                btnArrs[i].SetActive(true);
            else
                btnArrs[i].SetActive(false);
        }
        OpenedUISet(btnArrs[index]);
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
        if (!openedUI.Contains(obj))
        {
            openedUI.Add(obj);
        }
    }

    public void ClosedUISet()
    {
        openedUI.RemoveAt(openedUI.Count - 1);
    }

    void Escape(InputAction.CallbackContext ctx)
    {
        if (openedUI.Count > 0)
        {
            switch (openedUI[openedUI.Count - 1].gameObject.name)
            {
                case "HostBtns":
                    BackBtnFunc();
                    break;
                case "PublicAndPrivateBtns":
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
                case "ConfirmPanel":
                    ConfirmPanel.instance.CancelBtnFunc();
                    break;

            }
        }
    }
}

public class ButtonStateWatcher : MonoBehaviour
{
    public event System.Action OnButtonDisabled;

    private void OnDisable()
    {
        OnButtonDisabled?.Invoke();
    }
}