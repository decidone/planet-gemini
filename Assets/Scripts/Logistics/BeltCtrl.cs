using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Netcode;
using Pathfinding;
using System.Collections;
using Unity.VisualScripting;
using System;

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
    public bool isGameStartItemReady = false;
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
        isGameStartItemReady = true;
    }

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
        spawn.spriteRenderer.sprite = sendItem.icon;
        spawn.spriteRenderer.sortingOrder = 2;
        spawn.item = sendItem;
        spawn.amount = 1;
        spawn.transform.position = pos;
        spawn.isOnBelt = true;
        spawn.setOnBelt = this;

        spawn.SetBeltData(
            NetworkManager.Singleton.ServerTime.Time,
            pos,    // startPos = 저장 위치
            pos,    // endPos = 동일 (Lerp(pos, pos, t) = pos → NaN 없음)
            1f      // duration 0 방지용 임시값
        );

        itemObjList.Add(spawn);
        isFull = itemObjList.Count >= maxAmount;
    }

    public void GameStartItemDataSet()
    {
        double now = NetworkManager.Singleton.ServerTime.Time;
        SetItemDir();
        for (int i = 0; i < itemObjList.Count; i++)
        {
            ItemProps item = itemObjList[i];
            int slotIndex = Mathf.Min(i, nextPos.Length - 1);
            item.SetBeltData(
                now,
                item.transform.position,    // 저장된 위치가 시작점
                nextPos[slotIndex],         // 리스트 순서 = 슬롯 인덱스
                structureData.SendSpeed[level]
            );
        }
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

    public void ItemMove(double serverNow)
    {
        for (int i = 0; i < itemObjList.Count; i++)
        {
            ItemProps item = itemObjList[i];
            double elapsed = serverNow - item.beltEnterTime;

            float t;
            if (item.beltTravelDuration <= 0)
                t = 1f;
            else
                t = Mathf.Clamp01((float)(elapsed / item.beltTravelDuration));

            // NaN 방어
            if (float.IsNaN(t)) t = 1f;

            Vector3 newPos = Vector3.Lerp(item.beltStartPos, item.beltEndPos, t);

            // NaN 방어
            if (!float.IsNaN(newPos.x) && !float.IsNaN(newPos.y))
                item.transform.position = newPos;
        }

        if (itemObjList.Count > 0)
        {
            ItemProps front = itemObjList[0];
            double elapsed = serverNow - front.beltEnterTime;
            isItemStop = front.beltTravelDuration <= 0 || elapsed >= front.beltTravelDuration;

            if (isItemStop)
                TryTransferToNextBelt(serverNow);
        }
    }

    void TryTransferToNextBelt(double serverNow)
    {
        if (nextBelt == null || beltState == BeltState.EndBelt) return;
        if (nextBelt.isFull || nextBelt.isPreBuilding || nextBelt.destroyStart) return;
        if (itemObjList.Count == 0) return;

        ItemProps front = itemObjList[0];

        double idealTransferTime = front.beltEnterTime + front.beltTravelDuration;

        // ★ 핵심 수정: 대기가 있었다면(벨트 새로 연결 등) 현재 시각 기준으로 시작
        // 정상 흐름이면 idealTransferTime ≈ serverNow 이므로 동일하게 동작
        double transferTime = Math.Max(idealTransferTime, serverNow);

        int slotIndex = Mathf.Min(nextBelt.itemObjList.Count, nextBelt.nextPos.Length - 1);

        front.SetBeltData(
            transferTime,
            front.beltEndPos,
            nextBelt.nextPos[slotIndex],
            nextBelt.structureData.SendSpeed[nextBelt.level]
        );
        nextBelt.ReceiveItemLocal(front);
        itemObjList.RemoveAt(0);
        ItemNumCheck();


        for (int i = 0; i < itemObjList.Count; i++)
        {
            ItemProps item = itemObjList[i];
            slotIndex = Mathf.Min(i, nextPos.Length - 1);

            double elapsed = transferTime - item.beltEnterTime;
            float t = Mathf.Clamp01((float)(elapsed / item.beltTravelDuration));
            Vector3 posAtTransfer = Vector3.Lerp(item.beltStartPos, item.beltEndPos, t);

            item.SetBeltData(
                transferTime,
                posAtTransfer,
                nextPos[slotIndex],
                structureData.SendSpeed[level]
            );
        }
    }

    // RPC 없이 로컬에서 직접 수신
    public void ReceiveItemLocal(ItemProps item)
    {
        item.setOnBelt = this;
        itemObjList.Add(item);
        beltGroupMgr?.groupItem.Add(item);  // 필요시
        isFull = itemObjList.Count >= maxAmount;
    }

    public bool OnBeltItem(ItemProps itemObj, Vector2 startPos)
    {
        if (IsServer && itemObjList.Count >= maxAmount) return false;

        double enterTime = NetworkManager.Singleton.ServerTime.Time;
        // 수정
        int slotIndex = Mathf.Min(itemObjList.Count, nextPos.Length - 1);

        itemObj.SetBeltData(
            enterTime,
            startPos,
            nextPos[slotIndex],
            structureData.SendSpeed[level]
        );

        itemObj.setOnBelt = this;
        itemObjList.Add(itemObj);
        beltGroupMgr?.groupItem.Add(itemObj);
        isFull = itemObjList.Count >= maxAmount;
        return true;
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
        item.ClientResetItemProps();

        if (itemObjList.Count >= maxAmount)
            isFull = true;
        else
            isFull = false;
    }

    public void PlayerRootFunc(ItemProps item)
    {
        itemObjList.Remove(item);
        beltGroupMgr.ItemRoot(item);
        item.ClientResetItemProps();

        if (itemObjList.Count >= maxAmount)
            isFull = true;
        else
            isFull = false;
    }

    public void ItemRootSync()
    {
        ItemProps item = itemObjList[itemObjList.Count - 1];
        itemObjList.RemoveAt(itemObjList.Count - 1);
        item.ClientResetItemProps();

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
                if (!itemProps || !itemProps.item)
                    continue;

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

        int posX = (int)transform.position.x;
        int posY = (int)transform.position.y;
        for (int i = 0; i < 4; i++)
        {
            int nearX = posX + oneDirections[i, 0];
            int nearY = posY + oneDirections[i, 1];
            Cell cell = GameManager.instance.GetCellDataFromPosWithoutMap(nearX, nearY);
            if (cell.structure != null)
            {
                cell.structure.ResetNearObj(this);
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
