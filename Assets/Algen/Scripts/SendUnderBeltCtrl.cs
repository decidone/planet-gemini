using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SendUnderBeltCtrl : SolidFactoryCtrl
{
    //public Sprite[] modelNum = new Sprite[4];
    //SpriteRenderer setModel;

    List<GameObject> inObj = new List<GameObject>();
    public List<GameObject> outObj = new List<GameObject>();

    GameObject[] nearObj = new GameObject[4];
    private int prevDirNum = -1; // ���� ���� ���� ������ ����

    int getObjNum = 0;

    Vector2[] checkPos = new Vector2[4];

    private Coroutine setFacDelayCoroutine; // ���� ���� �ڷ�ƾ�� �����ϴ� ����

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SetDirNum();
        if (inObj.Count > 0 && !isFull && !itemGetDelay)
        {
            GetItem();
        }
        if (itemList.Count > 0 && outObj.Count > 0 && !itemSetDelay)
        {
            SetItem();           
        }

        for (int i = 1; i < nearObj.Length; i++)
        {
            if (nearObj[i] == null)
            {
                if (i == 1)
                    CheckNearObj(checkPos[1], 1, obj => SetInObj(obj));
                else if (i == 2)
                    CheckNearObj(checkPos[2], 2, obj => SetInObj(obj));
                else if (i == 3)
                    CheckNearObj(checkPos[3], 3, obj => SetInObj(obj));
            }
        }
    }

    void SetDirNum()
    {
        if (dirNum < 4)
        {
            //setModel.sprite = modelNum[dirNum];
            if (dirNum != prevDirNum)
            {
                CheckPos();
                prevDirNum = dirNum;
            }
        }
    }

    void CheckPos()
    {
        Vector2[] dirs = { Vector2.up, Vector2.right, Vector2.down, Vector2.left };

        for (int i = 0; i < 4; i++)
        {
            checkPos[i] = dirs[(dirNum + i) % 4];
        }
    }

    void CheckNearObj(Vector2 direction, int index, Action<GameObject> callback)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, 1f);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hitCollider = hits[i].collider;
            if (hitCollider.CompareTag("Factory") &&
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
        if (obj.GetComponent<SolidFactoryCtrl>() != null)
        {
            inObj.Add(obj);

            if (obj.GetComponent<BeltCtrl>() != null)
            {
                BeltCtrl belt = obj.GetComponent<BeltCtrl>();

                int beltReNum = 0;

                if (dirNum == 0)
                {
                    if (nearObj[1] == obj)
                        beltReNum = 3;
                    else if (nearObj[2] == obj)
                        beltReNum = 0;
                    else if (nearObj[3] == obj)
                        beltReNum = 1;
                }
                if (dirNum == 1)
                {
                    if (nearObj[1] == obj)
                        beltReNum = 0;
                    else if (nearObj[2] == obj)
                        beltReNum = 1;
                    else if (nearObj[3] == obj)
                        beltReNum = 2;
                }
                if (dirNum == 2)
                {
                    if (nearObj[1] == obj)
                        beltReNum = 1;
                    else if (nearObj[2] == obj)
                        beltReNum = 2;
                    else if (nearObj[3] == obj)
                        beltReNum = 3;
                }
                if (dirNum == 3)
                {
                    if (nearObj[1] == obj)
                        beltReNum = 2;
                    else if (nearObj[2] == obj)
                        beltReNum = 3;
                    else if (nearObj[3] == obj)
                        beltReNum = 0;
                }
                if (beltReNum != belt.dirNum)
                {
                    belt.dirNum = beltReNum;
                    belt.BeltModelSet();
                }
            }
        }
    }

    void GetItem()
    {
        itemGetDelay = true;

        if (inObj[getObjNum].TryGetComponent(out BeltCtrl belt))
        {
            if (belt.isItemStop == true)
            {
                OnFactoryItem(belt.itemObjList[0]);
                belt.itemObjList[0].transform.position = this.transform.position;
                belt.isItemStop = false;
                belt.itemObjList.RemoveAt(0);
                belt.beltGroupMgr.GroupItem.RemoveAt(0);
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

    void SetItem()
    {
        if (setFacDelayCoroutine != null)
        {
            return;
        }

        itemSetDelay = true;

        SolidFactoryCtrl outFactory = outObj[0].GetComponent<SolidFactoryCtrl>();

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

        spawnItem.transform.position = this.transform.position;

        var targetPos = outObj[0].transform.position;
        var startTime = Time.time;
        var distance = Vector3.Distance(spawnItem.transform.position, targetPos);

        while (spawnItem != null && spawnItem.transform.position != targetPos)
        {
            var elapsed = Time.time - startTime;
            var t = Mathf.Clamp01(elapsed / (distance / solidFactoryData.SendSpeed));
            spawnItem.transform.position = Vector3.Lerp(spawnItem.transform.position, targetPos, t);

            sprite.color = new Color(1f, 1f, 1f, t);

            yield return null;
        }

        if (spawnItem != null && spawnItem.transform.position == targetPos)
        {
            if (itemList.Count > 0)
            {
                var outFactory = outObj[0].GetComponent<SolidFactoryCtrl>();
                outFactory.OnFactoryItem(itemList[0]);

                itemList.RemoveAt(0);

                ItemNumCheck();
            }
        }

        if (spawnItem != null)
        {
            itemPool.Release(spawnItem);
            setFacDelayCoroutine = null;
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

}