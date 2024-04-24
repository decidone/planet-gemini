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

    public BeltManager beltManager;

    public List<BeltCtrl> beltList = new List<BeltCtrl>();
    public List<ItemProps> groupItem = new List<ItemProps>();

    public GameObject nextObj = null;
    public GameObject preObj = null;

    public bool nextCheck = true;
    public bool preCheck = true;

    public bool isSetBuildingOk = false;

    [HideInInspector]
    public NetworkObjManager networkObjManager;

    void Start()
    {
        networkObjManager = NetworkObjManager.instance;
    }

    void Update()
    {
        if (!IsServer)
            return;

        if (isSetBuildingOk && beltList.Count > 0)
        {
            if (nextCheck)
            {
                nextObj = NextObjCheck();
                if (!nextCheck)
                {
                    var objID = networkObjManager.FindNetObjID(nextObj);
                    NearObjSetClientRpc(objID, true);
                }
            }
            if (preCheck)
            {
                preObj = PreObjCheck();
                if (! preCheck)
                {
                    var objID = networkObjManager.FindNetObjID(preObj);
                    NearObjSetClientRpc(objID, false);                    
                }
            }
        }
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        ClientConnectSyncServerRpc();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        NetworkObjManager.instance.NetObjAdd(gameObject);
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        }
    }

    public override void OnNetworkDespawn()
    {
        // For this example, we always want to invoke the base
        base.OnNetworkDespawn();

        // Whether server or not, unregister this.
        NetworkManager.OnClientConnectedCallback -= OnClientConnectedCallback;
    }

    [ServerRpc(RequireOwnership = false)]
    public virtual void ClientConnectSyncServerRpc()
    {
        for(int i = 0; i < beltList.Count; i ++)
        {            
            ulong mainId = beltList[i].GetComponent<NetworkObject>().NetworkObjectId;

            if (i >= 1)
            {
                ulong preId = beltList[i - 1].GetComponent<NetworkObject>().NetworkObjectId;
                PreBeltSetClientRpc(mainId, preId);
            }           
            if (i < beltList.Count - 1)
            {
                ulong nextId = beltList[i + 1].GetComponent<NetworkObject>().NetworkObjectId;
                NextBeltSetClientRpc(mainId, nextId);
            }
        }

        ClientConnectSyncClientRpc(isSetBuildingOk);

        if(nextObj)
        {
            var objID = networkObjManager.FindNetObjID(nextObj);
            NearObjSetClientRpc(objID, true);
        }
        else if (preObj)
        {
            var objID = networkObjManager.FindNetObjID(preObj);
            NearObjSetClientRpc(objID, false);
        }
    }

    [ClientRpc]
    public virtual void ClientConnectSyncClientRpc(bool syncSetBuilding)
    {
        if (IsServer)
            return;

        isSetBuildingOk = syncSetBuilding;
        //NetworkObjManager.instance.NetObjAdd(gameObject);
        //ConnecTimeStop.instance.RemoveNetObj(gameObject);
    }

    public void SetBelt(GameObject belt, int level, int beltDir, int objHeight, int objWidth, bool isHostMap)
    {
        //GameObject belt = Instantiate(beltObj, this.transform.position, Quaternion.identity);
        //belt.TryGetComponent(out NetworkObject netObj);
        //if (!netObj.IsSpawned) belt.GetComponent<NetworkObject>().Spawn();
        belt.transform.parent = this.transform;
        BeltCtrl beltCtrl = belt.GetComponent<BeltCtrl>();
        beltList.Add(beltCtrl);
        //BeltListAddClientRpc(netObj.NetworkObjectId);
        beltCtrl.SettingClientRpc(level, beltDir, objHeight, objWidth, isHostMap);
        //NetworkObjManager.instance.NetObjAdd(gameObject);
        //NetObjAddClientRpc();
    }

    public void SetBeltData()
    {
        Reconfirm();
        for (int i = 0; i < beltList.Count; i++)
        {
            ulong mainId = beltList[i].GetComponent<NetworkObject>().NetworkObjectId;

            if (i >= 1)
            {
                ulong preId = beltList[i - 1].GetComponent<NetworkObject>().NetworkObjectId;
                PreBeltSetClientRpc(mainId, preId);
            }
            if (i < beltList.Count - 1)
            {
                ulong nextId = beltList[i + 1].GetComponent<NetworkObject>().NetworkObjectId;
                NextBeltSetClientRpc(mainId, nextId);
            }
        }
    }


    //[ClientRpc]
    //public void NetObjAddClientRpc()
    //{
    //    NetworkObjManager.instance.NetObjAdd(gameObject);
    //}

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

            if ((collider.CompareTag("Factory") || collider.CompareTag("Tower")) && collider.GetComponent<Structure>().isSetBuildingOk &&
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

                    return collider.gameObject;
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
        if (otherBelt.beltGroupMgr != null && this != otherBelt.beltGroupMgr)
        {        
            if (isNextFind)
            {
                if (otherBelt.beltState == BeltState.StartBelt || otherBelt.beltState == BeltState.SoloBelt)
                {
                    if (belt.dirNum == otherBelt.dirNum)                
                        CombineFunc(belt, otherBelt, isNextFind);
                
                    else if (belt.dirNum != otherBelt.dirNum)
                    {
                        if (belt.dirNum % 2 == 0)
                        {
                            if (otherBelt.dirNum % 2 == 1)                        
                                CombineFunc(belt, otherBelt, isNextFind);                        
                            else
                                return;
                        }
                        else if (belt.dirNum % 2 == 1)
                        {
                            if (otherBelt.dirNum % 2 == 0)                        
                                CombineFunc(belt, otherBelt, isNextFind);                        
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
                                    CombineFunc(belt, otherBelt, isNextFind);
                                else
                                    return;
                            }
                            else if (belt.dirNum % 2 == 1)
                            {
                                if (otherBelt.dirNum % 2 == 0)
                                    CombineFunc(belt, otherBelt, isNextFind);
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
                                    CombineFunc(belt, otherBelt, isNextFind);
                                else
                                    return;
                            }
                            else if (belt.dirNum % 2 == 1)
                            {
                                if (otherBelt.dirNum % 2 == 0)
                                    CombineFunc(belt, otherBelt, isNextFind);
                                else
                                    return;
                            }
                        }
                    }
                }
            }
        }
    }

    void CombineFunc(BeltCtrl belt, BeltCtrl otherBelt, bool isNextFind)
    {
        BeltManager beltManager = this.GetComponentInParent<BeltManager>();
        if (isNextFind)
        {
            beltManager.BeltCombine(this, otherBelt.beltGroupMgr);
            ulong thisId = belt.GetComponent<NetworkObject>().NetworkObjectId;
            ulong othId = otherBelt.GetComponent<NetworkObject>().NetworkObjectId;
            NextBeltSetClientRpc(thisId, othId);
            PreBeltSetClientRpc(othId, thisId);

            otherBelt.BeltModelSet();
        }
        else
        {
            otherBelt.beltGroupMgr.CombineFuncOthGroupMgr(this, belt, otherBelt);
        }
    }

    public void CombineFuncOthGroupMgr(BeltGroupMgr beltGroupMgr, BeltCtrl belt, BeltCtrl otherBelt)
    {
        BeltManager beltManager = this.GetComponentInParent<BeltManager>();

        beltManager.BeltCombine(this, beltGroupMgr);
        ulong thisId = belt.GetComponent<NetworkObject>().NetworkObjectId;
        ulong othId = otherBelt.GetComponent<NetworkObject>().NetworkObjectId;
        PreBeltSetClientRpc(thisId, othId);
        NextBeltSetClientRpc(othId, thisId);

        otherBelt.BeltDirSetClientRpc(belt.dirNum);
        otherBelt.BeltModelSet();
    }

    [ClientRpc]
    public void NextBeltSetClientRpc(ulong thisBeltID, ulong othBeltID, ClientRpcParams rpcParams = default)
    {
        var BeltCtrlArr = GetComponentsInChildren<BeltCtrl>();
        BeltCtrl thisBelt = null;
        BeltCtrl othBelt = null;
        foreach (var beltCtrl in BeltCtrlArr)
        {
            if (beltCtrl.GetComponent<NetworkObject>().NetworkObjectId == thisBeltID)
            {
                thisBelt = beltCtrl;
            }
            if (beltCtrl.GetComponent<NetworkObject>().NetworkObjectId == othBeltID)
            {
                othBelt = beltCtrl;
            }
        }

        thisBelt.nextBelt = othBelt;
    }


    [ClientRpc]
    public void PreBeltSetClientRpc(ulong thisBeltID, ulong othBeltID, ClientRpcParams rpcParams = default)
    {
        var BeltCtrlArr = GetComponentsInChildren<BeltCtrl>();
        BeltCtrl thisBelt = null;
        BeltCtrl othBelt = null;

        foreach (var beltCtrl in BeltCtrlArr)
        {
            if (beltCtrl.GetComponent<NetworkObject>().NetworkObjectId == thisBeltID)
            {
                thisBelt = beltCtrl;
            }
            if (beltCtrl.GetComponent<NetworkObject>().NetworkObjectId == othBeltID)
            {
                othBelt = beltCtrl;
            }
        }

        thisBelt.preBelt = othBelt;
    }

    [ClientRpc]
    public void NearObjSetClientRpc(ulong ObjID, bool isNextObj, ClientRpcParams rpcParams = default)
    {
        if (IsServer)
            return;

        NetworkObject obj = NetworkObjManager.instance.FindNetworkObj(ObjID);

        if (!obj)
            return;

        if (isNextObj)
        {
            nextObj = obj.gameObject;
        }
        else
        {
            preObj = obj.gameObject;
        }
    }

    public ulong BeltFindId(BeltCtrl belt)
    {
        BeltCtrl FindBelt = null;

        foreach (BeltCtrl beltCtrl in beltList)
        {
            if(beltCtrl == belt)
            {
                FindBelt = belt;
            }
        }

        return FindBelt.GetComponent<NetworkObject>().NetworkObjectId;
    }
}