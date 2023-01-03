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
    InventorySlot tempSlot; // �巡�׿� ����
    InventorySlot selectedSlot; // �巡�� �ϱ� ���� Ŭ���� ����
    InventorySlot focusedSlot;  // ���콺�� �ö� ����

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
                // �Ϲ� ����
                AddEvent(slot, EventTriggerType.PointerEnter, delegate { OnEnter(slot); });
                AddEvent(slot, EventTriggerType.PointerExit, delegate { OnExit(slot); });
            }
            else
            {
                // �巡�׿� ����
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
                    // �κ��丮 UI �ٱ�
                    inventory.Drop(tempSlot);
                }
                else
                {
                    // �κ��丮 UI ����, ���õ� ���� ���� ���
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

        // ����(�ӽ�) ���߿� ui��ư����
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
