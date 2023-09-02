using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// UTF-8 설정
public class GameManager : MonoBehaviour
{
    public GameObject inventoryUiCanvas;
    public Map map;
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

    bool debug;
    DragSlot dragSlot;
    List<GameObject> openedUI;
    StructureClickEvent clickEvent;
    StructureClickEvent newClickEvent;
    LogisticsClickEvent logisticsClickEvent;
    LogisticsClickEvent newLogisticsClickEvent;

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
        openedUI = new List<GameObject>();
        dragSlot = DragSlot.instance;
        onUIChangedCallback += UIChanged;
    }

    void Update()
    {
        InputCheck();

        if (dragSlot.slot.item != null)
        {
            dragSlot.GetComponent<RectTransform>().position = Input.mousePosition;
        }
    }

    void InputCheck()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            debug = !debug;
            Debug.Log("debug : " + debug);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            if (rManager.isOpened)
                return;

            //건물 위 오브젝트가 있을때 클릭이 안되서 Raycast > RaycastAll로 변경
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.RaycastAll(pos, Vector2.zero);

            int x = Mathf.FloorToInt(pos.x);
            int y = Mathf.FloorToInt(pos.y);
            if (debug && map.IsOnMap(x, y))
            {
                string buildable = "";
                foreach(string str in map.mapData[x][y].buildable)
                {
                    buildable = buildable + " " + str;
                }

                if (map.mapData[x][y].obj == null)
                {
                    Debug.Log("x : " + x + ", y : " + y +
                    ", biome : " + map.mapData[x][y].biome +
                    ", buildable : " + buildable
                    );
                }
                else
                {
                    Debug.Log("x : " + x + ", y : " + y +
                    ", biome : " + map.mapData[x][y].biome +
                    ", obj : " + map.mapData[x][y].obj.name +
                    ", buildable : " + buildable
                    );
                }
            }

            if (hits.Length > 0)
            {
                foreach (RaycastHit2D hit in hits)
                {
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
                        break;
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
                        logisticsClickEvent.LogisticsCheck();
                        logisticsClickEvent.OpenUI();
                        break;
                    }
                }
            }
            else
            {
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
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
                    case "SplitterMenu":
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

        if (Input.GetButtonDown("Inventory"))
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
        
        if (Input.GetButtonDown("Building"))
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
        
        if (Input.GetButtonDown("ScienceTree"))
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
}
