using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkObjManager : NetworkBehaviour
{
    public List<NetworkObject> networkObjects = new List<NetworkObject>();

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

    public void NetObjAdd(NetworkObject netObj)
    {
        networkObjects.Add(netObj);
    }

    public void NetObjRemove(NetworkObject netObj)
    {
        networkObjects.Remove(netObj);
    }

    public ulong FindNetObjID(GameObject obj)
    {
        ulong ObjID = 0;

        foreach (NetworkObject networkObject in networkObjects)
        {
            if(obj.GetComponent<NetworkObject>() == networkObject)
            {
                ObjID = networkObject.NetworkObjectId;
                break;
            }
        }

        return ObjID;
    }

    public NetworkObject FindNetworkObj(ulong netObjID)
    {
        NetworkObject netObj = null;

        foreach (NetworkObject networkObjects in networkObjects)
        {
            if (networkObjects.NetworkObjectId == netObjID)
            {
                netObj = networkObjects;
                break;
            }
        }

        return netObj;
    }
}
