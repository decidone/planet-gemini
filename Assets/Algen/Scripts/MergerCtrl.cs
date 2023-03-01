using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MergerCtrl : SolidFactoryCtrl
{
    [SerializeField]
    Sprite[] modelNum = new Sprite[4];
    SpriteRenderer setModel;

    List<GameObject> inObj = new List<GameObject>();
    GameObject outObj = null;

    GameObject[] nearObj = new GameObject[4];

    int getObjNum = 0;

    Vector2[] checkPos = new Vector2[4];

    // Start is called before the first frame update
    void Start()
    {
        setModel = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        SetDirNum();
        if (inObj.Count > 0 && isFull == false)
        {
            if (itemGetDelay == false)
                StartCoroutine("GetItem");
        }
        if (itemList.Count > 0 && outObj != null)
        {
            if (outObj.GetComponent<SolidFactoryCtrl>() != null)
            {
                if (outObj.GetComponent<SolidFactoryCtrl>().isFull == false)
                {
                    if (itemSetDelay == false)
                        StartCoroutine("SetItem");
                }
            }
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
            setModel.sprite = modelNum[dirNum];
            CheckPos();
        }
    }

    void CheckPos()
    {
        if (dirNum == 0)
        {
            checkPos[0] = transform.up;
            checkPos[1] = transform.right;
            checkPos[2] = -transform.up;
            checkPos[3] = -transform.right;
        }
        else if (dirNum == 1)
        {
            checkPos[0] = transform.right;
            checkPos[1] = -transform.up;
            checkPos[2] = -transform.right;
            checkPos[3] = transform.up;
        }
        else if (dirNum == 2)
        {
            checkPos[0] = -transform.up;
            checkPos[1] = -transform.right;
            checkPos[2] = transform.up;
            checkPos[3] = transform.right;
        }
        else if (dirNum == 3)
        {
            checkPos[0] = -transform.right;
            checkPos[1] = transform.up;
            checkPos[2] = transform.right;
            checkPos[3] = -transform.up;
        }
    }

    void UpObjCheck()
    {
        RaycastHit2D[] upHits = Physics2D.RaycastAll(this.gameObject.transform.position, checkPos[0], 1f);

        for (int a = 0; a < upHits.Length; a++)
        {
            if (upHits[a].collider.GetComponent<MergerCtrl>() != this.gameObject.GetComponent<MergerCtrl>())
            {
                if (upHits[a].collider.CompareTag("Factory"))
                {
                    nearObj[0] = upHits[a].collider.gameObject;
                    SetOutObj(nearObj[0]);
                }
            }
        }
    }

    void RightObjCheck()
    {
        RaycastHit2D[] rightHits = Physics2D.RaycastAll(this.gameObject.transform.position, checkPos[1], 1f);

        for (int a = 0; a < rightHits.Length; a++)
        {
            if (rightHits[a].collider.GetComponent<MergerCtrl>() != this.gameObject.GetComponent<MergerCtrl>())
            {
                if (rightHits[a].collider.CompareTag("Factory"))
                {
                    nearObj[1] = rightHits[a].collider.gameObject;
                    SetInObj(nearObj[1]);
                }
            }
        }        
    }
    void DownObjCheck()
    {
        RaycastHit2D[] downHits = Physics2D.RaycastAll(this.gameObject.transform.position, checkPos[2], 1f);

        for (int a = 0; a < downHits.Length; a++)
        {
            if (downHits[a].collider.GetComponent<MergerCtrl>() != this.gameObject.GetComponent<MergerCtrl>())
            {
                if (downHits[a].collider.CompareTag("Factory"))
                {
                    nearObj[2] = downHits[a].collider.gameObject;
                    SetInObj(nearObj[2]);
                }
            }
        }
    }

    void LeftObjCheck()
    {
        RaycastHit2D[] leftHits = Physics2D.RaycastAll(this.gameObject.transform.position, checkPos[3], 1f);

        for (int a = 0; a < leftHits.Length; a++)
        {
            if (leftHits[a].collider.GetComponent<MergerCtrl>() != this.gameObject.GetComponent<MergerCtrl>())
            {
                if (leftHits[a].collider.CompareTag("Factory"))
                {
                    nearObj[3] = leftHits[a].collider.gameObject;
                    SetInObj(nearObj[3]);
                }
            }
        }        
    }

    void SetInObj(GameObject obj)
    {
        if (obj.GetComponent<SolidFactoryCtrl>() != null)
        {
            inObj.Add(obj);

            if(obj.GetComponent<BeltCtrl>() != null)
            {
                BeltCtrl belt = obj.GetComponent<BeltCtrl>();

                int beltReNum = 0;

                if(dirNum == 0)
                {
                    if(nearObj[1] == obj)
                        beltReNum = 3;
                    else if(nearObj[2] == obj)
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
                if(beltReNum != belt.dirNum)
                {
                    belt.dirNum = beltReNum;
                    belt.BeltModelSet();                    
                }
            }
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

            outObj = obj;
        }
    }

    IEnumerator GetItem()
    {
        itemGetDelay = true;

        if (inObj[getObjNum].GetComponent<BeltCtrl>() != null)
        {
            BeltCtrl belt = inObj[getObjNum].GetComponent<BeltCtrl>();
            if (belt.isItemStop == true)
            {
                //OnBeltItem(belt.itemObjList[0]);
                OnFactoryItem(belt.itemObjList[0]);
                belt.itemObjList[0].transform.position = this.transform.position;
                belt.isItemStop = false;
                belt.itemObjList.RemoveAt(0);
                belt.beltGroupMgr.GroupItem.RemoveAt(0);
                belt.ItemNumCheck();

                getObjNum++;
                if (getObjNum >= inObj.Count)
                    getObjNum = 0;

                yield return new WaitForSeconds(factoryData.SendDelay);
                itemGetDelay = false;
            }
            else if (belt.isItemStop == false)
            {
                getObjNum++;
                if (getObjNum >= inObj.Count)
                    getObjNum = 0;

                itemGetDelay = false;
                yield break;
            }
        }   
        else if(inObj[getObjNum].GetComponent<BeltCtrl>() == null)
        {
            getObjNum++;
            if (getObjNum >= inObj.Count)
                getObjNum = 0;

            yield return new WaitForSeconds(factoryData.SendDelay);
            itemGetDelay = false;
        }        
    }

    IEnumerator SetItem()
    {
        itemSetDelay = true;

        SolidFactoryCtrl outFactory = outObj.GetComponent<SolidFactoryCtrl>();
        if (outObj.GetComponent<BeltCtrl>() != null)
        {
            ItemProps spawnItem = itemPool.Get();
            SpriteRenderer sprite = spawnItem.GetComponent<SpriteRenderer>();
            sprite.sprite = itemList[0].icon;
            spawnItem.item = itemList[0];
            spawnItem.amount = 1;
            spawnItem.transform.position = this.transform.position;

            if (outObj.GetComponent<BeltCtrl>() != null)
            {
                outObj.GetComponent<BeltCtrl>().beltGroupMgr.GroupItem.Add(spawnItem);
            }

            outFactory.OnBeltItem(spawnItem);
            itemList.RemoveAt(0);
            ItemNumCheck();
        }
        else
        {
            StartCoroutine("SetFacDelay");
            //outFactory.OnFactoryItem(itemList[0]);
        }



        yield return new WaitForSeconds(factoryData.SendDelay);
        itemSetDelay = false;
    }
    IEnumerator SetFacDelay()
    {
        var spawnItem = itemPool.Get();
        SpriteRenderer sprite = spawnItem.GetComponent<SpriteRenderer>();
        sprite.enabled = false;

        spawnItem.transform.position = this.transform.position;

        while (spawnItem.transform.position != outObj.transform.position)
        {
            spawnItem.transform.position = Vector3.MoveTowards(spawnItem.transform.position, outObj.transform.position, factoryData.SendSpeed * Time.deltaTime);

            yield return null;
        }

        if (spawnItem.transform.position == outObj.transform.position)
        {
            if (itemList.Count > 0)
            {
                SolidFactoryCtrl outFactory = outObj.GetComponent<SolidFactoryCtrl>();
                outFactory.OnFactoryItem(itemList[0]);

                itemList.RemoveAt(0);
                ItemNumCheck();
            }            
        }
        Destroy(spawnItem.gameObject);
    }
}
