using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GetUnderBeltCtrl : SolidFactoryCtrl
{
    [SerializeField]
    Sprite[] modelNum = new Sprite[4];
    SpriteRenderer setModel;

    public GameObject inObj = null;
    [SerializeField]
    List<GameObject> outObj = new List<GameObject>();

    [SerializeField]
    GameObject[] nearObj = new GameObject[4];

    int getObjNum = 0;

    Vector2[] checkPos = new Vector2[4];

    // Start is called before the first frame update
    void Start()
    {
        dirCount = 4;
        setModel = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        SetDirNum();
        if (!isPreBuilding)
        {
            if (itemList.Count > 0 && outObj.Count > 0 && !itemSetDelay)
            {
                SetItem();
            }

            for (int i = 0; i < nearObj.Length; i++)
            {
                if (nearObj[i] == null)
                {
                    if (i == 0)
                        CheckNearObj(checkPos[0], 0, obj => SetInObj(obj));
                    else if (i == 1)
                        CheckNearObj(checkPos[1], 1, obj => SetOutObj(obj));
                    else if (i == 2)
                        CheckNearObj(checkPos[2], 2, obj => SetOutObj(obj));
                    else if (i == 3)
                        CheckNearObj(checkPos[3], 3, obj => SetOutObj(obj));
                }
            }
        }
    }

    protected override void SetDirNum()
    {
        if (dirNum < 4)
        {
            setModel.sprite = modelNum[dirNum];
            CheckPos();
        }
    }

    protected override void CheckPos()
    {
        Vector2[] dirs = { Vector2.down, Vector2.left, Vector2.up, Vector2.right };

        for (int i = 0; i < 4; i++)
        {
            checkPos[i] = dirs[(dirNum + i) % 4];
        }
    }
    protected override void CheckNearObj(Vector2 direction, int index, Action<GameObject> callback)
    {
        float dist = 0;

        if (index == 0)
            dist = 10;
        else
            dist = 1;

        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, dist);

        for (int i = 0; i < hits.Length; i++)
        {
            if(index != 0)
            {
                Collider2D hitCollider = hits[i].collider;
                if (hitCollider.CompareTag("Factory") &&
                    !hitCollider.GetComponent<FactoryCtrl>().isPreBuilding &&
                    hitCollider.GetComponent<GetUnderBeltCtrl>() != GetComponent<GetUnderBeltCtrl>())
                {
                    nearObj[index] = hits[i].collider.gameObject;
                    callback(hitCollider.gameObject);
                    break;
                }
            }
            else
            {
                Collider2D hitCollider = hits[i].collider;
                if (hitCollider.CompareTag("Factory") &&
                    !hitCollider.GetComponent<FactoryCtrl>().isPreBuilding &&
                    hitCollider.GetComponent<GetUnderBeltCtrl>() != GetComponent<GetUnderBeltCtrl>() &&
                    hitCollider.GetComponent<SendUnderBeltCtrl>() != null)
                {
                    nearObj[index] = hits[i].collider.gameObject;
                    callback(hitCollider.gameObject);
                    break;
                }
            }
        }
    }

    void SetInObj(GameObject obj)
    {
        //if (obj.GetComponent<SolidFactoryCtrl>() != null && obj.GetComponent<SendUnderBeltCtrl>() != null)
        {        
            SendUnderBeltCtrl sendUnderbelt = obj.GetComponent<SendUnderBeltCtrl>();

            if (sendUnderbelt.dirNum == dirNum)
            {
                inObj = obj;
                //sendUnderbelt.outObj = this.gameObject;
                sendUnderbelt.SetOutObj(this.gameObject);
            }
            else
                return;            
        }
    }

    public void ResetInObj()
    {
        nearObj[0] = null;
        inObj = null;
    }

    void SetOutObj(GameObject obj)
    {
        if (obj.GetComponent<SolidFactoryCtrl>() != null)
        {
            if (obj.TryGetComponent(out BeltCtrl belt))
            {
                if (obj.GetComponentInParent<BeltGroupMgr>().nextObj == this.gameObject)
                    return;

                if (belt.beltState == BeltState.SoloBelt || belt.beltState == BeltState.StartBelt)
                {
                    belt.FactoryVecCheck(GetComponentInParent<SolidFactoryCtrl>());
                }
            }

            else
            {
                outSameList.Add(obj);
                StartCoroutine("OutCheck", obj);
            }
            outObj.Add(obj);
        }
    }
    IEnumerator OutCheck(GameObject otherObj)
    {
        yield return new WaitForSeconds(0.1f);

        SolidFactoryCtrl otherFacCtrl = otherObj.GetComponent<SolidFactoryCtrl>();

        foreach (GameObject otherList in otherFacCtrl.outSameList)
        {
            if (otherList == this.gameObject)
            {
                for (int a = outObj.Count - 1; a >= 0; a--)
                {
                    if (otherObj == outObj[a])
                    {
                        outObj.RemoveAt(a);
                        StopCoroutine("SetFacDelay");
                        break;
                    }
                }
            }
        }
    }

    protected override void SetItem()
    {
        itemSetDelay = true;

        SolidFactoryCtrl outFactory = outObj[getObjNum].GetComponent<SolidFactoryCtrl>();

        if (outFactory.isFull == false)
        {
            if (outObj[getObjNum].GetComponent<BeltCtrl>())
            {
                ItemProps spawnItem = itemPool.Get();
                SpriteRenderer sprite = spawnItem.GetComponent<SpriteRenderer>();
                sprite.sprite = itemList[0].icon;
                spawnItem.item = itemList[0];
                spawnItem.amount = 1;
                spawnItem.transform.position = this.transform.position;

                outFactory.OnBeltItem(spawnItem);

                itemList.RemoveAt(0);
                ItemNumCheck();
            }
            else
            {
                StartCoroutine("SetFacDelay", outObj[getObjNum]);
            }

            getObjNum++;
            if (getObjNum >= outObj.Count)
            {
                getObjNum = 0;
            }
            Invoke("DelaySetItem", solidFactoryData.SendDelay);
        }
        else
        {
            getObjNum++;
            if (getObjNum >= outObj.Count)
            {
                getObjNum = 0;
            }

            itemSetDelay = false;
        }
    }

    IEnumerator SetFacDelay(int getObjNum)
    {
        var spawnItem = itemPool.Get();
        SpriteRenderer sprite = spawnItem.GetComponent<SpriteRenderer>();
        sprite.enabled = false;

        spawnItem.transform.position = this.transform.position;

        while (spawnItem.transform.position != outObj[getObjNum].transform.position)
        {
            spawnItem.transform.position = Vector3.MoveTowards(spawnItem.transform.position, outObj[getObjNum].transform.position, solidFactoryData.SendSpeed * Time.deltaTime);

            yield return null;
        }

        if (spawnItem.transform.position == outObj[getObjNum].transform.position)
        {
            if (itemList.Count > 0)
            {
                SolidFactoryCtrl outFactory = outObj[getObjNum].GetComponent<SolidFactoryCtrl>();
                outFactory.OnFactoryItem(itemList[0]);

                itemList.RemoveAt(0);
                ItemNumCheck();
            }
        }
        Destroy(spawnItem.gameObject);
    }

    void DelaySetItem()
    {
        itemSetDelay = false;
    }
}
