using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemInputField : MonoBehaviour
{
    public InputField inputField;
    int fullAmount;
    int invenItemAmount;
    int amount;
    bool hasItem;


    public void InputFieldFGetData(int amount, int invenAmount, bool invenHasItem)
    {
        fullAmount = amount;
        invenItemAmount = invenAmount;
        hasItem = invenHasItem;
        inputField = GetComponent<InputField>();
        inputField.onValueChanged.AddListener(OnValueChanged);
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
}
