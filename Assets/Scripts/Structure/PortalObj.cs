using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PortalObj : Production
{
    // Start is called before the first frame update
    public Portal myPortal;
    public virtual void ConnectObj(GameObject othObj) { }
    public virtual void ConnectMyObj(GameObject othObj) { }

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

    public override (Item, int) QuickPullOut()
    {
        return (null, 0);
    }
}
