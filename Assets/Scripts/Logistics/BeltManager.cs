using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

// UTF-8 설정
public class BeltManager : NetworkBehaviour
{
    [SerializeField]
    GameObject beltGroupMgrObj;

    [SerializeField]
    Animator[] beltAnimators;
    public static BeltManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        for (int i = 0; i < beltAnimators.Length; i++)
        {
            beltAnimators[i].SetFloat("Level", i);
        }
    }

    public Animator AnimSync(int level)
    {
        return beltAnimators[level];
    }

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
            ulong objID = NetworkObjManager.instance.FindNetObjID(secGroupMgr.nextObj);
            fstGroupMgr.NearObjSetClientRpc(objID, true);
        }

        NetworkObjManager.instance.NetObjRemove(secGroupMgr.gameObject);
        NetworkObject destroyObj = secGroupMgr.GetComponent<NetworkObject>();
        if (destroyObj != null && destroyObj.IsSpawned)
        {
            destroyObj.Despawn();
        }
        Destroy(secGroupMgr.gameObject);
    }

    public bool BeltDivide(BeltGroupMgr beltGroup, GameObject removeBelt)
    {
        if (beltGroup.beltList.Count <= 1)
        {
            beltGroup.beltList[0].transform.parent = null;
            NetworkObjManager.instance.NetObjRemove(beltGroup.gameObject);
            NetworkObject destroyObj = beltGroup.GetComponent<NetworkObject>();
            if (destroyObj != null && destroyObj.IsSpawned)
            {
                destroyObj.Despawn();
            }
            Destroy(beltGroup.gameObject);
            return false;
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
        return true;
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
        beltGroup.preCheck = true;

        beltGroup.beltList[0].preBelt = null;


        if (beltGroup.beltList.Count == 1)
        {
            beltGroup.beltList[0].BeltStateSetClientRpc((int)BeltState.SoloBelt);
            beltGroup.beltList[0].FactoryModelSet();
        }
    }

    private void CreateNewBeltGroup(List<BeltCtrl> beltList)
    {
        GameObject newObj = Instantiate(beltGroupMgrObj);
        newObj.TryGetComponent(out NetworkObject netObj);
        if (!netObj.IsSpawned) newObj.GetComponent<NetworkObject>().Spawn(true);

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
        newBeltGroup.preCheck = true;

        newBeltGroup.beltList[newBeltGroup.beltList.Count - 1].nextBelt = null;

        if (newBeltGroup.beltList.Count == 1)
        {
            newBeltGroup.beltList[0].BeltStateSetClientRpc((int)BeltState.SoloBelt);
            newBeltGroup.beltList[0].FactoryModelSet();
        }
    }
}
