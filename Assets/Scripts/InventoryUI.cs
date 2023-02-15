using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryUI;
    public Transform inventoryItem;
    public bool isPlayerInven;
    public Inventory inventory;

    InventorySlot[] slots;
    InventorySlot dragSlot; // 드래그용 슬롯
    InventorySlot focusedSlot;  // 마우스가 올라간 슬롯
    Inventory playerInven;
    GameManager gameManager;

    private float timer;

    void Start()
    {
        if (isPlayerInven)
            inventory = PlayerInventory.instance;

        gameManager = GameManager.instance;
        playerInven = PlayerInventory.instance;
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
        if (Input.GetButtonDown("Inventory") && isPlayerInven)
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);

            if (gameManager.OpenedInvenCheck())
            {
                gameManager.dragSlot.SetActive(true);
            }
            else
            {
                gameManager.dragSlot.SetActive(false);
            }

            if (dragSlot.item != null)
            {
                // 드래그 도중 인벤토리를 닫았을 때
                //inventory.CancelDrag();
                //DragSlot.instance.slotObj.SetActive(false);
                //inventory.Drop();
            }
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
        {
            if (!isPlayerInven)
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
                } else if (!EventSystem.current.IsPointerOverGameObject() && GameManager.instance.OpenedInvenCheck())
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

    public void SortBtn()
    {
        if (dragSlot.item == null)
        {
            inventory.Sort();
        }
    }

    public void CloseBtn()
    {
        inventoryUI.SetActive(false);

        if (!gameManager.OpenedInvenCheck())
        {
            gameManager.dragSlot.SetActive(false);
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
