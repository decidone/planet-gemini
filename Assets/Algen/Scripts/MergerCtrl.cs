using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MergerCtrl : FactoryCtrl
{
    public Sprite[] modelNum = new Sprite[4];
    SpriteRenderer setModel;

    public List<GameObject> inObj = new List<GameObject>();
    public GameObject outObj = null;

    public GameObject[] nearObj = new GameObject[4];

    int getObjNum = 0;

    bool itemGetDelay = false;
    bool itemSetDelay = false;
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
        if (inObj.Count > 0 && isFull == false)
        {
            if (itemGetDelay == false)
                StartCoroutine("GetItem");
        }
        if (itemList.Count > 0 && outObj != null)
        {
            if (outObj.GetComponent<FactoryCtrl>() != null)
            {
                if (outObj.GetComponent<FactoryCtrl>().isFull == false)
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
                    SetOutObj();
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
        if (obj.GetComponent<FactoryCtrl>() != null)
        {
            inObj.Add(obj);

            if(obj.GetComponent<BeltCtrl>() != null)
            {
                BeltCtrl belt = obj.GetComponent<BeltCtrl>();

                if(nearObj[1] == obj)
                    belt.dirNum = 3;
                else if(nearObj[2] == obj)
                    belt.dirNum = 0;
                else if (nearObj[3] == obj)
                    belt.dirNum = 1;

                belt.BeltModelSet();
            }
        }
    }

    void SetOutObj()
    {
        if(nearObj[dirNum].GetComponent<FactoryCtrl>() != null)
        {
            if (nearObj[dirNum].GetComponent<BeltCtrl>() != null)
            {
                if (nearObj[dirNum].GetComponentInParent<BeltGroupMgr>().nextObj == this.GetComponent<FactoryCtrl>())
                    return;
            }
            outObj = nearObj[dirNum];
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
                belt.ItemNumCheck();

                getObjNum++;
                if (getObjNum >= inObj.Count)
                    getObjNum = 0;

                yield return new WaitForSeconds(delaySpeed);
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
    }

    IEnumerator SetItem()
    {
        itemSetDelay = true;
        if (outObj.GetComponent<FactoryCtrl>() != null)
        {
            FactoryCtrl outFactory = outObj.GetComponent<FactoryCtrl>();
            if (outObj.GetComponent<BeltCtrl>() != null)
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
            else
                outFactory.OnFactoryItem(itemList[0]);

            itemList.RemoveAt(0);
            ItemNumCheck();

            yield return new WaitForSeconds(delaySpeed);
            itemSetDelay = false;
        }
    }
}
