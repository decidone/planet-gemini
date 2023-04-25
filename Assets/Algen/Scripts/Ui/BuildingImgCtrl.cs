using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingImgCtrl : MonoBehaviour
{
    public delegate void OnSlotChanged();
    public OnSlotChanged onSlotChangedCallback;

    public Image icon;
    public Text amountText;

    public Item item;
    public List<Item> inputItem;  //inputSlot 받는 아이템
    public int amount;

    //public Inventory inventory = null;

    // Start is called before the first frame update
    void Start()
    {
        onSlotChangedCallback += SlotChanged;
    }

    void SlotChanged()
    {
        if (inputItem.Count == 1)
        {
            icon.sprite = inputItem[0].icon;
            icon.enabled = true;
        }        
    }

    public void AddItem(Item newItem, int itemAmount, bool isEnough)
    {
        item = newItem;
        amount = itemAmount;

        icon.sprite = item.icon;
        icon.enabled = true;
        amountText.text = amount.ToString();
        amountText.enabled = true;

        Color iconColor = icon.color;

        if (isEnough)
        {
            iconColor.a = 1f;
            amountText.color = Color.white;
        }
        else
        {
            iconColor.a = 0.5f;
            amountText.color = Color.red;
        }
        icon.color = iconColor;

        onSlotChangedCallback?.Invoke();
    }

    public void Refresh()
    {
        onSlotChangedCallback?.Invoke();
    }
}
