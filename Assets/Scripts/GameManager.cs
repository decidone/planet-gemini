using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// UTF-8 설정
public class GameManager : MonoBehaviour
{
    public GameObject inventoryUiCanvas;
    public Map map;
    public GameObject player;
    public PlayerController playerController;
    CameraController mainCam;
    public MapCameraController mapCameraController;
    public GameObject preBuildingObj;

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
    DragSlot dragSlot;
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

    Vector3 playerSpawnPos;

    public delegate void OnUIChanged(GameObject ui);
    public OnUIChanged onUIChangedCallback;

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
        openedUI = new List<GameObject>();
        dragSlot = DragSlot.instance;
        onUIChangedCallback += UIChanged;

        Vector3 playerSpawnPos = new Vector3(map.width/2, map.height/2, 0);

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

    void Update()
    {
        if (dragSlot.slot.item != null)
        {
            dragSlot.GetComponent<RectTransform>().position = Input.mousePosition;
        }
    }

    void StrClick()
    {
        if (RaycastUtility.IsPointerOverUI(Input.mousePosition))
            return;
        if (rManager.isOpened)
            return;

        //건물 위 오브젝트가 있을때 클릭이 안되서 Raycast > RaycastAll로 변경
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.RaycastAll(pos, Vector2.zero);

        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        if (debug && inputManager.ctrl && map.IsOnMap(x, y))
        {
            string buildable = "";
            foreach (string str in map.mapData[x][y].buildable)
            {
                buildable = buildable + " " + str;
            }

            if (map.mapData[x][y].obj == null)
            {
                Debug.Log("x : " + x + ",   y : " + y +
                ",   biome : " + map.mapData[x][y].biome +
                ",   resource : " + map.mapData[x][y].resource +
                ",   buildable : " + buildable +
                ",   structure : " + map.mapData[x][y].structure
                );
            }
            else
            {
                Debug.Log("x : " + x + ",   y : " + y +
                ",   biome : " + map.mapData[x][y].biome +
                ",   resource : " + map.mapData[x][y].resource +
                ",   obj : " + map.mapData[x][y].obj.name +
                ",   buildable : " + buildable +
                ",   structure : " + map.mapData[x][y].structure
                );
            }
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
            Inventory inven = this.GetComponent<Inventory>();
            foreach (Item item in ItemList.instance.itemList)
            {
                if (item.name != "EmptyFilter" && item.name != "FullFilter" && item.name != "Water" && item.name != "CrudeOil")
                    inven.Add(item, 99);
            }
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
            dragSlot.gameObject.SetActive(true);
        }
        else
        {
            dragSlot.gameObject.SetActive(false);
        }
    }

    public void BuildAndSciUiReset()
    {
        if (BuildingInfo.instance != null && BuildingInfo.instance.gameObject.activeSelf)
        {
            BuildingInfo.instance.SetItemSlot();
            if(PreBuilding.instance != null)
                PreBuilding.instance.isEnough = BuildingInfo.instance.AmountsEnoughCheck();
        }
        if (InfoWindow.instance != null && InfoWindow.instance.gameObject.activeSelf)
        {
            InfoWindow.instance.SetNeedItem();
        }
    }

    public void SetPlayer(GameObject playerObj)
    {
        player = playerObj;
        playerController = player.GetComponent<PlayerController>();
        player.transform.position = playerSpawnPos;
        mainCam = Camera.main.gameObject.GetComponent<CameraController>();
        mainCam.target = player.transform;
        mapCameraController.target = player.transform;
        GameObject fogOfWar = ResourcesManager.instance.fogOfWar;
        FollowTransform followTransform = fogOfWar.GetComponent<FollowTransform>();
        followTransform.SetTargetTransform(player.transform);
    }

    public void SetPlayerPos(float x, float y)
    {
        playerSpawnPos = new Vector3(x, y, 0);
        //player.transform.position = playerSpawnPos;
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
}
