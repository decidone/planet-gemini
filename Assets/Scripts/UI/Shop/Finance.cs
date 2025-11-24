using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Finance : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] Text text;
    public int finance = 0;

    public int GetFinance()
    {
        return finance;
    }

    public void SetFinance(int _finance)
    {
        SetFinance(_finance, true);
    }

    public void SetFinance(int _finance, bool isEnough)
    {
        text.color = isEnough ? Color.white : Color.red;
        finance = _finance;

        if (finance > 999999)
        {
            float temp = (float)finance / 1000000;
            text.text = string.Format("{0:N1}", temp) + "M";
        }
        else if (finance > 99999)
        {
            float temp = (float)finance / 1000;
            text.text = string.Format("{0:N1}", temp) + "K";
        }
        else if (finance > 0)
        {
            text.text = finance.ToString();
        }
        else
        {
            text.text = "00";
        }
    }

    public void AddFinance(int _finance)
    {
        SetFinance(finance + _finance);
    }

    public void SubFinance(int _finance)
    {
        if (finance - _finance < 0)
        {
            Debug.Log("Finance can't be negative number");
        }
        else
        {
            Debug.Log("SubFinance : " + _finance);
            SetFinance(finance - _finance);
        }
    }
}
