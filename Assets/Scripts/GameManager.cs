using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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

    public delegate void OnUIChanged(GameObject ui);
    public OnUIChanged onUIChangedCallback;

    OptionCanvas optionCanvas;

    #region Singleton
    public static GameManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of GameManager found!");
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
                if (hits[i].collider.TryGetComponent(out Structure str))
                    newStructure = str;
                newClickEvent = hits[i].collider.GetComponent<StructureClickEvent>();
                newLogisticsClickEvent = hits[i].collider.GetComponent<LogisticsClickEvent>();

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

                if (hits[i].collider.TryGetComponent(out InfoInteract _info))
                {
                    infoList.Add(_info);
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
            Debug.Log("Escape");
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
                ConfirmPanel.instance.UIClose();
                break;
            default:
                if (openedUI[order].gameObject.TryGetComponent<Shop>(out Shop shop))
                    shop.CloseUI();
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

    public void Inven(InputAction.CallbackContext ctx)
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

    public void ScienceTree(InputAction.CallbackContext ctx)
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
        followTransform.SetTargetTransform(player.transform);
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
    public void RemoveMapObjServerRpc(Vector3 vector3, bool isHostMapRequest)
    {
        RemoveMapObjClientRpc(vector3, isHostMapRequest);
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
            HostConnected();
            MapGenerator.instance.SpawnerAreaMapSet();
            if (!MainGameSetting.instance.isNewGame)
                DataManager.instance.Load(MainGameSetting.instance.loadDataIndex);
        }
        else
        {
            Debug.Log("Client");
            ClientConnected();
        }
    }
}
