using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : SolidFactoryCtrl
{
    [SerializeField]
    Item itemData;

    List<GameObject> outObj = new List<GameObject>();
    GameObject[] nearObj = new GameObject[4];
    Vector2[] checkPos = new Vector2[4];

    int getObjNum = 0;

    // Start is called before the first frame update
    void Start()
    {
        CheckPos();
    }

    // Update is called once per frame
    void Update()
    {
        if (outObj.Count > 0)
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
    void CheckPos()
    {
        checkPos[0] = transform.up;
        checkPos[1] = transform.right;
        checkPos[2] = -transform.up;
        checkPos[3] = -transform.right;
    }
    void UpObjCheck()
    {
        if (nearObj[0] == null)
        {
            RaycastHit2D[] upHits = Physics2D.RaycastAll(this.gameObject.transform.position, checkPos[0], 1f);

            for (int a = 0; a < upHits.Length; a++)
            {
                if (upHits[a].collider.GetComponent<ItemSpawner>() != this.gameObject.GetComponent<ItemSpawner>())
                {
                    if (upHits[a].collider.CompareTag("Factory"))
                    {
                        nearObj[0] = upHits[a].collider.gameObject;
                        SetOutObj(nearObj[0]);
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
                if (rightHits[a].collider.GetComponent<ItemSpawner>() != this.gameObject.GetComponent<ItemSpawner>())
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
                if (downHits[a].collider.GetComponent<ItemSpawner>() != this.gameObject.GetComponent<ItemSpawner>())
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
                if (leftHits[a].collider.GetComponent<ItemSpawner>() != this.gameObject.GetComponent<ItemSpawner>())
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
                sprite.sprite = itemData.icon;
                spawnItem.item = itemData;
                spawnItem.amount = 1;
                spawnItem.transform.position = this.transform.position;
                outFactory.OnBeltItem(spawnItem);
                //outObj[getObjNum].GetComponent<BeltCtrl>().beltGroupMgr.GroupItem.Add(spawnItem);
            }
            else if (outObj[getObjNum].GetComponent<BeltCtrl>() == null)
            {
                StartCoroutine("SetFacDelay", getObjNum);
                //objFactory.OnFactoryItem(itemData);
            }

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
            SolidFactoryCtrl outFactory = outObj[getObjNum].GetComponent<SolidFactoryCtrl>();
            outFactory.OnFactoryItem(itemData);
        }
        Destroy(spawnItem.gameObject);
    }
}
