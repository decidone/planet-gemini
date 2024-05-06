using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    [SerializeField] MerchandiseListSO merchandiseList;
    [SerializeField] GameObject merchListObj;
    [HideInInspector] public Merch[] merchList;
    [SerializeField] bool isPurchase;

    void Awake()
    {
        if (merchListObj != null)
        {
            merchList = merchListObj.GetComponentsInChildren<Merch>();

            for (int i = 0; i < merchList.Length; i++)
            {
                Merch merch = merchList[i];
                merch.SetMerch(merchandiseList.MerchandiseSOList[i], isPurchase);
            }
        }
    }
}
