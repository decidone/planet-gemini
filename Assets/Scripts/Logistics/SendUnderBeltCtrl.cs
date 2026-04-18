using System.Collections;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Steamworks.Ugc;

// UTF-8 설정
public class SendUnderBeltCtrl : LogisticsCtrl
{
    float sendDist;
    List<(int, float)> sendingItems = new List<(int, float)>(); // 아이템 인덱스, 아이템 전송 남은 시간
    Coroutine sendCoroutine;
    float[] sendTimes = new float[] { 1.2f, 0.4f, 0.2f }; // 레벨별 전송 시간

    void Start()
    {
        isStartCalled = true;
        if (isCellCalled)
            StrBuilt();
    }

    protected override void Update()
    {
        base.Update();
        if (!removeState)
        {
            if (IsServer && !isPreBuilding)
            {
                if (inObj.Count > 0 && !isFull && !itemGetDelay && sendingItems.Count < sendDist * 4)
                {
                    GetItem();
                }
                if (itemList.Count > 0 && outObj.Count > 0)
                {
                    int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(itemList[0]);
                    SendingItem(itemIndex, sendDist * sendTimes[level]);
                }
            }
        }
    }

    public void EndRenderer()
    {
        if (outObj.Count > 0)
        {
            outObj[0].TryGet(out GetUnderBeltCtrl get);
            get.EndRenderer();
        }
    }

    [ClientRpc]
    public override void UpgradeFuncClientRpc()
    {
        //base.UpgradeFuncClientRpc();
        UpgradeFunc();

        setModel.sprite = modelNum[dirNum + (level * 4)];
    }

    public override void StrBuilt()
    {
        base.StrBuilt();

        float dist = 10;

        for (int i = 1; i <= dist; i++)
        {
            Cell cell = GameManager.instance.GetCellDataFromPosWithoutMap(
                (int)transform.position.x + (int)checkPos[0].x * i,
                (int)transform.position.y + (int)checkPos[0].y * i
            );

            if (cell.structure == null || cell.structure.destroyStart)
                continue;

            Structure str = cell.structure;

            if (str.TryGet(out GetUnderBeltCtrl getUnderBelt))
            {
                if (getUnderBelt.dirNum == dirNum)
                {
                    getUnderBelt.NearStrBuilt();
                    return;
                }
            }
        }
    }

    public override void NearStrBuilt()
    {
        // 건물을 지었을 때나 근처에 새로운 건물이 지어졌을 때 동작
        // 변경사항이 생기면 DelayNearStrBuiltCoroutine()에도 반영해야 함
        if (IsServer)
        {
            CheckPos();
            if (!nearObj[2])
                CheckNearObj(checkPos[2], 2, obj => StartCoroutine(SetInObjCoroutine(obj)));
            setModel.sprite = modelNum[dirNum + (level * 4)];
        }
        else
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

        CheckPos();
        if (!nearObj[2])
            CheckNearObj(checkPos[2], 2, obj => StartCoroutine(SetInObjCoroutine(obj)));
        setModel.sprite = modelNum[dirNum + (level * 4)];
    }

    protected override IEnumerator SetInObjCoroutine(Structure obj)
    {
        yield return new WaitForSeconds(0.1f);

        if (obj)
        {
            if (obj.TryGet<BeltCtrl>(out var belt))
            {
                if (belt.beltGroupMgr.nextObj != this)
                    yield break;
                if (belt.beltState != BeltState.EndBelt && belt.beltState != BeltState.SoloBelt)
                    yield break;

                belt.FactoryPosCheck(this);
            }
            inObj.Add(obj);
        }
    }

    [ServerRpc]
    protected override void SendItemServerRpc(int itemIndex, int outObjIndex)
    {
        SendItemClientRpc(itemIndex, outObjIndex);
    }

    protected override void SendItemFunc(int itemIndex, int outObjIndex)
    {
        Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);

        if (!outObj[0].isFull)
        {
            SendFacDelay(outObj[0], item);
        }

        Invoke(nameof(DelaySetItem), sendDelay);
    }

    protected override void SendFacDelay(Structure outFac, Item item)
    {
        if (sendingItems.Count > 0)
        {
            if (outObj.Count > 0 && outFac)
            {
                outFac.OnFactoryItem(item);
            }

            sendingItems.RemoveAt(0);
        }

        if(sendingItems.Count >= maxAmount)
            isFull = true;
        else
            isFull = false;
    }

    public void SetOutObj(Structure obj)
    {
        if (outObj.Count > 0)
        {
            outObj[0].Get<GetUnderBeltCtrl>().ResetInObj();
            outObj.Remove(outObj[0]);
        }
        nearObj[0] = obj;
        if (!outObj.Contains(obj))
        {            
            outObj.Add(obj);
            sendDist = Vector2.Distance(transform.position, obj.transform.position) - 1;
            maxAmount = Mathf.CeilToInt(sendDist * 4);
        }
    }

    [ClientRpc]
    public override void SettingClientRpc(int _level, int _beltDir, int objHeight, int objWidth, bool isHostMap, int index)
    {
        level = _level;
        dirNum = _beltDir;
        height = objHeight;
        width = objWidth;
        buildingIndex = index;
        isInHostMap = isHostMap;
        settingEndCheck = true;
        SetBuild();
        ColliderTriggerOnOff(true);

        if (col != null)
        {
            // 3. A* 그래프 업데이트 (해당 영역을 길막으로 인식시킴)
            Bounds b = col.bounds;
            AstarPath.active.UpdateGraphs(b);
        }
        //gameObject.AddComponent<DynamicGridObstacle>();
        myVision.SetActive(true);
        DataSet();

        if (energyUse)
        {
            GameObject TriggerObj = new GameObject("Trigger");
            CircleCollider2D coll = TriggerObj.AddComponent<CircleCollider2D>();
            coll.isTrigger = true;
            TriggerObj.transform.position = Vector3.zero;
            StartCoroutine(Move(TriggerObj));
        }
        soundManager.PlaySFX(gameObject, "structureSFX", "BuildingSound");
    }

    protected override void SendItem(int itemIndex)
    {
        if (itemIndex < 0) return;

        itemSetDelay = true;

        if (outObj.Count <= sendItemIndex)
        {
            SendItemIndexSet();
            itemSetDelay = false;
            return;
        }
        else
        {
            if (outObj[0].isFull || outObj[0].destroyStart || outObj[0].isPreBuilding)
            {
                SendItemIndexSet();
                Invoke(nameof(ItemSetDelayReset), 0.05f);
                return;
            }
            else if (outObj[0].TryGet(out Production production))
            {
                Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
                if (!production.CanTakeItem(item))
                {
                    SendItemIndexSet();
                    Invoke(nameof(ItemSetDelayReset), 0.05f);
                    return;
                }
            }
            else if (!outObj[0].canTakeItem)
            {
                SendItemIndexSet();
                Invoke(nameof(ItemSetDelayReset), 0.05f);
                return;
            }
        }

        SendItemServerRpc(itemIndex, sendItemIndex);
        SendItemIndexSet();
    }

    void SendingItem(int itemIndex, float sendTime)
    {
        SendingItemsAddSyncServerRpc(itemIndex, sendTime);
        itemList.RemoveAt(0);
        ItemNumCheck();
        if (sendCoroutine == null)
        {
            sendCoroutine = StartCoroutine(SendingItemCor());
        }

        if (sendingItems.Count >= maxAmount)
            isFull = true;
        else
            isFull = false;
    }

    IEnumerator SendingItemCor()
    {
        while (sendingItems.Count > 0)
        {
            for (int i = sendingItems.Count - 1; i >= 0; i--)
            {
                var (itemIndex, timeLeft) = sendingItems[i];
                timeLeft -= Time.deltaTime;

                if (timeLeft <= 0)
                {
                    SendItem(itemIndex);
                }
                else
                {
                    sendingItems[i] = (itemIndex, timeLeft);
                }
            }
            yield return null; // Update랑 동일한 주기
        }
        sendCoroutine = null;
    }

    public override void ColliderTriggerOnOff(bool isOn)
    {
        col.isTrigger = true;
    }

    public override void Focused()
    {
        if (outObj.Count > 0)
        {
            outObj[0].Get<GetUnderBeltCtrl>().StartRenderer();
        }
    }

    public override void DisableFocused()
    {
        if (outObj.Count > 0)
        {
            outObj[0].Get<GetUnderBeltCtrl>().EndRenderer();
        }
    }

    [ClientRpc]
    public override void RemoveObjClientRpc()
    {
        StopAllCoroutines();

        if (InfoUI.instance.str == this)
            InfoUI.instance.SetDefault();

        for (int i = 0; i < nearObj.Length; i++)
        {
            if (nearObj[i])
            {
                nearObj[i].ResetNearObj(this);
            }
        }

        EndRenderer();
        
        if (GameManager.instance.focusedStructure == this)
        {
            GameManager.instance.focusedStructure = null;
        }

        DestroyFuncServerRpc();
    }

    protected override void ItemDrop()
    {
        if (itemList.Count > 0)
        {
            foreach (Item item in itemList)
            {
                ItemToItemProps(item, 1);
            }
        }

        if (sendingItems.Count > 0)
        {
            for (int i = 0; i < sendingItems.Count; i++)
            {
                Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(sendingItems[i].Item1);
                ItemToItemProps(item, 1);
            }
        }
    }

    public override StructureSaveData SaveData()
    {
        StructureSaveData data = base.SaveData();

        data.sendUnderBeltItems = new List<(int, float)>(sendingItems);

        return data;
    }

    public void LoadSendingItems(List<(int, float)> data)
    {
        sendingItems = new List<(int, float)>(data);

        if (sendingItems.Count >= maxAmount)
        {
            isFull = true;
        }
    }

    public override Dictionary<Item, int> PopUpItemCheck()
    {
        if (sendingItems.Count > 0)
        {
            Dictionary<Item, int> returnDic = new Dictionary<Item, int>();
            foreach (var data in sendingItems)
            {
                Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(data.Item1);
                if (!returnDic.ContainsKey(item))
                    returnDic.Add(item, 1);
                else
                {
                    int currentValue = returnDic[item];
                    int newValue = currentValue + 1;
                    returnDic[item] = newValue;
                }
            }

            return returnDic;
        }
        else
            return null;
    }


    public override void ItemNumCheck()
    {
        if (sendingItems.Count >= maxAmount)
        {
            isFull = true;
        }
        else
            isFull = false;        
    }

    public override void GameStartItemSet(int itemIndex)
    {
        Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
        itemList.Add(item);
    }

    [ClientRpc]
    protected override void OnFactoryItemClientRpc(int itemIndex)
    {
        Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
        itemList.Add(item);
    }

    [ServerRpc]
    void SendingItemsAddSyncServerRpc(int itemIndex, float time)
    {
        SendingItemsAddSyncClientRpc(itemIndex, time);
    }

    [ClientRpc]
    void SendingItemsAddSyncClientRpc(int itemIndex, float time)
    {
        sendingItems.Add((itemIndex, time));
    }

    [ServerRpc(RequireOwnership = false)]
    public override void ItemSyncServerRpc()
    {
        ItemListClearClientRpc();
        SendingItemsListClearServerRpc();

        for (int i = 0; i < itemList.Count; i++)
        {
            int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(itemList[i]);
            ItemSyncClientRpc(itemIndex);
        }

        for (int i = 0; i < sendingItems.Count; i++)
        {
            int itemIndex = sendingItems[i].Item1;
            float time = sendingItems[i].Item2;
            SendingItemsAddSyncClientRpc(itemIndex, time);
        }
    }

    [ServerRpc]
    public void SendingItemsListClearServerRpc()
    {
        SendingItemsListClearClientRpc();
    }

    [ClientRpc]
    public void SendingItemsListClearClientRpc()
    {
        if (!IsServer)
            sendingItems.Clear();
    }


}
