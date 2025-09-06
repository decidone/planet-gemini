using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Pathfinding;
using Unity.VisualScripting;
using Unity.Burst.Intrinsics;

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

    bool beltSyncCheck;

    void Start()
    {
        if (!beltSyncCheck && !IsServer) 
        {
            beltSyncCheck = true;
            Invoke(nameof(ClientBeltSyncFunc), 0.2f);
        }

        beltManager = BeltManager.instance;
    }

    public void BeltGroupRefresh()
    {
        if (!IsServer)
            return;

        StartCoroutine(DelayBeltGroupRefresh());
    }

    IEnumerator DelayBeltGroupRefresh()
    {
        // 동시 건설 시 근처 오브젝트를 체크하기 위한 딜레이
        yield return new WaitForEndOfFrame();

        if (beltList.Count > 0)
        {
            if (nextCheck)
            {
                nextObj = NextObjCheck();
                if (!nextCheck)
                {
                    NearObjSetClientRpc(nextObj.GetComponent<NetworkObject>(), true);
                }
            }
            if (preCheck)
            {
                preObj = PreObjCheck();
                if (!preCheck)
                {
                    NearObjSetClientRpc(preObj.GetComponent<NetworkObject>(), false);
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

        NetworkObjManager.instance.NetObjRemove(gameObject);

        // Whether server or not, unregister this.
        NetworkManager.OnClientConnectedCallback -= OnClientConnectedCallback;
    }

    [ServerRpc(RequireOwnership = false)]
    public virtual void ClientConnectSyncServerRpc()
    {
        for(int i = 0; i < beltList.Count; i ++)
        {            
            NetworkObjectReference mainId = beltList[i].GetComponent<NetworkObject>();

            if (i >= 1)
            {
                NetworkObjectReference preId = beltList[i - 1].GetComponent<NetworkObject>();
                PreBeltSetClientRpc(mainId, preId);
            }
            if (i < beltList.Count - 1)
            {
                NetworkObjectReference nextId = beltList[i + 1].GetComponent<NetworkObject>();
                NextBeltSetClientRpc(mainId, nextId);
            }
        }

        if(nextObj)
        {
            NearObjSetClientRpc(nextObj.GetComponent<NetworkObject>(), true);
        }
        else if (preObj)
        {
            NearObjSetClientRpc(preObj.GetComponent<NetworkObject>(), false);
        }
    }

    public void SetBelt(GameObject belt, int level, int beltDir, int objHeight, int objWidth, bool isHostMap, int index)
    {        
        //belt.transform.parent = this.transform;
        belt.GetComponent<NetworkObject>().TrySetParent(this.NetworkObject);
        BeltCtrl beltCtrl = belt.GetComponent<BeltCtrl>();
        beltList.Add(beltCtrl);
        beltCtrl.SettingClientRpc(level, beltDir, objHeight, objWidth, isHostMap, index);
    }

    public void SetBeltData()
    {
        Reconfirm();
        for (int i = 0; i < beltList.Count; i++)
        {
            NetworkObjectReference mainId = beltList[i].GetComponent<NetworkObject>();

            if (i >= 1)
            {
                NetworkObjectReference preId = beltList[i - 1].GetComponent<NetworkObject>();
                PreBeltSetClientRpc(mainId, preId);
            }
            if (i < beltList.Count - 1)
            {
                NetworkObjectReference nextId = beltList[i + 1].GetComponent<NetworkObject>();
                NextBeltSetClientRpc(mainId, nextId);
            }
        }

        foreach (BeltCtrl belt in beltList)
        {
            belt.AnimSyncFunc();
        }
    }

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

            if ((collider.CompareTag("Factory") || collider.CompareTag("Tower")) && collider.GetComponent<BeltCtrl>() != belt)
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
            preBelt.BeltStateSetClientRpc((int)BeltState.StartBelt);
        }
        else if (preBelt != beltList[0])
        {
            preBelt.BeltStateSetClientRpc((int)BeltState.RepeaterBelt);
        }

        nextBelt.BeltStateSetClientRpc((int)BeltState.EndBelt);
    }



    [ServerRpc(RequireOwnership = false)]
    public void ClientBeltSyncServerRpc()
    {
        ClientBeltSyncClientRpc();
    }

    [ClientRpc]
    void ClientBeltSyncClientRpc()
    {
        if (IsServer) return;
        beltSyncCheck = true;
        StartCoroutine(ClientBeltSyncCoroutine());
    }

    IEnumerator ClientBeltSyncCoroutine()
    {
        yield return null;
        yield return null;
        ClientBeltSyncFunc();
    }

    void ClientBeltSyncFunc()
    {
        int index = 0;
        BeltCtrl[] beltArr = GetComponentsInChildren<BeltCtrl>();

        beltList.Clear();
        BeltCtrl startbelt = null;
        if (beltArr.Length == 0)
        {
            return;
        }
        else if (beltArr.Length == 1)
        {
            startbelt = beltArr[0];
        }
        else
        {
            foreach (BeltCtrl belt in beltArr)
            {
                if (belt.beltState == BeltState.StartBelt)
                {
                    startbelt = belt;
                }
            }
        }

        if (startbelt)
        {
            bool isFindEndBelt = true;
            BeltCtrl belt = startbelt;
            if (!beltList.Contains(belt))
            {
                beltList.Add(belt);
            }

            while (isFindEndBelt)
            {
                belt = beltList[beltList.Count - 1];
                if (belt.beltState == BeltState.EndBelt || belt.nextBelt == null)
                {
                    isFindEndBelt = false;
                    break;
                }
                else
                {
                    belt = belt.nextBelt;
                    if (beltList.Contains(belt))
                    {
                        continue;
                    }
                    beltList.Add(belt);
                }
            }
        }

        foreach (BeltCtrl belt in beltArr)
        {
            if (beltList.Count - 1 > index)
            {
                BeltModelSet(belt, beltList[index + 1]);
                index++;
            }
            belt.beltGroupMgr = this;
            belt.AnimSyncFunc();
        }

        groupItem.Clear();

        foreach (BeltCtrl belt in beltList)
        {
            foreach (ItemProps item in belt.itemObjList)
            {
                groupItem.Add(item);
            }
        }

        beltSyncCheck = false;
    }

    public void Reconfirm()
    {
        if (!IsServer) return;
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

    public void ItemIndexSet()
    {
        for (int i = 0; i < groupItem.Count; i++)
        {
            groupItem[i].beltGroupIndex = i;
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

            if ((collider.CompareTag("Factory") || collider.CompareTag("Tower")) && collider.GetComponent<BeltCtrl>() != belt)
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
        if (!IsSpawned || !otherBelt.beltGroupMgr.IsSpawned)
            return;

        //BeltManager beltManager = this.GetComponentInParent<BeltManager>();
        if (isNextFind)
        {
            beltManager.BeltCombine(this, otherBelt.beltGroupMgr);
            //otherBelt.beltGroupMgr.ClientBeltSyncServerRpc();
            NetworkObjectReference thisId = belt.GetComponent<NetworkObject>();
            NetworkObjectReference othId = otherBelt.GetComponent<NetworkObject>();
            NextBeltSetClientRpc(thisId, othId);
            PreBeltSetClientRpc(othId, thisId);

            otherBelt.BeltModelSet();
        }
        else
        {
            otherBelt.beltGroupMgr.CombineFuncOthGroupMgr(this, belt, otherBelt);
        }

        belt.DelayNearStrBuilt();
        otherBelt.DelayNearStrBuilt();
    }

    public void CombineFuncOthGroupMgr(BeltGroupMgr beltGroupMgr, BeltCtrl belt, BeltCtrl otherBelt)
    {
        //BeltManager beltManager = this.GetComponentInParent<BeltManager>();

        beltManager.BeltCombine(this, beltGroupMgr);
        NetworkObjectReference thisId = belt.GetComponent<NetworkObject>();
        NetworkObjectReference othId = otherBelt.GetComponent<NetworkObject>();
        PreBeltSetClientRpc(thisId, othId);
        NextBeltSetClientRpc(othId, thisId);
        otherBelt.dirNum = belt.dirNum;
        belt.BeltModelSet();
        otherBelt.BeltModelSet();
        otherBelt.BeltDirSetServerRpc();
        //ClientBeltSyncServerRpc();
    }

    [ClientRpc]
    public void NextBeltSetClientRpc(NetworkObjectReference thisBeltID, NetworkObjectReference othBeltID, ClientRpcParams rpcParams = default)
    {
        var BeltCtrlArr = GetComponentsInChildren<BeltCtrl>();
        BeltCtrl thisBelt = thisBeltID.TryGet(out NetworkObject thisBeltObj) ? thisBeltObj.GetComponent<BeltCtrl>() : null;
        BeltCtrl othBelt = othBeltID.TryGet(out NetworkObject othBeltObj) ? othBeltObj.GetComponent<BeltCtrl>() : null;
        thisBelt.nextBelt = othBelt;
    }

    [ClientRpc]
    public void PreBeltSetClientRpc(NetworkObjectReference thisBeltID, NetworkObjectReference othBeltID, ClientRpcParams rpcParams = default)
    {
        var BeltCtrlArr = GetComponentsInChildren<BeltCtrl>();
        BeltCtrl thisBelt = thisBeltID.TryGet(out NetworkObject thisBeltObj) ? thisBeltObj.GetComponent<BeltCtrl>() : null;
        BeltCtrl othBelt = othBeltID.TryGet(out NetworkObject othBeltObj) ? othBeltObj.GetComponent<BeltCtrl>() : null;
        thisBelt.preBelt = othBelt;
    }

    [ClientRpc]
    public void NearObjSetClientRpc(NetworkObjectReference networkObjectReference, bool isNextObj, ClientRpcParams rpcParams = default)
    {
        if (IsServer)
            return;
        networkObjectReference.TryGet(out NetworkObject obj);

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

    public BeltGroupSaveData SaveData()
    {
        BeltGroupSaveData data = new BeltGroupSaveData();

        foreach (BeltCtrl beltCtrl in beltList)
        {
            BeltSaveData beltData = new BeltSaveData();
            beltData = beltCtrl.BeltSaveData();
            StructureSaveData structureData = new StructureSaveData();
            structureData = beltCtrl.SaveData();
            data.beltList.Add((beltData, structureData));
        }

        return data;
    }

    public void ItemRoot(ItemProps item)
    {
        if (groupItem.Contains(item))
        {
            groupItem.Remove(item);
        }
    }

    public void GroupItemLoot(BeltCtrl belt ,int beltGroupIndex, bool isServer)
    {
        int index = beltList.IndexOf(belt);
        GroupItemLootServerRpc(index, beltGroupIndex, isServer);
    }

    [ServerRpc(RequireOwnership = false)]
    public void GroupItemLootServerRpc(int beltIndex, int beltGroupIndex, bool isServer)
    {
        GroupItemLootClientRpc(beltIndex, beltGroupIndex, isServer);
    }

    [ClientRpc]
    public void GroupItemLootClientRpc(int beltIndex, int beltGroupIndex, bool isServer)
    {
        ItemProps findItemProps = null;

        foreach (ItemProps itemProps in beltList[beltIndex].itemObjList)
        {
            if (itemProps.beltGroupIndex == beltGroupIndex)
            {
                findItemProps = itemProps;
                beltList[beltIndex].PlayerRootFuncTest(findItemProps);
                break;
            }
        }

        if (!findItemProps)
        {
            int previousIndex = beltIndex - 1;
            if (previousIndex >= 0)
            {
                foreach (ItemProps itemProps in beltList[previousIndex].itemObjList)
                {
                    if (itemProps.beltGroupIndex == beltGroupIndex)
                    {
                        findItemProps = itemProps;
                        beltList[previousIndex].PlayerRootFuncTest(findItemProps);
                        break;
                    }
                }
            }

            int nextIndex = beltIndex + 1;
            if (!findItemProps && nextIndex < beltList.Count)
            {
                foreach (ItemProps itemProps in beltList[nextIndex].itemObjList)
                {
                    if (itemProps.beltGroupIndex == beltGroupIndex)
                    {
                        findItemProps = itemProps;
                        beltList[nextIndex].PlayerRootFuncTest(findItemProps);
                        break;
                    }
                }
            }

            if (!findItemProps)
            {
                Debug.Log("Can't Found Item Index" + beltGroupIndex);
            }
        }

        if (findItemProps && isServer == IsServer)
        {
            LootListManager.instance.DisplayLootInfo(findItemProps.item, findItemProps.amount);
        }
    }
}