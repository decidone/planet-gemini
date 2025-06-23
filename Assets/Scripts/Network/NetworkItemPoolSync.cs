using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkItemPoolSync : NetworkBehaviour
{
    public List<ItemProps> netItemPool = new List<ItemProps>();

    #region Singleton
    public static NetworkItemPoolSync instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    #endregion

    public void NetPoolItemSet(ItemProps item)
    {
        netItemPool.Add(item);
    }

    public void NetPoolItemSub(ItemProps item)
    {
        netItemPool.Remove(item);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnectedCallback;
        }
    }

    protected virtual void OnClientConnectedCallback(ulong clientId)
    {
        ItemDataSyncServerRpc();
    }

    [ServerRpc]
    void ItemDataSyncServerRpc()
    {
        foreach(ItemProps itemProps in netItemPool)
        {
            int index = GeminiNetworkManager.instance.GetItemSOIndex(itemProps.item);
            NetworkObject itemNetworkObject = itemProps.GetComponent<NetworkObject>();
            GeminiNetworkManager.instance.SetItemPropsClientRpc(itemNetworkObject, index, itemProps.amount);
        }
    }

    public List<NetItemPropsData> NetItemSaveData()
    {
        List<NetItemPropsData> data = new List<NetItemPropsData>();

        foreach (ItemProps itemProps in netItemPool)
        {
            NetItemPropsData itemData = new NetItemPropsData();
            itemData.itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(itemProps.item);
            itemData.amount = itemProps.amount;
            itemData.pos = Vector3Extensions.FromVector3(itemProps.gameObject.transform.position);
            data.Add(itemData);
        }
        return data;
    }

    public void NetItemLoadData(List<NetItemPropsData> saveData)
    {
        foreach (NetItemPropsData data in saveData)
        {
            GeminiNetworkManager.instance.ItemSpawnServerRpc(data.itemIndex, data.amount, Vector3Extensions.ToVector3(data.pos));
        }
    }
}
