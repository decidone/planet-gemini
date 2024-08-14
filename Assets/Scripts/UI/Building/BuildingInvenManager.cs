using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// UTF-8 설정
public class BuildingInvenManager : MonoBehaviour
{
    public GameObject buildingInventoryUI;
    public BuildingInven buildingInventory;

    [HideInInspector]
    public Slot[] slots;
    protected GameManager gameManager;
    protected Slot focusedSlot;  // 마우스 위치에 있는 슬롯
    protected Slot selectSlot;

    BuildingData buildingData;
    InputManager inputManager;

    public ItemInfoWindow itemInfoWindow;

    SoundManager soundManager;

    #region Singleton
    public static BuildingInvenManager instance;

    void Awake()
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
        gameManager = GameManager.instance;
        SetInven(buildingInventory, buildingInventoryUI);
        
        itemInfoWindow = gameManager.inventoryUiCanvas.GetComponent<ItemInfoWindow>();
        soundManager = SoundManager.instance;
    }
    void OnEnable()
    {
        inputManager = InputManager.instance;
        inputManager.controls.Inventory.BuildingInven.performed += BuildingInfo;
    }
    void OnDisable()
    {
        inputManager.controls.Inventory.BuildingInven.performed -= BuildingInfo;
    }
    public void SetInven(BuildingInven inven, GameObject invenUI)
    {
        buildingInventory = inven;
        buildingInventoryUI = invenUI;
        buildingInventory.onItemChangedCallback += UpdateUI;
        slots = buildingInventoryUI.transform.Find("Slots").gameObject.GetComponentsInChildren<Slot>();
        for (int i = 0; i < slots.Length; i++)
        {
            Slot slot = slots[i];
            slot.slotNum = i;

            slot.amountText.gameObject.SetActive(false);
            EventTrigger trigger = slot.GetComponent<EventTrigger>();
            trigger.triggers.RemoveRange(0, trigger.triggers.Count);
            AddEvent(slot, EventTriggerType.PointerEnter, delegate { OnEnter(slot); });
            AddEvent(slot, EventTriggerType.PointerExit, delegate { OnExit(slot); });
        }
        buildingInventory.Refresh();
    }

    void BuildingInfo(InputAction.CallbackContext ctx)
    {
        if (focusedSlot != null)
        {
            if (focusedSlot.item != null)
            {
                BuildingInfoCheck();
            }
        }
    }

    void BuildingInfoCheck()
    {
        buildingData = new BuildingData();
        buildingData = BuildingDataGet.instance.GetBuildingName(focusedSlot.item.name, focusedSlot.amount);

        for (int i = 0; i < buildingInventory.buildingDic.Count; i++)
        {
            if (buildingInventory.buildingDic[i].item == focusedSlot.item && buildingInventory.buildingDic[i].level == focusedSlot.amount)
            {
                global::BuildingInfo.instance.SetItemSlot(buildingData, buildingInventory.buildingDic[i]);
            }
        }

        if (selectSlot == null)
        {
            selectSlot = focusedSlot;
            Image slotImage = selectSlot.GetComponentInChildren<Image>();
            SetSlotColor(slotImage, Color.blue, 0.5f);
        }
        else if (selectSlot != focusedSlot)
        {
            Image prevSlotImage = selectSlot.GetComponentInChildren<Image>();
            SetSlotColor(prevSlotImage, Color.white, 1.0f);

            selectSlot = focusedSlot;
            Image currSlotImage = selectSlot.GetComponentInChildren<Image>();
            SetSlotColor(currSlotImage, Color.blue, 0.5f);
        }

        global::BuildingInfo.instance.BuildingClick();
    }

    public void PreBuildingCancel()
    {
        if (selectSlot)
        {
            Image prevSlotImage = selectSlot.GetComponentInChildren<Image>();
            SetSlotColor(prevSlotImage, Color.white, 1.0f);
            global::BuildingInfo.instance.ClearArr();
            selectSlot = null;
        }
    }

    void SetSlotColor(Image image, Color color, float alpha)
    {
        Color slotColor = image.color;
        slotColor = color;
        slotColor.a = alpha;
        image.color = slotColor;
    }

    void UpdateUI()
    {
        if (selectSlot != null)
        {
            Image slotImage = selectSlot.GetComponentInChildren<Image>();
            SetSlotColor(slotImage, Color.white, 1.0f);

            global::BuildingInfo.instance.ClearArr();
            selectSlot = null;
        }

        //if(PreBuilding.instance)
        //    PreBuilding.instance.ReSetImage();

        for (int i = 0; i < slots.Length; i++)
        {
            if (buildingInventory.buildingDic.ContainsKey(i))
            {
                slots[i].AddItem(buildingInventory.buildingDic[i].item, buildingInventory.buildingDic[i].level);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }
    }

    void AddEvent(Slot slot, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry eventTrigger = new EventTrigger.Entry();
        eventTrigger.eventID = type;
        eventTrigger.callback.AddListener(action);

        EventTrigger trigger = slot.GetComponent<EventTrigger>();
        trigger.triggers.Add(eventTrigger);
    }

    void OnEnter(Slot slot)
    {
        focusedSlot = slot;
        itemInfoWindow.OpenWindow(slot);
    }

    void OnExit(Slot slot)
    {
        focusedSlot = null;
        itemInfoWindow.CloseWindow();
    }

    public void OpenUI()
    {
        buildingInventoryUI.SetActive(true);
        gameManager.onUIChangedCallback?.Invoke(buildingInventoryUI);
    }

    public void CloseUI()
    {
        buildingInventoryUI.SetActive(false);
        itemInfoWindow.CloseWindow();
        soundManager.PlayUISFX("CloseUI");
        gameManager.onUIChangedCallback?.Invoke(buildingInventoryUI);
    }
}
