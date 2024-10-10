using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using Unity.Netcode;

// UTF-8 설정
public class SplitterCtrl : LogisticsCtrl
{
    bool filterOn = false;
    int filterindex = 0;
    LogisticsClickEvent clickEvent;

    [Serializable]
    public struct Filter
    {
        public GameObject outObj;
        public bool isFilterOn;
        public bool isFullFilterOn;
        public bool isItemFilterOn;
        public bool isReverseFilterOn;
        public Item selItem;
    }
    public Filter[] arrFilter = new Filter[3]; // 0 좌 1 상 2 우

    void Start()
    {
        setModel = GetComponent<SpriteRenderer>();
        CheckPos();
        clickEvent = GetComponent<LogisticsClickEvent>();
    }

    protected override void Update()
    {
        base.Update();

        if (!removeState)
        {
            SetDirNum();

            if (isSetBuildingOk)
            {
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
            }               

            if (IsServer && !isPreBuilding && checkObj)
            { 
                if (inObj.Count > 0 && !isFull && !itemGetDelay)
                    GetItem();
                if (itemList.Count > 0 && outObj.Count > 0 && !itemSetDelay)
                {
                    if (filterOn && level > 0) 
                    {
                        FilterSendItem();
                    }
                    else
                    {
                        int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(itemList[0]);
                        SendItem(itemIndex);
                    }
                }
            }
            if (DelaySendList.Count > 0 && !outObj[DelaySendList[0].Item2].GetComponent<Structure>().isFull)
            {
                SendDelayFunc(DelaySendList[0].Item1, DelaySendList[0].Item2, 0);
            }
            if (DelayGetList.Count > 0 && inObj.Count > 0)
            {
                GetDelayFunc(DelayGetList[0], 0);
            }
        }
    }

    protected override void SetDirNum()
    {
        setModel.sprite = modelNum[dirNum + (level * 4)];
        CheckPos();
    }

    [ServerRpc(RequireOwnership = false)]
    public override void ClientConnectSyncServerRpc()
    {
        base.ClientConnectSyncServerRpc();
        for (int a = 0; a < arrFilter.Length; a++)
        {
            if (arrFilter[a].selItem == null)
                continue;
            int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(arrFilter[a].selItem);
            ulong objID = arrFilter[a].outObj.GetComponent<Structure>().ObjFindId();

            ClientFillterSetClientRpc(a, arrFilter[a].isFilterOn, arrFilter[a].isReverseFilterOn, itemIndex, objID);
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

    public void GameStartFillterSet(int num, bool filterOn, bool reverseFilterOn, int itemIndex)
    {
        arrFilter[num].isFilterOn = filterOn;
        arrFilter[num].isReverseFilterOn = reverseFilterOn;
        arrFilter[num].selItem = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
        ItemFilterCheck();
    }

    bool FilterCheck()
    {
        for (int a = 0; a < arrFilter.Length; a++)
        {
            if (arrFilter[a].isFilterOn)                
                return true;
        }

        return false;
    }

    public void ItemFilterCheck()
    {
        filterOn = FilterCheck();
    }

    void FilterArr(GameObject obj, int num)
    {
        arrFilter[num].outObj = obj;
    }


    [ServerRpc(RequireOwnership = false)]
    public void FilterSetServerRpc(int num, bool filterOn, bool reverseFilterOn, int itemIndex)
    {
        FilterSetClientRpc(num, filterOn, reverseFilterOn, itemIndex);
    }

    [ClientRpc]
    public void FilterSetClientRpc(int num, bool filterOn, bool reverseFilterOn, int itemIndex)
    {
        arrFilter[num].isFilterOn = filterOn;
        arrFilter[num].isReverseFilterOn = reverseFilterOn;
        arrFilter[num].selItem = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
        Debug.Log(arrFilter[num].selItem);
        ItemFilterCheck();
        UIReset();
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
        ItemFilterCheck();
        UIReset();
    }

    void UIReset()
    {
        if(clickEvent.sFilterManager != null)
            clickEvent.sFilterManager.UIReset();
    }

    void FilterIndexCheck()
    {
        filterindex++;
        if (filterindex >= arrFilter.Length)
        {
            filterindex = 0;
        }
    }

    void FilterSendItem()
    {
        Filter filter = arrFilter[filterindex];
        Item sendItem = itemList[0];
        Item selectedFilterItem = filter.selItem;

        if (filter.outObj == null || !filter.isFilterOn)
        {
            FilterindexSet();
            return;
        }

        if (filter.isReverseFilterOn && selectedFilterItem == sendItem)
        {
            FilterindexSet();
            return;
        }

        if (!filter.isReverseFilterOn && selectedFilterItem != sendItem)
        {
            FilterindexSet();
            return;
        }        

        GameObject outObject = filter.outObj;
        Structure outFactory = outObject.GetComponent<Structure>();

        if (outFactory.isFull)
        {
            FilterindexSet();
            return;
        }
        else if (outObject.TryGetComponent(out Production production) && !production.CanTakeItem(sendItem))
        {
            FilterindexSet();
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

    [ClientRpc]
    void FilterSetItemClientRpc(int index)
    {
        if (setFacDelayCoroutine != null)
        {
            return;
        }

        itemSetDelay = true;
        Item sendItem = itemList[0];

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
        }
        else if (outObject.GetComponent<LogisticsCtrl>())
        {
            setFacDelayCoroutine = StartCoroutine(SendFacDelayArguments(outObject, sendItem));
        }
        else if (outObject.TryGetComponent(out Production production) && production.CanTakeItem(sendItem))
        {
            setFacDelayCoroutine = StartCoroutine(SendFacDelayArguments(outObject, sendItem));
        }
        Debug.Log("sp");
        itemListRemove();
        ItemNumCheck();
        
        Invoke(nameof(DelaySetItem), structureData.SendDelay[level]);
    }

    bool ItemFilterFullCheck(Item item)
    {
        bool isFacNotFull1 = true;
        bool isFacNotFull2 = true;

        for (int a = 0; a < arrFilter.Length; a++)
        {
            Filter filter = arrFilter[a];
            if (filter.outObj == null) continue;

            if (filter.isFilterOn && filter.isItemFilterOn)
            {
                Structure factoryCtrl = filter.outObj.GetComponent<Structure>();
                if (factoryCtrl.TryGetComponent(out LogisticsCtrl fac))
                {
                    if (!fac.isFull)
                    {
                        if ((!filter.isReverseFilterOn && filter.selItem == item) ||
                            (filter.isReverseFilterOn && filter.selItem != item))
                        {
                            if (!isFacNotFull1)
                            {
                                isFacNotFull2 = false;
                                break;
                            }
                            isFacNotFull1 = false;
                        }
                    }
                }
            }
        }
        return !(isFacNotFull1 && isFacNotFull2);
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
