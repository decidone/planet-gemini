using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Merch : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] Text ItemOwnedAmountText;
    [SerializeField] Text priceText;
    [SerializeField] Slider amountSlider;
    [SerializeField] Button plusButton;
    [SerializeField] Button minusButton;
    [SerializeField] Text amountText;
    [SerializeField] MerchHover hover;

    MerchandiseListSO merchandiseListSO;
    List<Merchandise> merchandiseList;
    Shop shop;
    public int merchNum;
    public Item item;
    public int price;
    public int amount;

    void Start()
    {
        amountSlider.onValueChanged.AddListener(delegate { SliderValueChanged(); });
        plusButton.onClick.AddListener(delegate { PlusButtonClicked(); });
        minusButton.onClick.AddListener(delegate { MinusButtonClicked(); });
    }

    public void SetMerch(Shop _shop, Merchandise merchandise, bool isPurchase)
    {
        merchandiseListSO = Resources.Load<MerchandiseListSO>("SOList/MerchandiseListSO");
        merchandiseList = merchandiseListSO.MerchandiseSOList;
        shop = _shop;
        item = merchandise.item;
        icon.sprite = item.icon;
        hover.SetItemName(item.name);

        for (int i = 0; i < merchandiseList.Count; i++)
        {
            if (merchandiseList[i].item == item)
                merchNum = i;
        }

        if (isPurchase)
            price = merchandise.buyPrice;
        else
            price = merchandise.sellPrice;

        priceText.text = price.ToString();
    }

    public void SetOwnedItemAmount(int _amount)
    {
        ItemOwnedAmountText.text = _amount.ToString();
        SetSliderMaxValue(_amount);
        if (amount > _amount)
            amount = _amount;
    }

    public void SetSliderMaxValue(int value)
    {
        amountSlider.maxValue = value;
    }

    void SliderValueChanged()
    {
        amount = (int)amountSlider.value;

        if (amountSlider.value != 0)
            amountText.text = amount.ToString();
        else
            amountText.text = "";

        shop.onTotalPriceChangedCallback?.Invoke();
    }

    void PlusButtonClicked()
    {
        amountSlider.value++;
    }

    void MinusButtonClicked()
    {
        amountSlider.value--;
    }

    public int SumPrice()
    {
        return amount * price;
    }

    public void ResetValue()
    {
        amountSlider.value = 0;
    }
}
