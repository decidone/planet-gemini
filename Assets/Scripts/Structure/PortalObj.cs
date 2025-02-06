using Unity.Netcode;
using UnityEngine;

public class PortalObj : Production
{
    // Start is called before the first frame update
    public Portal myPortal;

    [ServerRpc]
    public virtual void ConnectObjServerRpc(ulong objId) { }
    [ClientRpc]
    public virtual void ConnectObjClientRpc(ulong objId) { }
    [ServerRpc]
    public virtual void ConnectMyObjServerRpc(ulong objId) { }
    [ClientRpc]
    public virtual void ConnectMyObjClientRpc(ulong objId) { }

    protected override void OnClientConnectedCallback(ulong clientId)
    {
        base.OnClientConnectedCallback(clientId);
        PortalObjConnectServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    protected virtual void PortalObjConnectServerRpc() { PortalObjConnectClientRpc(transform.position); }

    [ClientRpc]
    protected virtual void PortalObjConnectClientRpc(Vector3 tr)
    {
        transform.position = tr;
    }

    public virtual void RemovePortalData()
    {
        if (myPortal == null)
            myPortal = GetComponentInParent<Portal>();
        myPortal.RemovePortalObj(buildName);
    }

    public override bool CanTakeItem(Item item)
    {
        return false;
    }
}
