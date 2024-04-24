using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ConnecTimeStop : NetworkBehaviour
{
    List<GameObject> NetObjList = new List<GameObject>();
    public bool clientConnect;
    public bool connEnd;

    #region Singleton
    public static ConnecTimeStop instance;

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

    // Start is called before the first frame update
    void Start()
    {
        clientConnect = false;
        connEnd = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    [ServerRpc(RequireOwnership = false)]
    public void ClientConnServerRpc()
    {
        ClientConnClientRpc();
    }

    [ClientRpc]
    void ClientConnClientRpc()
    {
        clientConnect = true;
    }

    public void AddNetObj(GameObject obj)
    {
        //if (!clientConnect)
        //    return;

        //NetObjList.Add(obj);
        //if (NetObjList.Count > 0)
        //{
        //    TimeScaleSetServerRpc(0);
        //}
    }

    public void RemoveNetObj(GameObject obj)
    {
        //NetObjList.Remove(obj);
        //if (NetObjList.Count == 0)
        //{
        //    TimeScaleSetServerRpc(1);
        //    clientConnect = false;
        //    connEnd = true;
        //}
    }

    [ServerRpc(RequireOwnership = false)]
    void TimeScaleSetServerRpc(int scale)
    {
        TimeScaleSetClientRpc(scale);
    }

    [ClientRpc]
    void TimeScaleSetClientRpc(int scale)
    {
        Time.timeScale = scale;
        Debug.Log("Time.timeScale == " + scale);
    }
}
