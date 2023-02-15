using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitterCtrl : FactoryCtrl
{
    public Sprite[] modelNum = new Sprite[4];
    SpriteRenderer setModel;

    //public BeltCtrl inBelt = null;

    public GameObject inObj = null;
    public List<GameObject> outObj = new List<GameObject>();

    public GameObject[] nearObj = new GameObject[4];

    int getObjNum = 0;

    //bool itemGetDelay = false;
    //bool itemSetDelay = false;
    float delaySpeed = 0.4f;

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
        if (inObj != null && isFull == false) 
        {
            if (itemGetDelay == false)
                StartCoroutine("GetItem");
        }
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
            setModel.sprite = modelNum[dirNum];
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
            RaycastHit2D[] upHits = Physics2D.RaycastAll(this.gameObject.transform.position, checkPos[0], 1f);

            for (int a = 0; a < upHits.Length; a++)
            {
                if (upHits[a].collider.GetComponent<SplitterCtrl>() != this.gameObject.GetComponent<SplitterCtrl>())
                {
                    if (upHits[a].collider.CompareTag("Factory"))
                    {
                        nearObj[0] = upHits[a].collider.gameObject;
                        StartCoroutine("SetInObj", nearObj[0]);
                        //SetInObj();
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
                if (rightHits[a].collider.GetComponent<SplitterCtrl>() != this.gameObject.GetComponent<SplitterCtrl>())
                {
                    if (rightHits[a].collider.CompareTag("Factory"))
                    {
                        nearObj[1] = rightHits[a].collider.gameObject;
                        StartCoroutine("SetOutObj", nearObj[1]);
                        //SetOutObj(nearObj[1]);
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
                if (downHits[a].collider.GetComponent<SplitterCtrl>() != this.gameObject.GetComponent<SplitterCtrl>())
                {
                    if (downHits[a].collider.CompareTag("Factory"))
                    {
                        nearObj[2] = downHits[a].collider.gameObject;
                        StartCoroutine("SetOutObj", nearObj[2]);
                        //SetOutObj(nearObj[2]);
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
                if (leftHits[a].collider.GetComponent<SplitterCtrl>() != this.gameObject.GetComponent<SplitterCtrl>())
                {
                    if (leftHits[a].collider.CompareTag("Factory"))
                    {
                        nearObj[3] = leftHits[a].collider.gameObject;
                        StartCoroutine("SetOutObj", nearObj[3]);
                        //SetOutObj(nearObj[3]);
                    }
                }
            }
        }
    }

    IEnumerator SetInObj(GameObject obj)
    {
        yield return new WaitForSeconds(0.1f);

        if (obj.GetComponent<FactoryCtrl>() != null)
        {
            inObj = obj;

            if(inObj.GetComponent<BeltCtrl>() != null)
            {
                BeltCtrl inBelt = inObj.GetComponent<BeltCtrl>();
                if (inBelt.dirNum != dirNum)
                {
                    inBelt.dirNum = dirNum;

                    //if (inBelt.isTurn == false)
                    //{
                    //    //inBelt.isTurn = true;
                    inBelt.BeltModelSet();
                    //}
                }
            }
        }
     }

    IEnumerator SetOutObj(GameObject obj)
    {
        yield return new WaitForSeconds(0.1f);

        if (obj.GetComponent<FactoryCtrl>() != null)
        {
            if(obj.GetComponent<BeltCtrl>() != null)
            {
                if (obj.GetComponentInParent<BeltGroupMgr>().nextObj == this.GetComponent<FactoryCtrl>())
                    yield break;

                BeltCtrl belt = obj.GetComponent<BeltCtrl>();
                if(belt.beltState == BeltState.SoloBelt || belt.beltState == BeltState.StartBelt)
                {
                    belt.FactoryVecCheck(GetComponentInParent<FactoryCtrl>());
                }
            }
            outObj.Add(obj);
        }
    }

    IEnumerator GetItem()
    {
        itemGetDelay = true;

        if(inObj.GetComponent<BeltCtrl>() != null)
        {
            BeltCtrl inBelt = inObj.GetComponent<BeltCtrl>();
            if (inBelt.isItemStop == true)
            {
                OnFactoryItem(inBelt.itemObjList[0]);
                inBelt.itemObjList[0].transform.position = this.transform.position;
                inBelt.isItemStop = false;
                inBelt.itemObjList.RemoveAt(0);
                inBelt.ItemNumCheck();

                yield return new WaitForSeconds(delaySpeed);
                itemGetDelay = false;
            }
            else if (inBelt.isItemStop == false)
            {
                itemGetDelay = false;
                yield break;
            }
        }
    }

    IEnumerator SetItem()
    {
        itemSetDelay = true;

        FactoryCtrl outFactory = outObj[getObjNum].GetComponent<FactoryCtrl>();

        if (outFactory.isFull == false)
        {
            if(outObj[getObjNum].GetComponent<BeltCtrl>() != null)
            {
                var spawnItem = itemPool.Get();
                SpriteRenderer sprite = spawnItem.GetComponent<SpriteRenderer>();
                sprite.sprite = itemList[0].icon;
                ItemProps itemProps = spawnItem.GetComponent<ItemProps>();
                itemProps.item = itemList[0];
                itemProps.amount = 1;
                spawnItem.transform.position = this.transform.position;
                outFactory.OnBeltItem(spawnItem);
            }
            else if (outObj[getObjNum].GetComponent<BeltCtrl>() == null)
            {
                outFactory.OnFactoryItem(itemList[0]);
            }    

            
            itemList.RemoveAt(0);
            ItemNumCheck();

            getObjNum++;
            if (getObjNum >= outObj.Count)
                getObjNum = 0;

            yield return new WaitForSeconds(delaySpeed);
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
}
