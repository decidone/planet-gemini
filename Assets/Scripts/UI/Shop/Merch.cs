using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Merch : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] List<Sprite> moneyImages;
    [SerializeField] Image firstMoneyIcon;
    [SerializeField] Image secondMoneyIcon;
    [SerializeField] Text firstCostText;
    [SerializeField] Text secondCostText;
    [SerializeField] Slider amountSlider;

    public Item item;
    public int price;
    public int amount;
    
    public void SetMerch(Merchandise merchandise, bool isPurchase)
    {
        item = merchandise.item;
        icon.sprite = item.icon;

        if (isPurchase)
            price = merchandise.buyPrice;
        else
            price = merchandise.sellPrice;

        if (price < 99)
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

            string str = price.ToString();
            if (str.Length >= 4)
            {
                firstCostText.text = str.Substring(0, 2);
                secondCostText.text = str.Substring(2, 2);
            }
            else
            {
                firstCostText.text = str.Substring(0, 1);
                secondCostText.text = str.Substring(1, 2);
            }

            if (secondCostText.text == "00")
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
}
