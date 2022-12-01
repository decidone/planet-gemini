using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemProps : MonoBehaviour
{
    public Item item;
    public int amount;

    private void Awake()
    {
        Debug.Log("item props : " + item.name);
    }
}
