using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkObjManager : NetworkBehaviour
{

    public List<Portal> netPortals = new List<Portal>();
    public List<Structure> netStructures = new List<Structure>();
    public List<BeltGroupMgr> netBeltGroupMgrs = new List<BeltGroupMgr>();
    public List<UnitCommonAi> netUnitCommonAis = new List<UnitCommonAi>();
    public List<BeltCtrl> networkBelts = new List<BeltCtrl>();

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
        if(netObj.TryGetComponent(out Portal portal))
        {
            netPortals.Add(portal);
        }
        else if (netObj.TryGetComponent(out Structure structure) && !netObj.GetComponent<BeltCtrl>())
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
        else
        {
            networkBelts.Add(netObj.GetComponent<BeltCtrl>());
        }
    }

    public void NetObjRemove(GameObject netObj)
    {
        NetObjRemove(FindNetObjID(netObj));
    }

    public void NetObjRemove(ulong netObjID)
    {
        NetworkObject netObj = FindNetworkObj(netObjID);

        if(netObj.GetComponent<BeltCtrl>())
        {
            networkBelts.Remove(netObj.GetComponent<BeltCtrl>());
        }
        else if (netObj.TryGetComponent(out Structure structure))
        {
            netStructures.Remove(structure);
        }
        else if (netObj.GetComponent<BeltGroupMgr>())
        {
            BeltGroupRemoveServerRpc(netObjID);
            //netBeltGroupMgrs.Remove(beltGroupMgr);
        }
        else if (netObj.TryGetComponent(out UnitCommonAi unitCommonAi))
        {
            netUnitCommonAis.Remove(unitCommonAi);
        }
    }

    [ServerRpc]
    void BeltGroupRemoveServerRpc(ulong netObjID)
    {
        BeltGroupRemoveClientRpc(netObjID);
    }

    [ClientRpc]
    void BeltGroupRemoveClientRpc(ulong netObjID)
    {
        NetworkObject netObj = FindNetworkObj(netObjID);
        netObj.TryGetComponent(out BeltGroupMgr beltGroupMgr);
        netBeltGroupMgrs.Remove(beltGroupMgr);
    }

    public ulong FindNetObjID(GameObject obj)
    {
        ulong ObjID = 0;

        if (obj.GetComponent<Portal>())
        {
            foreach (Portal networkObject in netPortals)
            {
                if (obj == networkObject.gameObject)
                {
                    ObjID = networkObject.GetComponent<NetworkObject>().NetworkObjectId;
                    return ObjID;
                }
            }
        }
        else if (obj.GetComponent<Structure>() && !obj.GetComponent<BeltCtrl>())
        {
            foreach (Structure networkObject in netStructures)
            {
                if (obj == networkObject.gameObject)
                {
                    ObjID = networkObject.GetComponent<NetworkObject>().NetworkObjectId;
                    return ObjID;
                }
            }
        }
        else if (obj.GetComponent<BeltGroupMgr>())
        {
            foreach (BeltGroupMgr networkObject in netBeltGroupMgrs)
            {
                if (obj == networkObject.gameObject)
                {
                    ObjID = networkObject.GetComponent<NetworkObject>().NetworkObjectId;
                    return ObjID;
                }
            }
        }
        else if (obj.GetComponent<UnitCommonAi>())
        {
            foreach (UnitCommonAi networkObject in netUnitCommonAis)
            {
                if (obj == networkObject.gameObject)
                {
                    ObjID = networkObject.GetComponent<NetworkObject>().NetworkObjectId;
                    return ObjID;
                }
            }
        }
        else
        {
            foreach (BeltCtrl networkObject in networkBelts)
            {
                if (obj == networkObject.gameObject)
                {
                    ObjID = networkObject.GetComponent<NetworkObject>().NetworkObjectId;
                    return ObjID;
                }
            }
        }

        //foreach (GameObject networkObject in networkBelts)
        //{
        //    if(obj == networkObject)
        //    {
        //        ObjID = networkObject.GetComponent<NetworkObject>().NetworkObjectId;
        //        return ObjID;
        //    }
        //}

        return ObjID;
    }

    public NetworkObject FindNetworkObj(ulong netObjID)
    {
        NetworkObject netObj = null;

        foreach (Portal networkObjects in netPortals)
        {
            if (networkObjects.TryGetComponent(out NetworkObject networkObject) && networkObject.NetworkObjectId == netObjID)
            {
                netObj = networkObject;
                return netObj;
            }
        }
        foreach (Structure networkObjects in netStructures)
        {
            if (networkObjects.TryGetComponent(out NetworkObject networkObject) && networkObject.NetworkObjectId == netObjID)
            {
                netObj = networkObject;
                return netObj;
            }
        }
        foreach (BeltGroupMgr networkObjects in netBeltGroupMgrs)
        {
            if (networkObjects.TryGetComponent(out NetworkObject networkObject) && networkObject.NetworkObjectId == netObjID)
            {
                netObj = networkObject;
                return netObj;
            }
        }
        foreach (UnitCommonAi networkObjects in netUnitCommonAis)
        {
            if (networkObjects.TryGetComponent(out NetworkObject networkObject) && networkObject.NetworkObjectId == netObjID)
            {
                netObj = networkObject;
                return netObj;
            }
        }
        foreach (BeltCtrl networkObjects in networkBelts)
        {        
            if (networkObjects.TryGetComponent(out NetworkObject networkObject) && networkObject.NetworkObjectId == netObjID)
            {
                netObj = networkObject;
                return netObj;
            }
        }

        return netObj;
    }
}
