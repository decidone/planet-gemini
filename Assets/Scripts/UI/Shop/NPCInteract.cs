using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteract : MonoBehaviour
{
    [SerializeField] bool isShop;
    [SerializeField] bool isBounty;

    public GameObject UI;
    Shop shop;
    Bounty bounty;

    void Start()
    {
        if (isShop)
            shop = UI.GetComponent<Shop>();
        if (isBounty)
            bounty = UI.GetComponent<Bounty>();
    }

    public void OpenUI()
    {
        if (isShop)
            shop.OpenUI();
        if (isBounty)
            bounty.OpenUI();
    }

    public void CloseUI()
    {
        if (isShop)
            shop.CloseUI();
        if (isBounty)
            bounty.CloseUI();
    }
}
