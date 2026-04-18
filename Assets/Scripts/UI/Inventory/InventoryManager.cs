using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// UTF-8 설정
public abstract class InventoryManager : MonoBehaviour
{
    public GameObject inventoryUI;
    public Inventory inventory;

    //[HideInInspector]
    public Slot[] slots;
    public Slot displaySlot;
    protected GameManager gameManager;
    protected Slot focusedSlot;  // 마우스 위치에 있는 슬롯
    float splitCooldown;
    float splitTimer;
    readonly float holdTime = 0.3f;
    float holdTimer;

    protected InputManager inputManager;
    protected bool slotRightClickHold;

    PreBuilding preBuilding;

    public ItemInfoWindow itemInfoWindow;

    protected SoundManager soundManager;

    public abstract void OpenUI();
    public abstract void CloseUI();

    protected virtual void Start()
    {
        splitCooldown = 0.08f;
        slotRightClickHold = false;
        gameManager = GameManager.instance;
        itemInfoWindow = gameManager.inventoryUiCanvas.GetComponent<ItemInfoWindow>();
        soundManager = SoundManager.instance;
        preBuilding = PreBuilding.instance;
    }

    void OnEnable()
    {
        inputManager = InputManager.instance;
        inputManager.controls.Inventory.SlotLeftClick.performed += SlotLeftClick;
        inputManager.controls.Inventory.SlotRightClickHold.performed += SlotRightClickHold;
    }

    void OnDisable()
    {
        inputManager.controls.Inventory.SlotLeftClick.performed -= SlotLeftClick;
        inputManager.controls.Inventory.SlotRightClickHold.performed -= SlotRightClickHold;
    }

    protected virtual void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        if (slotRightClickHold)
        {
            splitTimer += Time.deltaTime;
            holdTimer += Time.deltaTime;
            if (holdTimer > holdTime)
                SlotRightClick();
        }
    }

    public void SetInven(Inventory inven, GameObject invenUI = null, int? invenSize = null)
    {
        displaySlot = null;

        if (inventory != null)
        {
            inventory.onItemChangedCallback -= UpdateUI;
            inventory.invenAllSlotUpdate -= UpdateAllUI;
        }

        inventory = inven;

        if (invenUI != null)
            inventoryUI = invenUI;

        inventory.onItemChangedCallback += UpdateUI;
        inventory.invenAllSlotUpdate += UpdateAllUI;

        Slot[] slotObjects = inventoryUI.transform.Find("Slots").gameObject.GetComponentsInChildren<Slot>();
        int size = invenSize ?? slotObjects.Length;
        slots = new Slot[size];

        for (int i = 0; i < slotObjects.Length; i++)
        {
            if (i < size)
            {
                Slot slot = slots[i] = slotObjects[i];
                slot.GetComponentInChildren<Image>().enabled = true;
                slot.slotNum = i;

                EventTrigger trigger = slot.GetComponent<EventTrigger>();
                trigger.triggers.RemoveRange(0, trigger.triggers.Count);
                AddEvent(slot, EventTriggerType.PointerEnter, delegate { OnEnter(slot); });
                AddEvent(slot, EventTriggerType.PointerExit, delegate { OnExit(); });
            }
            else
            {
                slotObjects[i].ClearSlot();
                slotObjects[i].GetComponentInChildren<Image>().enabled = false;
            }
        }

        inventory.Refresh();
    }

    public void DisplaySetInven(int saveFluidNum, int waterRequirement, bool fluid, Inventory inven, GameObject invenUI = null, int? invenSize = null )
    {
        SetInven(inven, invenUI, invenSize);

        displaySlot = inventoryUI.transform.Find("DisplaySlot").gameObject.GetComponentInChildren<Slot>();
        if (fluid)
        {
            displaySlot.SetInputItem(ItemList.instance.itemDic["Water"]);
            displaySlot.AddItem(ItemList.instance.itemDic["Water"], 0);
        }
        else
        {
            displaySlot.SetInputItem(ItemList.instance.itemDic["CrudeOil"]);
            displaySlot.AddItem(ItemList.instance.itemDic["CrudeOil"], 0);
        }
        displaySlot.SetItemAmount(saveFluidNum);
        displaySlot.SetReqAmount(waterRequirement);
    }

    public void DisplaySlotUpdate(int saveFluidNum)
    {
        displaySlot.SetItemAmount(saveFluidNum);
    }

    protected void SlotLeftClick(InputAction.CallbackContext ctx)
    {
        if (inventory == null) return;
        if (inputManager.shift) return;
        if (ItemDragManager.instance.inventory == null) return;

        Item dragItem = ItemDragManager.instance.GetItem(GameManager.instance.isHost);

        if (dragItem == null)
        {
            if (focusedSlot != null)
            {
                if (focusedSlot.item != null)
                {
                    inventory.SwapOrMerge(focusedSlot.slotNum);
                    soundManager.PlayUISFX("ItemSelect");
                    PreBuildEnable();
                }
            }
        }
        else
        {
            if (focusedSlot != null)
            {
                if (!focusedSlot.outputSlot)
                {
                    if (dragItem != focusedSlot.item)
                    {
                        if (focusedSlot.inputSlot)
                        {
                            foreach (Item _item in focusedSlot.inputItem)
                            {
                                if (dragItem == _item)
                                {
                                    inventory.SwapOrMerge(focusedSlot.slotNum);
                                    soundManager.PlayUISFX("ItemSelect");
                                    break;
                                }
                            }
                        }
                        else
                        {
                            inventory.SwapOrMerge(focusedSlot.slotNum);
                            soundManager.PlayUISFX("ItemSelect");
                        }
                    }
                    else
                    {
                        inventory.SwapOrMerge(focusedSlot.slotNum);
                        soundManager.PlayUISFX("ItemSelect");
                    }
                }
            }
            else if (!RaycastUtility.IsPointerOverUI(Input.mousePosition) && ItemDragManager.instance.IsDragging(GameManager.instance.isHost))
            {
                // 인벤토리 UI 바깥
                inventory.DragDrop();
            }
            else
            {
                // 인벤토리 UI 내부, 선택된 슬롯 없는 경우
            }
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            slotRightClickHold = false;
    }

    protected void SlotRightClickHold(InputAction.CallbackContext ctx)
    {
        bool pressed = ctx.ReadValueAsButton();
        slotRightClickHold = pressed;

        if (slotRightClickHold)
        {
            holdTimer = 0;
        }
        else
        {
            if (holdTimer < holdTime)
                SlotRightClickTap();
        }
    }

    protected void SlotRightClick()
    {
        if (inventory == null) return;
        if (splitTimer <= splitCooldown) return;
        if (focusedSlot == null) return;
        if (focusedSlot.item == null) return;

        Item dragItem = ItemDragManager.instance.GetItem(GameManager.instance.isHost);
        if (dragItem == null || dragItem == focusedSlot.item)
        {
            inventory.Split(focusedSlot.slotNum);
            PreBuildEnable();
        }

        splitTimer = 0;
    }

    protected void SlotRightClickTap()
    {
        if (inventory == null) return;
        if (focusedSlot == null) return;
        if (focusedSlot.item == null) return;

        Item dragItem = ItemDragManager.instance.GetItem(GameManager.instance.isHost);
        if (dragItem == null || dragItem == focusedSlot.item)
        {
            inventory.SplitHalf(focusedSlot.slotNum);
            PreBuildEnable();
        }

        splitTimer = 0;
    }

    void UpdateAllUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (inventory.items.ContainsKey(i))
            {
                slots[i].AddItem(inventory.items[i], inventory.amounts[i]);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }
    }

    protected virtual void UpdateUI(int slotindex)
    {
        if (slots.Length <= slotindex)
            return;

        if (inventory.items.ContainsKey(slotindex))
            slots[slotindex].AddItem(inventory.items[slotindex], inventory.amounts[slotindex]);
        else
            slots[slotindex].ClearSlot();
    }

    void PreBuildEnable()
    {
        if (preBuilding.isBuildingOn)
            preBuilding.CancelBuild();
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

    void OnExit()
    {
        focusedSlot = null;
        itemInfoWindow.CloseWindow();
    }
}
