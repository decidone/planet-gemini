using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemInputField : MonoBehaviour
{
    public InputField inputField;
    int fullAmount;
    public int invenItemAmount;
    int amount;
    bool hasItem;
    SciItemSetWindow setWindow;
    public bool isFinance;
    int index;

    public void InputFieldFGetData(int amount, int invenAmount, bool invenHasItem)
    {
        fullAmount = amount;
        invenItemAmount = invenAmount;
        hasItem = invenHasItem;
        isFinance = false;
        inputField = GetComponent<InputField>();
        inputField.onValueChanged.RemoveAllListeners();
        inputField.onValueChanged.AddListener(OnValueChanged);
    }

    public void FinanceInputFieldFGetData(int amount, SciItemSetWindow sciItem, int inputIndex, int invenAmount, bool invenHasItem)
    {
        fullAmount = amount;
        invenItemAmount = invenAmount;
        hasItem = invenHasItem;
        setWindow = sciItem;
        index = inputIndex;
        isFinance = true;
        inputField = GetComponent<InputField>();
        inputField.onValueChanged.RemoveAllListeners();
        inputField.onValueChanged.AddListener(FinanceOnValueChanged);
    }

    void OnValueChanged(string text)
    {
        if (!int.TryParse(inputField.text, out int textInt))
        {
            return;
        }
        else if (!hasItem || textInt < 0)
        {
            inputField.text = "0";
            return;
        }

        if (fullAmount <= textInt)
            amount = fullAmount;
        else
            amount = textInt;

        bool isEnough = hasItem && invenItemAmount >= amount;

        if (isEnough)
        {
            inputField.text = amount.ToString();
        }
        else
        {
            inputField.text = invenItemAmount.ToString();
        }
    }

    void FinanceOnValueChanged(string text)
    {
        if (!int.TryParse(inputField.text, out int textInt))
        {
            return;
        }
        else if (textInt < 0)
        {
            inputField.text = "0";
            return;
        }
        setWindow.FinanceInputItemCheck(index, 0);

        if (fullAmount <= textInt)
            amount = fullAmount;
        else
            amount = textInt;

        bool isEnough = hasItem && invenItemAmount >= amount;
        if (isEnough)
        {
            inputField.text = amount.ToString();
            setWindow.FinanceInputItemCheck(index, amount);
        }
        else
        {
            inputField.text = invenItemAmount.ToString();
            setWindow.FinanceInputItemCheck(index, invenItemAmount);
        }
    }
}
