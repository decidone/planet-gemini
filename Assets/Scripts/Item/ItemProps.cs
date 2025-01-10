using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Pool;

// UTF-8 설정
public class ItemProps : MonoBehaviour
{
    NetworkObjectPool networkObjectPool;

    public IObjectPool<GameObject> itemPool { get; set; }

    public Item item;
    public int amount;
    [HideInInspector]
    public bool waitingForDestroy = false;
    [HideInInspector]
    public bool isOnBelt = false;
    [HideInInspector]
    public BeltCtrl setOnBelt;
    public int beltGroupIndex;

    private void Start()
    {
        networkObjectPool = NetworkObjectPool.Singleton;
    }

    public void ResetItemProps()
    {
        if (GameManager.instance.isHost)
        {
            int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(item);
            GeminiNetworkManager.instance.ItemSpawnServerRpc(itemIndex, amount, transform.position);
        }
        Debug.Log("drop : " + beltGroupIndex);
        itemPool.Release(gameObject);
    }

    public void ClientResetItemProps()
    {
        Debug.Log("beltGroupIndex : " + beltGroupIndex);
        itemPool.Release(gameObject);
    }
}