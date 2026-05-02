using Unity.Netcode;
using UnityEngine;

public class PortalObj : Production
{
    public Portal myPortal;

    [ServerRpc]
    public void ConnectObjServerRpc(NetworkObjectReference networkObjectReference)
    {
        ConnectObjClientRpc(networkObjectReference);
    }

    [ClientRpc]
    public void ConnectObjClientRpc(NetworkObjectReference networkObjectReference)
    {
        ConnectObj(networkObjectReference);
    }

    public virtual void ConnectObj(NetworkObjectReference networkObjectReference) { }

    [ServerRpc]
    public void ConnectMyObjServerRpc(NetworkObjectReference networkObjectReference)
    {
        ConnectMyObjClientRpc(networkObjectReference);
    }

    [ClientRpc]
    public void ConnectMyObjClientRpc(NetworkObjectReference networkObjectReference)
    {
        ConnectMyObj(networkObjectReference);
    }

    public virtual void ConnectMyObj(NetworkObjectReference networkObjectReference) { }

    public override void OnClientConnectedCallback()
    {
        base.OnClientConnectedCallback();
        PortalObjConnectServerRpc();
    }

    //protected override void OnClientConnectedCallback(ulong clientId)
    //{
    //    base.OnClientConnectedCallback(clientId);
    //    PortalObjConnectServerRpc();
    //}

    [ServerRpc(RequireOwnership = false)]
    protected void PortalObjConnectServerRpc()
    {
        PortalObjConnectServer();
    }

    protected virtual void PortalObjConnectServer()
    {
        PortalObjConnectClientRpc(transform.position);
    }

    [ClientRpc]
    protected void PortalObjConnectClientRpc(Vector3 tr)
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

    public override void RemoveObjClient()
    {
        StopAllCoroutines();

        if (isUIOpened)
            CloseUI();

        if (InfoUI.instance.str == this)
            InfoUI.instance.SetDefault();

        for (int i = 0; i < nearObj.Length; i++)
        {
            if (nearObj[i])
            {
                nearObj[i].ResetNearObj(this);
            }
        }

        if (overclockTower != null && TryGet(out Production prod))
            overclockTower.RemoveObjectsOutOfRange(prod);

        RemovePortalData();
       
        if (GameManager.instance.focusedStructure == this)
        {
            GameManager.instance.focusedStructure = null;
        }

        DestroyFuncServerRpc();
    }
}
