using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SplitterCtrl : SolidFactoryCtrl
{
    [SerializeField]
    Sprite[] modelNum = new Sprite[4];
    SpriteRenderer setModel;
    private int prevDirNum = -1; // 이전 방향 값을 저장할 변수

    GameObject inObj = null;
    [SerializeField]
    List<GameObject> outObj = new List<GameObject>();
    [SerializeField]
    GameObject[] nearObj = new GameObject[4];

    int sendObjNum = 0;
    Vector2[] checkPos = new Vector2[4];

    protected Coroutine setFacDelayCoroutine; // 실행 중인 코루틴을 저장하는 변수

    bool filterOn = false;
    int filterindex = 0;
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

    // Start is called before the first frame update
    void Start()
    {
        dirCount = 4;
        setModel = GetComponent<SpriteRenderer>();
        CheckPos();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (!removeState)
        {
            SetDirNum();
            if (!isPreBuilding)
            { 
                if (inObj != null && !isFull && !itemGetDelay)
                    GetItem();
        
                for (int i = 0; i < nearObj.Length; i++)
                {
                    if (nearObj[i] == null)
                    {
                        if (i == 0)
                            CheckNearObj(checkPos[0], 0, obj => StartCoroutine(SetInObjCoroutine(obj)));
                        else if (i == 1)
                            CheckNearObj(checkPos[1], 1, obj => StartCoroutine(SetOutObjCoroutine(obj, 0)));
                        else if (i == 2)
                            CheckNearObj(checkPos[2], 2, obj => StartCoroutine(SetOutObjCoroutine(obj, 1)));
                        else if (i == 3)
                            CheckNearObj(checkPos[3], 3, obj => StartCoroutine(SetOutObjCoroutine(obj, 2)));
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
                        SetItem();
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

    protected override void SetDirNum()
    {
        if (dirNum < 4)
        {
            setModel.sprite = modelNum[dirNum];

            // dirNum 값이 변경됐는지 체크하고, 변경됐으면 CheckPos() 실행
            if (dirNum != prevDirNum)
            {
                CheckPos();
                prevDirNum = dirNum;
            }
        }
    }
    protected override void CheckPos()
    {
        Vector2[] dirs ={ Vector2.down, Vector2.left, Vector2.up, Vector2.right };

        for (int i = 0; i < dirs.Length; i++)
        {
            checkPos[i] = dirs[(i + dirNum) % dirs.Length];
        }
    }

    protected override void CheckNearObj(Vector2 direction, int index, Action<GameObject> callback)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, 1f);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hitCollider = hits[i].collider;
            if (hitCollider.CompareTag("Factory") && !hitCollider.GetComponent<Structure>().isPreBuilding &&
                hitCollider.GetComponent<SplitterCtrl>() != GetComponent<SplitterCtrl>())
            {
                nearObj[index] = hits[i].collider.gameObject;
                callback(hitCollider.gameObject);
                break;
            }
        }
    }

    IEnumerator SetInObjCoroutine(GameObject obj)
    {
        yield return new WaitForSeconds(0.1f);

        if (obj.GetComponent<Structure>() != null)
        {
            inObj = obj;
            if (inObj.TryGetComponent(out BeltCtrl belt) && belt.dirNum != dirNum)
            {
                if (belt.beltState == BeltState.SoloBelt || belt.beltState == BeltState.StartBelt)
                {
                    belt.dirNum = dirNum;
                    belt.BeltModelSet();
                }
            }
        }
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
                    belt.FactoryVecCheck(GetComponentInParent<Structure>());
            }
            else if(obj.GetComponent<SolidFactoryCtrl>())
            {
                outSameList.Add(obj);
                StartCoroutine(OutCheck(obj));
            }
            outObj.Add(obj);
            FilterArr(obj, num);
        }
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
    }

    IEnumerator OutCheck(GameObject otherObj)
    {
        yield return new WaitForSeconds(0.1f);

        Structure otherFacCtrl = otherObj.GetComponent<Structure>();
        GameObject[] outObjArray = outObj.ToArray();

        foreach (GameObject otherList in otherFacCtrl.outSameList)
        {
            if (otherList == this.gameObject)
            {
                for (int a = outObjArray.Length - 1; a >= 0; a--)
                {
                    if (otherObj == outObjArray[a])
                    {
                        for (int b = 0; b < arrFilter.Length; b++)
                        {
                            if (arrFilter[b].outObj == outObjArray[a])
                            {
                                FilterReset(b);
                            }
                        }
                        outObj.RemoveAt(a);
                        Invoke("RemoveSameOutList", 0.1f);
                        break;
                    }
                }
            }
        }
    }
    protected override void GetItem()
    {
        itemGetDelay = true;
        if (inObj.TryGetComponent(out BeltCtrl belt) && belt.isItemStop)
        {
            if(belt.itemObjList.Count > 0)
            {
                OnFactoryItem(belt.itemObjList[0]);
                belt.itemObjList[0].transform.position = transform.position;
                belt.isItemStop = false;
                belt.itemObjList.RemoveAt(0);
                belt.beltGroupMgr.GroupItem.RemoveAt(0);
                belt.ItemNumCheck();
            }
            Invoke("DelayGetItem", solidFactoryData.SendDelay);
        }
        itemGetDelay = false;
    }


    protected override void SetItem()
    {
        if (setFacDelayCoroutine != null)
        {
            return;
        }

        itemSetDelay = true;

        Structure outFactory = outObj[sendObjNum].GetComponent<Structure>();

        if (outFactory.isFull == false)        
        //if (outFactory.CheckOutItemNum() == false)
        {
            if (outObj[sendObjNum].GetComponent<BeltCtrl>())
            {
                ItemProps spawnItem = itemPool.Get();
                if (outFactory.OnBeltItem(spawnItem))
                {
                    SpriteRenderer sprite = spawnItem.GetComponent<SpriteRenderer>();
                    sprite.sprite = itemList[0].icon;
                    spawnItem.item = itemList[0];
                    spawnItem.amount = 1;
                    spawnItem.transform.position = transform.position;

                    itemList.RemoveAt(0);
                    ItemNumCheck();
                }
                else
                {
                    OnDestroyItem(spawnItem);
                    itemSetDelay = false;
                    return;
                }
            }
            else if (outObj[sendObjNum].GetComponent<SolidFactoryCtrl>())
            {
                setFacDelayCoroutine = StartCoroutine("SetFacDelay", outObj[sendObjNum]);
            }
            else if (outObj[sendObjNum].TryGetComponent(out Production production))
            {
                if (production.CanTakeItem(itemList[0]))
                {
                    setFacDelayCoroutine = StartCoroutine("SetFacDelay", outObj[sendObjNum]);
                }
            }

            sendObjNum++;
            if (sendObjNum >= outObj.Count)
            {
                sendObjNum = 0;
            }
            Invoke("DelaySetItem", solidFactoryData.SendDelay);
        }
        else
        {
            sendObjNum++;
            if (sendObjNum >= outObj.Count)
            {
                sendObjNum = 0;
            }

            itemSetDelay = false;
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

        Debug.Log(index);

        itemSetDelay = true;
        Item sendItem = itemList[0];
        Structure outFactory = null;

        Filter filter = arrFilter[index];

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
            if(outFactory.OnBeltItem(spawnItem))
            {
                SpriteRenderer sprite = spawnItem.GetComponent<SpriteRenderer>();
                sprite.sprite = sendItem.icon;
                spawnItem.item = sendItem;
                spawnItem.amount = 1;
                spawnItem.transform.position = transform.position;
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

    //void FilterSetItem()
    //{
    //    if (setFacDelayCoroutine != null)
    //    {
    //        return;
    //    }

    //    itemSetDelay = true;
    //    Item sendItem = itemList[0];
    //    int listIdx = 0;
    //    Structure outFactory = null;

    //    for (int i = 0; i < arrFilter.Length; i++)
    //    {
    //        int index = (sendFilterObjNum + i) % arrFilter.Length;
    //        Filter filter = arrFilter[index];
    //        bool isFound = false;

    //        if (filter.outObj == null || filter.outObj.GetComponent<Structure>().isFull || !filter.isFilterOn)
    //        {
    //            continue;
    //        }

    //        if (filter.isItemFilterOn)
    //        {
    //            bool isReverseFilter = filter.isReverseFilterOn;
    //            Item selectedFilterItem = filter.selItem;

    //            for (int a = 0; a < itemList.Count; a++)
    //            {
    //                bool isMatched = selectedFilterItem == itemList[a];
    //                if (isReverseFilter)
    //                {
    //                    if (!isMatched)
    //                    {
    //                        isFound = true;
    //                        sendItem = itemList[a];
    //                        listIdx = a;
    //                        break;
    //                    }
    //                }
    //                else
    //                {
    //                    if (isMatched)
    //                    {
    //                        isFound = true;
    //                        sendItem = itemList[a];
    //                        listIdx = a;
    //                        break;
    //                    }
    //                }
    //            }

    //            Debug.Log(i +" : "+ isFound + " : "+ listIdx);
    //            if (!isFound)
    //                continue;
    //        }
    //        else if (filter.isFullFilterOn)
    //        {
    //            for (int a = 0; a < itemList.Count; a++)
    //            {
    //                if (!ItemFilterFullCheck(itemList[a]))
    //                {
    //                    sendItem = itemList[a];
    //                    listIdx = a;
    //                    isFound = true;
    //                    break;
    //                }
    //            }

    //            if (!isFound)
    //                continue;
    //        }

    //        //if (filter.isItemFilterOn)
    //        //{
    //        //    if (!filter.isReverseFilterOn)
    //        //    {
    //        //        for (int a = 0; a < itemList.Count; a++)
    //        //        {
    //        //            if (filter.selItem == itemList[a])
    //        //            {
    //        //                sendItem = itemList[a];
    //        //                listIdx = a;
    //        //                isFound = true;
    //        //                break;
    //        //            }
    //        //        }
    //        //        if (!isFound) continue;
    //        //    }
    //        //    else
    //        //    {
    //        //        for (int a = 0; a < itemList.Count; a++)
    //        //        {
    //        //            bool isOtherFilterItem = false;

    //        //            if (filter.selItem != itemList[a])
    //        //            {
    //        //                for (int b = 0; b < arrFilter.Length; b++)
    //        //                {
    //        //                    if (arrFilter[b].isItemFilterOn && !arrFilter[b].isReverseFilterOn && arrFilter[b].selItem == itemList[a])
    //        //                    {
    //        //                        isOtherFilterItem = true;
    //        //                        continue;
    //        //                    }
    //        //                }
    //        //                if (isOtherFilterItem) continue;

    //        //                sendItem = itemList[a];
    //        //                listIdx = a;
    //        //                isFound = true;
    //        //                break;
    //        //            }
    //        //        }

    //        //        if (isFound == false) continue;
    //        //    }
    //        //}
    //        //else if (filter.isFullFilterOn)
    //        //{
    //        //    for (int a = 0; a < itemList.Count; a++)
    //        //    {
    //        //        if (!ItemFilterFullCheck(itemList[a]))
    //        //        {
    //        //            sendItem = itemList[a];
    //        //            listIdx = a;
    //        //            isFound = true;
    //        //            break;
    //        //        }
    //        //    }
    //        //    if (isFound == false) continue;
    //        //}

    //        GameObject outObject = filter.outObj;
    //        outFactory = outObject.GetComponent<Structure>();

    //        if (outObject.TryGetComponent(out BeltCtrl beltCtrl))
    //        {
    //            ItemProps spawnItem = itemPool.Get();
    //            SpriteRenderer sprite = spawnItem.GetComponent<SpriteRenderer>();
    //            sprite.sprite = sendItem.icon;
    //            spawnItem.item = sendItem;
    //            spawnItem.amount = 1;
    //            spawnItem.transform.position = transform.position;

    //            outFactory.OnBeltItem(spawnItem);
    //        }
    //        else if (outObject.GetComponent<SolidFactoryCtrl>())
    //        {
    //            setFacDelayCoroutine = StartCoroutine("SetFacDelay", outObject);
    //        }
    //        else if (outObject.TryGetComponent(out Production production))
    //        {
    //            if (production.CanTakeItem(sendItem))
    //            {
    //                setFacDelayCoroutine = StartCoroutine("SetFacDelay", outObject);
    //            }
    //        }
    //        itemList.RemoveAt(listIdx);
    //        ItemNumCheck();
    //    }

    //    sendFilterObjNum = (sendFilterObjNum + arrFilter.Length) % arrFilter.Length;
    //    itemSetDelay = false;
    //}

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
        Debug.Log(isFacNotFull1 + " : " + isFacNotFull2);
        return !(isFacNotFull1 && isFacNotFull2);
    }

    IEnumerator SetFacDelay(GameObject outFac)
    {
        var spawnItem = itemPool.Get();
        var sprite = spawnItem.GetComponent<SpriteRenderer>();
        sprite.color = new Color(1f, 1f, 1f, 0f);

        spawnItem.transform.position = transform.position;

        var targetPos = outFac.transform.position;
        var startTime = Time.time;
        var distance = Vector3.Distance(spawnItem.transform.position, targetPos);

        while (spawnItem != null && spawnItem.transform.position != targetPos)
        {
            var elapsed = Time.time - startTime;
            var t = Mathf.Clamp01(elapsed / (distance / solidFactoryData.SendSpeed));
            spawnItem.transform.position = Vector3.Lerp(spawnItem.transform.position, targetPos, t);

            //sprite.color = new Color(1f, 1f, 1f, t);

            yield return null;
        }

        if (spawnItem != null && spawnItem.transform.position == targetPos)
        {
            if (itemList.Count > 0)
            {
                var outFactory = outFac.GetComponent<Structure>();
                outFactory.OnFactoryItem(itemList[0]);

                itemList.RemoveAt(0);

                ItemNumCheck();
            }
        }

        if (spawnItem != null)
        {
            sprite.color = new Color(1f, 1f, 1f, 1f);
            setFacDelayCoroutine = null;
            itemPool.Release(spawnItem);
        }
    }

    void DelaySetItem()
    {
        itemSetDelay = false;
    }     
    void DelayGetItem()
    {
        itemGetDelay = false;
    }
    //public override void AddProductionFac(GameObject obj)
    //{
    //    outObj.Add(obj);
    //}
    public override void RemoveObj()
    {
        base.RemoveObj();
    }
}
