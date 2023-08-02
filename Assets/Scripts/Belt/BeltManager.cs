using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeltManager : MonoBehaviour
{
    public void BeltCombine(BeltGroupMgr fstGroupMgr, BeltGroupMgr secGroupMgr)
    {
        fstGroupMgr.beltList.AddRange(secGroupMgr.beltList);

        foreach (BeltCtrl belt in secGroupMgr.beltList)
        {
            belt.transform.parent = fstGroupMgr.transform;
            belt.beltGroupMgr = fstGroupMgr;
        }
        foreach (ItemProps item in secGroupMgr.groupItem)
        {
            fstGroupMgr.groupItem.Add(item);
        }

        fstGroupMgr.Reconfirm();
        if (secGroupMgr.nextObj != null)
            fstGroupMgr.nextObj = secGroupMgr.nextObj;
        Destroy(secGroupMgr.gameObject);
    }
}
