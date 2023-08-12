using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

public class ScienceManager : MonoBehaviour
{
    [SerializeField]
    Button[] tagBtns = null;
    [SerializeField]
    GameObject[] contents = null;
    [SerializeField]
    ScrollRect scrollRect = null;
    [SerializeField]
    GameObject[] infoWindow;

    public GameObject coreLvUI = null;
    public ScienceBtn[] scienceBtns = null;

    ScienceCoreLvCtrl[] buildContent = new ScienceCoreLvCtrl[5];
    ScienceCoreLvCtrl[] battleContent = new ScienceCoreLvCtrl[5];

    public GameObject scienceTreeUI;

    ScienceInfoData scienceInfoData;
    protected GameManager gameManager;
    protected ScienceBtn focusedSciBtn;  // 마우스 위치에 있는 슬롯

    TempScienceDb scienceDb;

    void Awake()
    {
        UISetting();
    }

    void Start()
    {
        gameManager = GameManager.instance;
        scienceDb = TempScienceDb.instance;

        contents[0].SetActive(true);
        contents[1].SetActive(false);

        for (int i = 0; i < tagBtns.Length; i++)
        {
            int buttonIndex = i;
            tagBtns[i].onClick.AddListener(() => SwContent(buttonIndex));
        }

        scienceBtns = contents[0].GetComponentsInChildren<ScienceBtn>();
        ScienceBtn[] additionalBtns = contents[1].GetComponentsInChildren<ScienceBtn>();

        scienceBtns = scienceBtns.Concat(additionalBtns).ToArray();

        for (int i = 0; i < scienceBtns.Length; i++)
        {
            ScienceBtn btn = scienceBtns[i];

            AddEvent(btn, EventTriggerType.PointerEnter, delegate { OnEnter(btn); });
            AddEvent(btn, EventTriggerType.PointerExit, delegate { OnExit(); });
        }
    }

    void Update()
    {
        if (infoWindow[0].activeSelf)
        {
            Vector3 mousePosition = Input.mousePosition;

            // 팝업의 크기를 고려하여 화면 밖으로 나갈 경우 위치 조정
            float popupWidth = 495f;

            // 팝업이 화면 밖으로 나갈 때 X 좌표 조정
            if (mousePosition.x + popupWidth > Screen.width)
            {
                mousePosition.x = Screen.width - popupWidth - 10.0f;
            }
            else if (mousePosition.x < 0)
            {
                mousePosition.x = 0;
            }

            infoWindow[0].transform.position = mousePosition;
        }
        else if (infoWindow[1].activeSelf)
        {
            Vector3 mousePosition = Input.mousePosition;
            infoWindow[1].transform.position = mousePosition;
        }
    }

    void UISetting()
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject buildUI = Instantiate(coreLvUI);
            buildUI.transform.SetParent(contents[0].transform, false);
            buildContent[i] = buildUI.GetComponent<ScienceCoreLvCtrl>();
            buildContent[i].UISetting(i);

            GameObject battleUI = Instantiate(coreLvUI);
            battleUI.transform.SetParent(contents[1].transform, false);
            battleContent[i] = battleUI.GetComponent<ScienceCoreLvCtrl>();
            battleContent[i].UISetting(i);
        }
    }

    void SciDbGet(int index)
    {
        ScienceBtn[] btns = contents[index].GetComponentsInChildren<ScienceBtn>();
        for (int i = 0; i < btns.Length; i++)
        {
            ScienceBtn btn = btns[i];

            if (scienceDb.scienceNameDb.TryGetValue(btn.sciName, out List<int> levels))
            {
                bool sciNameFound = levels.Contains(btn.level);

                if (sciNameFound)
                {
                    btn.LockUiActiveFalse();
                }
            }
        }
    }

    void SwContent(int index)
    {
        scrollRect.content = contents[index].GetComponent<RectTransform>();

        for (int i = 0; i < contents.Length; i++)
        {
            contents[i].SetActive(i == index);
            SciDbGet(index);
        }
    }

    void AddEvent(ScienceBtn btn, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry eventTrigger = new EventTrigger.Entry();
        eventTrigger.eventID = type;
        eventTrigger.callback.AddListener(action);

        EventTrigger trigger = btn.GetComponent<EventTrigger>();
        trigger.triggers.Add(eventTrigger);
    }

    void OnEnter(ScienceBtn btn)
    {
        focusedSciBtn = btn;
        if (focusedSciBtn.sciName != "")
        {
            scienceInfoData = new ScienceInfoData();
            scienceInfoData = ScienceInfoGet.instance.GetBuildingName(focusedSciBtn.sciName, focusedSciBtn.level);

            if (focusedSciBtn.isCore)
            {
                infoWindow[1].GetComponent<InfoWindow>().SetNeedItem(scienceInfoData, focusedSciBtn.sciName, focusedSciBtn.level, focusedSciBtn.isCore);
                infoWindow[1].SetActive(true);
            }
            else
            {
                infoWindow[0].GetComponent<InfoWindow>().SetNeedItem(scienceInfoData, focusedSciBtn.sciName, focusedSciBtn.level, focusedSciBtn.isCore);
                infoWindow[0].SetActive(true);
            }
        }
    }

    void OnExit()
    {
        focusedSciBtn = null;
        infoWindow[0].SetActive(false);
        infoWindow[1].SetActive(false);
    }

    public void OpenUI()
    {
        scienceTreeUI.SetActive(true);
        gameManager.onUIChangedCallback?.Invoke(scienceTreeUI);
    }

    public void CloseUI()
    {
        scienceTreeUI.SetActive(false);
        gameManager.onUIChangedCallback?.Invoke(scienceTreeUI);
    }
}
