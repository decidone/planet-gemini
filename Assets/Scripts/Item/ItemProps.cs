using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ItemProps : MonoBehaviour
{
    IObjectPool<ItemProps> itemPool;

    public Item item;
    public int amount;

    [HideInInspector]
    public bool isOnBelt = false;
    [HideInInspector]
    public BeltCtrl setOnBelt = null;

    public bool isReleased = false;
    public bool IsReleased
    {
        get { return isReleased; }
    }

    public void SetReleased(bool released)
    {
        isReleased = released;
    }

    public void SetPool(IObjectPool<ItemProps> pool)
    {
        itemPool = pool;
    }

    public void DestroyItem()
    {
        if (isOnBelt || isReleased)
        {
            itemPool.Release(this);
        }
        else
            Destroy(this.gameObject);
    }
}
