using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

public class MergerCtrl : SolidFactoryCtrl
{
    [SerializeField]
    Sprite[] modelNum = new Sprite[4];
    SpriteRenderer setModel;
    private int prevDirNum = -1; // 이전 방향 값을 저장할 변수

    List<GameObject> inObj = new List<GameObject>();

    [SerializeField]
    List<GameObject> outObj = new List<GameObject>();

    //GameObject[] nearObj = new GameObject[4];

    int getObjNum = 0;

    Vector2[] checkPos = new Vector2[4];

    private Coroutine setFacDelayCoroutine; // 실행 중인 코루틴을 저장하는 변수

    void Start()
    {
        dirCount = 4;
        setModel = GetComponent<SpriteRenderer>();
        base.nearObj = new GameObject[4];
        CheckPos();
    }

    protected override void Update()
    {
        base.Update();
        SetDirNum();
        if (!removeState)
        {
            if (!isPreBuilding)
            {
                if (inObj.Count > 0 && !isFull && !itemGetDelay && checkObj)
                {
                    GetItem();
                }

                if (itemList.Count > 0 && outObj.Count > 0 && !itemSetDelay && checkObj)
                {
                    SetItem();    
                }

                for (int i = 0; i < nearObj.Length; i++)
                {
                    if (nearObj[i] == null)
                    {
                        if (i == 0)
                            CheckNearObj(checkPos[0], 0, obj => StartCoroutine(SetOutObjCoroutine(obj)));
                        else if (i == 1)
                            CheckNearObj(checkPos[1], 1, obj => StartCoroutine(SetInObjCoroutine(obj)));
                        else if (i == 2)
                            CheckNearObj(checkPos[2], 2, obj => StartCoroutine(SetInObjCoroutine(obj)));
                        else if (i == 3)
                            CheckNearObj(checkPos[3], 3, obj => StartCoroutine(SetInObjCoroutine(obj)));
                    }
                }
            }
        }
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
        Vector2[] dirs = { Vector2.up, Vector2.right, Vector2.down, Vector2.left };
        
        for (int i = 0; i < 4; i++)
        {
            checkPos[i] = dirs[(dirNum + i) % 4];
        }
    }

    protected override void CheckNearObj(Vector2 direction, int index, Action<GameObject> callback)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, 1f);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hitCollider = hits[i].collider;
            if (hitCollider.CompareTag("Factory") && !hitCollider.GetComponent<Structure>().isPreBuilding &&
                hitCollider.GetComponent<MergerCtrl>() != GetComponent<MergerCtrl>())
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

        Structure solidFactory = obj.GetComponent<Structure>();
        if (solidFactory == null) yield break;

        inObj.Add(obj);

        BeltCtrl belt = obj.GetComponent<BeltCtrl>();
        if (belt == null) yield break;
    }

    IEnumerator SetOutObjCoroutine(GameObject obj)
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
            else if (obj.GetComponent<SolidFactoryCtrl>())
            {
                outSameList.Add(obj);
                StartCoroutine(OutCheck(obj));
            }
            outObj.Add(obj);
        }
    }

    IEnumerator OutCheck(GameObject otherObj)
    {
        yield return new WaitForSeconds(0.1f);

        Structure otherFacCtrl = otherObj.GetComponent<Structure>();

        foreach (GameObject otherList in otherFacCtrl.outSameList)
        {
            if (otherList == this.gameObject)
            {
                for (int a = outObj.Count - 1; a >= 0; a--)
                {
                    if (otherObj == outObj[a])
                    {
                        outObj.RemoveAt(a);
                        Invoke("RemoveSameOutList", 0.1f);
                        StopCoroutine("SetFacDelay");
                        break;
                    }
                }
            }
        }
    }

    protected override void GetItem()
    {
        itemGetDelay = true;

        if (inObj[getObjNum].TryGetComponent(out BeltCtrl belt) && belt.isItemStop)
        {
            if (belt.itemObjList.Count > 0)
            {
                OnFactoryItem(belt.itemObjList[0]);
                belt.itemObjList[0].transform.position = this.transform.position;
                belt.isItemStop = false;
                belt.itemObjList.RemoveAt(0);
                belt.beltGroupMgr.groupItem.RemoveAt(0);
                belt.ItemNumCheck();
            }

            getObjNum++;
            if (getObjNum >= inObj.Count)
                getObjNum = 0;

            Invoke("DelayGetItem", solidFactoryData.SendDelay);

            itemGetDelay = false;
        }
        else
        {
            getObjNum++;
            if (getObjNum >= inObj.Count)
                getObjNum = 0;

            itemGetDelay = false;
            return;
        }        
    }

    protected override void SetItem()
    {
        if (setFacDelayCoroutine != null)
        {
            return;
        }

        itemSetDelay = true;

        Structure outFactory = outObj[0].GetComponent<Structure>();

        if (outFactory.isFull == false)
        //if (outFactory.CheckOutItemNum() == false)
        {
            if (outObj[0].TryGetComponent(out BeltCtrl beltCtrl))
            {
                ItemProps spawnItem = itemPool.Get();
                if (outFactory.OnBeltItem(spawnItem))
                {
                    SpriteRenderer sprite = spawnItem.GetComponent<SpriteRenderer>();
                    sprite.sprite = itemList[0].icon;
                    spawnItem.item = itemList[0];
                    spawnItem.GetComponent<SortingGroup>().sortingOrder = 2;
                    spawnItem.amount = 1;
                    spawnItem.transform.position = transform.position;
                    spawnItem.isOnBelt = true;
                    spawnItem.setOnBelt = beltCtrl.GetComponent<BeltCtrl>();
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
            else if (outObj[0].GetComponent<SolidFactoryCtrl>())
            {
                setFacDelayCoroutine = StartCoroutine("SetFacDelay");
            }
            else if (outObj[0].TryGetComponent(out Production production))
            {
                if (production.CanTakeItem(itemList[0]))
                {
                    setFacDelayCoroutine = StartCoroutine("SetFacDelay");
                }
            }
            Invoke("DelaySetItem", solidFactoryData.SendDelay);
        }
        itemSetDelay = false;
    }

    IEnumerator SetFacDelay()
    {
        var spawnItem = itemPool.Get();
        var sprite = spawnItem.GetComponent<SpriteRenderer>();
        sprite.color = new Color(1f, 1f, 1f, 0f);
        CircleCollider2D coll = spawnItem.GetComponent<CircleCollider2D>();
        coll.enabled = false;

        spawnItem.transform.position = this.transform.position;

        var targetPos = outObj[0].transform.position;
        var startTime = Time.time;
        var distance = Vector3.Distance(spawnItem.transform.position, targetPos);


        while (spawnItem != null && spawnItem.transform.position != targetPos)
        {
            var elapsed = Time.time - startTime;
            var t = Mathf.Clamp01(elapsed / (distance / solidFactoryData.SendSpeed[level]));
            spawnItem.transform.position = Vector3.Lerp(spawnItem.transform.position, targetPos, t);

            yield return null;
        }

        if (spawnItem != null && spawnItem.transform.position == targetPos)
        {
            if (itemList.Count > 0)
            {
                if (checkObj && outObj.Count > 0 && outObj[0] != null)
                {
                    if (outObj[0].TryGetComponent(out Structure outFactory))
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
    }
    public override void ResetCheckObj(GameObject game)
    {
        base.ResetCheckObj(game);

        for (int i = 0; i < outObj.Count; i++)
        {
            if (outObj[i] == game)
                outObj.Remove(game);
        }
        for (int i = 0; i < inObj.Count; i++)
        {
            if (inObj[i] == game)
                inObj.Remove(game);
        }

        getObjNum = 0;
    }

    void DelaySetItem()
    {
        itemSetDelay = false;
    }

    void DelayGetItem()
    {
        itemGetDelay = false;
    }

}
