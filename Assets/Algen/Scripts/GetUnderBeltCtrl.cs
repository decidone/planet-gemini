using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetUnderBeltCtrl : SolidFactoryCtrl
{
    public GameObject inObj = null;
    List<GameObject> outObj = new List<GameObject>();
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

        if (nearObj[0] == null)
            UpObjCheck();
        if (nearObj[1] == null)
            RightObjCheck();
        if (nearObj[2] == null)
            DownObjCheck();
        if (nearObj[3] == null)
            LeftObjCheck();
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
        if (dirNum == 0)
        {
            checkPos[0] = -transform.up;
            checkPos[1] = -transform.right;
            checkPos[2] = transform.up;
            checkPos[3] = transform.right;
        }
        else if (dirNum == 1)
        {
            checkPos[0] = -transform.right;
            checkPos[1] = transform.up;
            checkPos[2] = transform.right;
            checkPos[3] = -transform.up;
        }
        else if (dirNum == 2)
        {
            checkPos[0] = transform.up;
            checkPos[1] = transform.right;
            checkPos[2] = -transform.up;
            checkPos[3] = -transform.right;
        }
        else if (dirNum == 3)
        {
            checkPos[0] = transform.right;
            checkPos[1] = -transform.up;
            checkPos[2] = -transform.right;
            checkPos[3] = transform.up;
        }
    }
    void UpObjCheck()
    {
        if (nearObj[0] == null)
        {
            RaycastHit2D[] upHits = Physics2D.RaycastAll(this.gameObject.transform.position, checkPos[0], 10f);

            for (int a = 0; a < upHits.Length; a++)
            {
                if (upHits[a].collider.GetComponent<GetUnderBeltCtrl>() != this.gameObject.GetComponent<GetUnderBeltCtrl>())
                {
                    if (upHits[a].collider.CompareTag("Factory"))
                    {
                        if (upHits[a].collider.GetComponent<SendUnderBeltCtrl>())
                        {
                            nearObj[0] = upHits[a].collider.gameObject;
                            SetInObj(nearObj[0]);
                        }
                    }
                }
            }
        }
    }

    void RightObjCheck()
    {
        if (nearObj[1] == null)
        {
            RaycastHit2D[] rightHits = Physics2D.RaycastAll(this.gameObject.transform.position, checkPos[1], 1f);

            for (int a = 0; a < rightHits.Length; a++)
            {
                if (rightHits[a].collider.GetComponent<GetUnderBeltCtrl>() != this.gameObject.GetComponent<GetUnderBeltCtrl>())
                {
                    if (rightHits[a].collider.CompareTag("Factory"))
                    {
                        nearObj[1] = rightHits[a].collider.gameObject;
                        SetOutObj(nearObj[1]);
                    }
                }
            }
        }
    }
    void DownObjCheck()
    {
        if (nearObj[2] == null)
        {
            RaycastHit2D[] downHits = Physics2D.RaycastAll(this.gameObject.transform.position, checkPos[2], 1f);

            for (int a = 0; a < downHits.Length; a++)
            {
                if (downHits[a].collider.GetComponent<GetUnderBeltCtrl>() != this.gameObject.GetComponent<GetUnderBeltCtrl>())
                {
                    if (downHits[a].collider.CompareTag("Factory"))
                    {
                        nearObj[2] = downHits[a].collider.gameObject;
                        SetOutObj(nearObj[2]);
                    }
                }
            }
        }
    }

    void LeftObjCheck()
    {
        if (nearObj[3] == null)
        {
            RaycastHit2D[] leftHits = Physics2D.RaycastAll(this.gameObject.transform.position, checkPos[3], 1f);

            for (int a = 0; a < leftHits.Length; a++)
            {
                if (leftHits[a].collider.GetComponent<GetUnderBeltCtrl>() != this.gameObject.GetComponent<GetUnderBeltCtrl>())
                {
                    if (leftHits[a].collider.CompareTag("Factory"))
                    {
                        nearObj[3] = leftHits[a].collider.gameObject;
                        SetOutObj(nearObj[3]);
                    }
                }
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
            if (obj.GetComponent<BeltCtrl>() != null)
            {
                if (obj.GetComponentInParent<BeltGroupMgr>().nextObj == this.gameObject)
                    return;

                BeltCtrl belt = obj.GetComponent<BeltCtrl>();
                if (belt.beltState == BeltState.SoloBelt || belt.beltState == BeltState.StartBelt)
                {
                    belt.FactoryVecCheck(GetComponentInParent<SolidFactoryCtrl>());
                }
            }

            else if (obj.GetComponent<BeltCtrl>() == null)
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
