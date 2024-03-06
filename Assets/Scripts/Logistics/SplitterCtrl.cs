using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

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

            if (!isPreBuilding && checkObj)
            { 
                if (inObj.Count > 0 && !isFull && !itemGetDelay)
                    GetItem();
                if (itemList.Count > 0 && outObj.Count > 0 && !itemSetDelay)
                {
                    if (filterOn && level > 0) 
                    {
                        FilterSetItem(filterindex);
                    }
                    else
                    {
                        SendItem(itemList[0]);
                    }
                }
            }
        }
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

    public void FilterSet(int num, bool filterOn, bool fullFilterOn, bool itemFilterOn, bool reverseFilterOn, Item itemNum)
    {
        arrFilter[num].isFilterOn = filterOn;
        arrFilter[num].isFullFilterOn = fullFilterOn;
        arrFilter[num].isItemFilterOn = itemFilterOn;
        arrFilter[num].isReverseFilterOn = reverseFilterOn;
        arrFilter[num].selItem = itemNum;
    }

    public void SlotReset(int num)
    {
        arrFilter[num].isFilterOn = false;
        arrFilter[num].isFullFilterOn = false;
        arrFilter[num].isItemFilterOn = false;
        arrFilter[num].isReverseFilterOn = false;
        arrFilter[num].selItem = null;
        ItemFilterCheck();
    }

    void FilterReset(int num)
    {
        arrFilter[num].outObj = null;
        arrFilter[num].isFilterOn = false;
        arrFilter[num].isFullFilterOn = false;
        arrFilter[num].isItemFilterOn = false;
        arrFilter[num].isReverseFilterOn = false;
        arrFilter[num].selItem = null;
        if (clickEvent.sFilterManager != null)
        {
            clickEvent.sFilterManager.UiReset();
        }
    }

    void FilterIndexCheck()
    {
        filterindex++;
        if (filterindex >= arrFilter.Length)
        {
            filterindex = 0;
        }
    }

    void FilterSetItem(int index)
    {
        if (setFacDelayCoroutine != null)
        {
            return;
        }

        itemSetDelay = true;
        Item sendItem = itemList[0];
        Structure outFactory = null;

        Filter filter = arrFilter[index];

        if (filter.outObj == null)
        {
            FilterIndexCheck();
            DelaySetItem();
            return;
        }

        bool isReverseFilter = filter.isItemFilterOn && filter.isReverseFilterOn;
        Item selectedFilterItem = filter.selItem;

        if (!filter.isFullFilterOn)
        {
            if (isReverseFilter && selectedFilterItem == sendItem)
            {
                FilterIndexCheck();
                DelaySetItem();
                return;
            }

            if (!isReverseFilter && selectedFilterItem != sendItem)
            {
                FilterIndexCheck();
                DelaySetItem();
                return;
            }
        }
        else if (!isReverseFilter && filter.isFullFilterOn && ItemFilterFullCheck(sendItem))
        {
            FilterIndexCheck();
            DelaySetItem();
            return;
        }

        GameObject outObject = filter.outObj;
        outFactory = outObject.GetComponent<Structure>();

        if (outFactory.isFull == false)
        {
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
                else
                {
                    FilterIndexCheck();
                    DelaySetItem();
                    return;
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
            itemList.RemoveAt(0);
            ItemNumCheck();
        }

        FilterIndexCheck();
        Invoke(nameof(DelaySetItem), structureData.SendDelay);
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
}
