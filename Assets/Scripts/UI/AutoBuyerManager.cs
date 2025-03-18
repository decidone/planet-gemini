using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Steamworks.InventoryItem;

public class AutoBuyerManager : MonoBehaviour
{
    [SerializeField] Button selectBtn;

    [SerializeField] Slider maxSlider;
    [SerializeField] Button maxPlusBtn;
    [SerializeField] Button maxMinusBtn;
    [SerializeField] Text maxText;

    [SerializeField] Slider minSlider;
    [SerializeField] Button minPlusBtn;
    [SerializeField] Button minMinusBtn;
    [SerializeField] Text minText;

    AutoBuyer buyer;

    #region Singleton
    public static AutoBuyerManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    #endregion

    void Start()
    {
        maxSlider.onValueChanged.AddListener(delegate { MaxSliderValueChanged(); });
        maxPlusBtn.onClick.AddListener(delegate { MaxPlusButtonClicked(); });
        maxMinusBtn.onClick.AddListener(delegate { MaxMinusButtonClicked(); });

        minSlider.onValueChanged.AddListener(delegate { MinSliderValueChanged(); });
        minPlusBtn.onClick.AddListener(delegate { MinPlusButtonClicked(); });
        minMinusBtn.onClick.AddListener(delegate { MinMinusButtonClicked(); });
    }

    public void SetBuyer(AutoBuyer autoBuyer)
    {
        buyer = autoBuyer;
        maxSlider.value = buyer.maxBuyAmount;
        minSlider.value = buyer.minBuyAmount;
        minSlider.maxValue = buyer.maxBuyAmount;
        selectBtn.onClick.RemoveAllListeners();
        selectBtn.onClick.AddListener(buyer.OpenRecipe);
    }

    void MaxSliderValueChanged()
    {
        int amount = (int)maxSlider.value;

        if (maxSlider.value != 0)
            maxText.text = amount.ToString();
        else
            maxText.text = "";

        if (buyer != null)
        {
            buyer.maxBuyAmount = amount;
            buyer.TransportableCheck();
        }

        minSlider.maxValue = amount;
    }

    void MaxPlusButtonClicked()
    {
        maxSlider.value++;
    }

    void MaxMinusButtonClicked()
    {
        maxSlider.value--;
    }

    void MinSliderValueChanged()
    {
        int amount = (int)minSlider.value;

        if (minSlider.value != 0)
            minText.text = amount.ToString();
        else
            minText.text = "";

        if (buyer != null)
        {
            buyer.minBuyAmount = amount;
            buyer.TransportableCheck();
        }
    }

    void MinPlusButtonClicked()
    {
        minSlider.value++;
    }

    void MinMinusButtonClicked()
    {
        minSlider.value--;
    }

    public void ResetValue()
    {
        buyer = null;
        maxSlider.value = 0;
        minSlider.value = 0;
        minSlider.maxValue = 0;
        selectBtn.onClick.RemoveAllListeners();
    }
}
