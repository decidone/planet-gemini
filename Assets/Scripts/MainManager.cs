using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
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
    }

    void HostBtnFunc()
    {
        mainBtns.SetActive(false);
        hostBtns.SetActive(true);
    }

    void JoinBtnFunc()
    {
        LoadingSceneManager.LoadScene("LobbyScene");
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
}
