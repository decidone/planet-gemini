using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;
using System.Linq;

// UTF-8 설정
public class SplitterCtrl : LogisticsCtrl
{
    bool canSend = false;
    int filterIndex = 0;
    LogisticsClickEvent clickEvent;

    [Serializable]
    public struct Filter
    {
        public GameObject outObj;
        public bool isFilterOn;
        public bool isReverseFilterOn;
        public Item selItem;
    }
    public Filter[] arrFilter = new Filter[3]; // 0 좌 1 상 2 우
    List<int> recentItemSend = new List<int>();
    int smartFilterItemIndex = 0;

    void Start()
    {
        //setModel = GetComponent<SpriteRenderer>();
        CanSendCheck();
        clickEvent = GetComponent<LogisticsClickEvent>();

        StrBuilt();
    }

    protected override void Update()
    {
        base.Update();

        if (!removeState)
        {
            //SetDirNum();

            //if (isSetBuildingOk)
            //{
            //    for (int i = 0; i < nearObj.Length; i++)
            //    {
            //        if (nearObj[i] == null)
            //        {
            //            if (i == 0)
            //                CheckNearObj(checkPos[0], 0, obj => StartCoroutine(SetOutObjCoroutine(obj, 1)));
            //            else if (i == 1)
            //                CheckNearObj(checkPos[1], 1, obj => StartCoroutine(SetOutObjCoroutine(obj, 2)));
            //            else if (i == 2)
            //                CheckNearObj(checkPos[2], 2, obj => StartCoroutine(SetInObjCoroutine(obj)));
            //            else if (i == 3)
            //                CheckNearObj(checkPos[3], 3, obj => StartCoroutine(SetOutObjCoroutine(obj, 0)));
            //        }
            //    }
            //}

            if (IsServer && !isPreBuilding)
            { 
                if (inObj.Count > 0 && !isFull && !itemGetDelay)
                    GetItem();
                if (canSend && itemList.Count > 0 && outObj.Count > 0 && !itemSetDelay)
                {
                    if(level == 1)
                        FilterSendItem(true);
                    else if(level == 0)
                        FilterSendItem(false);
                }
            }
            if (DelaySendList.Count > 0 && outObj.Count > 0 && !outObj[DelaySendList[0].Item2].GetComponent<Structure>().isFull)
            {
                SendDelayFunc(DelaySendList[0].Item1, DelaySendList[0].Item2, 0);
            }
            if (DelayGetList.Count > 0 && inObj.Count > 0)
            {
                GetDelayFunc(DelayGetList[0], 0);
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
            for (int i = 0; i < nearObj.Length; i++)
            {
                if (nearObj[i] == null)
                {
                    if (i == 0)
                        CheckNearObj(checkPos[0], 0, obj => StartCoroutine(SetOutObjCoroutine(obj, 1)));
                    else if (i == 1)
                        CheckNearObj(checkPos[1], 1, obj => StartCoroutine(SetOutObjCoroutine(obj, 2)));
                    else if (i == 2)
                        CheckNearObj(checkPos[2], 2, obj => StartCoroutine(SetInObjCoroutine(obj)));
                    else if (i == 3)
                        CheckNearObj(checkPos[3], 3, obj => StartCoroutine(SetOutObjCoroutine(obj, 0)));
                }
            }
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
        for (int i = 0; i < nearObj.Length; i++)
        {
            if (nearObj[i] == null)
            {
                if (i == 0)
                    CheckNearObj(checkPos[0], 0, obj => StartCoroutine(SetOutObjCoroutine(obj, 1)));
                else if (i == 1)
                    CheckNearObj(checkPos[1], 1, obj => StartCoroutine(SetOutObjCoroutine(obj, 2)));
                else if (i == 2)
                    CheckNearObj(checkPos[2], 2, obj => StartCoroutine(SetInObjCoroutine(obj)));
                else if (i == 3)
                    CheckNearObj(checkPos[3], 3, obj => StartCoroutine(SetOutObjCoroutine(obj, 0)));
            }
        }
        setModel.sprite = modelNum[dirNum + (level * 4)];
    }

    [ServerRpc(RequireOwnership = false)]
    public override void ClientConnectSyncServerRpc()
    {
        base.ClientConnectSyncServerRpc();
        for (int a = 0; a < arrFilter.Length; a++)
        {
            int itemIndex = -1;
            if (arrFilter[a].selItem != null)
            {
                itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(arrFilter[a].selItem);
            }

            if(arrFilter[a].outObj != null)
            {
                arrFilter[a].outObj.TryGetComponent(out Structure str);
                ulong objID = str.ObjFindId();
                ClientFillterSetClientRpc(a, arrFilter[a].isFilterOn, arrFilter[a].isReverseFilterOn, itemIndex, objID);
            }
            else
            {
                ClientFillterSetClientRpc(a, arrFilter[a].isFilterOn, arrFilter[a].isReverseFilterOn, itemIndex);
            }
        }
    }

    [ClientRpc]
    void ClientFillterSetClientRpc(int num, bool filterOn, bool reverseFilterOn, int itemIndex, ulong objID)
    {
        if (IsServer)
            return;

        NetworkObject obj = NetworkObjManager.instance.FindNetworkObj(objID);
        arrFilter[num].outObj = obj.gameObject;
        GameStartFillterSet(num, filterOn, reverseFilterOn, itemIndex);
    }

    [ClientRpc]
    void ClientFillterSetClientRpc(int num, bool filterOn, bool reverseFilterOn, int itemIndex)
    {
        if (IsServer)
            return;

        GameStartFillterSet(num, filterOn, reverseFilterOn, itemIndex);
    }

    public void GameStartFillterSet(int num, bool filterOn, bool reverseFilterOn, int itemIndex)
    {
        arrFilter[num].isFilterOn = filterOn;
        arrFilter[num].isReverseFilterOn = reverseFilterOn;
        if (itemIndex != -1)
        {
            arrFilter[num].selItem = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
        }
    }

    void FilterArr(GameObject obj, int num)
    {
        arrFilter[num].outObj = obj;
        recentItemSend.Clear();
    }


    [ServerRpc(RequireOwnership = false)]
    public void FilterSetServerRpc(int num, bool filterOn , bool reverseFilterOn = false, int itemIndex = -1)
    {
        FilterSetClientRpc(num, filterOn, reverseFilterOn, itemIndex);
    }

    [ClientRpc]
    public void FilterSetClientRpc(int num, bool filterOn, bool reverseFilterOn, int itemIndex)
    {
        arrFilter[num].isFilterOn = filterOn;
        arrFilter[num].isReverseFilterOn = reverseFilterOn;
        if (itemIndex != -1)
        {
            arrFilter[num].selItem = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
        }
        recentItemSend.Clear();
        UIReset();
        CanSendCheck();
    }

    void CanSendCheck()
    {
        bool canSendCheck = false;

        for (int i = 0; i < arrFilter.Length; i++)
        {
            if (arrFilter[i].isFilterOn)
            {
                canSendCheck = true;
                break;
            }
        }
        canSend = canSendCheck;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SlotResetServerRpc(int num)
    {
        SlotResetClientRpc(num);
    }

    [ClientRpc]
    public void SlotResetClientRpc(int num)
    {
        arrFilter[num].isFilterOn = false;
        arrFilter[num].isReverseFilterOn = false;
        arrFilter[num].selItem = null;
        UIReset();
    }

    void UIReset()
    {
        if(clickEvent.sFilterManager != null)
            clickEvent.sFilterManager.UIReset();
    }

    void FilterSendItem(bool isSmart)
    {
        itemSetDelay = true;
        Filter filter = arrFilter[filterIndex];
        if (smartFilterItemIndex >= itemList.Count)
        {
            smartFilterItemIndex = 0;
        }
        Item sendItem = itemList[smartFilterItemIndex];
        Item selectedFilterItem = filter.selItem;

        if (!isSmart)
        {
            if (filter.outObj == null || !filter.isFilterOn)
            {
                FilterindexSet();
                itemSetDelay = false;
                return;
            }

            GameObject outObject = filter.outObj;
            Structure outFactory = outObject.GetComponent<Structure>();

            if (outFactory.isFull)
            {
                FilterindexSet();
                itemSetDelay = false;
                return;
            }
            else if (outObject.TryGetComponent(out Production production) && !production.CanTakeItem(sendItem))
            {
                FilterindexSet();
                itemSetDelay = false;
                return;
            }
        }
        else
        {
            Dictionary<int, List<int>> canSendIndex = new Dictionary<int, List<int>>() // 가중치, 방향인덱스
            {
                { 0, new List<int>() }, // selItem == sendItem
                { 1, new List<int>() }, // reverse 필터가 켜져 있고 sendItem != selItem
                { 2, new List<int>() }  // 조건 없음
            };

            for (int i = 0; i < arrFilter.Length; i++)
            {
                filter = arrFilter[i];
                if (!IsValidOutput(filter, sendItem)) continue;

                int weight = GetFilterWeight(filter, sendItem);
                if (weight >= 0)
                {
                    canSendIndex[weight].Add(i);
                }
            }

            // 유효한 인덱스가 아무 것도 없을 경우
            if (canSendIndex.All(pair => pair.Value.Count == 0))
            {
                smartFilterItemIndex++;
                itemSetDelay = false;
                return;
            }

            // 우선순위 높은 가중치부터 처리
            foreach (var kv in canSendIndex.OrderBy(pair => pair.Key))
            {
                var indexList = kv.Value;
                if (indexList.Count == 0) continue;
                filterIndex = (indexList.Count == 1)
                    ? indexList[0]
                    : GetLeastRecentlyUsedIndex(indexList);

                if (recentItemSend.Count > 5)
                {
                    recentItemSend.Clear();
                }

                recentItemSend.Add(filterIndex);
                break;
            }
        }
        FilterSetItemClientRpc(filterIndex, smartFilterItemIndex);
        FilterindexSet();
    }

    // 연결된 오브젝트가 유효하고, 꽉 차지 않았고, 해당 아이템을 받을 수 있는지
    bool IsValidOutput(Filter filter, Item sendItem)
    {
        if (!filter.isFilterOn || !filter.outObj) return false;

        var structure = filter.outObj.GetComponent<Structure>();
        if (structure && structure.isFull) return false;

        if (filter.outObj.TryGetComponent(out Production production) && !production.CanTakeItem(sendItem))
            return false;

        return true;
    }

    // 필터 조건에 따른 가중치 반환
    int GetFilterWeight(Filter filter, Item sendItem)
    {
        if (filter.selItem)
        {
            if (filter.isReverseFilterOn)
                return (filter.selItem == sendItem) ? -1 : 1;
            else
                return (filter.selItem == sendItem) ? 0 : -1;
        }
        else
        {
            return filter.isReverseFilterOn ? -1 : 2;
        }
    }

    // 최근 전송 기록을 기준으로 가장 적게 사용된 인덱스 선택
    int GetLeastRecentlyUsedIndex(List<int> candidates)
    {
        var costMap = new Dictionary<int, int>();
        foreach (var index in candidates)
            costMap[index] = 0;

        foreach (var recent in recentItemSend)
        {
            if (costMap.ContainsKey(recent))
                costMap[recent]++;
        }

        // 최소 사용 횟수 계산
        int minUsage = costMap.Values.Min();

        // 최소 사용 후보 목록 추출
        var minCandidates = costMap
            .Where(kv => kv.Value == minUsage)
            .Select(kv => kv.Key)
            .ToList();

        // recentItemSend 중 가장 먼저 등장한 인덱스를 우선으로 선택
        foreach (var recent in recentItemSend)
        {
            if (minCandidates.Contains(recent))
                return recent;
        }

        // recentItemSend에 없는 경우, 후보 리스트 순서대로 반환
        return minCandidates.First();
    }

    void FilterindexSet()
    {
        filterIndex++;
        if (filterIndex >= arrFilter.Length)
            filterIndex = 0;
    }

    (bool, int) CheckFilterItem(Item item)
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i] == item)
            {
                return (true, i);
            }
        }
        return (false, -1);
    }

    bool OthFilterCheck(Item item, int filterIndex)
    {
        for (int i = 0; i < arrFilter.Length; i++)
        {
            if (i == filterIndex)
                continue;
            if (arrFilter[i].selItem == item && arrFilter[i].isFilterOn && !arrFilter[i].isReverseFilterOn) 
            {
                return false;
            }
        }
        return true;
    }

    [ClientRpc]
    void FilterSetItemClientRpc(int outObjIndex, int itemIndex)
    {
        Item sendItem = itemList[itemIndex];

        Filter filter = arrFilter[outObjIndex];

        GameObject outObject = filter.outObj;

        if (outObject.TryGetComponent(out BeltCtrl beltCtrl))
        {
            var itemPool = ItemPoolManager.instance.Pool.Get();
            spawnItem = itemPool.GetComponent<ItemProps>();

            if (beltCtrl.OnBeltItem(spawnItem))
            {
                SpriteRenderer sprite = spawnItem.GetComponent<SpriteRenderer>();
                sprite.sprite = sendItem.icon;
                sprite.sortingOrder = 2;
                spawnItem.item = sendItem;
                spawnItem.amount = 1;
                spawnItem.transform.position = transform.position;
                spawnItem.isOnBelt = true;
                spawnItem.setOnBelt = beltCtrl;
            }
            itemList.RemoveAt(itemIndex);
        }
        else if (outObject.GetComponent<LogisticsCtrl>())
        {
            SendFacDelay(outObject, sendItem);
        }
        else if (outObject.TryGetComponent(out Production production) && production.CanTakeItem(sendItem))
        {
            SendFacDelay(outObject, sendItem);
        }
        ItemNumCheck();
        
        Invoke(nameof(DelaySetItem), sendDelay);
    }

    IEnumerator SetOutObjCoroutine(GameObject obj, int num)
    {
        yield return new WaitForSeconds(0.1f);
        //yield return null;

        if (obj.GetComponent<WallCtrl>())
            yield break;

        if (obj.GetComponent<Structure>() != null)
        {
            if (obj.TryGetComponent(out BeltCtrl belt))
            {
                if (obj.GetComponentInParent<BeltGroupMgr>().nextObj == this.gameObject)
                    yield break;

                belt.FactoryPosCheck(GetComponentInParent<Structure>());
            }
            else
            {
                outSameList.Add(obj);
                StartCoroutine(OutCheck(obj));
            }
            if (!outObj.Contains(obj))
                outObj.Add(obj);
            StartCoroutine(UnderBeltConnectCheck(obj));
            FilterArr(obj, num);
        }
    }

    protected override IEnumerator OutCheck(GameObject otherObj)
    {
        yield return new WaitForSeconds(0.1f);

        if (otherObj.TryGetComponent(out Structure otherFacCtrl))
        {
            if (otherFacCtrl.outSameList.Contains(this.gameObject) && outSameList.Contains(otherObj))
            {
                if (otherObj.GetComponent<Production>())
                    yield break;

                for (int i = 0; i < arrFilter.Length; i++)
                {
                    if (arrFilter[i].outObj == otherObj)
                    {
                        FilterArr(null, i);
                    }
                }
                outObj.Remove(otherObj); 
                Invoke(nameof(RemoveSameOutList), 0.1f);
                StopCoroutine(nameof(SendFacDelay));
            }
        }
    }
    protected override IEnumerator UnderBeltConnectCheck(GameObject game)
    {
        yield return new WaitForSeconds(0.1f);
        bool isReomveFilter = false;

        if (game.TryGetComponent(out GetUnderBeltCtrl getUnder))
        {
            if (!getUnder.outObj.Contains(this.gameObject) && inObj.Contains(game))
            {
                inObj.Remove(game);
                isReomveFilter = true;
            }
        }
        else if (game.TryGetComponent(out SendUnderBeltCtrl sendUnder))
        {
            if (!sendUnder.inObj.Contains(this.gameObject) && outObj.Contains(game))
            {
                outObj.Remove(game);
                outSameList.Remove(game);
                isReomveFilter = true;
            }
        }

        if (isReomveFilter)
        {
            for (int i = 0; i < arrFilter.Length; i++)
            {
                if (arrFilter[i].outObj == game)
                {
                    FilterArr(null, i);
                }
            }
        }
    }
    public override StructureSaveData SaveData()
    {
        StructureSaveData data = base.SaveData();

        for (int a = 0; a < arrFilter.Length; a++)
        {
            FilterSaveData filterSaveData = new FilterSaveData();
            filterSaveData.filterItemIndex = GeminiNetworkManager.instance.GetItemSOIndex(arrFilter[a].selItem);
            filterSaveData.filterOn = arrFilter[a].isFilterOn;
            filterSaveData.filterInvert = arrFilter[a].isReverseFilterOn;
            data.filters.Add(filterSaveData);
        }

        return data;
    }
}
