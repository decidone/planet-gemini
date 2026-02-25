using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public AutoBuyer buyer;

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
        SetMaxSliderValue(buyer.maxBuyAmount);
        SetMinSliderValue(buyer.buyInterval);
        selectBtn.onClick.RemoveAllListeners();
        selectBtn.onClick.AddListener(buyer.OpenRecipe);
    }

    public void SetMaxSliderValue(int value)
    {
        maxSlider.SetValueWithoutNotify(value);

        if (maxSlider.value != 0)
            maxText.text = value.ToString();
        else
            maxText.text = "";

        //minSlider.maxValue = value;
    }

    public void SetMinSliderValue(int value)
    {
        minSlider.SetValueWithoutNotify(value);

        if (minSlider.value != 0)
            minText.text = value.ToString();
        else
            minText.text = "";
    }

    void MaxSliderValueChanged()
    {
        int amount = (int)maxSlider.value;
        
        if (buyer != null)
        {
            buyer.MaxSliderUIValueChanged(amount);
        }
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

        if (buyer != null)
        {
            buyer.MinSliderUIValueChanged(amount);
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
        maxText.text = "";
        minSlider.value = 10;
        minSlider.minValue = 10;
        minSlider.maxValue = 60;
        minText.text = "";
        selectBtn.onClick.RemoveAllListeners();
    }
}
