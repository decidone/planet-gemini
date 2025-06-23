using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// UTF-8 설정
public class Slot : MonoBehaviour
{
    public delegate void OnSlotChanged();
    public OnSlotChanged onSlotChangedCallback;

    public Image icon;
    public Text amountText;
    public Item item;
    public List<Item> inputItem;  //inputSlot 받는 아이템
    public int amount;
    public int needAmount;
    public int slotNum;
    public bool inputSlot;
    public bool outputSlot;
    public string inGameName;

    // 건물 UI 전용
    public bool strDataSet;
    public bool isBuildingUI;
    public bool isEnergyUse; 
    public bool isEnergyStr;
    public float energyConsumption;
    public float energyProduction;

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
                    if (needAmount != 0)
                    {
                        amountText.text = needAmount.ToString();
                        amountText.enabled = true;
                    }
                }
                else
                {
                    color.a = 1f;
                }
                icon.color = color;
            }
        }
    }

    public void AddItem(Item newItem, int itemAmount, string gameName)
    {
        AddItem(newItem, itemAmount);
        inGameName = gameName;
    }

    public void AddItem(Item newItem, int itemAmount)
    {
        item = newItem;
        amount = itemAmount;
        string name = InGameNameDataGet.instance.ReturnName(1, newItem.name);
        inGameName = name;

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

        List<Item> itemTemp = new List<Item>(items);
        inputItem = itemTemp;

        onSlotChangedCallback?.Invoke();
    }

    public void SetItemAmount(int _amount) //디스플레이 슬롯용
    {
        amount = _amount;
        amountText.text = _amount + "";

        onSlotChangedCallback?.Invoke();
    }

    public void SetNeedAmount(int _needAmount) //디스플레이 슬롯용
    {
        needAmount = _needAmount;

        onSlotChangedCallback?.Invoke();
    }

    public void StrData(Building building)
    {
        isEnergyStr = false;
        isEnergyUse = false;
        strDataSet = false;

        isEnergyStr = building.isEnergyStr;
        isEnergyUse = building.isEnergyUse;
        if (!isEnergyStr && !isEnergyUse)
            return;

        building.gameObj.TryGetComponent(out Structure str);
        energyConsumption = str.structureData.Consumption[building.level - 1];
        energyProduction = str.structureData.Production;

        if (isEnergyStr && energyProduction == 0)
            return;

        strDataSet = true;
    }
}
