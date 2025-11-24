using NUnit.Framework.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scrap : MonoBehaviour
{
    [SerializeField] Text text;
    public int scrap;

    public int GetScrap()
    {
        return scrap;
    }

    public void SetScrap(int _scrap)
    {
        scrap = _scrap;
        if (scrap > 999999)
        {
            float temp = (float)scrap / 1000000;
            text.text = string.Format("{0:N1}", temp) + "M";
        }
        else if (scrap > 99999)
        {
            float temp = (float)scrap / 1000;
            text.text = string.Format("{0:N1}", temp) + "K";
        }
        else
        {
            text.text = scrap.ToString();
        }
    }

    public void AddScrap(int _scrap)
    {
        SetScrap(scrap + _scrap);
    }

    public void SubScrap(int _scrap)
    {
        if (scrap - _scrap < 0)
        {
            Debug.Log("scrap can't be negative number");
        }
        else
        {
            SetScrap(scrap - _scrap);
        }
    }
}
