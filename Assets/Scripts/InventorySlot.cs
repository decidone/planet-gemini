using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image icon;
    public Text amountText;

    public int slotNum;
    public Item item;
    int amount;

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

    public void Selected()
    {
        icon.enabled = false;
        amountText.enabled = false;
    }

    public void Release()
    {
        icon.enabled = true;
        amountText.enabled = true;
    }
}
