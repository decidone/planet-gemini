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

    InventorySlot selectedSlot;
    InventorySlot focusedSlot;
    GameObject mouseDrag;

    void Start()
    {
        inventory = Inventory.instance;
        inventory.onItemChangedCallback += UpdateUI;

        slots = inventoryItem.GetComponentsInChildren<InventorySlot>();

        for (int i = 0; i < slots.Length; i++)
        {
            InventorySlot slot = slots[i];
            slot.slotNum = i;
            AddEvent(slot, EventTriggerType.PointerEnter, delegate { OnEnter(slot); });
            AddEvent(slot, EventTriggerType.PointerExit, delegate { OnExit(slot); });
        }
    }

    void Update()
    {
        InputCheck();

        if (selectedSlot != null && mouseDrag != null)
        {
            mouseDrag.GetComponent<RectTransform>().position = Input.mousePosition;
        }
    }

    void InputCheck()
    {
        if (Input.GetButtonDown("Inventory"))
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);

            if (selectedSlot != null)
            {
                if (selectedSlot.item != null)
                {
                    selectedSlot.Release();
                    Destroy(mouseDrag);
                }

                selectedSlot = null;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (selectedSlot == null)
            {
                if (focusedSlot != null)
                {
                    if (focusedSlot.item != null)
                    {
                        selectedSlot = focusedSlot;
                        selectedSlot.Selected();

                        GameObject temp = Instantiate(slotPref);
                        temp.transform.SetParent(inventoryItem);
                        InventorySlot tempSlot = temp.GetComponent<InventorySlot>();
                        tempSlot.Copy(selectedSlot.GetComponent<InventorySlot>());
                        Image[] images = temp.GetComponentsInChildren<Image>();
                        foreach (Image image in images)
                        {
                            image.raycastTarget = false;
                        }

                        mouseDrag = temp;
                    }
                }
            }
            else
            {
                if (selectedSlot.item != null)
                {
                    selectedSlot.Release();
                    Destroy(mouseDrag);

                    if (focusedSlot != null && selectedSlot != focusedSlot)
                    {
                        if (selectedSlot.item != focusedSlot.item)
                        {
                            inventory.Swap(selectedSlot, focusedSlot);
                        }
                        else
                        {
                            inventory.Merge(selectedSlot, focusedSlot);
                        }
                    } else if (!EventSystem.current.IsPointerOverGameObject())
                    {
                        inventory.Drop(selectedSlot);
                    }
                }

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
