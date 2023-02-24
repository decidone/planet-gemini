using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    PlayerInvenUI playerInvenUI;
    DragSlot dragSlot;
    List<GameObject> openedUI;
    ClickEvent clickEvent;

    public delegate void OnUIChanged(GameObject window);
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
        onUIChangedCallback += DragUIActive;
    }

    void Update()
    {
        if (dragSlot.slot.item != null)
        {
            dragSlot.GetComponent<RectTransform>().position = Input.mousePosition;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);

            if (hit.collider != null)
            {
                clickEvent = hit.collider.GetComponent<ClickEvent>();
                if (clickEvent != null)
                {
                    clickEvent.OpenUI();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (openedUI.Count > 0)
            {
                switch (openedUI[openedUI.Count - 1].gameObject.name)
                {
                    case "Inventory":
                        playerInvenUI.CloseUI();
                        break;
                    case "StructureInfo":
                        clickEvent.CloseUI();
                        break;
                    default:
                        break;
                }
            }
        }

        if (Input.GetButtonDown("Inventory"))
        {
            if (!playerInvenUI.inventoryUI.activeSelf)
            {
                playerInvenUI.OpenUI();
            }
            else
            {
                playerInvenUI.CloseUI();
            }
        }
    }

    void UIChanged(GameObject window)
    {
        if (window.activeSelf)
        {
            if (!openedUI.Contains(window))
                openedUI.Add(window);
        }
        else
        {
            if (openedUI.Contains(window))
                openedUI.Remove(window);
        }
    }

    void DragUIActive(GameObject window)
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
