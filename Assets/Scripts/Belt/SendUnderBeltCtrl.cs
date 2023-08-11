using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SendUnderBeltCtrl : SolidFactoryCtrl
{
    [SerializeField]
    Sprite[] modelNum;
    SpriteRenderer setModel;

    List<GameObject> inObj = new List<GameObject>();
    public GameObject outObj = null;

    private int prevDirNum = -1; // 이전 방향 값을 저장할 변수
    int getObjNum = 0;

    private Coroutine setFacDelayCoroutine; // 실행 중인 코루틴을 저장하는 변수

    void Start()
    {
        dirCount = 4;
        setModel = GetComponent<SpriteRenderer>();
    }

    protected override void Update()
    {
        base.Update();
        if (!removeState)
        {
            SetDirNum();
            if (!isPreBuilding)
            {

                if (inObj.Count > 0 && !isFull && !itemGetDelay)
                {
                    GetItem();
                }
                if (itemList.Count > 0 && outObj != null && !itemSetDelay)
                {
                    SendItem();
                }

                for (int i = 1; i < nearObj.Length; i++)
                {
                    if (nearObj[i] == null)
                    {
                        if (i == 2)
                            CheckNearObj(checkPos[2], 2, obj => SetInObj(obj));
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
                hitCollider.GetComponent<SendUnderBeltCtrl>() != GetComponent<SendUnderBeltCtrl>())
            {
                nearObj[index] = hits[i].collider.gameObject;
                callback(hitCollider.gameObject);
                break;
            }
        }
    }

    void SetInObj(GameObject obj)
    {
        if (obj.GetComponent<Structure>() != null)
        {
            inObj.Add(obj);
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

                getObjNum++;
                if (getObjNum >= inObj.Count)
                    getObjNum = 0;

                Invoke("DelayGetItem", solidFactoryData.SendDelay);

                itemGetDelay = false;
            }
            else if (belt.isItemStop == false)
            {
                getObjNum++;
                if (getObjNum >= inObj.Count)
                    getObjNum = 0;

                itemGetDelay = false;
                return;
            }
        }
        else
        {
            getObjNum++;
            if (getObjNum >= inObj.Count)
                getObjNum = 0;

            Invoke("DelayGetItem", solidFactoryData.SendDelay);
            itemGetDelay = false;
        }
    }

    protected override void SendItem()
    {
        if (setFacDelayCoroutine != null)
        {
            return;
        }

        itemSetDelay = true;

        Structure outFactory = outObj.GetComponent<Structure>();

        if (outFactory.isFull == false)
        {
            setFacDelayCoroutine = StartCoroutine("SetFacDelay");
        }

        Invoke("DelaySetItem", solidFactoryData.SendDelay);
        itemSetDelay = false;
    }

    IEnumerator SetFacDelay()
    {
        var spawnItem = itemPool.Get();
        SpriteRenderer sprite = spawnItem.GetComponent<SpriteRenderer>();
        sprite.color = new Color(1f, 1f, 1f, 0f);
        CircleCollider2D coll = spawnItem.GetComponent<CircleCollider2D>();
        coll.enabled = false;

        spawnItem.transform.position = this.transform.position;

        var targetPos = outObj.transform.position;
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
                if (checkObj && outObj != null)
                {
                    if (outObj.TryGetComponent(out Structure outFactory))
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

    public void SetOutObj(GameObject Obj)
    {
        if (outObj != null)        
            outObj.GetComponent<GetUnderBeltCtrl>().ResetInObj();

        outObj = Obj;
    }

    public override void ResetCheckObj(GameObject game)
    {
        base.ResetCheckObj(game);

        for (int i = 0; i < inObj.Count; i++)
        {
            if (inObj[i] == game)
            {
                Debug.Log(game.name);
                inObj.Remove(game);

            }
        }
        if (outObj == game)
        {
            Debug.Log(game.name);
            outObj = null;
        }
    }
}
