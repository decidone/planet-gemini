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
        playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ClientSpawnServerRPC(ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        Transform playerTransform = Instantiate(clientChar);
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
}
