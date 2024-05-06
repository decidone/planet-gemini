using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopInteract : MonoBehaviour
{
    public GameObject shopUI;

    public void OpenUI()
    {
        shopUI.SetActive(true);
    }

    public void CloseUI()
    {
        shopUI.SetActive(false);
    }
}
