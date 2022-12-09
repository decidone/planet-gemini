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
            AddEvent(slot, EventTriggerType.BeginDrag, delegate { OnDragStart(slot); });
            AddEvent(slot, EventTriggerType.EndDrag, delegate { OnDragEnd(slot); });
            AddEvent(slot, EventTriggerType.Drag, delegate { OnDrag(slot); });
        }
    }

    void Update()
    {
        if (Input.GetButtonDown("Inventory"))
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);
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
        //Debug.Log("Enter : " + slot);
        focusedSlot = slot;
    }

    private void OnExit(InventorySlot slot)
    {
        //Debug.Log("Exit : " + slot);
        focusedSlot = null;
    }

    private void OnDragStart(InventorySlot slot)
    {
        //Debug.Log("DragStart : " + slot);
        slot.Selected();
        selectedSlot = slot;

        if (selectedSlot.item != null)
        {
            GameObject temp = new GameObject();
            RectTransform rt = temp.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(60, 60);
            temp.transform.SetParent(inventoryItem);
            Image img = temp.AddComponent<Image>();
            img.sprite = selectedSlot.icon.sprite;
            img.raycastTarget = false;

            mouseDrag = temp;
        }
    }

    private void OnDrag(InventorySlot slot)
    {
        //Debug.Log("Drag : " + slot);
        if (mouseDrag != null)
        {
            mouseDrag.GetComponent<RectTransform>().position = Input.mousePosition;
        }
    }

    private void OnDragEnd(InventorySlot slot)
    {
        //Debug.Log("DragEnd : " + slot);
        slot.Release();
        Destroy(mouseDrag);

        if (selectedSlot != null && focusedSlot != null)
        {
            if (selectedSlot.item != null)
            {
                Swap(selectedSlot, focusedSlot);

                if (inventory.onItemChangedCallback != null)
                    inventory.onItemChangedCallback.Invoke();
            }
        }

        selectedSlot = null;
    }

    private void Swap(InventorySlot slot1, InventorySlot slot2)
    {
        Item tempItem = inventory.items[slot1.slotNum];
        int tempAmount = inventory.amounts[slot1.slotNum];

        if (slot2.item != null)
        {
            inventory.items[slot1.slotNum] = inventory.items[slot2.slotNum];
            inventory.items[slot2.slotNum] = tempItem;

            inventory.amounts[slot1.slotNum] = inventory.amounts[slot2.slotNum];
            inventory.amounts[slot2.slotNum] = tempAmount;
        }
        else
        {
            inventory.items.Remove(slot1.slotNum);
            inventory.items.Add(slot2.slotNum, tempItem);

            inventory.amounts.Remove(slot1.slotNum);
            inventory.amounts.Add(slot2.slotNum, tempAmount);
        }
    }
}
