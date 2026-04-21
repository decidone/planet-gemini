using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Pathfinding;

public class NetworkObjManager : NetworkBehaviour
{
    public List<Portal> netPortals = new List<Portal>();
    public List<Structure> netStructures = new List<Structure>();
    public List<BeltGroupMgr> netBeltGroupMgrs = new List<BeltGroupMgr>();
    public List<UnitCommonAi> netUnitCommonAis = new List<UnitCommonAi>();
    public List<BeltCtrl> networkBelts = new List<BeltCtrl>();

    public delegate void OnStructureChanged(int type);
    public OnStructureChanged onStructureChangedCallback;

    public delegate void OnUnitChanged(int type);
    public OnUnitChanged onUnitChangedCallback;

    private int _syncTargetStructureCount = -1;
    private int _syncTargetBeltGroupCount = -1;
    private int _syncTargetBeltCount = -1;

    #region SingletonAwake
    public static NetworkObjManager instance;

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

    public override void OnNetworkSpawn()
    {
        if (IsClient && !IsServer)
        {
            RequestSyncServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSyncServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        ClientRpcParams target = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { clientId }
            }
        };

        SendSyncTargetClientRpc(netStructures.Count, netBeltGroupMgrs.Count, networkBelts.Count, target);
    }

    [ClientRpc]
    private void SendSyncTargetClientRpc(int structureCount, int beltGroupCount, int beltCount, ClientRpcParams rpcParams = default)
    {
        _syncTargetStructureCount = structureCount;
        _syncTargetBeltGroupCount = beltGroupCount;
        _syncTargetBeltCount = beltCount;
        StartCoroutine(WaitForSyncCoroutine());
    }

    public void NetObjAdd(WorldObj worldObj)
    {
        if(worldObj.TryGet(out Portal portal))
        {
            netPortals.Add(portal);
        }
        else if (worldObj.TryGet(out Structure structure))
        {
            if(worldObj.TryGet(out BeltCtrl belt))
            {
                networkBelts.Add(belt);
            }
            else
            {
                netStructures.Add(structure);
                onStructureChangedCallback?.Invoke(20);
            }
        }
        else if (worldObj.TryGet(out UnitCommonAi unitCommonAi))
        {
            netUnitCommonAis.Add(unitCommonAi);
            onUnitChangedCallback?.Invoke(23);
        }
    }

    public void BeltGroupAdd(BeltGroupMgr beltGroupMgr)
    {
        netBeltGroupMgrs.Add(beltGroupMgr);
        onStructureChangedCallback?.Invoke(24);
    }

    public void NetObjRemove(WorldObj netObj)
    {
        if (netObj.TryGet(out BeltCtrl belt))
        {
            networkBelts.Remove(belt);
        }
        else if (netObj.TryGet(out Structure structure))
        {
            netStructures.Remove(structure);
        }
        else if (netObj.TryGet(out UnitCommonAi unitCommonAi))
        {
            netUnitCommonAis.Remove(unitCommonAi);
        }
    }

    private IEnumerator WaitForSyncCoroutine()
    {
        yield return new WaitUntil(() =>
            netStructures.Count >= _syncTargetStructureCount &&
            netBeltGroupMgrs.Count >= _syncTargetBeltGroupCount &&
            networkBelts.Count >= _syncTargetBeltCount
        );

        OnSyncComplete();
    }

    private void OnSyncComplete()
    {
        Debug.Log($"Sync End - Structure {netStructures.Count} / BeltGroup {netBeltGroupMgrs.Count} / Belt {networkBelts.Count}");
        foreach (BeltGroupMgr beltGroup in netBeltGroupMgrs)
        {
            beltGroup.ItemSyncServerRpc();
        }
        StartCoroutine(NotifySyncDelay());
    }

    private IEnumerator NotifySyncDelay()
    {
        yield return new WaitForSecondsRealtime(1f);

        GameManager gameManager = GameManager.instance;
        gameManager.SetClientSyncPauseServerRpc(false);
        gameManager.LoadingPopupServerRpc();
        Debug.Log("Sync End");
    }

    public void BeltGroupRemove(BeltGroupMgr beltGroupMgr)
    {
        netBeltGroupMgrs.Remove(beltGroupMgr);
    }

    public bool StructureCheck(StructureData strData)
    {
        for (int i = 0; i < netStructures.Count; i++)
        {
            if (netStructures[i].structureData == strData)
            {
                return true;
            }
        }

        return false;
    }

    public bool UnitCheck(UnitCommonData unitData)
    {
        for (int i = 0; i < netUnitCommonAis.Count; i++)
        {
            if (netUnitCommonAis[i].unitCommonData == unitData)
            {
                return true;
            }
        }

        return false;
    }

    public void InitConnectors()
    {
        Debug.Log("InitConnectors");
        for (int i = 0; i < netStructures.Count; i++)
        {
            EnergyGroupConnector connector = netStructures[i].GetComponentInChildren<EnergyGroupConnector>();
            if (connector != null)
            {
                connector.RemoveGroup();
            }
        }

        for (int i = 0; i < netStructures.Count; i++)
        {
            EnergyGroupConnector connector = netStructures[i].GetComponentInChildren<EnergyGroupConnector>();
            if (connector != null)
            {
                connector.Init();
            }
        }
    }
}
