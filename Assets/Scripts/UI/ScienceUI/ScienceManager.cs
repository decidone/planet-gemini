using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

// UTF-8 설정
public class ScienceManager : MonoBehaviour
{
    [SerializeField]
    Button[] tagBtns;
    [SerializeField]
    GameObject[] contents;
    [SerializeField]
    ScrollRect scrollRect;
    [SerializeField]
    GameObject[] infoWindow;
    [SerializeField]
    GameObject[] infoWindowObj;
    [SerializeField]
    GameObject itemInputWindow;
    SciItemSetWindow sciItemSetWindow;
    [SerializeField]
    UpgradeWindow upgradeWindow;
    public RectTransform canvasRectTransform; // 캔버스의 RectTransform

    public GameObject coreLvUI;
    [HideInInspector]
    public ScienceBtn[] scienceBtns;

    ScienceCoreLvCtrl[] buildContent = new ScienceCoreLvCtrl[5];
    ScienceCoreLvCtrl[] battleContent = new ScienceCoreLvCtrl[5];
    int[] canCoreUpgradeCount = new int[5] {0, 1, 2, 3, 4 };  
    public GameObject scienceTreeUI;

    ScienceInfoData scienceInfoData;
    protected GameManager gameManager;
    protected ScienceBtn focusedSciBtn;

    ScienceDb scienceDb;
    BuildingInven buildingInven;

    float popupWidth;
    Vector3 mousePos;

    PortalSciManager portalSciManager;

    SoundManager soundManager;

    public bool isAnyUpgradeCompleted = false;
    public delegate void OnUpgradeCompleted(int type);
    public OnUpgradeCompleted onUpgradeCompletedCallback;
    
    public delegate void ToggleMapChange();
    public ToggleMapChange onToggleMapChangeCallback;


    public static ScienceManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    void Start()
    {
        gameManager = GameManager.instance;
        scienceDb = ScienceDb.instance;
        buildingInven = gameManager.GetComponent<BuildingInven>();
        sciItemSetWindow = itemInputWindow.GetComponent<SciItemSetWindow>();
        portalSciManager = PortalSciManager.instance;
        soundManager = SoundManager.instance;
        onToggleMapChangeCallback += OnExit;
        UISetting();

        contents[0].SetActive(true);
        contents[1].SetActive(false);
        popupWidth = infoWindow[0].transform.Find("Menu").gameObject.GetComponent<RectTransform>().rect.width;

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
            btn.btnIndex = i;
            AddEvent(btn, EventTriggerType.PointerEnter, delegate { OnEnter(btn); });
            AddEvent(btn, EventTriggerType.PointerExit, delegate { OnExit(); });
            btn.UiSetting();
        }

        scienceDb.ScienceBtnArrGet(scienceBtns);
    }

    void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        if (infoWindow[0].activeSelf)
        {
            mousePos = Input.mousePosition;
            Vector2 anchoredPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, mousePos, null, out anchoredPos);

            float popupWidth = infoWindowObj[0].GetComponent<RectTransform>().rect.width;
            float popupHeight = infoWindowObj[0].GetComponent<RectTransform>().rect.height;

            float clampedX = Mathf.Clamp(anchoredPos.x, -canvasRectTransform.rect.width / 2 + popupWidth / 2, canvasRectTransform.rect.width / 2 - popupWidth);
            float clampedY = Mathf.Clamp(anchoredPos.y, -canvasRectTransform.rect.height / 2 + popupHeight / 2, canvasRectTransform.rect.height / 2 - popupHeight);

            // 위치 설정
            infoWindow[0].transform.localPosition = new Vector2(clampedX, clampedY);
        }
        else if (infoWindow[1].activeSelf)
        {
            mousePos = Input.mousePosition;
            Vector2 anchoredPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, mousePos, null, out anchoredPos);

            float popupWidth = infoWindowObj[1].GetComponent<RectTransform>().rect.width;
            float popupHeight = infoWindowObj[1].GetComponent<RectTransform>().rect.height;

            float clampedX = Mathf.Clamp(anchoredPos.x, -canvasRectTransform.rect.width / 2 + popupWidth / 2, canvasRectTransform.rect.width / 2 - popupWidth);
            float clampedY = Mathf.Clamp(anchoredPos.y, -canvasRectTransform.rect.height / 2 + popupHeight / 2, canvasRectTransform.rect.height / 2 - popupHeight);

            // 위치 설정
            infoWindow[1].transform.localPosition = new Vector2(clampedX, clampedY);
            //mousePos = Input.mousePosition;
            //infoWindow[1].transform.position = mousePos;
        }
        else if (infoWindow[2].activeSelf)
        {
            mousePos = Input.mousePosition;
            Vector2 anchoredPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, mousePos, null, out anchoredPos);

            float popupWidth = infoWindowObj[2].GetComponent<RectTransform>().rect.width;
            float popupHeight = infoWindowObj[2].GetComponent<RectTransform>().rect.height;

            float clampedX = Mathf.Clamp(anchoredPos.x, -canvasRectTransform.rect.width / 2 + popupWidth / 2, canvasRectTransform.rect.width / 2 - popupWidth);
            float clampedY = Mathf.Clamp(anchoredPos.y, -canvasRectTransform.rect.height / 2 + popupHeight / 2, canvasRectTransform.rect.height / 2 - popupHeight);

            // 위치 설정
            infoWindow[2].transform.localPosition = new Vector2(clampedX, clampedY);
        }
    }

    void UISetting()
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject buildUI = Instantiate(coreLvUI);
            buildUI.transform.SetParent(contents[0].transform, false);
            buildContent[i] = buildUI.GetComponent<ScienceCoreLvCtrl>();
            buildContent[i].UISetting(i, "Build");

            GameObject battleUI = Instantiate(coreLvUI);
            battleUI.transform.SetParent(contents[1].transform, false);
            battleContent[i] = battleUI.GetComponent<ScienceCoreLvCtrl>();
            battleContent[i].UISetting(i, "Battle");
        }

        for (int i = 1; i < 5; i++)
        {
            buildContent[i].scienceBtn.itemAmountList = battleContent[i].scienceBtn.itemAmountList;
            buildContent[i].scienceBtn.isMain = true;
            buildContent[i].scienceBtn.CoreSet(battleContent[i].scienceBtn);
            battleContent[i].scienceBtn.CoreSet(buildContent[i].scienceBtn);
        }
    }

    void SciDbGet(int index)
    {
        ScienceBtn[] btns = contents[index].GetComponentsInChildren<ScienceBtn>();
        for (int i = 0; i < btns.Length; i++)
        {
            ScienceBtn btn = btns[i];

            if (scienceDb.scienceNameDb.TryGetValue(btn.sciName, out Dictionary<int, int> levels))
            {
                bool sciNameFound = levels.ContainsKey(btn.level);

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
        sciItemSetWindow.CloseUI();
        upgradeWindow.CloseUI();
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
            scienceInfoData = focusedSciBtn.scienceInfoData;

            if (focusedSciBtn.isCore)
            {
                if (!focusedSciBtn.upgrade)
                {
                    infoWindow[1].GetComponent<InfoWindow>().SetNeedItem(scienceInfoData, focusedSciBtn.gameName, focusedSciBtn.level, focusedSciBtn.isCore, focusedSciBtn);
                    infoWindow[1].SetActive(true);
                }
            }
            else
            {
                if (focusedSciBtn.upgrade)
                {
                    infoWindow[2].GetComponent<InfoWindow>().SetNeedItem(scienceInfoData, focusedSciBtn.gameName, focusedSciBtn.isCore);
                    infoWindow[2].SetActive(true);
                }
                else
                {
                    infoWindow[0].GetComponent<InfoWindow>().SetNeedItem(scienceInfoData, focusedSciBtn.gameName, focusedSciBtn.level, focusedSciBtn.isCore, focusedSciBtn);
                    infoWindow[0].SetActive(true);
                }
            }
        }
    }

    public void OpenItemSetWindow()
    {
        if (scienceInfoData.coreLv > scienceDb.coreLevel)
            return;

        //mousePos = Input.mousePosition;
        //if (mousePos.x + popupWidth > Screen.width)
        //{
        //    mousePos.x = Screen.width - popupWidth - 10.0f;
        //}
        //else if (mousePos.x < 0)
        //{
        //    mousePos.x = 0;
        //}
        //itemInputWindow.transform.position = mousePos;
        itemInputWindow.SetActive(true);
        sciItemSetWindow.SetUI(focusedSciBtn);
    }

    public void OpenUpgradeWindow()
    {
        //mousePos = Input.mousePosition;
        //upgradeWindow.transform.position = mousePos;
        upgradeWindow.gameObject.SetActive(true);
        if (focusedSciBtn.isCore && !focusedSciBtn.isMain)
        {
            upgradeWindow.SetBtn(focusedSciBtn.othCoreBtn);
        }
        else
            upgradeWindow.SetBtn(focusedSciBtn);
    }

    public void CoreUpgradeWarningWindow(int coreLevel)
    {
        //mousePos = Input.mousePosition;
        //upgradeWindow.transform.position = mousePos;
        upgradeWindow.gameObject.SetActive(true);
        upgradeWindow.CoreWaring(canCoreUpgradeCount[coreLevel - 1]);
    }

    public void UpgradeStart(ScienceBtn btn)
    {
        //btn.ItemSaveEnd();
        scienceDb.SciBtnUpgradeServerRpc(btn.btnIndex);
        if (btn.isCore)
        {
            gameManager.WaveStartSet(btn.coreLevel);
            //웨이브 발생 코드
        }
    }

    public void SciUpgradeEnd(string sciName, int sciLevel, int coreLv, bool isLoad)
    {
        if (sciName == "Core")
        {
            scienceDb.coreLevel = sciLevel;
        }
        if (sciName.Contains("Portal"))
        {
            portalSciManager.PortalSciUpgrade(sciName);
        }

        isAnyUpgradeCompleted = true;
        onUpgradeCompletedCallback?.Invoke(40);
        Debug.Log(sciName + " : " + sciLevel + " : " + coreLv);
        scienceDb.SaveSciDb(sciName, sciLevel, coreLv, isLoad);
        buildingInven.Refresh();
    }

    void OnExit()
    {
        focusedSciBtn = null;
        infoWindow[0].SetActive(false);
        infoWindow[1].SetActive(false);
        infoWindow[2].SetActive(false);
    }

    public void OpenUI()
    {
        scienceTreeUI.SetActive(true);
        gameManager.onUIChangedCallback?.Invoke(scienceTreeUI);
    }

    public void CloseUI()
    {
        scienceTreeUI.SetActive(false);
        sciItemSetWindow.CloseUI();
        upgradeWindow.CloseUI();
        soundManager.PlayUISFX("CloseUI");
        gameManager.onUIChangedCallback?.Invoke(scienceTreeUI);
    }

    public void SyncSciBtnItem(int btnIndex, int index, int amount)
    {
        scienceDb.SyncSciBtnItemServerRpc(btnIndex, index, amount);
    }

    public bool CoreUpgradeCheck(int coreLevel)
    {
        bool canUpgrade = false;

        int upgradeCount = scienceDb.CoreLevelUpgradeCount(coreLevel - 1);
        if (upgradeCount >= canCoreUpgradeCount[coreLevel - 1]) 
        {
            canUpgrade = true;
        }

        return canUpgrade;
    }

    public bool CoreSaveCheck(ScienceBtn btn)
    {
        for (int i = 1; i < 5; i++)
        {
            if (battleContent[i].scienceBtn == btn)
            {
                return true;
            }
        }
        return false;
    }
}
