using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scrap : MonoBehaviour
{
    [SerializeField] Text text;
    public int scrap;

    void Awake()
    {
        scrap = 0;
    }

    public int GetScrap()
    {
        return scrap;
    }

    public void SetScrap(int _scrap)
    {
        scrap = _scrap;

        ResetText();
        text.text = scrap.ToString();
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

    void ResetText()
    {
        text.text = "";
    }
}
