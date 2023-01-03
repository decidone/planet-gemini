using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public Transform inventoryItem;
    public GameObject inventoryUI;
    public GameObject slotPref;

    Inventory inventory;
    InventorySlot[] slots;
    InventorySlot tempSlot; // 드래그용 슬롯
    InventorySlot selectedSlot; // 드래그 하기 위해 클릭한 슬롯
    InventorySlot focusedSlot;  // 마우스가 올라간 슬롯

    void Start()
    {
        inventory = Inventory.instance;
        inventory.onItemChangedCallback += UpdateUI;

        slots = inventoryItem.GetComponentsInChildren<InventorySlot>();

        for (int i = 0; i < slots.Length; i++)
        {
            InventorySlot slot = slots[i];
            slot.slotNum = i;

            if (i != slots.Length - 1)
            {
                // 일반 슬롯
                AddEvent(slot, EventTriggerType.PointerEnter, delegate { OnEnter(slot); });
                AddEvent(slot, EventTriggerType.PointerExit, delegate { OnExit(slot); });
            }
            else
            {
                // 드래그용 슬롯
                Image[] images = slot.GetComponentsInChildren<Image>();
                foreach (Image image in images)
                {
                    image.raycastTarget = false;
                }
                tempSlot = slot;
            }
        }
    }

    void Update()
    {
        InputCheck();

        if (tempSlot.item != null)
        {
            tempSlot.GetComponent<RectTransform>().position = Input.mousePosition;
        }
    }

    void InputCheck()
    {
        if (Input.GetButtonDown("Inventory"))
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);

            if (selectedSlot != null)
            {
                if (tempSlot.item != null)
                {
                    inventory.Swap(tempSlot, selectedSlot);
                    tempSlot.ClearSlot();
                }

                selectedSlot = null;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (tempSlot.item == null)
            {
                if (focusedSlot != null)
                {
                    if (focusedSlot.item != null)
                    {
                        selectedSlot = focusedSlot;
                        inventory.Swap(selectedSlot, tempSlot);
                    }
                }
            }
            else
            {
                if (focusedSlot != null && selectedSlot != focusedSlot)
                {
                    if (tempSlot.item != focusedSlot.item)
                    {
                        inventory.Swap(tempSlot, focusedSlot);
                        if (tempSlot.item != null)
                        {
                            inventory.Swap(tempSlot, selectedSlot);
                        }
                    }
                    else
                    {
                        inventory.Merge(tempSlot, focusedSlot);
                        if (tempSlot.item != null)
                        {
                            inventory.Swap(tempSlot, selectedSlot);
                        }
                    }
                } else if (!EventSystem.current.IsPointerOverGameObject())
                {
                    // 인벤토리 UI 바깥
                    inventory.Drop(tempSlot);
                }
                else
                {
                    // 인벤토리 UI 내부, 선택된 슬롯 없는 경우
                    inventory.Swap(tempSlot, selectedSlot);
                }

                tempSlot.ClearSlot();
                selectedSlot = null;
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (focusedSlot != null && selectedSlot == null)
            {
                if (focusedSlot.item != null)
                {
                    inventory.Split(focusedSlot, 1);
                }
            }
        }

        // 정렬(임시) 나중에 ui버튼으로
        if (Input.GetKeyDown("t"))
        {
            inventory.Sort();
        }
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
    }

    private void AddEvent(InventorySlot slot, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry eventTrigger = new EventTrigger.Entry();
        eventTrigger.eventID = type;
        eventTrigger.callback.AddListener(action);

        EventTrigger trigger = slot.GetComponent<EventTrigger>();
        trigger.triggers.Add(eventTrigger);
    }

    private void OnEnter(InventorySlot slot)
    {
        focusedSlot = slot;
    }

    private void OnExit(InventorySlot slot)
    {
        focusedSlot = null;
    }
}
