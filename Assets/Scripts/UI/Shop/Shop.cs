using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public delegate void OnTotalPriceChanged();
    public OnTotalPriceChanged onTotalPriceChangedCallback;

    [SerializeField] MerchandiseListSO merchandiseList;
    [SerializeField] GameObject merchListObj;
    [HideInInspector] public Merch[] merchList;
    [SerializeField] Finance finance;
    [SerializeField] Button btn;
    [SerializeField] bool isPurchase;
    [SerializeField] int sliderBasicValue;

    public int totalPrice;

    void Awake()
    {
        onTotalPriceChangedCallback += SumTotalPrice;

        if (merchListObj != null)
        {
            merchList = merchListObj.GetComponentsInChildren<Merch>();

            for (int i = 0; i < merchList.Length; i++)
            {
                Merch merch = merchList[i];
                merch.SetMerch(this, merchandiseList.MerchandiseSOList[i], isPurchase);
            }

            if (isPurchase)
                btn.onClick.AddListener(BuyMerch);
            else
                btn.onClick.AddListener(SellMerch);
        }
    }

    public void UIOpened()
    {
        if (!isPurchase)
        {
            SetUIAmount();
            GameManager.instance.inventory.onItemChangedCallback += SetUIAmount;
        }
    }

    public void UIClosed()
    {
        if (!isPurchase)
        {
            GameManager.instance.inventory.onItemChangedCallback -= SetUIAmount;
        }
    }

    public void SetUIAmount()
    {
        foreach (Merch merch in merchList)
        {
            int itemAmount = GameManager.instance.inventory.GetItemAmount(merch.item);
            merch.SetOwnedItemAmount(itemAmount);
        }
    }

    public void SumTotalPrice()
    {
        totalPrice = 0;
        for (int i = 0; i < merchList.Length; i++)
        {
            Merch merch = merchList[i];
            totalPrice += merch.SumPrice();
        }

        finance.SetFinance(totalPrice);
    }

    public void BuyMerch()
    {
        if (GameManager.instance.inventory.MultipleSpaceCheck(merchList))
        {
            if (GameManager.instance.finance.finance >= totalPrice)
            {
                foreach (Merch merch in merchList)
                {
                    GameManager.instance.SubFinanceServerRpc(merch.price * merch.amount);
                    GameManager.instance.inventory.Add(merch.item, merch.amount);
                    merch.ResetValue();
                }
            }
            else
            {
                Debug.Log("Not enough money");
            }
        }
        else
        {
            Debug.Log("Not enough space");
        }
    }

    public void SellMerch()
    {
        foreach (Merch merch in merchList)
        {
            GameManager.instance.AddFinanceServerRpc(merch.price * merch.amount);
            GameManager.instance.inventory.Sub(merch.item, merch.amount);
            merch.ResetValue();
        }
    }
}
