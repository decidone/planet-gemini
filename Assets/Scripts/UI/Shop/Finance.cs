using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Finance : MonoBehaviour
{
    [SerializeField] Image firstImage;
    [SerializeField] Image secondImage;
    [SerializeField] Image thirdImage;
    [SerializeField] Text firstText;
    [SerializeField] Text secondText;
    [SerializeField] Text thirdText;
    [SerializeField] float offsetX;

    public int finance;
    bool isTripleDigitForm;

    void Awake()
    {
        finance = 0;
        isTripleDigitForm = false;
    }

    public void SetFinance(int _finance)
    {
        finance = _finance;
        int temp = finance;

        ResetText();
        CheckForm();
        
        if (temp >= 10000)
        {
            firstText.text = (temp/10000).ToString();
            temp %= 10000;
        }
        if (temp >= 100)
        {
            secondText.text = (temp/100).ToString();
            temp %= 100;
        }
        thirdText.text = temp.ToString();

        FillText();
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
            SetFinance(finance - _finance);
        }
    }

    void ResetText()
    {
        firstText.text = "";
        secondText.text = "";
        thirdText.text = "";
    }

    void FillText()
    {
        firstText.text = FillPlace(firstText.text);
        secondText.text = FillPlace(secondText.text);
        thirdText.text = FillPlace(thirdText.text);
    }

    string FillPlace(string str)
    {
        string temp = str;
        if (str.Length == 0)
            temp = "00";
        else if (str.Length == 1)
            temp = "0" + str;

        return temp;
    }

    void CheckForm()
    {
        if (finance >= 1000000 && !isTripleDigitForm)
        {
            isTripleDigitForm = true;
            secondImage.transform.position += new Vector3(offsetX, 0f, 0f);
            secondText.transform.position += new Vector3(offsetX, 0f, 0f);
            thirdImage.transform.position += new Vector3(offsetX, 0f, 0f);
            thirdText.transform.position += new Vector3(offsetX, 0f, 0f);
        }

        if (finance < 1000000 && isTripleDigitForm)
        {
            isTripleDigitForm = false;
            secondImage.transform.position -= new Vector3(offsetX, 0f, 0f);
            secondText.transform.position -= new Vector3(offsetX, 0f, 0f);
            thirdImage.transform.position -= new Vector3(offsetX, 0f, 0f);
            thirdText.transform.position -= new Vector3(offsetX, 0f, 0f);
        }
    }
}
