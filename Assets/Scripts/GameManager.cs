using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;
using UnityEngine.SceneManagement;

// UTF-8 설정
public class GameManager : NetworkBehaviour
{
    public GameObject inventoryUiCanvas;
    public bool isMultiPlay;
    public Inventory inventory;
    public Inventory hostMapInven;
    public Inventory clientMapInven;

    [SerializeField] MapGenerator mapGenerator;
    [HideInInspector] public Map hostMap;
    [HideInInspector] public Map clientMap;
    [HideInInspector] public Map map;
    public List<Vector3> destroyedMapObjectsPos = new List<Vector3>();

    public bool isPlayerInHostMap;
    public bool isPlayerInMarket;
    public bool isGameOver;
    public bool isWaitingForRespawn;

    public GameObject player;
    public PlayerStatus playerStatus;
    public float playerMaxHp = 100.0f;
    public float playerDataHp = -1;
    public Vector3 playerDataPos = Vector3.zero;
    bool loadTankOn;
    public (float, float) loadTankData;
    public PlayerController playerController;
    public Transform hostPlayerTransform;
    public Transform clientPlayerTransform;
    public Transform marketPortalTransform;

    CameraController mainCam;
    public MapCameraController mapCameraController;
    PreBuilding preBuilding;
    //public GameObject preBuildingObj;
    public Finance finance;
    public Scrap scrap;
    public Scrap shopScrap;
    public int questData;

    [SerializeField]
    public PlayerInvenManager pInvenManager;
    [SerializeField]
    BuildingInvenManager bManager;
    [SerializeField]
    RecipeManager rManager;
    [SerializeField]
    SplitterFilterManager sManager;
    [SerializeField]
    ItemSpManager iSpManager;
    [SerializeField]
    ScienceManager sTreeManager;
    public bool debug;
    public bool isHost;
    public bool isShopOpened;
    public Inventory hostDragInven;
    public Inventory clientDragInven;

    public List<GameObject> openedUI;
    StructureClickEvent clickEvent;
    StructureClickEvent newClickEvent;
    LogisticsClickEvent logisticsClickEvent;
    LogisticsClickEvent newLogisticsClickEvent;

    [SerializeField]
    GameObject selectPointPrefab;
    GameObject selectPoint;

    InputManager inputManager;

    Structure newStructure;
    [HideInInspector]
    public Structure focusedStructure;
    public Portal[] portal;

    [HideInInspector]
    public Vector3 hostPlayerSpawnPos;
    [HideInInspector]
    public Vector3 clientPlayerSpawnPos;

    [SerializeField]
    GameObject consoleUI;
    bool isConsoleOpened;
    InfoInteract info;

    public bool isBasicUIClose;
    [SerializeField] GameObject basicUI;
    [SerializeField] GameObject chat;

    public delegate void OnUIChanged(GameObject ui);
    public OnUIChanged onUIChangedCallback;

    OptionCanvas optionCanvas;

    ScienceBuildingInfo scienceBuildingInfo;
    ScienceManager scienceManager;

    public int day;                     // 일 수
    public bool isDay;                  // 밤 낮
    [SerializeField] float dayTime;     // 인게임 4시간을 현실 시간으로
    public float dayTimer;              // 게임 내 시간(타이머)
    public int dayIndex = 0;            // 24시간을 6등분해서 인덱스로 사용
    // 0 : 08:00 ~ 12:00
    // 1 : 12:00 ~ 16:00
    // 2 : 16:00 ~ 20:00
    // 3 : 20:00 ~ 24:00
    // 4 : 24:00 ~ 04:00
    // 5 : 04:00 ~ 08:00

    [SerializeField]
    int safeDay;                        // 게임 초기 안전한 날
    int[] randomStackValue = new int[2] { 20, 80 }; // 스택 랜덤 범위
    [SerializeField]
    float violentValue;                 // 광폭화의날 스택
    float violentMaxValue = 100;        // 광폭화의날 최대 값
    
    [SerializeField]
    bool violentDayCheck;                    // true면 광폭화의 날
    public bool violentDay;
    [SerializeField]
    int violentCycle;
    [SerializeField]
    int clientMapDateDifference;

    /// <summary>
    /// 광폭화 시스템
    /// 인게임 하루 = 현실 10분
    /// 안전한날 20일 = 현실 3시간 20분
    /// 20분에 1번 발생 한다면
    /// 인게임 2일 = 현실 20분
    /// 2일간 100 스택이 쌓이면 광폭화 발생이라 하면 평균 스택 50으로 잡고 상한선 80 하한선 20 으로 잡으면
    /// 최소 2일 ~ 최대 5일
    /// </summary>

    [SerializeField]
    Sprite[] timeImgSet;
    [SerializeField]
    Image timeImg;
    [SerializeField]
    Text timeText;
    [SerializeField]
    Text dayText;
    [SerializeField]
    SpriteRenderer brightness;

    float hours;        // 시간 계산
    int displayHour;    // 시 부분
    int displayMinute;  // 분 부분
    string timeDisplay; // 시간표시용 

    public float autoSaveTimer;
    public float autoSaveinterval;

    Dictionary<int, int[]> waveLevelsByMapSize = new Dictionary<int, int[]>
    {
        { 0, new int[] { 1, 1, 2, 2 } }, // mapSize 0에 따른 coreLevel별 waveLevel
        { 1, new int[] { 1, 1, 2, 3 } }, // mapSize 1에 따른 coreLevel별 waveLevel
        { 2, new int[] { 1, 2, 3, 4 } }  // mapSize 2에 따른 coreLevel별 waveLevel
    };

    public static event Action GenerationComplete;

    bool gameStop;

    #region Singleton
    public static GameManager instance;

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

    void Start()
    {
        Debug.Log("Gamestart");
        debug = false;
        isHost = false;
        isConsoleOpened = false;
        openedUI = new List<GameObject>();
        onUIChangedCallback += UIChanged;
        optionCanvas = OptionCanvas.instance;

        hostMap = mapGenerator.hostMap;
        clientMap = mapGenerator.clientMap;
        map = hostMap;
        isPlayerInHostMap = true;
        isPlayerInMarket = false;
        isGameOver = false;
        isWaitingForRespawn = false;
        isShopOpened = false;

        Vector3 playerSpawnPos = new Vector3(map.width/2, map.height/2, 0);
        mapCameraController.SetCamRange(map);
        preBuilding = PreBuilding.instance;
        scienceBuildingInfo = inventoryUiCanvas.GetComponent<ScienceBuildingInfo>();
        scienceManager = ScienceManager.instance;

        day = 0;
        isDay = true;
        dayTimer = 0;

        OtherPortalSet(); 
        SoundManager.instance.GameSceneLoad();
        autoSaveinterval = SettingsMenu.instance.autoSaveInterval;
        //GameStartSet();
    }

    void OnEnable()
    {
        inputManager = InputManager.instance;
        inputManager.controls.Structure.StrClick.performed += StrClick;
        inputManager.controls.HotKey.Debug.performed += DebugMode;
        inputManager.controls.HotKey.Supply.performed += Supply;
        inputManager.controls.HotKey.Escape.performed += Escape;
        inputManager.controls.Inventory.PlayerInven.performed += Inven;
        inputManager.controls.HotKey.ScienceTree.performed += ScienceTree;
        inputManager.controls.HotKey.EnergyCheck.performed += EnergyCheck;
        inputManager.controls.HotKey.GameStop.performed += GameStopSet;
        inputManager.controls.HotKey.UIClose.performed += BasicUIsClose;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    void OnDisable()
    {
        inputManager.controls.Structure.StrClick.performed -= StrClick;
        inputManager.controls.HotKey.Debug.performed -= DebugMode;
        inputManager.controls.HotKey.Supply.performed -= Supply;
        inputManager.controls.HotKey.Escape.performed -= Escape;
        inputManager.controls.Inventory.PlayerInven.performed -= Inven;
        inputManager.controls.HotKey.ScienceTree.performed -= ScienceTree;
        inputManager.controls.HotKey.EnergyCheck.performed -= EnergyCheck;
        inputManager.controls.HotKey.GameStop.performed -= GameStopSet;
        inputManager.controls.HotKey.UIClose.performed -= BasicUIsClose;
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log("Client connected with ID: " + clientId);
    }

    void GameStopSet(InputAction.CallbackContext ctx)
    {
        GameStopSetServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void GameStopSetServerRpc()
    {
        GameStopSetClientRpc();
    }

    [ClientRpc]
    void GameStopSetClientRpc()
    {
        if (gameStop)
        {
            Time.timeScale = 1;
            gameStop = !gameStop;
        }
        else
        {
            Time.timeScale = 0;
            gameStop = !gameStop;
        }
    }

    void GameStop(bool stop)
    {
        if (stop)
        {
            Time.timeScale = 0;
            gameStop = stop;
        }
        else
        {
            Time.timeScale = 1;
            gameStop = stop;
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        dayTimer += Time.deltaTime;
        hours = (dayTimer / 25f) + (dayIndex * 4) + 8;
        hours = Mathf.Repeat(hours, 24f);

        displayHour = Mathf.FloorToInt(hours); // 시 부분
        displayMinute = Mathf.FloorToInt((hours - displayHour) * 60); // 분 부분

        timeDisplay = string.Format("{0:D2} : {1:D2}", displayHour, displayMinute);

        timeText.text = timeDisplay;

        if (dayTimer > dayTime)
        {
            dayTimer = 0;

            dayIndex++;
            if (dayIndex > 5)
            {
                dayIndex = 0;
            }

            timeImg.sprite = timeImgSet[dayIndex];
            SetBrightness(dayIndex);

            if (dayIndex == 0)
            {
                isDay = true;
                SoundManager.instance.PlayBgmMapCheck();
                violentDay = false;
                violentDayCheck = false;
                timeImg.color = new Color32(255, 255, 255, 255);
            }
            else if (dayIndex == 3)
            {
                isDay = false;
                SoundManager.instance.PlayBgmMapCheck();
                if (violentDayCheck)
                {
                    violentDay = true;
                    timeImg.color = new Color32(255, 50, 50, 255);
                }
            }
            else if (dayIndex == 4)
            {
                day++;
                dayText.text = "Day : " + day;

                if (IsServer)
                {
                    SyncTimeServerRpc();
                    ViolentDayOnServerRpc();
                }
            }
        }

        if (!isConsoleOpened)
        {
            if (consoleUI.activeSelf)
            {
                inputManager.CommonDisableControls();
                isConsoleOpened = true;
            }
        }
        else
        {
            if (!consoleUI.activeSelf)
            {
                inputManager.CommonEnableControls();
                isConsoleOpened = false;
            }
        }

        if (IsServer)
        {
            autoSaveTimer += Time.deltaTime;
            if (autoSaveTimer > autoSaveinterval)
            {
                autoSaveTimer = 0;
                DataManager.instance.Save(0);
            }
        }
    }

    [ServerRpc]
    void ViolentDayOnServerRpc()
    {
        bool violentDaySync = false;

        if (violentDayCheck)
        {
            MonsterSpawnerManager.instance.ViolentDayStart();
            violentDaySync = false;
        }
        else
        {
            if (day >= safeDay)
            {
                if (day % violentCycle == 0)    // 호스트맵
                {
                    violentDaySync = true;
                    MonsterSpawnerManager.instance.ViolentDayOn(true);
                }
                else if ((day - clientMapDateDifference) % violentCycle == 0)    // 클라이언트 맵
                {
                    violentDaySync = true;
                    MonsterSpawnerManager.instance.ViolentDayOn(false);
                }
            }
        }

        ViolentDayOnClientRpc(violentDaySync);
    }

    [ClientRpc]
    void ViolentDayOnClientRpc(bool violentDaySync)
    {
        violentDayCheck = violentDaySync;
    }

    void SetBrightness(int level)
    {
        switch (level)
        {
            case 0:
                brightness.color = new Color32(0, 0, 0, 0);
                break;
            case 1:
                brightness.color = new Color32(0, 0, 0, 0);
                break;
            case 2:
                brightness.color = new Color32(0, 0, 0, 10);
                break;
            case 3:
                brightness.color = new Color32(0, 0, 0, 20);
                break;
            case 4:
                brightness.color = new Color32(0, 0, 0, 30);
                break;
            case 5:
                brightness.color = new Color32(0, 0, 0, 10);
                break;
            default:
                brightness.color = new Color32(0, 0, 0, 0);
                break;
        }
    }

    public void DestroyAllDontDestroyOnLoadObjects()
    {
        var marker = new GameObject("Marker");
        DontDestroyOnLoad(marker);

        foreach (var root in marker.scene.GetRootGameObjects())
            Destroy(root);
    }

    public void SetMapInven(bool isHostMap)
    {
        if (isHostMap)
        {
            inventory = hostMapInven;
            pInvenManager.SetInven(hostMapInven);
        }
        else
        {
            inventory = clientMapInven;
            pInvenManager.SetInven(clientMapInven);
        }
    }

    public Vector3 Teleport()
    {
        CloseAllOpenedUI();

        if (isPlayerInHostMap)
        {
            map = clientMap;
            isPlayerInHostMap = false;
            SetPlayerLocationServerRpc(false, false, IsServer);
            SetMapInven(false);
            mapCameraController.SetCamRange(map);
            return clientPlayerSpawnPos;
        }
        else
        {
            map = hostMap;
            isPlayerInHostMap = true;
            SetPlayerLocationServerRpc(true, false, IsServer);
            SetMapInven(true);
            mapCameraController.SetCamRange(map);
            return hostPlayerSpawnPos;
        }
    }

    public Vector3 TeleportMarket()
    {
        CloseAllOpenedUI();

        if (!isPlayerInMarket)
        {
            isPlayerInMarket = true;
            inputManager.InMarket();
            SetPlayerLocationServerRpc(isPlayerInHostMap, true, IsServer);
            SoundManager.instance.PlayerMarketBgm();
            return marketPortalTransform.position;
        }
        else
        {
            isPlayerInMarket = false;
            inputManager.OutMarket();
            SetPlayerLocationServerRpc(isPlayerInHostMap, false, IsServer);
            SoundManager.instance.PlayBgmMapCheck();
            if (isPlayerInHostMap)
            {
                return hostPlayerSpawnPos;
            }
            else
            {
                return clientPlayerSpawnPos;
            }
        }
    }

    void StrClick(InputAction.CallbackContext ctx)
    {
        if (RaycastUtility.IsPointerOverUI(Input.mousePosition))
            return;
        if (rManager.isOpened)
            return;
        if (preBuilding.isBuildingOn)
            return;

        //건물 위 오브젝트가 있을때 클릭이 안되서 Raycast > RaycastAll로 변경
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.RaycastAll(pos, Vector2.zero);

        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        if (debug && inputManager.ctrl && map.IsOnMap(x, y))
        {
            string buildable = "";
            Cell cell = map.GetCellDataFromPos(x, y);
            foreach (string str in cell.buildable)
            {
                buildable = buildable + " " + str;
            }

            if (cell.obj == null)
            {
                Debug.Log("x : " + x + ",   y : " + y +
                ",   biome : " + cell.biome +
                ",   resource : " + cell.resource +
                ",   buildable : " + buildable +
                ",   structure : " + cell.structure +
                ",   spawn area : " + cell.spawnArea
                );
            }
            else
            {
                Debug.Log("x : " + x + ",   y : " + y +
                ",   biome : " + cell.biome +
                ",   resource : " + cell.resource +
                ",   obj : " + cell.obj.name +
                ",   buildable : " + buildable +
                ",   structure : " + cell.structure +
                ",   spawn area : " + cell.spawnArea
                );
            }
        }
        else if (debug && inputManager.alt && map.IsOnMap(x, y))
        {
            Vector3 vec = new Vector3(x + 0.5f, y + 0.5f, 0);
            Bounds bounds = new Bounds(vec, new Vector3(0.4f, 0.4f, 0));

            var guo = new GraphUpdateObject(bounds);

            // Set some settings

            guo.updatePhysics = true;

            AstarPath.active.UpdateGraphs(guo);
        }

        if (hits.Length > 0)
        {
            List<InfoInteract> infoList = new List<InfoInteract>();
            bool isSamePosClicked = false;
            int focusedBefore = -1;

            //foreach (RaycastHit2D hit in hits)
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.TryGetComponent(out InfoInteract _info))
                {
                    infoList.Add(_info);
                    GameObject parent = _info.transform.parent.gameObject;

                    if (parent.TryGetComponent(out Structure str))
                        newStructure = str;
                    newClickEvent = parent.GetComponent<StructureClickEvent>();
                    newLogisticsClickEvent = parent.GetComponent<LogisticsClickEvent>();

                    if (player.TryGetComponent(out PlayerController playerController) && playerController.onTankData)
                    {
                        if (playerController.onTankData.tankUIOpen)
                        {
                            playerController.onTankData.CloseUI();
                        }
                    }
                    
                    if (newClickEvent != null && !newClickEvent.GetComponentInParent<Structure>().isPreBuilding)
                    {
                        if (clickEvent != null && clickEvent.openUI)
                        {
                            clickEvent.CloseUI();
                        }
                        if (logisticsClickEvent != null && logisticsClickEvent.openUI)
                        {
                            logisticsClickEvent.CloseUI();
                        }

                        clickEvent = newClickEvent;
                        clickEvent.StructureClick();
                        clickEvent.OpenUI();
                    }
                    else if (newLogisticsClickEvent != null && !newLogisticsClickEvent.GetComponentInParent<Structure>().isPreBuilding)
                    {
                        if (logisticsClickEvent != null && logisticsClickEvent.openUI)
                        {
                            logisticsClickEvent.CloseUI();
                        }
                        if (clickEvent != null && clickEvent.openUI)
                        {
                            clickEvent.CloseUI();
                        }

                        logisticsClickEvent = newLogisticsClickEvent;

                        if (logisticsClickEvent.LogisticsCheck())
                        {
                            logisticsClickEvent.OpenUI();
                        }
                        else
                        {
                            logisticsClickEvent = null;
                        }
                    }
                }
            }

            if (infoList.Count > 0)
            {
                for (int i = 0; i < infoList.Count; i++)
                {
                    if (info == infoList[i])
                    {
                        isSamePosClicked = true;
                        focusedBefore = i;
                    }
                }

                if (isSamePosClicked)
                {
                    int infoIndex = focusedBefore + 1;
                    if (infoIndex >= infoList.Count)
                        infoIndex = 0;

                    info = infoList[infoIndex];
                    info.Clicked();
                }
                else
                {
                    info = infoList[0];
                    info.Clicked();
                }
            }
            else
            {
                info = null;
                InfoUI.instance.SetDefault();
            }
        }
        else
        {
            info = null;
            InfoUI.instance.SetDefault();
        }

        if (focusedStructure == null)
        {
            if (newStructure != null && !newStructure.isPreBuilding)
            {
                focusedStructure = newStructure;
                focusedStructure.Focused();
            }
        }
        else if (!focusedStructure.isUIOpened)
        {
            if (newStructure == null)
            {
                focusedStructure.DisableFocused();
                focusedStructure = null;
            }
            else
            {
                if (newStructure != focusedStructure && !newStructure.isPreBuilding)
                {
                    focusedStructure.DisableFocused();
                    focusedStructure = newStructure;
                    focusedStructure.Focused();
                }
            }
        }
        newStructure = null;
    }

    public void TankUIOpen()
    {
        if (clickEvent && clickEvent.openUI)
        {
            clickEvent.CloseUI();
        }
        else if (logisticsClickEvent && logisticsClickEvent.openUI)
        {
            logisticsClickEvent.CloseUI();
        }
    }

    public void CheckAndCancelFocus(Structure str)
    {
        if (focusedStructure == str)
        {
            focusedStructure.DisableFocused();
            focusedStructure = null;
        }
    }

    public void DebugMode()
    {
        Debug.Log(LobbySaver.instance.currentLobby?.Id);
        Debug.Log(EventSystem.current.currentSelectedGameObject);
        debug = !debug;
        Debug.Log("debug : " + debug);

        //QuestManager.instance.SetQuest(questData);
    }

    public void DebugMode(InputAction.CallbackContext ctx)
    {
        DebugMode();
    }

    void Supply(InputAction.CallbackContext ctx)
    {
        if (debug)
        {
            foreach (Item item in ItemList.instance.itemList)
            {
                if (item.tier >= 0)
                    inventory.Add(item, 99);
            }
            AddFinanceServerRpc(100000);
            BuildAndSciUiReset();
        }
    }

    void Escape(InputAction.CallbackContext ctx)
    {
        if (openedUI.Count > 0)
        {
            CloseOpenedUI(openedUI.Count - 1);
        }
        else
        {
            optionCanvas.MainPanelSet(!optionCanvas.mainPanel.activeSelf);
        }
    }

    void CloseOpenedUI(int order)
    {
        switch (openedUI[order].gameObject.name)
        {
            case "Inventory":
                pInvenManager.CloseUI();
                break;
            case "StructureInfo":
                if(clickEvent)
                {
                    clickEvent.CloseUI();
                }
                else
                {
                    if (player.TryGetComponent(out PlayerController playerController) && playerController.onTankData)
                    {
                        if (playerController.onTankData.tankUIOpen)
                        {
                            playerController.onTankData.CloseUI();
                        }
                    }
                }
                break;
            case "RecipeMenu":
                rManager.CloseUI();
                break;
            case "BuildingInven":
                bManager.CloseUI();
                break;
            case "SplitterFillterMenu":
                logisticsClickEvent.CloseUI();
                break;
            case "LogisticsMenu":
                logisticsClickEvent.CloseUI();
                break;
            case "ItemSpwanerFilter":
                logisticsClickEvent.CloseUI();
                break;
            case "ScienceTree":
                sTreeManager.CloseUI();
                break;
            case "Camera":
                mapCameraController.ToggleMap();
                break;
            case "OverallDisplay":
                OverallDisplay.instance.CloseUI();
                break;
            case "SettingsPanel":
                SettingsMenu.instance.MenuClose();
                break;
            case "SaveLoadPanel":
                SaveLoadMenu.instance.MenuClose();
                break;
            case "ConfirmPanel":
                ConfirmPanel.instance.CanelBtnFunc();
                break;
            case "ScienceBuildingInfo":
                scienceBuildingInfo.CloseUI();
                break;
            default:
                if (openedUI[order].gameObject.TryGetComponent<Shop>(out Shop shop))
                    shop.CloseUI();
                else if (openedUI[order].gameObject.TryGetComponent<Bounty>(out Bounty bounty))
                    bounty.CloseUI();
                else if (openedUI[order].gameObject.TryGetComponent<PopUpCtrl>(out PopUpCtrl popup))
                    popup.CloseUI();
                break;
        }
    }

    public void CloseAllOpenedUI()
    {
        for (int i = openedUI.Count - 1; i >= 0; i--)
        {
            CloseOpenedUI(i);
            if (openedUI.Count < i)
            {
                CloseAllOpenedUI();
                break;
            }
        }
    }

    void BasicUIsClose(InputAction.CallbackContext ctx)
    {
        if (isBasicUIClose)
        {
            OpenBasicUIs();
        }
        else
        {
            CloseBasicUIs();
            inventoryUiCanvas.TryGetComponent(out ItemInfoWindow window);
            window.CloseWindow();
        }
        isBasicUIClose = !isBasicUIClose;
    }

    public void OpenBasicUIs()
    {
        basicUI.SetActive(true);
        chat.SetActive(true);
    }

    public void CloseBasicUIs()
    {
        basicUI.SetActive(false);
        chat.SetActive(false);
    }

    void Inven(InputAction.CallbackContext ctx)
    {
        Inven();
    }

    public void Inven()
    {
        if (!pInvenManager.inventoryUI.activeSelf)
        {
            pInvenManager.OpenUI();
        }
        else
        {
            pInvenManager.CloseUI();
        }
    }

    void ScienceTree(InputAction.CallbackContext ctx)
    {
        ScienceTree();
    }

    public void ScienceTree()
    {
        if (!scienceManager.isOpen)
        {
            scienceManager.OpenUI();
        }
        else
        {
            scienceManager.CloseUI();
        }
    }

    void UIChanged(GameObject ui)
    {
        SetOpenedUIList(ui);
        DragUIActive();
    }

    void SetOpenedUIList(GameObject ui)
    {
        if (ui.activeSelf)
        {
            if (!openedUI.Contains(ui))
                openedUI.Add(ui);
        }
        else
        {
            if (openedUI.Contains(ui))
                openedUI.Remove(ui);
        }
    }

    void DragUIActive()
    {
        bool isOpened = false;
        foreach (GameObject ui in openedUI)
        {
            if (ui.name == "Inventory" || ui.name == "StructureInfo")
                isOpened = true;
        }

        if (isOpened)
        {
            ItemDragManager.instance.slotObj.SetActive(true);
        }
        else
        {
            ItemDragManager.instance.slotObj.SetActive(false);
        }
    }

    public void BuildAndSciUiReset()
    {
        if (BuildingInfo.instance != null && BuildingInfo.instance.gameObject.activeSelf)
        {
            BuildingInfo.instance.SetItemSlot();

            if (PreBuilding.instance.isBuildingOn)
                PreBuilding.instance.isEnough = BuildingInfo.instance.AmountsEnoughCheck();

            //if(PreBuilding.instance != null)
            //    PreBuilding.instance.isEnough = BuildingInfo.instance.AmountsEnoughCheck();
        }
        if (InfoWindow.instance != null && InfoWindow.instance.gameObject.activeSelf)
        {
            InfoWindow.instance.SetNeedItem();
        }
    }

    public void HostConnected()
    {
        isHost = true;
        Time.timeScale = 0;
        ItemDragManager.instance.SetInven(hostDragInven);
        GeminiNetworkManager.instance.HostSpawnServerRPC();
        Debug.Log("HostConnected??");
    }

    public void ClientConnected()
    {
        ItemDragManager.instance.SetInven(clientDragInven);
        Time.timeScale = 0;
        Debug.Log("Time.timeScale = 0;");
        StartCoroutine(DataSync());
        //StartCoroutine(WaitForNetworkConnection());
    }

    IEnumerator DataSync()
    {
        float time = 15f;

        while (!SteamManager.instance.getData)
        {
            if (!SteamManager.instance.clientConnTry)
            {
                bool packetAvailable = SteamManager.instance.ReceiveP2PPacket();
                Debug.Log(packetAvailable + " : DataSync packetAvailable Check");
                if (!packetAvailable)
                    time = 3;
                else
                    time = 15f;
            }
            
            yield return new WaitForSecondsRealtime(time);
        }

        Debug.Log("ClientDataGet And StartClient");

        NetworkManager.Singleton.StartClient();
        StartCoroutine(WaitForNetworkConnection());
    }

    private IEnumerator WaitForNetworkConnection()
    {
        Debug.Log("Wait for Network connection");

        while (!NetworkManager.Singleton.IsConnectedClient)
        {
            //yield return null;
            yield return new WaitForEndOfFrame();
        }

        TestServerRpc();
        GeminiNetworkManager.instance.ClientSpawnServerRPC();
        Debug.Log("Connected to Network");
    }

    [ServerRpc(RequireOwnership = false)]
    void TestServerRpc()
    {
        Debug.Log("TestServerRpc");
    }

    public void LoadingEnd()
    {
        GenerationComplete?.Invoke();
        TimeScaleServerRpc();

        if (!isHost)
        {
            SyncTimeServerRpc();
        }
    }

    public void SetPlayer(GameObject playerObj)
    {
        player = playerObj;
        playerStatus = playerObj.GetComponent<PlayerStatus>();
        playerController = player.GetComponent<PlayerController>();
        if (playerDataHp == -1)
        {
            if (isHost)
            {
                player.transform.position = hostPlayerSpawnPos;
                SetMapInven(true);
                map = hostMap;
                isPlayerInHostMap = true;
            }
            else
            {
                player.transform.position = clientPlayerSpawnPos;
                SetMapInven(false);
                map = clientMap;
                isPlayerInHostMap = false;
            }
        }
        else
        {
            player.transform.position = playerDataPos;
            if (isPlayerInHostMap)
            {
                SetMapInven(true);
                map = hostMap;
            }
            else
            {
                SetMapInven(false);
                map = clientMap;
            }

            //if (loadTankOn && isHost)
            //{
            //    player.GetComponent<PlayerController>().LoadDataSetTankServerRpc(loadTankData.Item1, loadTankData.Item2);
            //}
        }

        SetPlayerLocationServerRpc(isPlayerInHostMap, isPlayerInMarket, IsServer);
        mainCam = Camera.main.gameObject.GetComponent<CameraController>();
        mainCam.target = player.transform;
        mapCameraController.target = player.transform;
        mapCameraController.SetCamRange(map);
        GameObject fogOfWar = ResourcesManager.instance.fogOfWar;
        FollowTransform followTransform = fogOfWar.GetComponent<FollowTransform>();
        followTransform.SetTargetTransform(mainCam.transform);
        WavePoint.instance.PlayerSet(player);
    }

    public void SetPlayerPos(float x, float y, bool isHostPos)
    {
        Vector3 spawnPos = new Vector3(x, y, 0);
        if (isHostPos)
            hostPlayerSpawnPos = spawnPos;
        else
            clientPlayerSpawnPos = spawnPos;

        //WavePoint.instance.SpawnPos(isHostPos, spawnPos);
        //player.transform.position = playerSpawnPos;
    }

    public Vector3 GetPlayerPos(bool isHostPlayer)
    {
        if (isHostPlayer)
            return hostPlayerTransform.position;
        else
            return clientPlayerTransform.position;
    }

    public GameObject SelectPointSpawn(GameObject build)
    {
        selectPoint = Instantiate(selectPointPrefab);
        Vector3 spawnPos = new Vector3(build.transform.position.x, build.transform.position.y + 0.7f, 0);
        selectPoint.transform.parent = build.transform;
        selectPoint.transform.position = spawnPos;

        selectPoint.GetComponent<SelectPointMovement>().initialPosition = spawnPos;

        return selectPoint;
    }

    public void SelectPointRemove()
    {
        if (selectPoint != null)
            Destroy(selectPoint);
    }

    void EnergyCheck(InputAction.CallbackContext ctx)
    {
        if (debug)
            EnergyGroupManager.instance.CheckGroups();
    }
    
    void OtherPortalSet()
    {
        portal[0].OtherPortalSet(portal[1]);
        portal[1].OtherPortalSet(portal[0]);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddFinanceServerRpc(int money)
    {
        AddFinanceClientRpc(money);
    }

    [ClientRpc]
    public void AddFinanceClientRpc(int money)
    {
        finance.AddFinance(money);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SubFinanceServerRpc(int money)
    {
        Debug.Log("SubFinanceServerRpc");
        SubFinanceClientRpc(money);
    }

    [ClientRpc]
    public void SubFinanceClientRpc(int money)
    {
        Debug.Log("SubFinanceClientRpc");
        finance.SubFinance(money);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddScrapServerRpc(int _scrap)
    {
        AddScrapClientRpc(_scrap);
    }

    [ClientRpc]
    public void AddScrapClientRpc(int _scrap)
    {
        scrap.AddScrap(_scrap);
        shopScrap.AddScrap(_scrap);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SubScrapServerRpc(int _scrap)
    {
        SubScrapClientRpc(_scrap);
    }

    [ClientRpc]
    public void SubScrapClientRpc(int _scrap)
    {
        scrap.SubScrap(_scrap);
        shopScrap.SubScrap(_scrap);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveMapObjServerRpc(Vector3 vector3, bool isHostMapRequest)
    {
        destroyedMapObjectsPos.Add(vector3);
        RemoveMapObjClientRpc(vector3, isHostMapRequest);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SellScrapServerRpc()
    {
        int scrapAmount = scrap.GetScrap();
        if (scrapAmount > 0)
        {
            SubScrapServerRpc(scrapAmount);
            AddFinanceServerRpc(scrapAmount);
        }
    }

    [ClientRpc]
    public void RemoveMapObjClientRpc(Vector3 vector3, bool isHostMapRequest)
    {
        RemoveMapObj(vector3, isHostMapRequest);
    }

    public void RemoveMapObj(Vector3 vector3, bool isHostMapRequest)
    {
        Cell cell;

        if (isHostMapRequest)
        {
            cell = hostMap.GetCellDataFromPos((int)vector3.x, (int)vector3.y);
        }
        else
        {
            cell = clientMap.GetCellDataFromPos((int)vector3.x, (int)vector3.y);
        }

        if (cell.obj != null)
        {
            if (cell.obj.TryGetComponent<MapObject>(out MapObject mapObj))
            {
                mapObj.RemoveMapObj();
            }
        }
    }
    
    public void GameStartSet()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            optionCanvas.SaveBtnOnOff(true);
            SteamManager.instance.HostLobby();
            HostConnected();
            MapGenerator.instance.SpawnerAreaMapSet();
            if (MainGameSetting.instance.isNewGame)
            {
                SetStartingItem();
            }
            else
            {
                DataManager.instance.Load(MainGameSetting.instance.loadDataIndex);
            }
        }
        else
        {
            SteamManager.instance.ClientConnectSend();
            //NetworkManager.Singleton.StartClient();
            //Debug.Log("Client");
            optionCanvas.SaveBtnOnOff(false);
            ClientConnected();
        }

        StartCoroutine(SetQuest());
        //GenerationComplete?.Invoke();
    }

    void SetStartingItem()
    {
        Item item = ItemList.instance.itemDic["Fuel"];
        inventory.Add(item, 5);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TimeScaleServerRpc()
    {
        TimeScaleClientRpc();
    }

    [ClientRpc]
    void TimeScaleClientRpc()
    {
        Time.timeScale = 1;
    }

    public InGameData SaveData()
    {
        InGameData inGameData = new InGameData();

        // 저장 시간
        DateTime currentDateTime = DateTime.Now;
        string formattedDateTime = currentDateTime.ToString("yyyy-MM-dd HH:mm:ss");
        inGameData.saveDate = formattedDateTime;
        // 파일 이름
        inGameData.mapSizeIndex = MainGameSetting.instance.mapSizeIndex;
        inGameData.seed = MainGameSetting.instance.randomSeed;
        inGameData.day = day;
        inGameData.isDay = isDay;
        inGameData.dayTimer = dayTimer;
        inGameData.dayIndex = dayIndex;

        inGameData.violentValue = violentValue;
        inGameData.violentDayCheck = violentDayCheck;

        inGameData.finance = finance.GetFinance();
        inGameData.scrap = scrap.GetScrap();
        inGameData.questIndex = QuestManager.instance.currentQuest;

        return inGameData;
    }

    public void LoadData(InGameData data)
    {
        day = data.day;
        isDay = data.isDay;
        dayTimer = data.dayTimer;
        dayIndex = data.dayIndex;
        SoundManager.instance.GameSceneLoad();
        violentValue = data.violentValue;
        violentDayCheck = data.violentDayCheck;
        violentDay = data.violentDay;
        timeImg.sprite = timeImgSet[dayIndex];
        SetBrightness(dayIndex);
        dayText.text = "Day : " + day;

        finance.SetFinance(data.finance);
        scrap.SetScrap(data.scrap);
        shopScrap.SetScrap(data.scrap);
        questData = data.questIndex;

        portal[0].portalName = data.hostPortalName;
        portal[1].portalName = data.clientPortalName;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncTimeServerRpc()
    {
        SyncTimeClientRpc(day, isDay, dayTimer, dayIndex, violentValue, violentDayCheck, violentDay);
    }

    [ClientRpc]
    public void SyncTimeClientRpc(int serverDay, bool serverIsDay, float serverDayTimer,
        int serverDayIndex, float serverViolentValue, bool serverViolentDayCheck, bool serverViolentDay)
    {
        day = serverDay;
        isDay = serverIsDay;
        dayTimer = serverDayTimer;
        dayIndex = serverDayIndex;
        violentValue = serverViolentValue;
        violentDayCheck = serverViolentDayCheck;
        violentDay = serverViolentDay;

        timeImg.sprite = timeImgSet[dayIndex];
        dayText.text = "Day : " + day;

        if (violentDay)
        {
            timeImg.color = new Color32(255, 50, 50, 255);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerLocationServerRpc(bool isHostMap, bool isInMarket, bool hostCheck)
    {
        SetPlayerLocationClientRpc(isHostMap, isInMarket, hostCheck);
    }

    [ClientRpc]
    public void SetPlayerLocationClientRpc(bool isHostMap, bool isInMarket, bool hostCheck)
    {
        PlayerStatus[] players = GameObject.FindObjectsOfType<PlayerStatus>();
        foreach (PlayerStatus p in players)
        {
            if (hostCheck && p.name == "Desire")
            {
                p.isPlayerInHostMap = isHostMap;
                p.isPlayerInMarket = isInMarket;
            }
            // 이 아래는 클라이언트 접속보다 데이터 교환이 먼저 이루어지게 바뀌면 다시 활성화
            else if (!hostCheck && p.name == "Pitaya")
            {
                p.isPlayerInHostMap = isHostMap;
                p.isPlayerInMarket = isInMarket;
            }
        }
        //playerStatus.isPlayerInHostMap = isHostMap;
        //playerStatus.isPlayerInMarket = isInMarket;
    }

    public PlayerSaveData PlayerSaveData(bool isHost)
    {
        PlayerSaveData data = new PlayerSaveData();
        data.hp = -1;
        PlayerStatus[] players = GameObject.FindObjectsOfType<PlayerStatus>();
        foreach(PlayerStatus p in players)
        {
            if (isHost && p.name == "Desire")
            {
                data.hp = p.hp;
                data.pos = Vector3Extensions.FromVector3(p.gameObject.transform.position);
                data.isPlayerInHostMap = p.isPlayerInHostMap;
                data.isPlayerInMarket = p.isPlayerInMarket;
                data.tankOn = p.tankOn;
                data.tankHp = p.tankHp;
                data.tankMaxHp = p.tankMaxHp;
            }
            // 이 아래는 클라이언트 접속보다 데이터 교환이 먼저 이루어지게 바뀌면 다시 활성화
            else if (!isHost && p.name == "Pitaya")
            {
                data.hp = p.hp;
                data.pos = Vector3Extensions.FromVector3(p.gameObject.transform.position);
                data.isPlayerInHostMap = p.isPlayerInHostMap;
                data.isPlayerInMarket = p.isPlayerInMarket;
                data.tankOn = p.tankOn;
                data.tankHp = p.tankHp;
                data.tankMaxHp = p.tankMaxHp;
                data.clientFirstConnection = true;
            }

            p.GetComponent<PlayerController>().TankSaveFunc();
        }

        return data;
    }

    public void LoadPlayerData(PlayerSaveData hostData, PlayerSaveData clientData)
    {
        PlayerSaveData data = new PlayerSaveData();
        if (isHost)
            data = hostData;
        else
            data = clientData;

        if (data.hp < 0)
            return;

        if (player != null)
        {
            PlayerStatus playerStatus = player.GetComponent<PlayerStatus>();
            playerStatus.hp = data.hp;
            SetPlayerLocationServerRpc(data.isPlayerInHostMap, data.isPlayerInMarket, IsServer);
            isPlayerInHostMap = data.isPlayerInHostMap;
            isPlayerInMarket = data.isPlayerInMarket;

            //리스폰 중 플레이어 데이터가 저장됐을 때
            if (data.hp == 0)
            {
                playerStatus.hp = playerMaxHp;
                if (isPlayerInHostMap)
                {
                    player.transform.position = hostPlayerSpawnPos;
                }
                else
                {
                    player.transform.position = clientPlayerSpawnPos;
                }
            }
            else
            {
                player.transform.position = Vector3Extensions.ToVector3(data.pos);
            }
        }
        else
        {
            playerDataHp = data.hp;
            isPlayerInHostMap = data.isPlayerInHostMap;
            isPlayerInMarket = data.isPlayerInMarket;

            //리스폰 중 플레이어 데이터가 저장됐을 때
            if (data.hp == 0)
            {
                playerDataHp = playerMaxHp;
                if (isPlayerInHostMap)
                {
                    playerDataPos = hostPlayerSpawnPos;
                }
                else
                {
                    playerDataPos = clientPlayerSpawnPos;
                }
            }
            else
            {
                playerDataPos = Vector3Extensions.ToVector3(data.pos);
            }
        }

        loadTankOn = data.tankOn;
        loadTankData = (data.tankHp, data.tankMaxHp);
    }

    public MapSaveData SaveMapData()
    {
        MapSaveData data = new MapSaveData();

        foreach (var objPos in destroyedMapObjectsPos)
        {
            data.objects.Add(Vector3Extensions.FromVector3(objPos));
        }

        data.fogState = MapGenerator.instance.fogState;

        return data;
    }

    public void LoadMapData(MapSaveData data)
    {
        foreach(var obj in data.objects)
        {
            bool isHostMap = true;
            if (obj.y > map.height)
            {
                isHostMap = false;
            }

            destroyedMapObjectsPos.Add(Vector3Extensions.ToVector3(obj));
            RemoveMapObj(Vector3Extensions.ToVector3(obj), isHostMap);
        }

        MapGenerator.instance.LoadFogState(data.fogState);
    }

    IEnumerator SetQuest()
    {
        while (player == null)
        {
            yield return new WaitForSeconds(1f);
        }

        QuestManager.instance.SetQuest(questData);
    }

    public void AutoSaveTimeIntervalSet(float interval)
    {
        autoSaveinterval = interval;
    }

    public void PlayerEscapeFromStuck()
    {
        Vector3 pos = MapGenerator.instance.GetNearGroundPos(player.transform.position);
        player.transform.position = pos;
    }

    public void SetRespawnUI()
    {
        isWaitingForRespawn = true;
        PlayerStatus status = player.GetComponent<PlayerStatus>();
        status.SetHpServerRpc(0); // 명령어로 동작시킬 경우를 생각해서 hp를 0으로 만들기 위해 넣음
        GameStatePopup.instance.SetRespawnUI();
    }

    public void PlayerRespawn()
    {
        isWaitingForRespawn = false;
        PlayerStatus status = player.GetComponent<PlayerStatus>();
        status.SetHpServerRpc(playerMaxHp);
        if (isPlayerInHostMap)
        {
            player.transform.position = hostPlayerSpawnPos;
        }
        else
        {
            player.transform.position = clientPlayerSpawnPos;
        }
    }

    public void SetGameOverUI()
    {
        GameOverServerRpc();
    }

    [ServerRpc (RequireOwnership = false)]
    public void GameOverServerRpc()
    {
        SteamManager.instance.LeaveLobby();
        NetworkManager.Singleton.Shutdown();
        Destroy(NetworkManager.Singleton.gameObject);

        GameOverClientRpc();
    }

    [ClientRpc]
    public void GameOverClientRpc()
    {
        isGameOver = true;
        GameStop(true);
        CloseAllOpenedUI();
        OptionCanvas.instance.MainPanelSet(false);
        GameStatePopup.instance.SetGameOverUI();
    }

    public void GameOver()
    {
        //GameStopSetServerRpc();
        GameStop(false);
        //OptionCanvas.instance.QuitFunc();
        GameManager.instance.DestroyAllDontDestroyOnLoadObjects();
        SceneManager.LoadScene("MainMenuScene");
    }
}
