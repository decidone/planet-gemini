using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GeminiNetworkManager : NetworkBehaviour
{
    [SerializeField]
    Transform hostChar;
    [SerializeField]
    Transform clientChar;
    [SerializeField]
    public ItemListSO itemListSO;
    [SerializeField]
    GameObject itemPref;

    public delegate void OnItemDestroyed();
    public OnItemDestroyed onItemDestroyedCallback;

    #region Singleton
    public static GeminiNetworkManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of GeminiMultiplayer found!");
            return;
        }

        instance = this;
    }
    #endregion

    [ServerRpc]
    public void HostSpawnServerRPC(ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        Transform playerTransform = Instantiate(hostChar);
        GameManager.instance.hostPlayerTransform = playerTransform;
        playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ClientSpawnServerRPC(ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        Transform playerTransform = Instantiate(clientChar);
        GameManager.instance.clientPlayerTransform = playerTransform;
        playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }

    public int GetItemSOIndex(Item item)
    {
        return itemListSO.itemSOList.IndexOf(item);
    }

    public Item GetItemSOFromIndex(int itemSOIndex)
    {
        return itemListSO.itemSOList[itemSOIndex];
    }

    [ServerRpc(RequireOwnership = false)]
    public void ItemSpawnServerRpc(int itemIndex, int amount, Vector3 spawnPos)
    {
        Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
        Debug.Log("Item : " + item.name + ", Amount : " + amount);
        GameObject dropItem = Instantiate(itemPref, spawnPos, Quaternion.identity);
        NetworkObject itemNetworkObject = dropItem.GetComponent<NetworkObject>();
        itemNetworkObject.Spawn(true);

        SetItemPropsClientRpc(itemNetworkObject, itemIndex, amount);
    }

    [ClientRpc]
    public void SetItemPropsClientRpc(NetworkObjectReference networkObjectReference, int itemIndex, int amount)
    {
        networkObjectReference.TryGet(out NetworkObject itemNetworkObject);
        Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
        SpriteRenderer sprite = itemNetworkObject.GetComponent<SpriteRenderer>();
        sprite.sprite = item.icon;
        ItemProps itemProps = itemNetworkObject.GetComponent<ItemProps>();
        itemProps.item = item;
        itemProps.amount = amount;
    }

    public void DestroyItem(NetworkObject itemObj)
    {
        DestroyItemServerRpc(itemObj.GetComponent<NetworkObject>());
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyItemServerRpc(NetworkObjectReference networkObjectReference)
    {
        networkObjectReference.TryGet(out NetworkObject networkObject);
        if (networkObject != null)
        {
            Destroy(networkObject.gameObject);
            onItemDestroyedCallback?.Invoke();
        }
    }
}
