using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    public delegate void OnSlotChanged();
    public OnSlotChanged onSlotChangedCallback;

    public Image icon;
    public Text amountText;
    public Item item;
    public List<Item> inputItem;  //inputSlot 받는 아이템
    public int amount;
    public int slotNum;
    public bool inputSlot;
    public bool outputSlot;

    void Start()
    {
        onSlotChangedCallback += SlotChanged;
        onSlotChangedCallback?.Invoke();
    }

    void SlotChanged()
    {
        if (inputSlot)
        {
            if (inputItem.Count == 1)
            {
                Color color = icon.color;
                icon.sprite = inputItem[0].icon;
                icon.enabled = true;

                if (amount == 0)
                {
                    color.a = 0.5f;
                }
                else
                {
                    color.a = 1f;
                }
                icon.color = color;
            }
        }
    }

    public void AddItem(Item newItem, int itemAmount)
    {
        item = newItem;
        amount = itemAmount;

        icon.sprite = item.icon;
        icon.enabled = true;
        amountText.text = amount.ToString();
        amountText.enabled = true;

        onSlotChangedCallback?.Invoke();
    }

    public void ClearSlot()
    {
        item = null;
        amount = 0;

        icon.sprite = null;
        icon.enabled = false;
        amountText.text = null;
        amountText.enabled = false;

        onSlotChangedCallback?.Invoke();
    }

    public void ResetOption()
    {
        inputSlot = false;
        outputSlot = false;
        inputItem.Clear();
    }

    public void SetInputItem(Item _item)
    {
        inputSlot = true;
        if (!inputItem.Contains(_item))
            inputItem.Add(_item);

        onSlotChangedCallback?.Invoke();
    }

    public void SetInputItem(List<Item> items)
    {
        inputSlot = true;
        inputItem = items;

        onSlotChangedCallback?.Invoke();
    }
}
