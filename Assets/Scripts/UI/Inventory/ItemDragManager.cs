using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemDragManager : MonoBehaviour
{
    public GameObject inventoryUI;
    [HideInInspector]
    public Inventory inventory;

    Inventory hostInven;
    Inventory clientInven;

    [HideInInspector]
    public GameObject slotObj;
    [HideInInspector]
    public Slot slot;
    public bool isDrag; //로컬 확인용

    #region Singleton
    public static ItemDragManager instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        isDrag = false;
        instance = this;
    }
    #endregion

    void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        if (isDrag)
        {
            slotObj.GetComponent<RectTransform>().position = Input.mousePosition;
        }
    }

    public void SetInven(Inventory inven)
    {
        if(inventory)
        {
            inventory.onItemChangedCallback -= UpdateUI;
            inventory.invenAllSlotUpdate -= UpdateUI;
        }

        inventory = inven;
        inventory.onItemChangedCallback += UpdateUI;
        inventory.invenAllSlotUpdate += UpdateUI;
        hostInven = GameManager.instance.hostDragInven;
        clientInven = GameManager.instance.clientDragInven;
        slotObj = inventoryUI.transform.Find("Slot").gameObject;
        slot = slotObj.transform.GetComponent<Slot>();
        slot.slotNum = 0;

        Image[] images = slot.GetComponentsInChildren<Image>();
        foreach (Image image in images)
        {
            image.raycastTarget = false;
        }

        inventory.Refresh();
    }

    void UpdateUI()
    {
        UpdateUI(0);
    }

    void UpdateUI(int slotindex)
    {
        if (inventory.items.Count > 0)
        {
            slot.AddItem(inventory.items[0], inventory.amounts[0]);
            isDrag = true;
        }
        else
        {
            slot.ClearSlot();
            isDrag = false;
        }
    }

    public Item GetItem(bool isHostTask)
    {
        if (isHostTask)
        {
            if (hostInven.items.Count > 0)
                return hostInven.items[0];
            else
                return null;
        }
        else
        {
            if (clientInven.items.Count > 0)
                return clientInven.items[0];
            else
                return null;
        }
    }

    public int GetAmount(bool isHostTask)
    {
        if (isHostTask)
        {
            if (hostInven.amounts.Count > 0)
                return hostInven.amounts[0];
            else
                return 0;
        }
        else
        {
            if (clientInven.amounts.Count > 0)
                return clientInven.amounts[0];
            else
                return 0;
        }
    }

    public bool IsDragging(bool isHostTask)
    {
        if (isHostTask)
        {
            return hostInven.HasItem();
        }
        else
        {
            return clientInven.HasItem();
        }
    }

    public void Clear(bool isHostTask)
    {
        if (isHostTask)
        {
            if (hostInven.items.Count > 0)
                hostInven.RemoveServerRpc(0);
        }
        else
        {
            if (clientInven.items.Count > 0)
                clientInven.RemoveServerRpc(0);
        }
    }

    public void Add(Item item, int amount, bool isHostTask)
    {
        if (isHostTask)
        {
            if (hostInven.items.Count == 0 || hostInven.items[0] == item)
                hostInven.SlotAdd(0, item, amount);
        }
        else
        {
            if (clientInven.items.Count == 0 || clientInven.items[0] == item)
                clientInven.SlotAdd(0, item, amount);
        }
    }

    public void Sub(int amount, bool isHostTask)
    {
        if (isHostTask)
        {
            if (hostInven.items.Count > 0)
                hostInven.SlotSubServerRpc(0, amount);
        }
        else
        {
            if (clientInven.items.Count > 0)
                clientInven.SlotSubServerRpc(0, amount);
        }
    }
}
