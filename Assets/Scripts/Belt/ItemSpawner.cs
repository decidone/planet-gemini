using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ItemSpawner : SolidFactoryCtrl
{
    public Item itemData;

    [SerializeField]
    List<GameObject> outObj = new List<GameObject>();
    GameObject[] nearObj = new GameObject[4];
    Vector2[] checkPos = new Vector2[4];

    int sendObjNum = 0;
    protected Coroutine setFacDelayCoroutine; // 실행 중인 코루틴을 저장하는 변수

    // Start is called before the first frame update
    void Start()
    {
        dirCount = 4;
        CheckPos();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (!removeState)
        {
            if (!isPreBuilding)
            {
                if (outObj.Count > 0 && !itemSetDelay)
                {
                    if(itemData.name != "emptFilter")
                        SetItem();
                }

                for (int i = 0; i < nearObj.Length; i++)
                {
                    if (nearObj[i] == null)
                    {
                        if (i == 0)
                            CheckNearObj(checkPos[0], 0, obj => StartCoroutine(SetOutObjCoroutine(obj)));
                        else if (i == 1)
                            CheckNearObj(checkPos[1], 1, obj => StartCoroutine(SetOutObjCoroutine(obj)));
                        else if (i == 2)
                            CheckNearObj(checkPos[2], 2, obj => StartCoroutine(SetOutObjCoroutine(obj)));
                        else if (i == 3)
                            CheckNearObj(checkPos[3], 3, obj => StartCoroutine(SetOutObjCoroutine(obj)));
                    }
                }
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
                hitCollider.GetComponent<ItemSpawner>() != GetComponent<ItemSpawner>())
            {
                nearObj[index] = hits[i].collider.gameObject;
                callback(hitCollider.gameObject);
                break;
            }
        }
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

    protected override void SetItem()
    {
        if (setFacDelayCoroutine != null)
        {
            return;
        }

        itemSetDelay = true;

        Structure outFactory = outObj[sendObjNum].GetComponent<Structure>();

        if (outFactory.isFull == false && outFactory.GetComponent<ItemSpawner>() == null)
        {
            if (outObj[sendObjNum].GetComponent<BeltCtrl>())
            {
                ItemProps spawnItem = itemPool.Get();
                if (outFactory.OnBeltItem(spawnItem))
                {
                    SpriteRenderer sprite = spawnItem.GetComponent<SpriteRenderer>();
                    sprite.sprite = itemData.icon;
                    spawnItem.item = itemData;
                    spawnItem.amount = 1;
                    spawnItem.transform.position = transform.position;
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
                StartCoroutine("SetFacDelay", outObj[sendObjNum]);
            }
            else if (outObj[sendObjNum].TryGetComponent(out Production production))
            {
                if (production.CanTakeItem(itemData))
                {
                    StartCoroutine("SetFacDelay", outObj[sendObjNum]);
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
            var t = Mathf.Clamp01(elapsed / (distance / solidFactoryData.SendSpeed[level]));
            spawnItem.transform.position = Vector3.Lerp(spawnItem.transform.position, targetPos, t);

            yield return null;
        }

        if (spawnItem != null && spawnItem.transform.position == targetPos)
        {
            var outFactory = outFac.GetComponent<Structure>();
            outFactory.OnFactoryItem(itemData);
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
    //public override void AddProductionFac(GameObject obj)
    //{
    //    outObj.Add(obj);
    //}
}
