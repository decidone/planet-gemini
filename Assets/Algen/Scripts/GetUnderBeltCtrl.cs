using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GetUnderBeltCtrl : SolidFactoryCtrl
{
    public GameObject inObj = null;
    List<GameObject> outObj = new List<GameObject>();

    [SerializeField]
    GameObject[] nearObj = new GameObject[4];

    int getObjNum = 0;

    Vector2[] checkPos = new Vector2[4];

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SetDirNum();
        if (itemList.Count > 0 && outObj.Count > 0)
        {
            if (itemSetDelay == false)
                StartCoroutine("SetItem");
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

    void SetDirNum()
    {
        if (dirNum < 4)
        {
            //setModel.sprite = modelNum[dirNum];
            CheckPos();
        }
    }

    void CheckPos()
    {
        Vector2[] dirs = { Vector2.down, Vector2.left, Vector2.up, Vector2.right };

        for (int i = 0; i < 4; i++)
        {
            checkPos[i] = dirs[(dirNum + i) % 4];
        }
    }
    void CheckNearObj(Vector2 direction, int index, Action<GameObject> callback)
    {
        float dist = 0;

        if (index == 0)
            dist = 10;
        else
            dist = 1;

        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, dist);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hitCollider = hits[i].collider;
            if (hitCollider.CompareTag("Factory") &&
                hitCollider.GetComponent<GetUnderBeltCtrl>() != GetComponent<GetUnderBeltCtrl>())
            {
                nearObj[index] = hits[i].collider.gameObject;
                callback(hitCollider.gameObject);
                break;
            }
        }
    }

    void SetInObj(GameObject obj)
    {
        if (obj.GetComponent<SolidFactoryCtrl>() != null && obj.GetComponent<SendUnderBeltCtrl>() != null)
        {        
            SendUnderBeltCtrl sendUnderbelt = obj.GetComponent<SendUnderBeltCtrl>();

            if (sendUnderbelt.dirNum == dirNum)
            {
                inObj = obj;
                sendUnderbelt.outObj[0] = this.gameObject;
            }
            else
                return;            
        }
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

    IEnumerator SetItem()
    {
        itemSetDelay = true;

        SolidFactoryCtrl outFactory = outObj[getObjNum].GetComponent<SolidFactoryCtrl>();

        if (outFactory.isFull == false)
        {
            if (outObj[getObjNum].GetComponent<BeltCtrl>() != null)
            {
                ItemProps spawnItem = itemPool.Get();
                SpriteRenderer sprite = spawnItem.GetComponent<SpriteRenderer>();
                sprite.sprite = itemList[0].icon;
                spawnItem.item = itemList[0];
                spawnItem.amount = 1;
                spawnItem.transform.position = this.transform.position;

                //outObj[getObjNum].GetComponent<BeltCtrl>().beltGroupMgr.GroupItem.Add(spawnItem);
                
                outFactory.OnBeltItem(spawnItem);
            }
            else if (outObj[getObjNum].GetComponent<BeltCtrl>() == null)
            {
                StartCoroutine("SetFacDelay", getObjNum);
                //outFactory.OnFactoryItem(itemList[0]);
            }    

            itemList.RemoveAt(0);
            ItemNumCheck();

            getObjNum++;
            if (getObjNum >= outObj.Count)
                getObjNum = 0;

            yield return new WaitForSeconds(solidFactoryData.SendDelay);
            itemSetDelay = false;
        }
        else if (outFactory.isFull == true)
        {
            getObjNum++;
            if (getObjNum >= outObj.Count)
                getObjNum = 0;

            itemSetDelay = false;
            yield break;
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
}