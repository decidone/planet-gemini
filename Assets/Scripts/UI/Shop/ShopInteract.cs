using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopInteract : MonoBehaviour
{
    public GameObject shopUI;
    public Shop shop;

    void Start()
    {
        shop = shopUI.GetComponent<Shop>();
    }

    public void OpenUI()
    {
        shopUI.SetActive(true);
        shop.UIOpened();
    }

    public void CloseUI()
    {
        shop.UIClosed();
        shopUI.SetActive(false);
    }
}
