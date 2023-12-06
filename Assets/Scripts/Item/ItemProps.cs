using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

// UTF-8 설정
public class ItemProps : MonoBehaviour
{
    public IObjectPool<GameObject> itemPool { get; set; }

    public Item item;
    public int amount;

    [HideInInspector]
    public bool isOnBelt = false;
    [HideInInspector]
    public BeltCtrl setOnBelt;

    public Collider2D col;
}
