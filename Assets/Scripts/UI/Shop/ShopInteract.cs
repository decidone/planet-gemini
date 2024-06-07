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
        //shopUI.SetActive(true);
        shop.OpenUI();
    }

    public void CloseUI()
    {
        shop.CloseUI();
        //shopUI.SetActive(false);
    }
}
