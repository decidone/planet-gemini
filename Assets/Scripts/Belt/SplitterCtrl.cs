using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

public class SplitterCtrl : SolidFactoryCtrl
{
    bool filterOn = false;
    int filterindex = 0;
    SolidFacClickEvent clickEvent;

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
    public Filter[] arrFilter = new Filter[3]; // 0 аб 1 ╩С 2 ©Л

    void Start()
    {
        dirCount = 4;
        setModel = GetComponent<SpriteRenderer>();
        CheckPos();
        clickEvent = GetComponent<SolidFacClickEvent>();
    }

    protected override void Update()
    {
        base.Update();

        if (!removeState)
        {
            SetDirNum();
            if (!isPreBuilding && checkObj)
            { 
                if (inObj.Count > 0 && !isFull && !itemGetDelay)
                    GetItem();
        
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

                if (itemList.Count > 0 && outObj.Count > 0 && !itemSetDelay)
                {
                    if (filterOn)
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
            itemSetDelay = false;
            return;
        }

        bool isReverseFilter = filter.isItemFilterOn && filter.isReverseFilterOn;
        Item selectedFilterItem = filter.selItem;

        if (!filter.isFullFilterOn)
        {
            if (isReverseFilter && selectedFilterItem == sendItem)
            {
                FilterIndexCheck();
                itemSetDelay = false;
                return;
            }

            if (!isReverseFilter && selectedFilterItem != sendItem)
            {
                FilterIndexCheck();
                itemSetDelay = false;
                return;
            }
        }
        else if (!isReverseFilter && filter.isFullFilterOn && ItemFilterFullCheck(sendItem))
        {
            FilterIndexCheck();
            itemSetDelay = false;
            return;
        }

        GameObject outObject = filter.outObj;
        outFactory = outObject.GetComponent<Structure>();

        if (outObject.TryGetComponent(out BeltCtrl beltCtrl))
        {
            ItemProps spawnItem = itemPool.Get();
            if (outFactory.OnBeltItem(spawnItem))
            {
                SpriteRenderer sprite = spawnItem.GetComponent<SpriteRenderer>();
                sprite.sprite = sendItem.icon;
                spawnItem.item = sendItem;
                spawnItem.GetComponent<SortingGroup>().sortingOrder = 2;
                spawnItem.amount = 1;
                spawnItem.transform.position = transform.position;
                spawnItem.isOnBelt = true;
                spawnItem.setOnBelt = beltCtrl;
            }
            else
            {
                OnDestroyItem(spawnItem);
                FilterIndexCheck();
                itemSetDelay = false;
                return;
            }
        }
        else if (outObject.GetComponent<SolidFactoryCtrl>())
        {
            setFacDelayCoroutine = StartCoroutine("SetFacDelay", outObject);
        }
        else if (outObject.TryGetComponent(out Production production) && production.CanTakeItem(sendItem))
        {
            setFacDelayCoroutine = StartCoroutine("SetFacDelay", outObject);
        }

        itemList.RemoveAt(0);
        ItemNumCheck();

        FilterIndexCheck();
        itemSetDelay = false;
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
                if (factoryCtrl.TryGetComponent(out SolidFactoryCtrl fac))
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
        yield return new WaitForSeconds(0.1f);

        if (obj.GetComponent<Structure>() != null)
        {
            if (obj.TryGetComponent(out BeltCtrl belt))
            {
                if (obj.GetComponentInParent<BeltGroupMgr>().nextObj == this.gameObject)
                    yield break;
                if (belt.beltState == BeltState.SoloBelt || belt.beltState == BeltState.StartBelt)
                    belt.FactoryPosCheck(GetComponentInParent<Structure>());
            }
            else
            {
                outSameList.Add(obj);
                StartCoroutine(OutCheck(obj));
            }
            outObj.Add(obj);
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
                        FilterReset(i);
                    }
                }
                outObj.Remove(otherObj);
                Invoke("RemoveSameOutList", 0.1f);
                StopCoroutine("SetFacDelay");
            }
        }
    }

    IEnumerator SetFacDelay(GameObject outFac)
    {
        var spawnItem = itemPool.Get();
        var sprite = spawnItem.GetComponent<SpriteRenderer>();
        sprite.color = new Color(1f, 1f, 1f, 0f);
        CircleCollider2D coll = spawnItem.GetComponent<CircleCollider2D>();
        coll.enabled = false;

        spawnItem.transform.position = transform.position;

        var targetPos = outFac.transform.position;
        var startTime = Time.time;
        var distance = Vector3.Distance(spawnItem.transform.position, targetPos);

        while (spawnItem != null && spawnItem.transform.position != targetPos)
        {
            var elapsed = Time.time - startTime;
            var t = Mathf.Clamp01(elapsed / (distance / solidFactoryData.SendSpeed[level]));
            spawnItem.transform.position = Vector3.Lerp(spawnItem.transform.position, targetPos, t);

            //sprite.color = new Color(1f, 1f, 1f, t);

            yield return null;
        }

        if (spawnItem != null && spawnItem.transform.position == targetPos)
        {
            if (itemList.Count > 0)
            {
                if (checkObj && outFac != null)
                {
                    if (outFac.TryGetComponent(out Structure outFactory))
                    {   
                        outFactory.OnFactoryItem(itemList[0]);
                    }
                }
                else
                {
                    spawnItem.item = itemList[0];
                    spawnItem.amount = 1; 
                    playerInven.Add(spawnItem.item, spawnItem.amount);
                    sprite.color = new Color(1f, 1f, 1f, 1f);
                    coll.enabled = true;
                    itemPool.Release(spawnItem);
                    spawnItem = null;
                }

                itemList.RemoveAt(0);
                ItemNumCheck();
            }
        }

        if (spawnItem != null)
        {
            sprite.color = new Color(1f, 1f, 1f, 1f);
            coll.enabled = true;
            setFacDelayCoroutine = null;
            itemPool.Release(spawnItem);
        }
        else
            setFacDelayCoroutine = null;
    }

    public override void ResetCheckObj(GameObject game)
    {
        for (int i = 0; i < arrFilter.Length; i++)
        {
            if (arrFilter[i].outObj == game)
            {
                FilterReset(i);
            }
        }
        base.ResetCheckObj(game);
    }
}
