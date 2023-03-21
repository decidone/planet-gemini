using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
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
        Init();
    }

    public void Init()
    {
        inputSlot = false;
        outputSlot = false;
        inputItem.Clear();
    }

    public void AddItem(Item newItem, int itemAmount)
    {
        item = newItem;
        amount = itemAmount;

        icon.sprite = item.icon;
        icon.enabled = true;
        amountText.text = amount.ToString();
        amountText.enabled = true;
    }

    public void ClearSlot()
    {
        item = null;
        amount = 0;

        icon.sprite = null;
        icon.enabled = false;
        amountText.text = null;
        amountText.enabled = false;
    }

    public void SetInputItem(Item _item)
    {
        inputSlot = true;
        if (!inputItem.Contains(_item))
            inputItem.Add(_item);
    }
}
