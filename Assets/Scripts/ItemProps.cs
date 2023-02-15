using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ItemProps : MonoBehaviour
{
    private IObjectPool<ItemProps> itemPool;

    public Item item;
    public int amount;

    public void SetPool(IObjectPool<ItemProps> pool)
    {
        itemPool = pool;
    }

    public void DestroyItem()
    {
        itemPool.Release(this);
    }
}
