using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

// UTF-8 설정
public class BeltManager : NetworkBehaviour
{
    [SerializeField]
    GameObject beltGroupMgrObj;

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
        {
            fstGroupMgr.nextObj = secGroupMgr.nextObj;

            ulong objID = secGroupMgr.networkObjManager.FindNetObjID(secGroupMgr.nextObj);
            fstGroupMgr.NearObjSetClientRpc(objID, true);
        }

        NetworkObjManager.instance.NetObjRemove(secGroupMgr.GetComponent<NetworkObject>());
        Destroy(secGroupMgr.gameObject);
    }

    public void BeltDivide(BeltGroupMgr beltGroup, GameObject removeBelt)
    {
        if (beltGroup.beltList.Count <= 1)
        {
            NetworkObjManager.instance.NetObjRemove(beltGroup.GetComponent<NetworkObject>());
            Destroy(beltGroup.gameObject);
            return;
        }

        List<BeltCtrl> groupAList = new List<BeltCtrl>();
        List<BeltCtrl> groupBList = new List<BeltCtrl>();
        bool isRemoveObj = false;

        foreach (BeltCtrl belt in beltGroup.beltList)
        {
            if (removeBelt == belt.gameObject)
            {
                isRemoveObj = true;
                continue;
            }

            if (isRemoveObj)
                groupAList.Add(belt);
            else
                groupBList.Add(belt);
        }

        if (groupAList.Count == 0)
        {
            if (groupBList.Count > 0)
            {
                UpdateBeltGroup(beltGroup, groupBList);
            }
        }
        else if (groupAList.Count > 0)
        {
            UpdateBeltGroup(beltGroup, groupAList);

            if (groupBList.Count > 0)
            {
                CreateNewBeltGroup(groupBList);
            }
        }
    }

    private void UpdateBeltGroup(BeltGroupMgr beltGroup, List<BeltCtrl> beltList)
    {
        beltGroup.beltList.Clear();
        foreach (BeltCtrl belt in beltList)
        {
            beltGroup.beltList.Add(belt);
            belt.BeltModelSet();
        }
        beltGroup.Reconfirm();
        beltGroup.nextCheck = true;

        if (beltGroup.beltList.Count == 1)
        {
            beltGroup.beltList[0].beltState = BeltState.SoloBelt;
            beltGroup.beltList[0].FactoryModelSet();
        }
    }

    private void CreateNewBeltGroup(List<BeltCtrl> beltList)
    {
        GameObject newObj = Instantiate(beltGroupMgrObj);
        newObj.transform.parent = this.gameObject.transform;

        BeltGroupMgr newBeltGroup = newObj.GetComponent<BeltGroupMgr>();
        foreach (BeltCtrl belt in beltList)
        {
            belt.transform.parent = newBeltGroup.transform;
            belt.beltGroupMgr = newBeltGroup;
            newBeltGroup.beltList.Add(belt);
            belt.BeltModelSet();
        }
        newBeltGroup.Reconfirm();
        newBeltGroup.nextCheck = true;

        if (newBeltGroup.beltList.Count == 1)
        {
            newBeltGroup.beltList[0].beltState = BeltState.SoloBelt;
            newBeltGroup.beltList[0].FactoryModelSet();
        }
        newBeltGroup.isSetBuildingOk = true;
    }
}
