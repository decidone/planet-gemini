using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendUnderBeltCtrl : FactoryCtrl
{
    //public Sprite[] modelNum = new Sprite[4];
    //SpriteRenderer setModel;

    public List<GameObject> inObj = new List<GameObject>();
    public GameObject outObj = null;

    public GameObject[] nearObj = new GameObject[4];

    int getObjNum = 0;

    float delaySpeed = 1f;

    Vector2[] checkPos = new Vector2[4];
    float underBeltDist = 10.0f;

    // Start is called before the first frame update
    void Start()
    {
        
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
            //setModel.sprite = modelNum[dirNum];
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
        RaycastHit2D[] upHits = Physics2D.RaycastAll(this.gameObject.transform.position, checkPos[0], 10f);

        for (int a = 0; a < upHits.Length; a++)
        {
            if (upHits[a].collider.GetComponent<SendUnderBeltCtrl>() != this.gameObject.GetComponent<SendUnderBeltCtrl>())
            {
                if (upHits[a].collider.GetComponent<GetUnderBeltCtrl>() != null)
                {
                    nearObj[0] = upHits[a].collider.gameObject;
                    StartCoroutine("SetOutObj", nearObj[0]);
                    //SetOutObj();
                }
            }
        }
    }

    void RightObjCheck()
    {
        RaycastHit2D[] rightHits = Physics2D.RaycastAll(this.gameObject.transform.position, checkPos[1], 1f);

        for (int a = 0; a < rightHits.Length; a++)
        {
            if (rightHits[a].collider.GetComponent<SendUnderBeltCtrl>() != this.gameObject.GetComponent<SendUnderBeltCtrl>())
            {
                if (rightHits[a].collider.CompareTag("Factory"))
                {
                    nearObj[1] = rightHits[a].collider.gameObject;
                    StartCoroutine("SetInObj", nearObj[1]);
                    //SetInObj(nearObj[1]);
                }
            }
        }
    }
    void DownObjCheck()
    {
        RaycastHit2D[] downHits = Physics2D.RaycastAll(this.gameObject.transform.position, checkPos[2], 1f);

        for (int a = 0; a < downHits.Length; a++)
        {
            if (downHits[a].collider.GetComponent<SendUnderBeltCtrl>() != this.gameObject.GetComponent<SendUnderBeltCtrl>())
            {
                if (downHits[a].collider.CompareTag("Factory"))
                {
                    nearObj[2] = downHits[a].collider.gameObject;
                    StartCoroutine("SetInObj", nearObj[2]);
                    //SetInObj(nearObj[2]);
                }
            }
        }
    }

    void LeftObjCheck()
    {
        RaycastHit2D[] leftHits = Physics2D.RaycastAll(this.gameObject.transform.position, checkPos[3], 1f);

        for (int a = 0; a < leftHits.Length; a++)
        {
            if (leftHits[a].collider.GetComponent<SendUnderBeltCtrl>() != this.gameObject.GetComponent<SendUnderBeltCtrl>())
            {
                if (leftHits[a].collider.CompareTag("Factory"))
                {
                    nearObj[3] = leftHits[a].collider.gameObject;
                    StartCoroutine("SetInObj", nearObj[3]);
                    //SetInObj(nearObj[3]);
                }
            }
        }
    }

    IEnumerator SetInObj(GameObject obj)
    {
        yield return new WaitForSeconds(0.1f);
        if (obj.GetComponent<FactoryCtrl>() != null)
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

    IEnumerator SetOutObj(GameObject obj)
    {
        if (obj.GetComponent<FactoryCtrl>() != null)
        {
            GetUnderBeltCtrl getUnderbelt = obj.GetComponent<GetUnderBeltCtrl>();
            if(getUnderbelt.dirNum == dirNum)
            {
                outObj = obj;
                getUnderbelt.inObj = this.gameObject;
                underBeltDist = Vector3.Distance(this.transform.position, outObj.transform.position);
            }
            else
                yield break;
        }
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator GetItem()
    {
        itemGetDelay = true;

        if (inObj[getObjNum].GetComponent<BeltCtrl>() != null)
        {
            BeltCtrl belt = inObj[getObjNum].GetComponent<BeltCtrl>();
            if (belt.isItemStop == true)
            {
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
        else if (inObj[getObjNum].GetComponent<BeltCtrl>() == null)
        {
            getObjNum++;
            if (getObjNum >= inObj.Count)
                getObjNum = 0;

            yield return new WaitForSeconds(delaySpeed);
            itemGetDelay = false;
        }
    }

    IEnumerator SetItem()
    {
        itemSetDelay = true;

        StartCoroutine("SetDistCheck");

        yield return new WaitForSeconds(delaySpeed);
        itemSetDelay = false;
    }

    IEnumerator SetDistCheck()
    {
        var spawnItem = itemPool.Get();
        SpriteRenderer sprite = spawnItem.GetComponent<SpriteRenderer>();
        sprite.enabled = false;

        spawnItem.transform.position = this.transform.position;

        while (spawnItem.transform.position != outObj.transform.position)
        {
            spawnItem.transform.position = Vector3.MoveTowards(spawnItem.transform.position, outObj.transform.position, 1.5f * Time.deltaTime);

            yield return null;
        }

        if (spawnItem.transform.position == outObj.transform.position)
        {
            if (spawnItem.transform.position == outObj.transform.position)
            {
                if (itemList.Count > 0)
                {
                    FactoryCtrl outFactory = outObj.GetComponent<FactoryCtrl>();
                    outFactory.OnFactoryItem(itemList[0]);

                    itemList.RemoveAt(0);
                    ItemNumCheck();
                }
            }
        }
        Destroy(spawnItem.gameObject);
    }
}
