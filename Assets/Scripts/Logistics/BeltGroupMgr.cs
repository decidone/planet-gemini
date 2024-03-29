using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Pathfinding;

// UTF-8 설정
public class BeltGroupMgr : NetworkBehaviour
{
    [SerializeField]
    GameObject beltObj;

    public List<BeltCtrl> beltList = new List<BeltCtrl>();
    public List<ItemProps> groupItem = new List<ItemProps>();

    public GameObject nextObj = null;
    public GameObject preObj = null;

    public bool nextCheck = true;
    public bool preCheck = true;

    public bool isSetBuildingOk = false;

    NetworkObject netBelt;

    void Update()
    {
        if (IsServer && isSetBuildingOk)
        {
            if(nextCheck)
            {
                if(beltList.Count > 0)
                    nextObj = NextObjCheck();
            }
            if (preCheck)
            {
                if (beltList.Count > 0)
                    preObj = PreObjCheck();
            }
        }
    }

    public void SetBelt(int level, int beltDir)
    {
        GameObject belt = Instantiate(beltObj, this.transform.position, Quaternion.identity);
        belt.TryGetComponent(out NetworkObject netObj);
        netBelt = netObj;
        if (!netObj.IsSpawned) belt.GetComponent<NetworkObject>().Spawn();
        belt.transform.parent = this.transform;
        BeltCtrl beltCtrl = netObj.GetComponent<BeltCtrl>();
        beltList.Add(beltCtrl);
        beltCtrl.SettingClientRpc(level, beltDir);
    }

    //public void SetBelt(int beltDir, int level, int height, int width, int dirCount)
    //{
    //    GameObject belt = Instantiate(beltObj, this.transform.position, Quaternion.identity);
    //    belt.transform.parent = this.transform;
    //    BeltCtrl beltCtrl = belt.GetComponent<BeltCtrl>();
    //    beltCtrl.beltGroupMgr = this.GetComponent<BeltGroupMgr>();
    //    beltList.Add(beltCtrl);
    //    beltCtrl.dirNum = beltDir;
    //    beltCtrl.beltState = BeltState.SoloBelt;
    //    beltCtrl.BuildingSetting(level, height, width, dirCount);
    //}

    private GameObject PreObjCheck()
    {
        var Check = -transform.up;

        BeltCtrl belt = beltList[0].GetComponent<BeltCtrl>();
        if (belt.dirNum == 0)
        {
            Check = -belt.transform.up;
        }
        else if (belt.dirNum == 1)
        {
            Check = -belt.transform.right;
        }
        else if (belt.dirNum == 2)
        {
            Check = belt.transform.up;
        }
        else if (belt.dirNum == 3)
        {
            Check = belt.transform.right;
        }

        RaycastHit2D[] raycastHits = Physics2D.RaycastAll(belt.transform.position, Check, 1f);

        for (int a = 0; a < raycastHits.Length; a++)
        {
            Collider2D collider = raycastHits[a].collider;

            if (collider.CompareTag("Factory") && collider.GetComponent<Structure>().isSetBuildingOk &&
                collider.GetComponent<BeltCtrl>() != belt)
            {
                if (collider.TryGetComponent(out BeltCtrl otherBelt))
                {
                    CheckGroup(belt, otherBelt, false);
                }
                else
                {
                    preCheck = false;
                }

                return collider.gameObject;
            }
        }

        return null;
    }

    void BeltModelSet(BeltCtrl preBelt, BeltCtrl nextBelt)
    {
        if(preBelt == beltList[0])
        {
            //preBelt.beltState = BeltState.StartBelt;
            preBelt.BeltStateSetClientRpc((int)BeltState.StartBelt);
        }
        else if (preBelt != beltList[0])
        {
            //preBelt.beltState = BeltState.RepeaterBelt;
            preBelt.BeltStateSetClientRpc((int)BeltState.RepeaterBelt);
        }

        nextBelt.BeltStateSetClientRpc((int)BeltState.EndBelt);
        //nextBelt.beltState = BeltState.EndBelt;
    }

    //벨트 그룹 병합
    public void Reconfirm()
    {
        groupItem.Clear();

        int index = 0;
        foreach(BeltCtrl belt in beltList)
        {
            foreach (ItemProps item in belt.itemObjList)
            {
                groupItem.Add(item);
            }
            if (beltList.Count - 1 > index)
            {
                BeltModelSet(belt, beltList[index + 1]);
                index++;
            }
            else
                return;
        }
    }

    private GameObject NextObjCheck()
    {
        var Check = transform.up;

        BeltCtrl belt = beltList[beltList.Count - 1].GetComponent<BeltCtrl>();
        if (belt.dirNum == 0)
        {
            Check = belt.transform.up;
        }
        else if (belt.dirNum == 1)
        {
            Check = belt.transform.right;
        }
        else if (belt.dirNum == 2)
        {
            Check = -belt.transform.up;
        }
        else if (belt.dirNum == 3)
        {
            Check = -belt.transform.right;
        }

        RaycastHit2D[] raycastHits = Physics2D.RaycastAll(belt.transform.position, Check, 1f);

        for (int a = 0; a < raycastHits.Length; a++)
        {
            Collider2D collider = raycastHits[a].collider;

            if ((collider.CompareTag("Factory") || collider.CompareTag("Tower")) && collider.GetComponent<Structure>().isSetBuildingOk && 
                collider.GetComponent<BeltCtrl>() != belt)
            {
                if (collider.TryGetComponent(out BeltCtrl otherBelt))
                {
                    CheckGroup(belt, otherBelt, true);
                    if (otherBelt.beltGroupMgr.nextObj != null)
                    {
                        return otherBelt.beltGroupMgr.nextObj;
                    }
                }
                else
                {
                    nextCheck = false;
                    return collider.gameObject;
                }
            }
        }

        return null;
    }

    void CheckGroup(BeltCtrl belt, BeltCtrl otherBelt, bool isNextFind)
    {
        BeltGroupMgr beltGroupMgr = this.GetComponent<BeltGroupMgr>();
        if (otherBelt.beltGroupMgr != null && beltGroupMgr != otherBelt.beltGroupMgr)
        {        
            if (isNextFind)
            {
                if (otherBelt.beltState == BeltState.StartBelt || otherBelt.beltState == BeltState.SoloBelt)
                {
                    if (belt.dirNum == otherBelt.dirNum)                
                        CombineFunc(beltGroupMgr, belt, otherBelt, isNextFind);
                
                    else if (belt.dirNum != otherBelt.dirNum)
                    {
                        if (belt.dirNum % 2 == 0)
                        {
                            if (otherBelt.dirNum % 2 == 1)                        
                                CombineFunc(beltGroupMgr, belt, otherBelt, isNextFind);                        
                            else
                                return;
                        }
                        else if (belt.dirNum % 2 == 1)
                        {
                            if (otherBelt.dirNum % 2 == 0)                        
                                CombineFunc(beltGroupMgr, belt, otherBelt, isNextFind);                        
                            else
                                return;
                        }
                    }
                }
            }
            else
            {
                if(otherBelt.beltState == BeltState.EndBelt || otherBelt.beltState == BeltState.SoloBelt)
                {
                    if (otherBelt.beltGroupMgr.nextObj == null)
                    {                        
                        if (belt.dirNum != otherBelt.dirNum)
                        {
                            if (belt.dirNum % 2 == 0)
                            {
                                if (otherBelt.dirNum % 2 == 1)
                                    CombineFunc(beltGroupMgr, belt, otherBelt, isNextFind);
                                else
                                    return;
                            }
                            else if (belt.dirNum % 2 == 1)
                            {
                                if (otherBelt.dirNum % 2 == 0)
                                    CombineFunc(beltGroupMgr, belt, otherBelt, isNextFind);
                                else
                                    return;
                            }
                        }
                    }
                    else if (otherBelt.beltGroupMgr.nextObj != null && otherBelt.beltGroupMgr.nextObj.GetComponent<BeltCtrl>() != null)
                    {
                        if (belt.dirNum != otherBelt.dirNum)
                        {
                            if (belt.dirNum % 2 == 0)
                            {
                                if (otherBelt.dirNum % 2 == 1)
                                    CombineFunc(beltGroupMgr, belt, otherBelt, isNextFind);
                                else
                                    return;
                            }
                            else if (belt.dirNum % 2 == 1)
                            {
                                if (otherBelt.dirNum % 2 == 0)
                                    CombineFunc(beltGroupMgr, belt, otherBelt, isNextFind);
                                else
                                    return;
                            }
                        }
                    }
                }
            }
        }
    }

    void CombineFunc(BeltGroupMgr beltGroupMgr, BeltCtrl belt, BeltCtrl otherBelt, bool isNextFind)
    {
        BeltManager beltManager = this.GetComponentInParent<BeltManager>();

        if (isNextFind)
        {
            beltManager.BeltCombine(beltGroupMgr, otherBelt.beltGroupMgr);
            belt.nextBelt = otherBelt;
            otherBelt.preBelt = belt;
            otherBelt.BeltModelSet();
        }
        else
        {
            beltManager.BeltCombine(otherBelt.beltGroupMgr, beltGroupMgr);
            belt.preBelt = otherBelt;
            otherBelt.nextBelt = belt;
            //int tempDir = otherBelt.dirNum;
            otherBelt.BeltDirSetClientRpc(belt.dirNum);
            //otherBelt.dirNum = belt.dirNum;
            otherBelt.BeltModelSet();
        }
    }
}