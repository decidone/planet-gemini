using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

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

    List<GameObject> openedUI;
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

    public delegate void OnUIChanged(GameObject ui);
    public OnUIChanged onUIChangedCallback;

    [SerializeField]
    GameObject tempOption;

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
        openedUI = new List<GameObject>();
        onUIChangedCallback += UIChanged;

        hostMap = mapGenerator.hostMap;
        clientMap = mapGenerator.clientMap;
        map = hostMap;
        isPlayerInHostMap = true;
        isPlayerInMarket = false;
        isShopOpened = false;
        Vector3 playerSpawnPos = new Vector3(map.width/2, map.height/2, 0);
        mapCameraController.SetCamRange(map);
        preBuilding = PreBuilding.instance;
        inputManager = InputManager.instance;
        inputManager.controls.Structure.StrClick.performed += ctx => StrClick();
        inputManager.controls.HotKey.Debug.performed += ctx => DebugMode();
        inputManager.controls.HotKey.Supply.performed += ctx => Supply();
        inputManager.controls.HotKey.Escape.performed += ctx => CloseOpenedUI();
        inputManager.controls.Inventory.PlayerInven.performed += ctx => Inven();
        inputManager.controls.HotKey.Building.performed += ctx => Building();
        inputManager.controls.HotKey.ScienceTree.performed += ctx => ScienceTree();
        inputManager.controls.HotKey.EnergyCheck.performed += ctx => EnergyCheck();
        
        OtherPortalSet();
        //Cursor.lockState = CursorLockMode.Confined;
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

    void StrClick()
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
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.TryGetComponent(out Structure str))
                    newStructure = str;
                newClickEvent = hit.collider.GetComponent<StructureClickEvent>();
                newLogisticsClickEvent = hit.collider.GetComponent<LogisticsClickEvent>();

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

    void DebugMode()
    {
        debug = !debug;
        Debug.Log("debug : " + debug);
    }

    void Supply()
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

    void CloseOpenedUI()
    {
        if (openedUI.Count > 0)
        {
            switch (openedUI[openedUI.Count - 1].gameObject.name)
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
                default:
                    break;
            }
        }
        else
        {
            tempOption.SetActive(!tempOption.activeSelf);
        }
    }

    void Inven()
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

    void Building()
    {
        if (!bManager.buildingInventoryUI.activeSelf)
        {
            bManager.OpenUI();
        }
        else
        {
            bManager.CloseUI();
        }
    }

    void ScienceTree()
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

    void EnergyCheck()
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
        SubFinanceClientRpc(money);
    }

    [ClientRpc]
    public void SubFinanceClientRpc(int money)
    {
        finance.SubFinance(money);
    }
}
