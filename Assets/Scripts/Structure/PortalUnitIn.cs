using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class PortalUnitIn : PortalObj
{
    public PortalUnitOut portalUnitOut;
    public PortalUnitOut myPortalUnitOut;
    public Vector2[] nearPos = new Vector2[8];

    protected override void Start()
    {
        base.Start();
        isPortalBuild = true;
        isStorageBuilding = true;
    }

    protected override void PortalObjConnectServer()
    {
        //base.PortalObjConnectServerRpc();
        PortalObjConnectClientRpc(transform.position);

        if (portalUnitOut != null)
        {
            ConnectObjClientRpc(portalUnitOut.NetworkObject);
        }
        if (myPortalUnitOut != null)
        {
            ConnectMyObjClientRpc(myPortalUnitOut.NetworkObject);
        }
    }

    public override void ConnectObj(NetworkObjectReference networkObjectReference)
    {
        networkObjectReference.TryGet(out NetworkObject networkObject);
        portalUnitOut = networkObject.GetComponent<PortalUnitOut>();
    }

    public override void ConnectMyObj(NetworkObjectReference networkObjectReference)
    {
        networkObjectReference.TryGet(out NetworkObject networkObject);
        myPortalUnitOut = networkObject.GetComponent<PortalUnitOut>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer)
            return;

        if (portalUnitOut && collision.collider.TryGetComponent(out UnitAi unitAi) && unitAi.playerUnitPortalIn)
        {
            portalUnitOut.SpawnUnitCheck(unitAi.gameObject);
        }
    }
}
