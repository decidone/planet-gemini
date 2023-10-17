using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// UTF-8 설정
public abstract class InventoryManager : MonoBehaviour
{
    public GameObject inventoryUI;
    public Inventory inventory;

    [HideInInspector]
    public Slot[] slots;
    protected GameManager gameManager;
    protected DragSlot dragSlot; // 드래그용 슬롯
    protected Slot focusedSlot;  // 마우스 위치에 있는 슬롯
    float splitCooldown;
    float splitTimer;

    protected InputManager inputManager;
    protected bool slotRightClickHold;

    public abstract void OpenUI();
    public abstract void CloseUI();

    protected virtual void Start()
    {
        splitCooldown = 0.12f;
        slotRightClickHold = false;
        gameManager = GameManager.instance;
        dragSlot = DragSlot.instance;
        inputManager = InputManager.instance;
        inputManager.controls.Inventory.SlotLeftClick.performed += ctx => SlotLeftClick();
        inputManager.controls.Inventory.SlotRightClickHold.performed += ctx => SlotRightClickHold();
    }

    protected virtual void Update()
    {
        splitTimer += Time.deltaTime;

        if (slotRightClickHold)
            SlotRightClick();
    }

    public void SetInven(Inventory inven, GameObject invenUI)
    {
        inventory = inven;
        inventoryUI = invenUI;
        inventory.onItemChangedCallback += UpdateUI;
        slots = inventoryUI.transform.Find("Slots").gameObject.GetComponentsInChildren<Slot>();
        for (int i = 0; i < slots.Length; i++)
        {
            Slot slot = slots[i];
            slot.slotNum = i;

            AddEvent(slot, EventTriggerType.PointerEnter, delegate { OnEnter(slot); });
            AddEvent(slot, EventTriggerType.PointerExit, delegate { OnExit(slot); });
        }
        inventory.Refresh();
    }

    public void SetSizeInven(Inventory inven, GameObject invenUI, int invenSize)
    {
        inventory = inven;
        inventoryUI = invenUI;
        inventory.onItemChangedCallback += UpdateUI;
        Slot[] slotObjects = inventoryUI.transform.Find("Slots").gameObject.GetComponentsInChildren<Slot>();
        slots = new Slot[invenSize];

        for (int i = 0; i < slotObjects.Length; i++)
        {
            if (i < invenSize)
            {
                Slot slot = slots[i] = slotObjects[i];
                slot.GetComponentInChildren<Image>().enabled = true;
                slot.slotNum = i;

                AddEvent(slot, EventTriggerType.PointerEnter, delegate { OnEnter(slot); });
                AddEvent(slot, EventTriggerType.PointerExit, delegate { OnExit(slot); });
            }
            else
            {
                slotObjects[i].GetComponentInChildren<Image>().enabled = false;
            }
        }

        inventory.Refresh();
    }

    protected void SlotLeftClick()
    {
        if (inventory == null) return;
        if (inputManager.shift) return;
        if (dragSlot == null) return;

        if (dragSlot.slot.item == null)
        {
            if (focusedSlot != null)
            {
                if (focusedSlot.item != null)
                {
                    inventory.Swap(focusedSlot);
                }
            }
        }
        else
        {
            if (focusedSlot != null)
            {
                if (!focusedSlot.outputSlot)
                {
                    if (dragSlot.slot.item != focusedSlot.item)
                    {
                        if (focusedSlot.inputSlot)
                        {
                            foreach (Item _item in focusedSlot.inputItem)
                            {
                                if (dragSlot.slot.item == _item)
                                {
                                    inventory.Swap(focusedSlot);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            inventory.Swap(focusedSlot);
                        }
                    }
                    else
                    {
                        inventory.Merge(focusedSlot);
                    }
                }
            }
            else if (!RaycastUtility.IsPointerOverUI(Input.mousePosition) && dragSlot.gameObject.activeSelf)
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

    protected void SlotRightClickHold()
    {
        slotRightClickHold = !slotRightClickHold;
    }

    protected void SlotRightClick()
    {
        if (inventory == null) return;
        if (splitTimer <= splitCooldown) return;
        if (focusedSlot == null) return;
        if (focusedSlot.item == null) return;

        if (dragSlot.slot.item == null || dragSlot.slot.item == focusedSlot.item)
        {
            inventory.Split(focusedSlot);
        }

        splitTimer = 0;
    }

    void UpdateUI()
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

        if (dragSlot != null)
        {
            if (dragSlot.slot.item != null)
            {
                dragSlot.slot.AddItem(dragSlot.slot.item, dragSlot.slot.amount);
            }
            else
            {
                dragSlot.slot.ClearSlot();
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
    }

    void OnExit(Slot slot)
    {
        focusedSlot = null;
    }
}
