using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public Transform inventoryItem;
    public GameObject inventoryUI;

    Inventory inventory;
    InventorySlot[] slots;
    InventorySlot dragSlot; // 드래그용 슬롯
    InventorySlot selectedSlot; // 드래그 하기 위해 선택한 슬롯
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
                dragSlot = slot;
            }
        }
    }

    void Update()
    {
        InputCheck();

        if (dragSlot.item != null)
        {
            dragSlot.GetComponent<RectTransform>().position = Input.mousePosition;
        }
    }

    void InputCheck()
    {
        if (Input.GetButtonDown("Inventory"))
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);

            if (selectedSlot != null)
            {
                if (dragSlot.item != null)
                {
                    //inventory.Swap(dragSlot, selectedSlot);
                    //inventory.Add(tempSlot.item, tempSlot.amount); 이럼 수량 2배 될 거
                    //dragSlot.ClearSlot();
                }

                selectedSlot = null;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (dragSlot.item == null)
            {
                if (focusedSlot != null)
                {
                    if (focusedSlot.item != null)
                    {
                        inventory.Swap(focusedSlot, dragSlot);
                    }
                }
            }
            else
            {
                if (focusedSlot != null && selectedSlot != focusedSlot)
                {
                    if (dragSlot.item != focusedSlot.item)
                    {
                        inventory.Swap(dragSlot, focusedSlot);
                        //if (dragSlot.item != null)
                        //{
                        //    inventory.Swap(dragSlot, selectedSlot);
                        //}
                    }
                    else
                    {
                        inventory.Merge(dragSlot, focusedSlot);
                        //if (dragSlot.item != null)
                        //{
                        //    inventory.Swap(dragSlot, selectedSlot);
                        //}
                    }
                } else if (!EventSystem.current.IsPointerOverGameObject())
                {
                    // 인벤토리 UI 바깥
                    inventory.Drop(dragSlot);
                }
                else
                {
                    // 인벤토리 UI 내부, 선택된 슬롯 없는 경우
                    // inventory.Swap(dragSlot, selectedSlot);
                }

                //dragSlot.ClearSlot();
                selectedSlot = null;
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (focusedSlot != null && selectedSlot == null)
            {
                if (focusedSlot.item != null)
                {
                    if(dragSlot.item == null || dragSlot.item == focusedSlot.item)
                    {
                        inventory.Split(focusedSlot);
                    }
                }
            }
        }

        // 정렬(임시) 나중에 ui버튼으로
        if (Input.GetKeyDown("t"))
        {
            if (dragSlot.item == null)
            {
                inventory.Sort();
            }
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
