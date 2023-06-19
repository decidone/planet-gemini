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
    GameObject scienceUi = null;
    [SerializeField]
    ScrollRect scrollRect = null;
    [SerializeField]
    GameObject infoWindow;

    public ScienceBtn[] scienceBtns = null;

    ScienceInfoData scienceInfoData;

    protected ScienceBtn focusedBtn;  // 마우스 위치에 있는 슬롯

    // Start is called before the first frame update
    void Start()
    {
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
            AddEvent(btn, EventTriggerType.PointerExit, delegate { OnExit(btn); });
        }
    }

    void Update()
    {
        if (infoWindow.activeSelf)
        {
            Vector3 mousePosition = Input.mousePosition;
            infoWindow.transform.position = mousePosition;
        }
    }

    void SwContent(int index)
    {
        scrollRect.content = contents[index].GetComponent<RectTransform>();

        for (int i = 0; i < contents.Length; i++)
        {
            contents[i].SetActive(i == index);
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
        focusedBtn = btn;

        if (focusedBtn.iconName != "")
        {
            scienceInfoData = new ScienceInfoData();
            scienceInfoData = ScienceInfoGet.instance.GetBuildingName(focusedBtn.iconName);
            infoWindow.GetComponent<InfoWindow>().SetNeedItem(scienceInfoData);
            infoWindow.SetActive(true);
        }

    }

    void OnExit(ScienceBtn btn)
    {
        focusedBtn = null;

        infoWindow.SetActive(false);
    }
}
