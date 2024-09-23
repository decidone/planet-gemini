using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;

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

    public bool isPlayerInHostMap;
    public bool isPlayerInMarket;

    public GameObject player;
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

    [SerializeField]
    PlayerInvenManager pInvenManager;
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

    [SerializeField] GameObject basicUI;
    [SerializeField] GameObject chat;

    public delegate void OnUIChanged(GameObject ui);
    public OnUIChanged onUIChangedCallback;

    OptionCanvas optionCanvas;

    public bool scienceBuildingSet;
    public bool sciBuildingMap;
    ScienceBuildingInfo scienceBuildingInfo;

    public int day;                     // 일 수
    public bool isDay;                  // 밤 낮
    [SerializeField] float dayTime;     // 인게임 4시간을 현실 시간으로
    public float dayTimer;              // 게임 내 시간(타이머)
    int dayIndex = 0;                   // 24시간을 6등분해서 인덱스로 사용
    // 0 : 08:00 ~ 12:00
    // 1 : 12:00 ~ 16:00
    // 2 : 16:00 ~ 20:00
    // 3 : 20:00 ~ 24:00
    // 4 : 24:00 ~ 04:00
    // 5 : 04:00 ~ 08:00

    int safeDay = 30;                   // 게임 초기 안전한 날
    int[] randomStackValue = new int[2] { 20, 80 }; // 스택 랜덤 범위
    [SerializeField]
    float violentValue;                 // 광폭화의날 스택
    float violentMaxValue = 100;        // 광폭화의날 최대 값
    [SerializeField]
    bool violentDay;                    // true면 광폭화의 날

    /// <summary>
    /// 광폭화 시스템
    /// 인게임 하루 = 현실 10분
    /// 안전한날 30일 = 현실 5시간
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

    float hours;        // 시간 계산
    int displayHour;    // 시 부분
    int displayMinute;  // 분 부분
    string timeDisplay; // 시간표시용 

    Dictionary<int, int[]> waveLevelsByMapSize = new Dictionary<int, int[]>
    {
        { 0, new int[] { 1, 1, 2, 2 } }, // mapSize 0에 따른 coreLevel별 waveLevel
        { 1, new int[] { 1, 1, 2, 3 } }, // mapSize 1에 따른 coreLevel별 waveLevel
        { 2, new int[] { 1, 2, 3, 4 } }  // mapSize 2에 따른 coreLevel별 waveLevel
    };

    public static event Action GenerationComplete;

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
        isShopOpened = false;

        Vector3 playerSpawnPos = new Vector3(map.width/2, map.height/2, 0);
        mapCameraController.SetCamRange(map);
        preBuilding = PreBuilding.instance;
        scienceBuildingInfo = inventoryUiCanvas.GetComponent<ScienceBuildingInfo>();

        day = 0;
        isDay = true;
        dayTimer = 0;

        OtherPortalSet();
        GameStartSet();
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
    }

    private void Update()
    {
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

            if (dayIndex == 0)
            {
                isDay = true;
                if (violentDay)
                {
                    violentDay = false;
                    timeImg.color = new Color32(255, 255, 255, 255);
                    MonsterSpawnerManager.instance.ViolentDayOff();
                }
            }
            else if (dayIndex == 3)
            {
                isDay = false;
                if (day > safeDay)
                {
                    violentValue += UnityEngine.Random.Range(randomStackValue[0], randomStackValue[1] + 1);
                    if (violentValue > violentMaxValue)
                    {
                        violentDay = true;
                        violentValue = 0;
                        timeImg.color = new Color32(255, 50, 50, 255);
                        MonsterSpawnerManager.instance.ViolentDayOn();
                    }
                }
            }
            else if (dayIndex == 4)
            {
                day++;
                dayText.text = "Day : " + day;
            }
        }

        if (!isConsoleOpened)
        {
            if (consoleUI.activeSelf)
            {
                inputManager.OpenConsole();
                isConsoleOpened = true;
            }
        }
        else
        {
            if (!consoleUI.activeSelf)
            {
                inputManager.CloseConsole();
                isConsoleOpened = false;
            }
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
            SetMapInven(false);
            mapCameraController.SetCamRange(map);
            return clientPlayerSpawnPos;
        }
        else
        {
            map = hostMap;
            isPlayerInHostMap = true;
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
            return marketPortalTransform.position;
        }
        else
        {
            isPlayerInMarket = false;
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

                    if (newClickEvent != null && !newClickEvent.GetComponentInParent<Structure>().isPreBuilding)
                    {
                        if (clickEvent != null)
                        {
                            clickEvent.CloseUI();
                        }
                        if (logisticsClickEvent != null)
                        {
                            logisticsClickEvent.CloseUI();
                        }

                        clickEvent = newClickEvent;
                        clickEvent.StructureClick();
                        clickEvent.OpenUI();
                    }
                    else if (newLogisticsClickEvent != null && !newLogisticsClickEvent.GetComponentInParent<Structure>().isPreBuilding)
                    {
                        if (logisticsClickEvent != null)
                        {
                            logisticsClickEvent.CloseUI();
                        }
                        if (clickEvent != null)
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

    public void CheckAndCancelFocus(Structure str)
    {
        if (focusedStructure == str)
        {
            focusedStructure.DisableFocused();
            focusedStructure = null;
        }
    }

    void DebugMode(InputAction.CallbackContext ctx)
    {
        Debug.Log(LobbySaver.instance.currentLobby?.Id);
        Debug.Log(EventSystem.current.currentSelectedGameObject);
        debug = !debug;
        Debug.Log("debug : " + debug);

        QuestManager.instance.SetQuest(0);
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
                clickEvent.CloseUI();
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
        }
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

    //void Building()
    //{
    //    if (!bManager.buildingInventoryUI.activeSelf)
    //    {
    //        bManager.OpenUI();
    //    }
    //    else
    //    {
    //        bManager.CloseUI();
    //    }
    //}

    void ScienceTree(InputAction.CallbackContext ctx)
    {
        ScienceTree();
    }

    public void ScienceTree()
    {
        if (scienceBuildingSet)
        {
            if (!sTreeManager.scienceTreeUI.activeSelf)
            {
                sTreeManager.OpenUI();
            }
            else
            {
                sTreeManager.CloseUI();
            }
        }
        else
        {
            scienceBuildingInfo.OpenUI();
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
        ItemDragManager.instance.SetInven(hostDragInven);
        GeminiNetworkManager.instance.HostSpawnServerRPC();
        Debug.Log("HostConnected??");
    }

    public void ClientConnected()
    {
        ItemDragManager.instance.SetInven(clientDragInven);
        StartCoroutine(WaitForNetworkConnection());
    }

    private IEnumerator WaitForNetworkConnection()
    {
        Debug.Log("Wait for Network connection");

        while (!NetworkManager.Singleton.IsConnectedClient)
        {
            yield return new WaitForEndOfFrame();
        }

        //ulong id = NetworkManager.Singleton.LocalClientId;
        GeminiNetworkManager.instance.ClientSpawnServerRPC();
        GeminiNetworkManager.instance.RequestJsonServerRpc();

        Debug.Log("Connected to Network");
    }

    public void LoadingEnd()
    {
        GenerationComplete?.Invoke();
    }

    public void SetPlayer(GameObject playerObj)
    {
        player = playerObj;
        playerController = player.GetComponent<PlayerController>();
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
        mainCam = Camera.main.gameObject.GetComponent<CameraController>();
        mainCam.target = player.transform;
        mapCameraController.target = player.transform;
        mapCameraController.SetCamRange(map);
        GameObject fogOfWar = ResourcesManager.instance.fogOfWar;
        FollowTransform followTransform = fogOfWar.GetComponent<FollowTransform>();
        followTransform.SetTargetTransform(mainCam.transform);
        if (isHost)
            WavePoint.instance.PlayerSet(player);
        else
            WavePoint.instance.PlayerSet(player);
    }

    public void SetPlayerPos(float x, float y, bool isHostPos)
    {
        if (isHostPos)
            hostPlayerSpawnPos = new Vector3(x, y, 0);
        else
            clientPlayerSpawnPos = new Vector3(x, y, 0);
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
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveMapObjServerRpc(Vector3 vector3, bool isHostMapRequest)
    {
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
    
    private void GameStartSet()
    {
        Debug.Log(NetworkManager.Singleton.IsHost);
        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log("Host");
            optionCanvas.SaveBtnOnOff(true);
            HostConnected();
            MapGenerator.instance.SpawnerAreaMapSet();
            if (!MainGameSetting.instance.isNewGame)
                DataManager.instance.Load(MainGameSetting.instance.loadDataIndex);
        }
        else
        {
            Debug.Log("Client");
            optionCanvas.SaveBtnOnOff(false);
            ClientConnected();
        }
        //GenerationComplete?.Invoke();
    }

    public void WaveStartSet(int coreLevel)
    {
        int mapSize = MainGameSetting.instance.mapSizeIndex;
        int waveLevel = 0;

        if (waveLevelsByMapSize.ContainsKey(mapSize))
        {
            waveLevel = waveLevelsByMapSize[mapSize][coreLevel - 2];
        }
        Debug.Log(waveLevel);
        MonsterSpawnerManager.instance.WavePointSet(waveLevel, sciBuildingMap);
    }

    public void SciBuildingSet(bool map)
    {
        scienceBuildingSet = true;
        sciBuildingMap = map;
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

        inGameData.day = day;
        inGameData.isDay = isDay;
        inGameData.dayTimer = dayTimer;
        inGameData.dayIndex = dayIndex;

        inGameData.violentValue = violentValue;
        inGameData.violentDay = violentDay;

        return inGameData;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncTimeServerRpc()
    {
        SyncTimeClientRpc(day, isDay, dayTimer, dayIndex, violentValue, violentDay);
    }

    [ClientRpc]
    public void SyncTimeClientRpc(int serverDay, bool serverIsDay, float serverDayTimer,
        int serverDayIndex, float serverViolentValue, bool serverViolentDay)
    {
        day = serverDay;
        isDay = serverIsDay;
        dayTimer = serverDayTimer;
        dayIndex = serverDayIndex;
        violentValue = serverViolentValue;
        violentDay = serverViolentDay;

        timeImg.sprite = timeImgSet[dayIndex];
        dayText.text = "Day : " + day;

        if(violentDay)
            timeImg.color = new Color32(255, 50, 50, 255);
    }
}
