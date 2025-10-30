using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OverallSlot : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] Text itemNameText;
    [SerializeField] Text productionAmountText;
    [SerializeField] Text consumptionAmountText;
    [SerializeField] Text purchasedAmountText;
    [SerializeField] Text soldAmountText;
    [SerializeField] Text sentAmountText;
    [SerializeField] Text receivedAmountText;

    public void SlotInit(Item item)
    {
        icon.sprite = item.icon;
        itemNameText.text = SetItemName(item.name);

        productionAmountText.text = "0";
        consumptionAmountText.text = "0";
        purchasedAmountText.text = "0";
        soldAmountText.text = "0";
        sentAmountText.text = "0";
        receivedAmountText.text = "0";

        if (item.tier < 0 || item.name == "UICancel")
            this.gameObject.SetActive(false);
    }

    public string SetItemName(string _name)
    {
        string name = _name;
        char[] charArray = name.ToCharArray();
        List<int> upper = new List<int>();
        int count = 0;

        for (int i = 0; i < charArray.Length; i++)
        {
            if (i != 0 && char.IsUpper(charArray[i]))
            {
                upper.Add(i + count++);
            }
        }

        foreach (int i in upper)
        {
            name = name.Insert(i, " ");
        }

        return name;
    }

    public string SplitNumber(int number)
    {
        string str;

        if (number > 999999)
        {
            int temp = number / 1000000;
            str = temp.ToString() + "M";
        }
        else if (number > 9999)
        {
            int temp = number / 1000;
            str = temp.ToString() + "K";
        }
        else
        {
            str = number.ToString();
        }

        return str;
    }

    public void SetProdAmount(int amount)
    {
        productionAmountText.text = SplitNumber(amount);
    }

    public void SetConsumptionAmount(int amount)
    {
        consumptionAmountText.text = SplitNumber(amount);
    }

    public void SetPurchasedAmount(int amount)
    {
        purchasedAmountText.text = SplitNumber(amount);
    }

    public void SetSoldAmount(int amount)
    {
        soldAmountText.text = SplitNumber(amount);
    }

    public void SetSentAmount(int amount)
    {
        sentAmountText.text = SplitNumber(amount);
    }

    public void SetReceivedAmount(int amount)
    {
        receivedAmountText.text = SplitNumber(amount);
    }
}
