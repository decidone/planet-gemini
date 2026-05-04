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
    int modelMotion = 0;  // 모션
    public BeltGroupMgr beltGroupMgr;
    BeltManager beltManager = null;
    //protected Animator animsync;

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
        foreach (var comp in GetComponents<Component>())
        {
            var type = comp.GetType();

            // 자기 자신부터 Component까지 올라가면서 전부 등록
            while (type != null && type != typeof(MonoBehaviour)
                                && type != typeof(Behaviour)
                                && type != typeof(Component))
            {
                if (!_cache.ContainsKey(type))
                    _cache[type] = comp;

                type = type.BaseType;
            }
        }
        gameManager = GameManager.instance;
        beltManager = BeltManager.instance;
        playerInven = gameManager.inventory;
        buildName = structureData.FactoryName;
        col = Get<BoxCollider2D>();
        setModel = Get<SpriteRenderer>();
        if (TryGet(out ShaderAnimController controller))
        {
            animController = controller;
        }

        maxLevel = structureData.MaxLevel;
        maxHp = structureData.MaxHp[level];
        defense = structureData.Defense[level];
        hp = structureData.MaxHp[level];
        canTakeItem = structureData.CanTakeItem;
        canSendItem = structureData.CanSendItem;
        canTakeFluid = structureData.CanTakeFluid;
        canSendFluid = structureData.CanSendFluid;
        getDelay = 0.05f;
        sendDelay = structureData.SendDelay[level];
        hpBar.enabled = false;
        hpBar.fillAmount = hp / maxHp;
        repairBar.fillAmount = 0;
        isStorageBuilding = false;
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
    }

    void Start()
    {
        beltGroupMgr = GetComponentInParent<BeltGroupMgr>();
        //AnimSyncFunc();
        isOperate = true;
        if (IsServer)
            StrBuilt();
        else if (NetworkObjManager.instance.clientSyncComplete == true)
            StrBuilt();
        BeltModelSet();
    }

    private void FixedUpdate()
    {
        if (destroyStart || isPreBuilding)
            return;

        if (itemObjList.Count > 0)
            ItemMove();
        else if (itemObjList.Count == 0 && isItemStop)
            isItemStop = false;
    }

    //public void AnimSyncFunc()
    //{
    //    animsync = beltManager.AnimSync(level);
    //    var info = animsync.GetCurrentAnimatorStateInfo(0);
    //    animator.Play(info.fullPathHash, 0, info.normalizedTime);
    //}

    void SetBeltAnim()
    {
        animController.SetAnimation(ShaderAnimSelector.instance.GetBeltAnimData(level, dirNum, modelMotion));
        switch (level)
        {
            case 0: animController.SetSpeedMultiplier(1f); break;
            case 1: animController.SetSpeedMultiplier(1.5f); break;
            case 2: animController.SetSpeedMultiplier(2f); break;
        }
    }

    public override void NearStrBuilt()
    {
        // 건물을 지었을 때나 근처에 새로운 건물이 지어졌을 때 동작
        // 변경사항이 생기면 DelayNearStrBuiltCoroutine()에도 반영해야 함
        if (IsServer)
        {
            if (!removeState)
            {
                CheckPos();
                ModelSet();
                beltGroupMgr = GetComponentInParent<BeltGroupMgr>();

                if (beltGroupMgr != null)
                {
                    beltGroupMgr.nextCheck = true;
                    beltGroupMgr.preCheck = true;
                    beltGroupMgr.BeltGroupRefresh();
                }

                //animator.SetFloat("DirNum", dirNum);
                //animator.SetFloat("ModelNum", modelMotion);
                //animator.SetFloat("Level", level);

                SetBeltAnim();
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
            CheckPos();
            ModelSet();
            beltGroupMgr = GetComponentInParent<BeltGroupMgr>();

            if (beltGroupMgr != null)
            {
                beltGroupMgr.nextCheck = true;
                beltGroupMgr.preCheck = true;
                beltGroupMgr.BeltGroupRefresh();
            }

            //animator.SetFloat("DirNum", dirNum);
            //animator.SetFloat("ModelNum", modelMotion);
            //animator.SetFloat("Level", level);

            SetBeltAnim();
        }
    }

    public override void UpgradeFunc()
    {
        base.UpgradeFunc();

        SetBeltAnim();
    }

    public override void ClientConnectSync()
    {
        var data = CollectBaseSyncData();

        // BeltCtrl 전용 데이터
        data.isUp = this.isUp;
        data.isRight = this.isRight;
        data.isDown = this.isDown;
        data.isLeft = this.isLeft;
        data.modelMotion = this.modelMotion;
        data.isTurn = this.isTurn;
        data.isRightTurn = this.isRightTurn;
        data.beltStateInt = (int)this.beltState;

        ClientConnectSyncClientRpc(data);
    }

    protected override void ApplyItemSync(StructureSyncData data) { }

    protected override void ApplyExtraSync(StructureSyncData data)
    {
        isUp = data.isUp;
        isRight = data.isRight;
        isDown = data.isDown;
        isLeft = data.isLeft;
        modelMotion = data.modelMotion;
        isTurn = data.isTurn;
        isRightTurn = data.isRightTurn;
        beltState = (BeltState)data.beltStateInt;

        DelayNearStrBuilt();
    }

    //public override void ClientConnectSync()
    //{
    //    base.ClientConnectSync();

    //    DirSyncClientRpc(isUp, isRight, isDown, isLeft);
    //    ClientConnectBeltSyncClientRpc(modelMotion, isTurn, isRightTurn, (int)beltState);
    //}

    //[ClientRpc]
    //void DirSyncClientRpc(bool up, bool right, bool down, bool left)
    //{
    //    if (!IsServer)
    //    {
    //        isUp = up;
    //        isRight = right;
    //        isDown = down;
    //        isLeft = left;
    //    }
    //}

    //public override void ItemSyncServer() { }

    //[ServerRpc(RequireOwnership = false)]
    //public override void ItemSyncServerRpc()
    //{
    //    int[] itemIndexs = new int[itemObjList.Count];
    //    Vector2[] vector2s = new Vector2[itemObjList.Count];
    //    int[] beltGroupIndexs = new int[itemObjList.Count];

    //    for (int i = 0; i < itemObjList.Count; i++)
    //    {
    //        int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(itemObjList[i].item);

    //        itemIndexs[i] = itemIndex;
    //        vector2s[i] = itemObjList[i].transform.position;
    //        beltGroupIndexs[i] = itemObjList[i].beltGroupIndex;
    //    }
    //    ItemSyncClientRpc(itemIndexs, vector2s, beltGroupIndexs);
    //}

    //[ClientRpc]
    //public void ItemSyncClientRpc(int[] itemIndex, Vector2[] tr, int[] beltGroupIndex)
    //{
    //    if (IsServer)
    //        return;

    //    for (int i = 0; i < itemIndex.Length; i++)
    //    {
    //        Item sendItem = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex[i]);
    //        var itemPool = ItemPoolManager.instance.Pool.Get();
    //        ItemProps spawn = itemPool.GetComponent<ItemProps>();
    //        SpriteRenderer sprite = spawn.spriteRenderer;
    //        sprite.sprite = sendItem.icon;
    //        sprite.sortingOrder = 2;
    //        spawn.item = sendItem;
    //        spawn.amount = 1;
    //        spawn.transform.position = tr[i];
    //        spawn.isOnBelt = true;
    //        spawn.setOnBelt = this;
    //        spawn.beltGroupIndex = beltGroupIndex[i];
    //        itemObjList.Add(spawn);

    //        if (itemObjList.Count >= maxAmount)
    //            isFull = true;
    //    }
    //}

    [ClientRpc]
    public void ClientBeltSyncClientRpc(int syncMotion, bool syncTurn, bool syncRightTurn, int syncBeltState)
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
        SpriteRenderer sprite = spawn.spriteRenderer;
        sprite.sprite = sendItem.icon;
        sprite.sortingOrder = 2;
        spawn.item = sendItem;
        spawn.amount = 1;
        spawn.transform.position = pos;
        spawn.isOnBelt = true;
        spawn.setOnBelt = this;
        itemObjList.Add(spawn);

        if (itemObjList.Count >= maxAmount)
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
            else if (beltState == BeltState.RepeaterBelt)
            {
                modelMotion = 2;
            }
            else if (beltState == BeltState.EndBelt)
            {
                modelMotion = 3;
            }
        }
        else if (isTurn)
        {
            if (!isRightTurn)
            {
                modelMotion = 4;
            }
            else
            {
                modelMotion = 5;
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

        if (dirNum == 0)    //up
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
        else if (dirNum == 1)   //right
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
        else if (dirNum == 2)   //down
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
        else if (dirNum == 3)   //left
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
        itemObjList.Add(itemObj);
        beltGroupMgr.groupItem.Add(itemObj);

        if (itemObjList.Count >= maxAmount)
            isFull = true;
        else
            isFull = false;

        return true;
    }

    void ItemSend()
    {
        if (nextBelt != null && beltState != BeltState.EndBelt)
        {
            if (!nextBelt.isFull && !nextBelt.isPreBuilding && !nextBelt.destroyStart && itemObjList.Count > 0)
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

    [ServerRpc]
    void SendItemServerRpc()
    {
        SendItemClientRpc();
    }

    [ClientRpc]
    void SendItemClientRpc()
    {
        if (itemObjList.Count == 0)
        {
            StartCoroutine(ItemSendDelay());
            return;
        }
        ProcessSendItem();
    }

    IEnumerator ItemSendDelay()
    {
        yield return new WaitUntil(() => itemObjList.Count > 0);
        ProcessSendItem();
    }

    void ProcessSendItem()
    {
        if (itemObjList.Count == 0 || !nextBelt) return; // 방어 코드

        nextBelt.BeltGroupSendItem(itemObjList[0]);
        itemObjList.RemoveAt(0);
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
        ClientBeltSyncClientRpc(modelMotion, isTurn, isRightTurn, (int)beltState);
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
                nearObj[3] = factory;
            }
            else if (xDiff < 0)
            {
                isRight = true;
                nearObj[1] = factory;
            }
            else if (yDiff > 0)
            {
                isDown = true;
                nearObj[2] = factory;
            }
            else if (yDiff < 0)
            {
                isUp = true;
                nearObj[0] = factory;
            }
        }
        else
        {
            if (xDiff > 0)
            {
                if (Mathf.Abs(xDiff) > Mathf.Abs(yDiff))
                {
                    isLeft = true;
                    nearObj[3] = factory;
                }
                else
                {
                    if (yDiff > 0)
                    {
                        isDown = true;
                        nearObj[2] = factory;
                    }
                    else
                    {
                        isUp = true;
                        nearObj[0] = factory;
                    }
                }
            }
            else
            {
                if (Mathf.Abs(xDiff) > Mathf.Abs(yDiff))
                {
                    isRight = true;
                    nearObj[1] = factory;
                }
                else
                {
                    if (yDiff > 0)
                    {
                        isDown = true;
                        nearObj[2] = factory;
                    }
                    else
                    {
                        isUp = true;
                        nearObj[0] = factory;
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

    public override void ResetNearObj(Structure game)
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

        if (itemObjList.Count >= maxAmount)
            isFull = true;
        else
            isFull = false;
    }

    public void PlayerRootFunc(ItemProps item)
    {
        itemObjList.Remove(item);
        beltGroupMgr.ItemRoot(item);
        item.itemPool.Release(item.gameObject);

        if (itemObjList.Count >= maxAmount)
            isFull = true;
        else
            isFull = false;
    }

    public void ItemRootSync()
    {
        ItemProps item = itemObjList[itemObjList.Count - 1];
        itemObjList.RemoveAt(itemObjList.Count - 1);
        item.itemPool.Release(item.gameObject);

        if (itemObjList.Count >= maxAmount)
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

        if (col != null)
        {
            // 3. A* 그래프 업데이트 (해당 영역을 길막으로 인식시킴)
            Bounds b = col.bounds;
            AstarPath.active.UpdateGraphs(b);
        }

        //gameObject.AddComponent<DynamicGridObstacle>();
        myVision.SetActive(true);
        DataSet();
    }

    public override void SettingClient(int _level, int _beltDir, int objHeight, int objWidth, bool isHostMap, int index)
    {
        beltGroupMgr = GetComponentInParent<BeltGroupMgr>();
        beltGroupMgr.GroupAddBelt(this);
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

        //if (col != null)
        //{
        //    // 3. A* 그래프 업데이트 (해당 영역을 길막으로 인식시킴)
        //    Bounds b = col.bounds;
        //    AstarPath.active.UpdateGraphs(b);
        //}
        //gameObject.AddComponent<DynamicGridObstacle>();
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
        //if(animator)
        //    animator.SetFloat("ModelNum", modelMotion);

        SetBeltAnim();
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
        //animator.enabled = isOn;

        if (isOn)
        {
            animController.Resume();
        }
        else
        {
            animController.Pause();
        }
    }

    public override void DestroyFunc()
    {
        if (preBelt != null)
        {
            preBelt.DelayNearStrBuilt();
        }
        if (nextBelt != null)
        {
            nextBelt.DelayNearStrBuilt();
        }

        base.DestroyFunc();
    }

    public override void ColliderTriggerOnOff(bool isOn)
    {
        col.isTrigger = true;
    }

    public override void RemoveObjClient()
    {
        StopAllCoroutines();

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

        if (IsServer && beltManager)
        {
            beltManager.BeltDivide(beltGroupMgr, gameObject);
        }

        if (GameManager.instance.focusedStructure == this)
        {
            GameManager.instance.focusedStructure = null;
        }

        DestroyFuncServerRpc();
    }
}
