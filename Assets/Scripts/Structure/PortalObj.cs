using Unity.Netcode;
using UnityEngine;

public class PortalObj : Production
{
    // Start is called before the first frame update
    public Portal myPortal;

    [ServerRpc]
    public virtual void ConnectObjServerRpc(NetworkObjectReference networkObjectReference) { }
    [ClientRpc]
    public virtual void ConnectObjClientRpc(NetworkObjectReference networkObjectReference) { }
    [ServerRpc]
    public virtual void ConnectMyObjServerRpc(NetworkObjectReference networkObjectReference) { }
    [ClientRpc]
    public virtual void ConnectMyObjClientRpc(NetworkObjectReference networkObjectReference) { }

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

    [ClientRpc]
    public override void RemoveObjClientRpc()
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
                if (nearObj[i].TryGet(out BeltCtrl belt))
                {
                    BeltGroupMgr beltGroup = belt.beltGroupMgr;
                    beltGroup.nextCheck = true;
                    beltGroup.preCheck = true;
                }
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
