using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using Unity.Netcode;

// UTF-8 설정
public class SplitterCtrl : LogisticsCtrl
{
    bool canSend = false;
    int filterindex = 0;
    int cantSentItemCount = 0;
    int smartFilterItemIndex = 0;
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

    void Start()
    {
        //setModel = GetComponent<SpriteRenderer>();
        for (int i = 0; i < arrFilter.Length; i++)
        {
            arrFilter[i].isFilterOn = true;
        }
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

            if (IsServer && !isPreBuilding && checkObj)
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
    }


    [ServerRpc(RequireOwnership = false)]
    public void FilterSetServerRpc(int num, bool reverseFilterOn, int itemIndex)
    {
        FilterSetClientRpc(num, true, reverseFilterOn, itemIndex);
    }


    [ServerRpc(RequireOwnership = false)]
    public void FilterSetServerRpc(int num, bool filterOn)
    {
        FilterSetClientRpc(num, filterOn, false, -1);
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
        UIReset();
        CanSendCheck();
    }

    void CanSendCheck()
    {
        bool canSendCheck = false;
        if (level == 1)
        {
            for (int i = 0; i < arrFilter.Length; i++)
            {
                if (arrFilter[i].selItem != null)
                {
                    canSendCheck = true;
                    break;
                }
            }
        }
        else if (level == 0)
        {
            for (int i = 0; i < arrFilter.Length; i++)
            {
                if (arrFilter[i].isFilterOn)
                {
                    canSendCheck = true;
                    break;
                }
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

        Filter filter = arrFilter[filterindex];
        if (smartFilterItemIndex >= itemList.Count)
        {
            smartFilterItemIndex = 0;
        }
        Item sendItem = itemList[smartFilterItemIndex];
        Item selectedFilterItem = filter.selItem;

        if (filter.outObj == null)
        {
            FilterindexSet();
            itemSetDelay = false;
            return;
        }

        if (!isSmart && !filter.isFilterOn)
        {
            FilterindexSet();
            itemSetDelay = false;
            return;
        }

        if (isSmart)
        {
            if (filter.isReverseFilterOn && selectedFilterItem == sendItem)
            {
                FilterindexSet();
                itemSetDelay = false;
                CantSendItemIndexSet();
                return;
            }

            if (!filter.isReverseFilterOn && selectedFilterItem != sendItem)
            {
                FilterindexSet();
                itemSetDelay = false;
                CantSendItemIndexSet();
                return;
            }        
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

        FilterSetItemClientRpc(filterindex);
        FilterindexSet();
    }

    void FilterindexSet()
    {
        filterindex++;
        if (filterindex >= arrFilter.Length)
            filterindex = 0;
    }

    void CantSendItemIndexSet()
    {
        cantSentItemCount++;
        if (cantSentItemCount > 3)
        {
            smartFilterItemIndex++;
            if (smartFilterItemIndex >= itemList.Count)
                smartFilterItemIndex = 0;

            cantSentItemCount = 0;
        }
    }

    [ClientRpc]
    void FilterSetItemClientRpc(int index)
    {
        Item sendItem = itemList[smartFilterItemIndex];

        Filter filter = arrFilter[index];

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
            itemList.RemoveAt(smartFilterItemIndex);
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
        checkObj = false;
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
                StopCoroutine("SendFacDelay");
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

        checkObj = true;
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
