using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetPoolSetting : NetworkBehaviour
{
    NetworkObjectPool networkPool;
    public GameObject bullet;
    public NetworkObject netObj;

    public bool getNetworkObject = false;
    public bool returnNetworkObject = false;
    public bool initializePool = false;
    public bool clearPool = false;


    #region Singleton
    public static NetPoolSetting instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of recipeList found!");
            return;
        }
        instance = this;
    }
    #endregion

    void Start()
    {
        networkPool = NetworkObjectPool.Singleton;
    }

    void Update()
    {
        if (getNetworkObject == true)
        {
            spServerRpc();
            getNetworkObject = false;
        }
        if (returnNetworkObject == true)
        {
            diServerRpc();
            returnNetworkObject = false;
        }
        if (initializePool == true)
        {
            inClientRpc();
            initializePool = false;
        }
        if (clearPool == true)
        {
            crClientRpc();
            clearPool = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void spServerRpc()
    {
        if (!IsServer)
        {
            return;
        }
        netObj = networkPool.GetNetworkObject(bullet, new Vector3(450, 450, 0), Quaternion.identity);
        if (!netObj.IsSpawned)
            netObj.Spawn();
    }

    [ServerRpc(RequireOwnership = false)]
    void diServerRpc()
    {
        if (!IsServer)
        {
            return;
        }
        diClientRpc();
    }

    [ClientRpc]
    void diClientRpc()
    {
        if (IsServer)
        {
            netObj.GetComponent<BulletCtrl>().DestroyBulletClientRpc();
        }

        //networkPool.ReturnNetworkObject(netObj, item);
    }

    [ClientRpc]
    void inClientRpc()
    {
        networkPool.InitializePool();
    }
    [ClientRpc]
    void crClientRpc()
    {
        networkPool.ClearPool();
    }
}
