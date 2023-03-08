using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryUI;
    [HideInInspector]
    public Inventory inventory;
    [HideInInspector]
    public InventorySlot[] slots;

    protected GameManager gameManager;
    protected DragSlot dragSlot; // 드래그용 슬롯
    InventorySlot focusedSlot;  // 마우스 위치에 있는 슬롯
    Inventory playerInven;
    float splitCooldown;

    protected virtual void Start()
    {
        gameManager = GameManager.instance;
        playerInven = PlayerInventory.instance;
        dragSlot = DragSlot.instance;
        SetInven(inventory, inventoryUI);
    }

    protected virtual void Update()
    {
        splitCooldown += Time.deltaTime;
        InputCheck();
    }

    protected virtual void InputCheck()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
        {
            if (inventory != playerInven)
            {
                if (focusedSlot != null)
                {
                    if (focusedSlot.item != null)
                    {
                        int containableAmount = playerInven.SpaceCheck(focusedSlot.item);
                        if (focusedSlot.amount <= containableAmount)
                        {
                            playerInven.Add(focusedSlot.item, focusedSlot.amount);
                            inventory.Remove(focusedSlot);
                        }
                        else if (containableAmount != 0)
                        {
                            playerInven.Add(focusedSlot.item, containableAmount);
                            inventory.Sub(focusedSlot, containableAmount);
                        }
                        else
                        {
                            Debug.Log("not enough space");
                        }
                    }
                }
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
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
                                if (dragSlot.slot.item == focusedSlot.inputItem)
                                {
                                    inventory.Swap(focusedSlot);
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
                } else if (!EventSystem.current.IsPointerOverGameObject() && dragSlot.gameObject.activeSelf)
                {
                    // 인벤토리 UI 바깥
                    inventory.Drop();
                }
                else
                {
                    // 인벤토리 UI 내부, 선택된 슬롯 없는 경우
                }
            }
        }

        if (Input.GetMouseButton(1))
        {
            if (splitCooldown > 0.12)
            {
                if (focusedSlot != null)
                {
                    if (focusedSlot.item != null)
                    {
                        if (dragSlot.slot.item == null || dragSlot.slot.item == focusedSlot.item)
                        {
                            inventory.Split(focusedSlot);
                        }
                    }
                }
                splitCooldown = 0;
            }
        }
    }

    public void SetInven(Inventory inven, GameObject invenUI)
    {
        inventory = inven;
        inventoryUI = invenUI;
        inventory.onItemChangedCallback += UpdateUI;
        slots = inventoryUI.transform.Find("Slots").gameObject.GetComponentsInChildren<InventorySlot>();
        for (int i = 0; i < slots.Length; i++)
        {
            InventorySlot slot = slots[i];
            slot.slotNum = i;

            AddEvent(slot, EventTriggerType.PointerEnter, delegate { OnEnter(slot); });
            AddEvent(slot, EventTriggerType.PointerExit, delegate { OnExit(slot); });
        }
        inventory.Refresh();
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

    void AddEvent(InventorySlot slot, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry eventTrigger = new EventTrigger.Entry();
        eventTrigger.eventID = type;
        eventTrigger.callback.AddListener(action);

        EventTrigger trigger = slot.GetComponent<EventTrigger>();
        trigger.triggers.Add(eventTrigger);
    }

    void OnEnter(InventorySlot slot)
    {
        focusedSlot = slot;
    }

    void OnExit(InventorySlot slot)
    {
        focusedSlot = null;
    }
}
