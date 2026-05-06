using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

// UTF-8 설정
public class BeltGroupMgr : NetworkBehaviour
{
    public BeltManager beltManager;

    public List<BeltCtrl> beltList = new List<BeltCtrl>();
    public List<ItemProps> groupItem = new List<ItemProps>();

    public Structure nextObj = null;
    public Structure preObj = null;

    public bool nextCheck = true;
    public bool preCheck = true;

    public bool loadConnStr;

    Coroutine clientBeltSyncCoroutine;

    void Start()
    {
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
                loadConnStr = false;
                if (!nextCheck)
                {
                    NearObjSetClientRpc(nextObj.NetworkObject, true);
                }
            }
            if (preCheck)
            {
                preObj = PreObjCheck();
                if (!preCheck)
                {
                    NearObjSetClientRpc(preObj.NetworkObject, false);
                }
            }
        }
    }

    //private void OnClientConnectedCallback(ulong clientId)
    //{
    //    ClientConnectSyncServerRpc();
    //}

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        NetworkObjManager.instance.BeltGroupAdd(this);
        //if (IsServer)
        //{
        //    NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        //}
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        NetworkObjManager.instance.BeltGroupRemove(this);
        //if (IsServer)
        //{
        //    NetworkManager.OnClientConnectedCallback -= OnClientConnectedCallback;
        //}
    }

    //[ServerRpc(RequireOwnership = false)]
    //public void ClientConnectSyncServerRpc()
    //{
    //    for(int i = 0; i < beltList.Count; i ++)
    //    {            
    //        NetworkObjectReference mainId = beltList[i].NetworkObject;

    //        if (i >= 1)
    //        {
    //            NetworkObjectReference preId = beltList[i - 1].NetworkObject;
    //            PreBeltSetClientRpc(mainId, preId);
    //        }
    //        if (i < beltList.Count - 1)
    //        {
    //            NetworkObjectReference nextId = beltList[i + 1].NetworkObject;
    //            NextBeltSetClientRpc(mainId, nextId);
    //        }
    //    }

    //    if(nextObj)
    //    {
    //        NearObjSetClientRpc(nextObj.NetworkObject, true);
    //    }
    //    else if (preObj)
    //    {
    //        NearObjSetClientRpc(preObj.NetworkObject, false);
    //    }
    //}

    [ServerRpc(RequireOwnership = false)]
    public void BeltGroupClientConnectSyncServerRpc(ulong targetClientId)
    {
        NetworkObjectReference[] beltRefs = new NetworkObjectReference[beltList.Count];
        for (int i = 0; i < beltList.Count; i++)
            beltRefs[i] = beltList[i].NetworkObject;

        NetworkObjectReference nextRef = default;
        bool hasNext = false;
        NetworkObjectReference preRef = default;
        bool hasPre = false;

        if (nextObj != null)
        {
            nextRef = nextObj.NetworkObject;
            hasNext = true;
        }
        else if (preObj != null)
        {
            preRef = preObj.NetworkObject;
            hasPre = true;
        }

        var itemIndexList = new List<int>();
        var itemPosList = new List<Vector2>();
        var beltGroupIndexList = new List<int>();
        var beltIndexList = new List<int>();

        for (int beltIdx = 0; beltIdx < beltList.Count; beltIdx++)
        {
            BeltCtrl belt = beltList[beltIdx];
            foreach (ItemProps item in belt.itemObjList)
            {
                itemIndexList.Add(GeminiNetworkManager.instance.GetItemSOIndex(item.item));
                itemPosList.Add(item.transform.position);
                beltGroupIndexList.Add(item.beltGroupIndex);
                beltIndexList.Add(beltIdx);
            }
        }

        var data = new BeltGroupSyncData
        {
            beltRefs = beltRefs,
            nextObjRef = nextRef,
            hasNextObj = hasNext,
            preObjRef = preRef,
            hasPreObj = hasPre,
            itemIndexes = itemIndexList.ToArray(),
            itemPositions = itemPosList.ToArray(),
            itemBeltGroupIndexes = beltGroupIndexList.ToArray(),
            itemBeltIndexes = beltIndexList.ToArray()
        };

        ClientRpcParams target = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { targetClientId }
            }
        };

        BeltGroupClientConnectSyncClientRpc(data, target);
    }

    [ClientRpc]
    void BeltGroupClientConnectSyncClientRpc(BeltGroupSyncData data, ClientRpcParams rpcParams = default)
    {
        if (IsServer) return;

        BeltCtrl[] belts = new BeltCtrl[data.beltRefs.Length];
        for (int i = 0; i < data.beltRefs.Length; i++)
        {
            if (data.beltRefs[i].TryGet(out NetworkObject obj))
                belts[i] = obj.GetComponent<BeltCtrl>();
        }

        for (int i = 0; i < belts.Length; i++)
        {
            if (belts[i] == null) continue;

            if (i >= 1 && belts[i - 1] != null)
                belts[i].preBelt = belts[i - 1];

            if (i < belts.Length - 1 && belts[i + 1] != null)
                belts[i].nextBelt = belts[i + 1];
        }

        if (data.hasNextObj)
        {
            if (data.nextObjRef.TryGet(out NetworkObject obj))
                nextObj = obj.GetComponent<Structure>();
        }
        if (data.hasPreObj)
        {
            if (data.preObjRef.TryGet(out NetworkObject obj))
                preObj = obj.GetComponent<Structure>();
        }

        ClientBeltSyncFunc();

        for (int i = 0; i < data.itemIndexes.Length; i++)
        {
            int beltIdx = data.itemBeltIndexes[i];
            if (beltIdx >= beltList.Count) continue;

            BeltCtrl targetBelt = beltList[beltIdx];

            Item sendItem = GeminiNetworkManager.instance.GetItemSOFromIndex(data.itemIndexes[i]);
            var itemPool = ItemPoolManager.instance.Pool.Get();
            ItemProps spawn = itemPool.GetComponent<ItemProps>();

            SpriteRenderer sprite = spawn.spriteRenderer;
            sprite.sprite = sendItem.icon;
            sprite.sortingOrder = 2;

            spawn.item = sendItem;
            spawn.amount = 1;
            spawn.transform.position = data.itemPositions[i];
            spawn.isOnBelt = true;
            spawn.setOnBelt = targetBelt;
            spawn.beltGroupIndex = data.itemBeltGroupIndexes[i];

            targetBelt.itemObjList.Add(spawn);
            groupItem.Add(spawn);

            if (targetBelt.itemObjList.Count >= targetBelt.maxAmount)
                targetBelt.isFull = true;
        }
    }

    public void SetBelt(BeltCtrl belt, int level, int beltDir, int objHeight, int objWidth, bool isHostMap, int index)
    {
        belt.NetworkObject.TrySetParent(NetworkObject);
        //beltList.Add(belt);
        belt.SettingClientRpc(level, beltDir, objHeight, objWidth, isHostMap, index);
    }

    public void GroupAddBelt(BeltCtrl belt)
    {
        if (!beltList.Contains(belt))
            beltList.Add(belt);
    }

    public void SetBeltData()
    {
        Reconfirm();
        for (int i = 0; i < beltList.Count; i++)
        {
            NetworkObjectReference mainId = beltList[i].NetworkObject;

            if (i >= 1)
            {
                NetworkObjectReference preId = beltList[i - 1].NetworkObject;
                PreBeltSetClientRpc(mainId, preId);
            }
            if (i < beltList.Count - 1)
            {
                NetworkObjectReference nextId = beltList[i + 1].NetworkObject;
                NextBeltSetClientRpc(mainId, nextId);
            }
        }
    }

    private Structure PreObjCheck()
    {
        var checkPos = -transform.up;

        BeltCtrl belt = beltList[0];
        if (belt.dirNum == 0)
        {
            checkPos = -belt.transform.up;
        }
        else if (belt.dirNum == 1)
        {
            checkPos = -belt.transform.right;
        }
        else if (belt.dirNum == 2)
        {
            checkPos = belt.transform.up;
        }
        else if (belt.dirNum == 3)
        {
            checkPos = belt.transform.right;
        }

        Cell cell = GameManager.instance.GetCellDataFromPosWithoutMap(
            (int)belt.transform.position.x + (int)checkPos.x,
            (int)belt.transform.position.y + (int)checkPos.y
        );

        if (!cell.structure || cell.structure.destroyStart)
            return null;

        Structure str = cell.structure;

        if (str.TryGet(out BeltCtrl otherBelt))
        {
            CheckGroup(belt, otherBelt, false);
        }
        else
        {
            preCheck = false;
        }

        return str;
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

    public void ClientBeltSync()
    {
        if(clientBeltSyncCoroutine == null)
            clientBeltSyncCoroutine = StartCoroutine(ClientBeltSyncCoroutine());
    }


    IEnumerator ClientBeltSyncCoroutine()
    {
        yield return null;
        ClientBeltSyncServerRpc();
        clientBeltSyncCoroutine = null;
    }

    [ServerRpc(RequireOwnership = false)]
    void ClientBeltSyncServerRpc()
    {
        ClientBeltSyncClientRpc();
    }

    [ClientRpc]
    void ClientBeltSyncClientRpc()
    {
        if (IsServer) return;
        ClientBeltSyncFunc();
        Debug.Log("ClientBeltSyncClientRpc call");
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
            //belt.AnimSyncFunc();
        }

        groupItem.Clear();

        foreach (BeltCtrl belt in beltList)
        {
            foreach (ItemProps item in belt.itemObjList)
            {
                groupItem.Add(item);
            }
        }
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

    private Structure NextObjCheck()
    {
        Vector2 checkPos = transform.up;

        BeltCtrl belt = beltList[beltList.Count - 1];
        if (belt.dirNum == 0)
        {
            checkPos = belt.transform.up;
        }
        else if (belt.dirNum == 1)
        {
            checkPos = belt.transform.right;
        }
        else if (belt.dirNum == 2)
        {
            checkPos = -belt.transform.up;
        }
        else if (belt.dirNum == 3)
        {
            checkPos = -belt.transform.right;
        }

        Cell cell = GameManager.instance.GetCellDataFromPosWithoutMap(
            (int)belt.transform.position.x + (int)checkPos.x,
            (int)belt.transform.position.y + (int)checkPos.y
        );

        if (!cell.structure || cell.structure.destroyStart)
            return null;

        Structure str = cell.structure;

        if (str.TryGet(out BeltCtrl otherBelt))
        {
            CheckGroup(belt, otherBelt, true);

            if (otherBelt.beltGroupMgr.nextObj)
            {
                return otherBelt.beltGroupMgr.nextObj;
            }
        }
        else
        {
            nextCheck = false;
        }

        return str;
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
                if ((otherBelt.beltState == BeltState.EndBelt || otherBelt.beltState == BeltState.SoloBelt) && !otherBelt.beltGroupMgr.loadConnStr)
                {
                    if (!otherBelt.beltGroupMgr.nextObj)
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
                    else if (otherBelt.beltGroupMgr.nextObj && otherBelt.beltGroupMgr.nextObj.Get<BeltCtrl>())
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
            NetworkObjectReference thisId = belt.NetworkObject;
            NetworkObjectReference othId = otherBelt.NetworkObject;
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
        NetworkObjectReference thisId = belt.NetworkObject;
        NetworkObjectReference othId = otherBelt.NetworkObject;
        PreBeltSetClientRpc(thisId, othId);
        NextBeltSetClientRpc(othId, thisId);
        otherBelt.dirNum = belt.dirNum;
        belt.BeltModelSet();
        otherBelt.BeltModelSet();
        otherBelt.BeltDirSetServerRpc();
        //ClientBeltSyncServerRpc();
    }

    [ClientRpc]
    public void NextBeltSetClientRpc(NetworkObjectReference thisBeltID, NetworkObjectReference othBeltID)
    {
        var BeltCtrlArr = GetComponentsInChildren<BeltCtrl>();
        BeltCtrl thisBelt = thisBeltID.TryGet(out NetworkObject thisBeltObj) ? thisBeltObj.GetComponent<BeltCtrl>() : null;
        BeltCtrl othBelt = othBeltID.TryGet(out NetworkObject othBeltObj) ? othBeltObj.GetComponent<BeltCtrl>() : null;
        thisBelt.nextBelt = othBelt;
    }

    [ClientRpc]
    public void PreBeltSetClientRpc(NetworkObjectReference thisBeltID, NetworkObjectReference othBeltID)
    {
        var BeltCtrlArr = GetComponentsInChildren<BeltCtrl>();
        BeltCtrl thisBelt = thisBeltID.TryGet(out NetworkObject thisBeltObj) ? thisBeltObj.GetComponent<BeltCtrl>() : null;
        BeltCtrl othBelt = othBeltID.TryGet(out NetworkObject othBeltObj) ? othBeltObj.GetComponent<BeltCtrl>() : null;
        thisBelt.preBelt = othBelt;
    }

    [ClientRpc]
    public void NearObjSetClientRpc(NetworkObjectReference networkObjectReference, bool isNextObj)
    {
        if (IsServer)
            return;
        networkObjectReference.TryGet(out NetworkObject obj);
        obj.TryGetComponent(out Structure str);

        if (!str)
            return;

        if (isNextObj)
        {
            nextObj = str;
        }
        else
        {
            preObj = str;
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

        if(nextObj)
            data.connStr = true;
        else
            data.connStr = false;

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
        BeltCtrl foundBelt = null;

        // 전체 탐색으로 변경 (beltIndex 불일치 우회)
        foreach (BeltCtrl belt in beltList)
        {
            foreach (ItemProps itemProps in belt.itemObjList)
            {
                if (itemProps.beltGroupIndex == beltGroupIndex)
                {
                    findItemProps = itemProps;
                    foundBelt = belt;
                    break;
                }
            }
        }

        if (findItemProps == null)
        {
            Debug.Log($"Can't Found Item - beltGroupIndex:{beltGroupIndex}");
            return;
        }

        foundBelt.PlayerRootFunc(findItemProps);

        if (isServer == IsServer)
        {
            LootListManager.instance.DisplayLootInfo(findItemProps.item, findItemProps.amount);
        }
    }

    //[ServerRpc(RequireOwnership = false)]
    //public void BeltItemSyncServerRpc(ulong targetClientId)
    //{
    //    // 벨트별 아이템 수집
    //    List<int> itemIndexList = new List<int>();
    //    List<Vector2> itemPosList = new List<Vector2>();
    //    List<int> beltGroupIndexList = new List<int>();
    //    List<int> beltIndexList = new List<int>(); // 어느 벨트 소속인지

    //    for (int beltIdx = 0; beltIdx < beltList.Count; beltIdx++)
    //    {
    //        BeltCtrl belt = beltList[beltIdx];
    //        foreach (ItemProps item in belt.itemObjList)
    //        {
    //            itemIndexList.Add(GeminiNetworkManager.instance.GetItemSOIndex(item.item));
    //            itemPosList.Add(item.transform.position);
    //            beltGroupIndexList.Add(item.beltGroupIndex);
    //            beltIndexList.Add(beltIdx); // 어느 벨트에 속하는지
    //        }
    //    }

    //    ClientRpcParams target = new ClientRpcParams
    //    {
    //        Send = new ClientRpcSendParams
    //        {
    //            TargetClientIds = new[] { targetClientId }
    //        }
    //    };

    //    ItemSyncClientRpc(
    //        itemIndexList.ToArray(),
    //        itemPosList.ToArray(),
    //        beltGroupIndexList.ToArray(),
    //        beltIndexList.ToArray(),
    //        target
    //    );
    //}

    //[ClientRpc]
    //void ItemSyncClientRpc(
    //    int[] itemIndexes,
    //    Vector2[] itemPositions,
    //    int[] beltGroupIndexes,
    //    int[] beltIndexes,
    //    ClientRpcParams rpcParams = default)
    //{
    //    if (IsServer) return;

    //    ClientBeltSyncFunc();
    //    beltSyncCheck = true;

    //    for (int i = 0; i < itemIndexes.Length; i++)
    //    {
    //        int beltIdx = beltIndexes[i];
    //        if (beltIdx >= beltList.Count)
    //        {
    //            continue;
    //        }

    //        BeltCtrl targetBelt = beltList[beltIdx];

    //        Item sendItem = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndexes[i]);
    //        var itemPool = ItemPoolManager.instance.Pool.Get();
    //        ItemProps spawn = itemPool.GetComponent<ItemProps>();

    //        SpriteRenderer sprite = spawn.spriteRenderer;
    //        sprite.sprite = sendItem.icon;
    //        sprite.sortingOrder = 2;

    //        spawn.item = sendItem;
    //        spawn.amount = 1;
    //        spawn.transform.position = itemPositions[i];
    //        spawn.isOnBelt = true;
    //        spawn.setOnBelt = targetBelt;
    //        spawn.beltGroupIndex = beltGroupIndexes[i];

    //        targetBelt.itemObjList.Add(spawn);
    //        groupItem.Add(spawn);

    //        if (targetBelt.itemObjList.Count >= targetBelt.maxAmount)
    //            targetBelt.isFull = true;
    //    }
    //}
}