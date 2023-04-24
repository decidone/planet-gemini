using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildingInvenManager : MonoBehaviour
{
    public GameObject buildingInventoryUI;
    public BuildingInven buildingInventory;

    [HideInInspector]
    public Slot[] slots;
    protected DragSlot dragSlot; // 드래그용 슬롯
    protected Slot focusedSlot;  // 마우스 위치에 있는 슬롯

    BuildingInfo buildingInfo;
    BuildingData buildingData;

    // Start is called before the first frame update
    void Start()
    {
        SetInven(buildingInventory, buildingInventoryUI);
        dragSlot = DragSlot.instance;
        buildingInfo = BuildingInfo.instance;
    }

    // Update is called once per frame
    void Update()
    {
        InputCheck();
    }

    public void SetInven(BuildingInven inven, GameObject invenUI)
    {
        buildingInventory = inven;
        buildingInventoryUI = invenUI;
        buildingInventory.onItemChangedCallback += UpdateUI;
        slots = buildingInventoryUI.transform.Find("Slots").gameObject.GetComponentsInChildren<Slot>();
        for (int i = 0; i < slots.Length; i++)
        {
            Slot slot = slots[i];
            slot.slotNum = i;

            slot.amountText.gameObject.SetActive(false);

            AddEvent(slot, EventTriggerType.PointerEnter, delegate { OnEnter(slot); });
            AddEvent(slot, EventTriggerType.PointerExit, delegate { OnExit(slot); });
        }
        buildingInventory.Refresh();
    }

    protected virtual void InputCheck()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (dragSlot.slot.item == null)
            {
                if (focusedSlot != null)
                {
                    if (focusedSlot.item != null)
                    {
                        BuildingInfoCheck();
                    }
                }
            }
        }
    }

    void BuildingInfoCheck()
    {
        buildingData = new BuildingData();
        buildingData = BuildingDataGet.instance.GetBuildingName(focusedSlot.item.name);

        for (int i = 0; i < buildingInventory.items.Count; i++)
        {
            if (buildingInventory.items[i].item == focusedSlot.item)
            {
                buildingInfo.CreateItemSlot(buildingData, buildingInventory.items[i]);
            }
        }

        //buildingInfo.CreateItemSlot(buildingData, focusedSlot.item);
    }

    void UpdateUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (buildingInventory.items.ContainsKey(i))
            {
                //slots[i].AddItem(buildingInventory.items[i], buildingInventory.amounts[i]);
                slots[i].AddItem(buildingInventory.items[i].item, buildingInventory.amounts[i]);
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
