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
    protected DragSlot dragSlot; // �巡�׿� ����
    protected Slot focusedSlot;  // ���콺 ��ġ�� �ִ� ����

    protected Slot selectSlot;

    BuildingData buildingData;

    // Start is called before the first frame update
    void Start()
    {
        SetInven(buildingInventory, buildingInventoryUI);
        dragSlot = DragSlot.instance;
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

        for (int i = 0; i < buildingInventory.BuildingDic.Count; i++)
        {
            if (buildingInventory.BuildingDic[i].item == focusedSlot.item)
            {
                BuildingInfo.instance.SetItemSlot(buildingData, buildingInventory.BuildingDic[i]);
            }
        }

        if (selectSlot == null)
        {
            selectSlot = focusedSlot;
            Image slotImage = selectSlot.GetComponentInChildren<Image>();
            SetSlotColor(slotImage, Color.red, 0.5f);
        }
        else if (selectSlot != focusedSlot)
        {
            Image prevSlotImage = selectSlot.GetComponentInChildren<Image>();
            SetSlotColor(prevSlotImage, Color.white, 1.0f);

            selectSlot = focusedSlot;
            Image currSlotImage = selectSlot.GetComponentInChildren<Image>();
            SetSlotColor(currSlotImage, Color.red, 0.5f);
        }
    }

    void SetSlotColor(Image image, Color color, float alpha)
    {
        Color slotColor = image.color;
        slotColor = color;
        slotColor.a = alpha;
        image.color = slotColor;
    }

    void UpdateUI()
    {
        if (selectSlot != null)
        {
            Image slotImage = selectSlot.GetComponentInChildren<Image>();
            SetSlotColor(slotImage, Color.white, 1.0f);

            BuildingInfo.instance.ClearArr();
            selectSlot = null;
        }

        if(PreBuilding.instance)
            PreBuilding.instance.ReSetImage();

        for (int i = 0; i < slots.Length; i++)
        {
            if (buildingInventory.BuildingDic.ContainsKey(i))
            {
                slots[i].AddItem(buildingInventory.BuildingDic[i].item, 0);
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
