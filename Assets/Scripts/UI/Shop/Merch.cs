using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Merch : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] Text ItemOwnedAmountText;
    [SerializeField] List<Sprite> moneyImages;
    [SerializeField] Image firstMoneyIcon;
    [SerializeField] Image secondMoneyIcon;
    [SerializeField] Text firstCostText;
    [SerializeField] Text secondCostText;
    [SerializeField] Slider amountSlider;
    [SerializeField] Button plusButton;
    [SerializeField] Button minusButton;
    [SerializeField] Text amountText;

    Shop shop;
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
        shop = _shop;
        item = merchandise.item;
        icon.sprite = item.icon;

        if (isPurchase)
            price = merchandise.buyPrice;
        else
            price = merchandise.sellPrice;

        if (price < 100)
        {
            firstMoneyIcon.sprite = moneyImages[2];
            secondMoneyIcon.enabled = false;
            secondCostText.enabled = false;

            firstCostText.text = price.ToString();
        }
        else
        {
            firstMoneyIcon.sprite = moneyImages[1];
            secondMoneyIcon.sprite = moneyImages[2];

            int temp = price;
            firstCostText.text = (temp/100).ToString();
            temp %= 100;
            secondCostText.text = temp.ToString();

            if (secondCostText.text == "0")
            {
                secondMoneyIcon.enabled = false;
                secondCostText.enabled = false;
            }
            else if(secondCostText.text.Substring(0, 1) == "0")
            {
                secondCostText.text = secondCostText.text.Substring(secondCostText.text.Length - 1);
            }
        }
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
