using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : FactoryCtrl
{
    public Item itemData;
    public List<BeltCtrl> beltList = new List<BeltCtrl>();
    //public GameObject itemPref;

    bool itemSetDelay = false;

    public List<GameObject> outObj = new List<GameObject>();
    public GameObject[] nearObj = new GameObject[4];
    Vector2[] checkPos = new Vector2[4];

    int getObjNum = 0;
    float delaySpeed = 1.0f;

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
        if (obj.GetComponent<FactoryCtrl>() != null)
        {
            outObj.Add(obj);
        }
    }

    IEnumerator SetItem()
    {
        itemSetDelay = true;

        FactoryCtrl objFactory = outObj[getObjNum].GetComponent<FactoryCtrl>();

        if (objFactory.isFull == false)
        {
            //GameObject spawnItem = Instantiate(itemPref);
            var spawnItem = itemPool.Get();
            SpriteRenderer sprite = spawnItem.GetComponent<SpriteRenderer>();
            sprite.sprite = itemData.icon;
            //Item item = spawnItem.GetComponent<ItemProps>().item;
            //objFactory.AddItem(spawnItem);

            //objFactory.AddItem(itemData);

            ItemProps itemProps = spawnItem.GetComponent<ItemProps>();
            itemProps.item = itemData;
            itemProps.amount = 1;
            spawnItem.transform.position = this.transform.position;
            objFactory.OnBeltItem(itemProps);

            //beltList[0].beltGroupMgr.GetComponent<BeltGroupMgr>().AddItem(spawnItem.GetComponent<ItemProps>());

            getObjNum++;
            if (getObjNum >= outObj.Count)
                getObjNum = 0;

            yield return new WaitForSeconds(delaySpeed);
            itemSetDelay = false;
        }
        else if (objFactory.isFull == true)
        {
            getObjNum++;
            if (getObjNum >= outObj.Count)
                getObjNum = 0;

            itemSetDelay = false;
            yield break;
        }
    }
}
