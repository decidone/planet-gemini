using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Netcode;
using Pathfinding;
using System.Collections;
using Unity.VisualScripting;

// UTF-8 설정
public enum BeltState
{
    SoloBelt,
    StartBelt,
    EndBelt,
    RepeaterBelt
}

public class BeltCtrl : LogisticsCtrl
{
    [SerializeField]
    int modelMotion = 0;  // 모션
    int preMotion = -1;
    public BeltGroupMgr beltGroupMgr;
    BeltManager beltManager = null;

    protected Animator anim;
    protected Animator animsync;

    public BeltState beltState;

    bool isTurn = false;
    bool isRightTurn = true;

    public BeltCtrl nextBelt;
    public BeltCtrl preBelt;

    public Vector2[] nextPos = new Vector2[3];

    public bool isItemStop = false;

    bool isUp = false;
    bool isRight = false;
    bool isDown = false;
    bool isLeft = false;

    protected override void Awake()
    {
        gameManager = GameManager.instance;
        beltManager = BeltManager.instance;
        playerInven = gameManager.inventory;
        buildName = structureData.FactoryName;
        col = GetComponent<BoxCollider2D>();
        unitSprite = GetComponent<SpriteRenderer>();

        maxLevel = structureData.MaxLevel;
        maxHp = structureData.MaxHp[level];
        defense = structureData.Defense[level];
        hp = structureData.MaxHp[level];
        getDelay = 0.05f;
        sendDelay = structureData.SendDelay[level];
        hpBar.enabled = false;
        hpBar.fillAmount = hp / maxHp;
        repairBar.fillAmount = 0;
        isStorageBuilding = false;
        isMainSource = false;
        isUIOpened = false;
        myVision.SetActive(false);
        maxAmount = structureData.MaxItemStorageLimit;
        cooldown = structureData.Cooldown;
        connectors = new List<EnergyGroupConnector>();
        conn = null;
        efficiency = 0;
        effiCooldown = 0;
        energyUse = structureData.EnergyUse[level];
        isEnergyStr = structureData.IsEnergyStr;
        energyProduction = structureData.Production;
        energyConsumption = structureData.Consumption[level];
        destroyInterval = structureData.RemoveGauge;
        soundManager = SoundManager.instance;
        repairEffect = GetComponentInChildren<RepairEffectFunc>();
        destroyTimer = destroyInterval;
        warningIconCheck = false;
        visionPos = transform.position;
        increasedStructure = new bool[5];
        onEffectUpgradeCheck += IncreasedStructureCheck;
        if (TryGetComponent(out Animator anim))
        {
            getAnim = true;
            animator = anim;
        }
    }

    void Start()
    {
        beltGroupMgr = GetComponentInParent<BeltGroupMgr>();
        animsync = beltManager.AnimSync(level);
        anim = GetComponent<Animator>();
        AnimSyncFunc();
        isOperate = true;
        StrBuilt();
        BeltModelSet();
    }

    protected override void Update()
    {
        base.Update();
    }

    private void FixedUpdate()
    {
        if (destroyStart)
            return;

        if (itemObjList.Count > 0)
            ItemMove();
        else if (itemObjList.Count == 0 && isItemStop)
            isItemStop = false;
    }

    public void AnimSyncFunc()
    {
        if(!anim)
            return;
        else if(!animsync)
            animsync = beltManager.AnimSync(level);

        animsync = beltManager.AnimSync(level);

        var info = animsync.GetCurrentAnimatorStateInfo(0);
        anim.Play(info.fullPathHash, 0, info.normalizedTime);
        Debug.Log(info.fullPathHash);
    }

    public override void NearStrBuilt()
    {
        // 건물을 지었을 때나 근처에 새로운 건물이 지어졌을 때 동작
        // 변경사항이 생기면 DelayNearStrBuiltCoroutine()에도 반영해야 함
        if (IsServer)
        {
            if (anim == null)
            {
                beltGroupMgr = GetComponentInParent<BeltGroupMgr>();
                animsync = beltManager.AnimSync(level);
                anim = GetComponent<Animator>();
            }

            if (!removeState)
            {
                CheckPos();
                ModelSet();
                beltGroupMgr.BeltGroupRefresh();    
                anim.SetFloat("DirNum", dirNum);
                anim.SetFloat("ModelNum", modelMotion);
                anim.SetFloat("Level", level);
            }
        }
        else// 이 라인은 딜레이를 주고 실행할 때는 뺌
        {
            DelayNearStrBuilt();
        }
    }

    public override void DelayNearStrBuilt()
    {
        // 동시 건설, 클라이언트 동기화 등의 이유로 딜레이를 주고 NearStrBuilt()를 실행할 때 사용
        StartCoroutine(DelayNearStrBuiltCoroutine());
    }

    protected override IEnumerator DelayNearStrBuiltCoroutine()
    {
        // 동시 건설이나 그룹핑을 따로 예외처리 하는 경우가 아니면 NearStrBuilt()를 그대로 사용
        yield return new WaitForEndOfFrame();

        if (!removeState)
        {
            if (anim == null)
            {
                beltGroupMgr = GetComponentInParent<BeltGroupMgr>();
                animsync = beltManager.AnimSync(level);
                anim = GetComponent<Animator>();
            }

            CheckPos();
            ModelSet();

            anim.SetFloat("DirNum", dirNum);
            anim.SetFloat("ModelNum", modelMotion);
            anim.SetFloat("Level", level);
        }
    }

    [ClientRpc]
    public override void UpgradeFuncClientRpc()
    {
        base.UpgradeFuncClientRpc();
        anim.SetFloat("DirNum", dirNum);
        anim.SetFloat("ModelNum", modelMotion);
        anim.SetFloat("Level", level);
        animsync = beltManager.AnimSync(level);
        AnimSyncFunc();
    }

    [ServerRpc(RequireOwnership = false)]
    public override void ClientConnectSyncServerRpc()
    {
        base.ClientConnectSyncServerRpc(); 
        DirSyncClientRpc(isUp, isRight, isDown, isLeft);
        ClientConnectBeltSyncClientRpc(modelMotion, isTurn, isRightTurn, (int)beltState);
    }

    [ClientRpc]
    void DirSyncClientRpc(bool up, bool right, bool down, bool left)
    {
        if (!IsServer)
        {
            isUp = up;
            isRight = right;
            isDown = down;
            isLeft = left;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public override void ItemSyncServerRpc()
    {
        for (int i = 0; i < itemObjList.Count; i++)
        {
            int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(itemObjList[i].item);
            ItemSyncClientRpc(itemIndex, itemObjList[i].transform.position, itemObjList[i].beltGroupIndex);
        }
    }

    [ClientRpc]
    public void ClientConnectBeltSyncClientRpc(int syncMotion, bool syncTurn, bool syncRightTurn, int syncBeltState)
    {
        if (IsServer)
        {
            DelayNearStrBuilt();
        }
        else
        {
            modelMotion = syncMotion;
            isTurn = syncTurn;
            isRightTurn = syncRightTurn;
            beltState = (BeltState)syncBeltState;
            DelayNearStrBuilt();
        }
    }

    public void GameStartBeltSet(int syncMotion, bool syncTurn, bool syncRightTurn, int syncBeltState)
    {
        modelMotion = syncMotion;
        isTurn = syncTurn;
        isRightTurn = syncRightTurn;
        beltState = (BeltState)syncBeltState;
    }

    public void GameStartItemSet(Vector3 pos, int itemIndex)
    {
        Item sendItem = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
        var itemPool = ItemPoolManager.instance.Pool.Get();
        ItemProps spawn = itemPool.GetComponent<ItemProps>();
        SpriteRenderer sprite = spawn.GetComponent<SpriteRenderer>();
        sprite.sprite = sendItem.icon;
        sprite.sortingOrder = 2;
        spawn.item = sendItem;
        spawn.amount = 1;
        spawn.transform.position = pos;
        spawn.isOnBelt = true;
        spawn.setOnBelt = this;
        itemObjList.Add(spawn);

        if (itemObjList.Count >= structureData.MaxItemStorageLimit)
            isFull = true;
    }

    [ClientRpc]
    public void ItemSyncClientRpc(int itemIndex, Vector3 tr, int beltGroupIndex)
    {
        if (IsServer)
            return;

        Item sendItem = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
        var itemPool = ItemPoolManager.instance.Pool.Get();
        ItemProps spawn = itemPool.GetComponent<ItemProps>();
        SpriteRenderer sprite = spawn.GetComponent<SpriteRenderer>();
        sprite.sprite = sendItem.icon;
        sprite.sortingOrder = 2;
        spawn.item = sendItem;
        spawn.amount = 1;
        spawn.transform.position = tr;
        spawn.isOnBelt = true;
        spawn.setOnBelt = this;
        spawn.beltGroupIndex = beltGroupIndex;
        itemObjList.Add(spawn);

        if (itemObjList.Count >= structureData.MaxItemStorageLimit)
            isFull = true;
    }

    void ModelSet()
    {
        if (!isTurn)
        {
            if (beltState == BeltState.SoloBelt)
            {
                modelMotion = 0;
            }
            else if (beltState == BeltState.StartBelt)
            {
                modelMotion = 1;
            }
            else if (beltState == BeltState.EndBelt)
            {
                modelMotion = 3;
            }
            else if (beltState == BeltState.RepeaterBelt)
            {
                modelMotion = 2;
            }
        }
        else if (isTurn)
        {
            if (isRightTurn)
            {
                modelMotion = 5;
            }
            else if (!isRightTurn)
            {
                modelMotion = 4;
            }
        }

        if(IsServer)
            BeltModelMotionSetClientRpc(modelMotion);

        SetItemDir();
    }

    protected void SetItemDir()
    {
        for (int a = 0; a < nextPos.Length; a++)
        {
            nextPos[a] = this.transform.position;
        }

        if (dirNum == 0)
        {
            if (modelMotion != 4 && modelMotion != 5)
            {
                nextPos[0] += Vector2.up * 0.34f;
                nextPos[2] += Vector2.down * 0.34f;
            }
            else if (modelMotion == 4)
            {
                nextPos[0] += Vector2.up * 0.34f;
                nextPos[1] += Vector2.up * 0.1f;
                nextPos[2] += Vector2.left * 0.34f;
                nextPos[2] += Vector2.up * 0.1f;
            }
            else if (modelMotion == 5)
            {
                nextPos[0] += Vector2.up * 0.34f;
                nextPos[1] += Vector2.up * 0.1f;
                nextPos[2] += Vector2.right * 0.34f;
                nextPos[2] += Vector2.up * 0.1f;
            }
        }
        else if (dirNum == 1)
        {
            if (modelMotion != 4 && modelMotion != 5)
            {
                nextPos[0] += Vector2.right * 0.34f;
                nextPos[2] += Vector2.left * 0.34f;
                for (int a = 0; a < nextPos.Length; a++)
                {
                    nextPos[a] += Vector2.up * 0.1f;
                }
            }
            else if (modelMotion == 4)
            {
                nextPos[0] += Vector2.right * 0.34f;
                nextPos[0] += Vector2.up * 0.1f;
                nextPos[1] += Vector2.up * 0.1f;
                nextPos[2] += Vector2.up * 0.34f;
            }
            else if (modelMotion == 5)
            {
                nextPos[0] += Vector2.right * 0.34f;
                nextPos[0] += Vector2.up * 0.1f;
                nextPos[1] += Vector2.up * 0.1f;
                nextPos[2] += Vector2.down * 0.34f;
            }
        }
        else if (dirNum == 2)
        {
            if (modelMotion != 4 && modelMotion != 5)
            {
                nextPos[0] += Vector2.down * 0.34f;
                nextPos[2] += Vector2.up * 0.34f;
            }
            else if (modelMotion == 4)
            {
                nextPos[0] += Vector2.down * 0.34f;
                nextPos[1] += Vector2.up * 0.1f;
                nextPos[2] += Vector2.right * 0.34f;
                nextPos[2] += Vector2.up * 0.1f;
            }
            else if (modelMotion == 5)
            {
                nextPos[0] += Vector2.down * 0.34f;
                nextPos[1] += Vector2.up * 0.1f;
                nextPos[2] += Vector2.left * 0.34f;
                nextPos[2] += Vector2.up * 0.1f;
            }
        }
        else if (dirNum == 3)
        {
            if (modelMotion != 4 && modelMotion != 5)
            {
                nextPos[0] += Vector2.left * 0.34f;
                nextPos[2] += Vector2.right * 0.34f;
                for (int a = 0; a < nextPos.Length; a++)
                {
                    nextPos[a] += Vector2.up * 0.1f;
                }
            }
            else if (modelMotion == 4)
            {
                nextPos[0] += Vector2.left * 0.34f;
                nextPos[0] += Vector2.up * 0.1f;
                nextPos[1] += Vector2.up * 0.1f;
                nextPos[2] += Vector2.down * 0.34f;
            }
            else if (modelMotion == 5)
            {
                nextPos[0] += Vector2.left * 0.34f;
                nextPos[0] += Vector2.up * 0.1f;
                nextPos[1] += Vector2.up * 0.1f;
                nextPos[2] += Vector2.up * 0.34f;
            }
        }
    }

    void ItemMove()
    {
        for (int i = 0; i < itemObjList.Count; i++)
        {
            if (nextPos.Length > i)
            {
                itemObjList[i].transform.position = Vector3.MoveTowards(itemObjList[i].transform.position, nextPos[i], Time.deltaTime * structureData.SendSpeed[level]);
            }
        }

        if (Vector2.Distance(itemObjList[0].transform.position, nextPos[0]) < 0.001f)
        {
            isItemStop = true;
        }
        else
        {
            isItemStop = false;
        }

        if(IsServer)
            ItemSend();
    }

    public bool OnBeltItem(ItemProps itemObj)
    {
        if (itemObjList.Count >= structureData.MaxItemStorageLimit)
        {
            if (nextBelt != null && beltState != BeltState.EndBelt)
            {
                if (!nextBelt.isFull && !nextBelt.destroyStart && itemObjList.Count > 0)
                {
                    nextBelt.BeltGroupSendItem(itemObjList[0]);
                    itemObjList.Remove(itemObjList[0]);
                    ItemNumCheck();
                }
            }
        }

        //if (itemObjList.Count < structureData.MaxItemStorageLimit)
        //{
        itemObjList.Add(itemObj);

        if (GetComponent<BeltCtrl>())
            GetComponent<BeltCtrl>().beltGroupMgr.groupItem.Add(itemObj);

        if (itemObjList.Count >= structureData.MaxItemStorageLimit)
            isFull = true;
        else
            isFull = false;

        //    return true;
        //}

        return true;
    }


    void AddNewItem(ItemProps newItem)
    {
        // 새로운 아이템을 리스트에 추가합니다.
        itemObjList.Add(newItem);

        // 새로운 아이템의 위치를 가져옵니다.
        Vector3 newItemPos = newItem.transform.position;

        // 새로운 아이템이 들어갈 위치를 찾습니다.
        int insertIndex = -1;
        float minDist = 1.0f;
        for (int i = 0; i < itemObjList.Count - 1; i++)
        {
            float dist = Vector3.Distance(newItemPos, itemObjList[i].transform.position);
            if (dist < minDist)
            {
                insertIndex = i;
                minDist = dist;
            }
        }

        // 새로운 아이템을 리스트에서 제거하고, insertIndex에 다시 추가합니다.
        itemObjList.Remove(newItem);
        itemObjList.Insert(insertIndex, newItem);

        //// 아이템의 위치를 다시 설정합니다.
        //for (int i = 0; i < itemObjList.Count; i++)
        //{
        //    itemObjList[i].transform.position = nextPos[i];
        //}
    }

    void ItemSend()
    {
        if (nextBelt != null && beltState != BeltState.EndBelt)
        {
            if (!nextBelt.isFull && !nextBelt.destroyStart && itemObjList.Count > 0)
            {
                Vector2 fstItemPos = itemObjList[0].transform.position;
                if (fstItemPos == nextPos[0])
                {
                    SendItemServerRpc();
                    //nextBelt.BeltGroupSendItem(itemObjList[0]);
                    //itemObjList.Remove(itemObjList[0]);
                    //ItemNumCheck();
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SendItemServerRpc()
    {
        SendItemClientRpc();
    }

    [ClientRpc]
    void SendItemClientRpc()
    {
        nextBelt.BeltGroupSendItem(itemObjList[0]);
        itemObjList.Remove(itemObjList[0]);
        ItemNumCheck();
    }

    public void BeltModelSet()
    {
        if (preBelt == null)
            return;
        else if (preBelt.dirNum != dirNum)
        {
            isTurn = true;
            if (preBelt.dirNum == 0)
            {
                if (dirNum == 1)
                {
                    isRightTurn = true;
                }
                else if (dirNum == 3)
                {
                    isRightTurn = false;
                }
            }
            else if (preBelt.dirNum == 1)
            {
                if (dirNum == 2)
                {
                    isRightTurn = true;
                }
                else if (dirNum == 0)
                {
                    isRightTurn = false;
                }
            }
            else if (preBelt.dirNum == 2)
            {
                if (dirNum == 3)
                {
                    isRightTurn = true;
                }
                else if (dirNum == 1)
                {
                    isRightTurn = false;
                }
            }
            else if (preBelt.dirNum == 3)
            {
                if (dirNum == 0)
                {
                    isRightTurn = true;
                }
                else if (dirNum == 2)
                {
                    isRightTurn = false;
                }
            }
        }
        else if (preBelt.dirNum == dirNum)
        {
            isTurn = false;
        }
        if (beltState == BeltState.StartBelt || beltState == BeltState.SoloBelt)
            Invoke(nameof(FactoryModelSet), 0.1f);
        BeltDataServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void BeltDataServerRpc()
    {
        ClientConnectBeltSyncClientRpc(modelMotion, isTurn, isRightTurn, (int)beltState);
    }

    public void FactoryPosCheck(Structure factory)
    {
        float xDiff = factory.transform.position.x - this.transform.position.x;
        float yDiff = factory.transform.position.y - this.transform.position.y;

        if (factory.sizeOneByOne)
        {
            if (xDiff > 0)
            {
                isLeft = true;
                nearObj[3] = factory.gameObject;
            }
            else if (xDiff < 0)
            {
                isRight = true;
                nearObj[1] = factory.gameObject;
            }
            else if (yDiff > 0)
            {
                isDown = true;
                nearObj[2] = factory.gameObject;
            }
            else if (yDiff < 0)
            {
                isUp = true;
                nearObj[0] = factory.gameObject;
            }
        }
        else
        {
            if (xDiff > 0)
            {
                if (Mathf.Abs(xDiff) > Mathf.Abs(yDiff))
                {
                    isLeft = true;
                    nearObj[3] = factory.gameObject;
                }
                else
                {
                    if (yDiff > 0)
                    {
                        isDown = true;
                        nearObj[2] = factory.gameObject;
                    }
                    else
                    {
                        isUp = true;
                        nearObj[0] = factory.gameObject;
                    }
                }
            }
            else
            {
                if (Mathf.Abs(xDiff) > Mathf.Abs(yDiff))
                {
                    isRight = true;
                    nearObj[1] = factory.gameObject;
                }
                else
                {
                    if (yDiff > 0)
                    {
                        isDown = true;
                        nearObj[2] = factory.gameObject;
                    }
                    else
                    {
                        isUp = true;
                        nearObj[0] = factory.gameObject;
                    }
                }
            }
        }
        if (beltState == BeltState.StartBelt || beltState == BeltState.SoloBelt)
            Invoke(nameof(FactoryModelSet), 0.1f);
    }

    public void FactoryModelSet()
    {
        if (isUp && !isRight && !isDown && !isLeft)
        {
            if (dirNum == 1)
            {
                isTurn = true;
                isRightTurn = true;
            }
            else if (dirNum == 3)
            {
                isTurn = true;
                isRightTurn = false;
            }
        }
        else if (!isUp && isRight && !isDown && !isLeft)
        {
            if (dirNum == 0)
            {
                isTurn = true;
                isRightTurn = false;
            }
            else if (dirNum == 2)
            {
                isTurn = true;
                isRightTurn = true;
            }
        }
        else if (!isUp && !isRight && isDown && !isLeft)
        {
            if (dirNum == 1)
            {
                isTurn = true;
                isRightTurn = false;
            }
            else if (dirNum == 3)
            {
                isTurn = true;
                isRightTurn = true;
            }
        }
        else if (!isUp && !isRight && !isDown && isLeft)
        {
            if (dirNum == 0)
            {
                isTurn = true;
                isRightTurn = true;
            }
            else if (dirNum == 2)
            {
                isTurn = true;
                isRightTurn = false;
            }
        }
        else
            isTurn = false;
        ModelSet();
        BeltDataServerRpc();
    }

    public override void ResetNearObj(GameObject game)
    {
        for (int i = 0; i < nearObj.Length; i++)
        {
            if (nearObj[i] != null && nearObj[i] == game)
            {
                nearObj[i] = null;
                if (i == 0)
                {
                    isUp = false;
                }
                else if (i == 1)
                {
                    isRight = false;
                }
                else if (i == 2)
                {
                    isDown = false;
                }
                else if (i == 3)
                {
                    isLeft = false;
                }
            }
        }
        BeltModelSet();
    }

    public List<ItemProps> PlayerRootItemCheck()
    {
        List<ItemProps> sendItemList = new List<ItemProps>();

        sendItemList = new List<ItemProps>(itemObjList);

        return sendItemList;
    }

    public void PlayerRootFunc(ItemProps item)
    {
        if (itemObjList.Contains(item))
        {
            int index = itemObjList.IndexOf(item);
            if (index != -1)
                PlayerRootFuncServerRpc(index);
            itemObjList.Remove(item);
            beltGroupMgr.ItemRoot(item);
            item.itemPool.Release(item.gameObject);

            if (itemObjList.Count >= structureData.MaxItemStorageLimit)
                isFull = true;
            else
                isFull = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerRootFuncServerRpc(int index)
    {
        PlayerRootFuncClientRpc(index);
    }

    [ClientRpc]
    public void PlayerRootFuncClientRpc(int index)
    {
        if (IsServer) return;
        ItemProps item = itemObjList[index];
        beltGroupMgr.ItemRoot(item);
        itemObjList.RemoveAt(index);
        item.itemPool.Release(item.gameObject);

        if (itemObjList.Count >= structureData.MaxItemStorageLimit)
            isFull = true;
        else
            isFull = false;
    }

    public void PlayerRootFuncTest(ItemProps item)
    {
        itemObjList.Remove(item);
        beltGroupMgr.ItemRoot(item);
        item.itemPool.Release(item.gameObject);

        if (itemObjList.Count >= structureData.MaxItemStorageLimit)
            isFull = true;
        else
            isFull = false;
    }

    //public void PlayerRootFunc(ItemProps item)
    //{
    //    if (itemObjList.Contains(item))
    //    {
    //        int index = itemObjList.IndexOf(item);
    //        if (index != -1)
    //            PlayerRootFuncServerRpc(index, IsServer);

    //        itemObjList.Remove(item);
    //        beltGroupMgr.ItemRoot(item);
    //        item.itemPool.Release(item.gameObject);

    //        if (itemObjList.Count >= structureData.MaxItemStorageLimit)
    //            isFull = true;
    //        else
    //            isFull = false;
    //    }
    //}

    //[ServerRpc(RequireOwnership = false)]
    //public void PlayerRootFuncServerRpc(int index, bool isServer)
    //{
    //    PlayerRootFuncClientRpc(index, isServer);
    //}

    //[ClientRpc]
    //public void PlayerRootFuncClientRpc(int index, bool isServer)
    //{
    //    if (isServer && IsServer)
    //    {
    //        return;
    //    }
    //    else if (!isServer && !IsServer)
    //    {
    //        return;
    //    }

    //    if (itemObjList.Count < index + 1)
    //    {
    //        nextBelt.ItemRootSync();
    //    }
    //    else
    //    {
    //        ItemProps item = itemObjList[index];
    //        beltGroupMgr.ItemRoot(item);
    //        itemObjList.RemoveAt(index);
    //        item.itemPool.Release(item.gameObject);
    //    }

    //    if (itemObjList.Count >= structureData.MaxItemStorageLimit)
    //        isFull = true;
    //    else
    //        isFull = false;
    //}

    public void ItemRootSync()
    {
        ItemProps item = itemObjList[itemObjList.Count - 1];
        itemObjList.RemoveAt(itemObjList.Count - 1);
        item.itemPool.Release(item.gameObject);

        if (itemObjList.Count >= structureData.MaxItemStorageLimit)
            isFull = true;
        else
            isFull = false;
    }

    public override Dictionary<Item, int> PopUpItemCheck()
    {
        if (itemObjList.Count > 0) 
        {
            Dictionary<Item, int> returnDic = new Dictionary<Item, int>();
            foreach (ItemProps itemProps in itemObjList)
            {
                Item item = itemProps.item;
                int amounts = itemProps.amount;

                if (!returnDic.ContainsKey(item))
                    returnDic.Add(item, amounts);
                else
                {
                    int currentValue = returnDic[item];
                    int newValue = currentValue + amounts;
                    returnDic[item] = newValue;
                }
            }
            return returnDic;
        }                
        else
            return null;
    }
    public override void GameStartSpawnSet(int _level, int _beltDir, int objHeight, int objWidth, bool isHostMap, int index)
    {
        level = _level;
        dirNum = _beltDir;
        height = objHeight;
        width = objWidth;
        buildingIndex = index;
        isInHostMap = isHostMap;
        settingEndCheck = true;
        gameObject.AddComponent<DynamicGridObstacle>();
        myVision.SetActive(true);
        DataSet();
    }

    [ClientRpc]
    public override void SettingClientRpc(int _level, int _beltDir, int objHeight, int objWidth, bool isHostMap, int index)
    {
        beltGroupMgr = GetComponentInParent<BeltGroupMgr>();
        level = _level;
        dirNum = _beltDir;
        height = objHeight;
        width = objWidth;
        buildingIndex = index;
        isInHostMap = isHostMap;
        settingEndCheck = true;
        beltState = BeltState.SoloBelt;
        SetBuild();
        DataSet();
        ColliderTriggerOnOff(true);
        gameObject.AddComponent<DynamicGridObstacle>();
        myVision.SetActive(true);
        soundManager.PlaySFX(gameObject, "structureSFX", "BuildingSound");
    }

    [ClientRpc]
    public void BeltStateSetClientRpc(int beltStateNum)
    {
        beltState = (BeltState)beltStateNum;
    }

    [ServerRpc(RequireOwnership = false)]
    public void BeltDirSetServerRpc()
    {
        BeltDirSetClientRpc(dirNum);
    }

    [ClientRpc]
    public void BeltDirSetClientRpc(int num)
    {
        dirNum = num;
        BeltModelSet();
    }

    [ClientRpc]
    public void BeltModelMotionSetClientRpc(int modelNum)
    {
        modelMotion = modelNum;
        if(anim)
            anim.SetFloat("ModelNum", modelMotion);
    }

    public BeltSaveData BeltSaveData()
    {
        BeltSaveData data = new BeltSaveData();

        data.modelMotion = modelMotion;
        data.isTrun = isTurn;
        data.isRightTurn = isRightTurn;
        data.beltState = (int)beltState;

        foreach (ItemProps items in itemObjList)
        {
            int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(items.item);
            data.itemPos.Add(Vector3Extensions.FromVector3(items.transform.position));
            data.itemIndex.Add(itemIndex);
        }

        return data;
    }

    protected override void NonOperateStateSet(bool isOn)
    {
        animator.enabled = isOn;
    }

    [ClientRpc]
    public override void DestroyFuncClientRpc()
    {
        if (preBelt != null)
        {
            preBelt.DelayNearStrBuilt();
        }
        if (nextBelt != null)
        {
            nextBelt.DelayNearStrBuilt();
        }

        base.DestroyFuncClientRpc();
    }

    public override void ColliderTriggerOnOff(bool isOn)
    {
        col.isTrigger = true;
    }
}
