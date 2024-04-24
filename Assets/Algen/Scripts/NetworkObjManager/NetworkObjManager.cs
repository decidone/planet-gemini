using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkObjManager : NetworkBehaviour
{
    public NetworkObjListSO networkObjListSO;
    public List<GameObject> networkObjects = new List<GameObject>();

    public List<Structure> netStructures = new List<Structure>();
    public List<BeltGroupMgr> netBeltGroupMgrs = new List<BeltGroupMgr>();
    public List<UnitCommonAi> netUnitCommonAis = new List<UnitCommonAi>();

    #region Singleton
    public static NetworkObjManager instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of PreBuilding found!");
            return;
        }

        instance = this;
    }
    #endregion

    public void NetObjAdd(GameObject netObj)
    {
        networkObjects.Add(netObj);

        if (netObj.TryGetComponent(out Structure structure) && !netObj.GetComponent<BeltCtrl>())
        {
            netStructures.Add(structure);
        }
        else if (netObj.TryGetComponent(out BeltGroupMgr beltGroupMgr))
        {
            netBeltGroupMgrs.Add(beltGroupMgr);
        }
        else if (netObj.TryGetComponent(out UnitCommonAi unitCommonAi))
        {
            netUnitCommonAis.Add(unitCommonAi);
        }
    }

    public void NetObjRemove(GameObject netObj)
    {
        NetObjRemoveClientRpc(FindNetObjID(netObj));
    }

    [ClientRpc]
    public void NetObjRemoveClientRpc(ulong netObjID)
    {
        NetworkObject netObj = FindNetworkObj(netObjID);

        if (netObj.TryGetComponent(out Structure structure))
        {
            netStructures.Remove(structure);
        }
        else if (netObj.TryGetComponent(out BeltGroupMgr beltGroupMgr))
        {
            netBeltGroupMgrs.Remove(beltGroupMgr);
        }
        else if (netObj.TryGetComponent(out UnitCommonAi unitCommonAi))
        {
            netUnitCommonAis.Remove(unitCommonAi);
        }

        networkObjects.Remove(netObj.gameObject);
    }

    public ulong FindNetObjID(GameObject obj)
    {
        ulong ObjID = 0;

        foreach (GameObject networkObject in networkObjects)
        {
            if(obj == networkObject)
            {
                ObjID = networkObject.GetComponent<NetworkObject>().NetworkObjectId;
                break;
            }
        }

        return ObjID;
    }

    public NetworkObject FindNetworkObj(ulong netObjID)
    {
        NetworkObject netObj = null;

        foreach (GameObject networkObjects in networkObjects)
        {        
            if (networkObjects.TryGetComponent(out NetworkObject networkObject) && networkObject.NetworkObjectId == netObjID)
            {
                netObj = networkObject;
                break;
            }
        }

        return netObj;
    }
}
