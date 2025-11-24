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
    //[SerializeField]
    //Button[] tagBtns;
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

    [HideInInspector]
    public ScienceCoreLvCtrl[] coreCtrl = new ScienceCoreLvCtrl[5];
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
    [HideInInspector]
    public bool isOpen;


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
        //contents[1].SetActive(false);
        popupWidth = infoWindow[0].transform.Find("Menu").gameObject.GetComponent<RectTransform>().rect.width;

        //for (int i = 0; i < tagBtns.Length; i++)
        //{
        //    int buttonIndex = i;
        //    tagBtns[i].onClick.AddListener(() => SwContent(buttonIndex));
        //}

        scienceBtns = contents[0].GetComponentsInChildren<ScienceBtn>();
        //ScienceBtn[] additionalBtns = contents[1].GetComponentsInChildren<ScienceBtn>();

        //scienceBtns = scienceBtns.Concat(additionalBtns).ToArray();

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

        for (int i = 0; i < infoWindow.Length; i++)
        {
            if (infoWindow[i].activeSelf)
            {
                mousePos = Input.mousePosition;
                Vector2 anchoredPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, mousePos, null, out anchoredPos);

                float popupWidth = infoWindowObj[i].GetComponent<RectTransform>().rect.width;

                float halfCanvasWidth = canvasRectTransform.rect.width / 2;

                // 마우스 오른쪽 기본 위치
                float targetX = anchoredPos.x;
                float targetY = anchoredPos.y;

                // 오른쪽을 벗어나면 왼쪽으로 배치
                if (targetX + popupWidth > halfCanvasWidth)
                {
                    targetX = anchoredPos.x - popupWidth;
                }

                // 위치 설정
                infoWindow[i].transform.localPosition = new Vector2(targetX, targetY);
                break; // 한 개만 활성화되므로 반복문 종료
            }
        }
    }

    void UISetting()
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject buildUI = Instantiate(coreLvUI);
            buildUI.transform.SetParent(contents[0].transform, false);
            coreCtrl[i] = buildUI.GetComponent<ScienceCoreLvCtrl>();
            coreCtrl[i].UISetting(i, "Build");
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
        itemInputWindow.SetActive(true);
        sciItemSetWindow.SetUI(focusedSciBtn);
        soundManager.PlayUISFX("ButtonClick");
        gameManager.onUIChangedCallback?.Invoke(itemInputWindow);
        gameManager.PopUpUISetting(true);
    }

    public void OpenUpgradeWindow()
    {
        upgradeWindow.gameObject.SetActive(true);
        upgradeWindow.SetBtn(focusedSciBtn);
        soundManager.PlayUISFX("ButtonClick");
        gameManager.onUIChangedCallback?.Invoke(upgradeWindow.gameObject);
        gameManager.PopUpUISetting(true);
    }

    public void CoreUpgradeWarningWindow(int coreLevel)
    {
        upgradeWindow.gameObject.SetActive(true);
        upgradeWindow.CoreWaring(canCoreUpgradeCount[coreLevel - 1]);
        soundManager.PlayUISFX("ButtonClick");
        gameManager.onUIChangedCallback?.Invoke(upgradeWindow.gameObject);
        gameManager.PopUpUISetting(true);
    }

    public void UpgradeStart(ScienceBtn btn)
    {
        scienceDb.SciBtnUpgradeServerRpc(btn.btnIndex);
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
        isOpen = true;
        scienceTreeUI.SetActive(true);
        gameManager.onUIChangedCallback?.Invoke(scienceTreeUI);
    }

    public void CloseUI()
    {
        isOpen = false;
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

    public void UnlockAll()
    {
        foreach (var btn in scienceBtns)
        {
            btn.upgradeStart = true;
            btn.UpgradeFunc(false);
        }
    }
}
