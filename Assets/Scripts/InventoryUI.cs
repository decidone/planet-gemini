using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryUI;
    public Transform inventoryItem;
    public bool isPlayerInven;
    public Inventory inventory; // �÷��̾� �κ��丮(GameManager), �ǹ� �κ��丮 ���� �� ���� �ʿ�

    InventorySlot[] slots;
    InventorySlot dragSlot; // �巡�׿� ����
    InventorySlot focusedSlot;  // ���콺�� �ö� ����

    private float timer;

    void Start()
    {
        if (isPlayerInven)
            inventory = PlayerInventory.instance;

        dragSlot = DragSlot.instance.slot;
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
        timer += Time.deltaTime;
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

            if (dragSlot.item != null)
            {
                // �巡�� ���� �κ��丮�� �ݾ��� ��
                inventory.CancelDrag();
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
                        inventory.Swap(focusedSlot);
                    }
                }
            }
            else
            {
                if (focusedSlot != null)
                {
                    if (dragSlot.item != focusedSlot.item)
                    {
                        inventory.Swap(focusedSlot);
                    }
                    else
                    {
                        inventory.Merge(focusedSlot);
                    }
                } else if (!EventSystem.current.IsPointerOverGameObject())
                {
                    // �κ��丮 UI �ٱ�
                    inventory.Drop(dragSlot);
                }
                else
                {
                    // �κ��丮 UI ����, ���õ� ���� ���� ���
                }
            }
        }

        if (Input.GetMouseButton(1))
        {
            if (timer > 0.12)
            {
                if (focusedSlot != null)
                {
                    if (focusedSlot.item != null)
                    {
                        if (dragSlot.item == null || dragSlot.item == focusedSlot.item)
                        {
                            inventory.Split(focusedSlot);
                        }
                    }
                }
                timer = 0;
            }
        }

        // ����(�ӽ�) ���߿� ui��ư����
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

        if(dragSlot.item != null)
        {
            dragSlot.AddItem(dragSlot.item, dragSlot.amount);
        }
        else
        {
            dragSlot.ClearSlot();
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
