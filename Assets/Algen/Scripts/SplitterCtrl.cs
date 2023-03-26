using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SplitterCtrl : SolidFactoryCtrl
{
    [SerializeField]
    Sprite[] modelNum = new Sprite[4];
    SpriteRenderer setModel;
    private int prevDirNum = -1; // 이전 방향 값을 저장할 변수

    GameObject inObj = null;
    [SerializeField]
    List<GameObject> outObj = new List<GameObject>();
    [SerializeField]
    GameObject[] nearObj = new GameObject[4];

    int getObjNum = 0;

    Vector2[] checkPos = new Vector2[4];

    List<Item> itemsList;

    //int itemListCount = 0;
    //int outObjCount = 0;

    [SerializeField]
    bool oKButtonTemp = false;
    [SerializeField]
    bool filterOn = false;

    [SerializeField]
    bool[] isFilterOn = new bool[3];
    [SerializeField]
    bool[] isFullFilterOn = new bool[3];
    [SerializeField]
    bool[] isItemFilterOn = new bool[3];
    [SerializeField]
    Item[] isSelItem = new Item[3];

    [Serializable]
    public struct Filter
    {
        public GameObject outObj;
        public bool isFilterOn;
        public bool isFullFilterOn;
        public bool isItemFilterOn;
        public Item selItem;
    }
    public Filter[] arrFilter = new Filter[3]; // 0 좌 1 상 2 우

    // Start is called before the first frame update
    void Start()
    {
        setModel = GetComponent<SpriteRenderer>();
        itemsList = ItemList.instance.itemList;
        CheckPos();
    }

    // Update is called once per frame
    void Update()
    {
        SetDirNum();

        if (inObj != null && !isFull && !itemGetDelay)
            GetItem();
        
        for (int i = 0; i < nearObj.Length; i++)
        {
            if (nearObj[i] == null)
            {
                if (i == 0)
                    CheckNearObj(checkPos[0], 0, obj => SetInObj(obj));
                else if (i == 1)
                    CheckNearObj(checkPos[1], 1, obj => SetOutObj(obj, 0));
                else if (i == 2)
                    CheckNearObj(checkPos[2], 2, obj => SetOutObj(obj, 1));
                else if (i == 3)
                    CheckNearObj(checkPos[3], 3, obj => SetOutObj(obj, 2));
            }
        }

        if (itemList.Count > 0 && outObj.Count > 0 && !itemSetDelay)
        {
            if (filterOn)
            {
                FilterSetItem();
            }
            else
            {
                SetItem();
            }
        }  

        if (oKButtonTemp == true)
        {
            ItemFilterCheck();
            filterOn = FilterCheck();
            //StopCoroutine("FilterSetItem");

            oKButtonTemp = false;
        }
    }

    bool FilterCheck()
    {
        for (int a = 0; a < arrFilter.Length; a++)
        {
            if (arrFilter[a].outObj != null)
            {
                if (arrFilter[a].isFilterOn)                
                    return true;
            }            
        }

        return false;
    }

    void ItemFilterCheck()
    {
        for (int a = 0; a < arrFilter.Length; a++)
        {
            if (arrFilter[a].outObj != null)
            {
                FilterSet(a, isFilterOn[a], isFullFilterOn[a], isItemFilterOn[a], isSelItem[a]);
                //arrFilter[a].selItem = itemsList[arrFilter[a].itemNum];
            }
        }
    }

    void SetDirNum()
    {
        if (dirNum < 4)
        {
            setModel.sprite = modelNum[dirNum];

            // dirNum 값이 변경됐는지 체크하고, 변경됐으면 CheckPos() 실행
            if (dirNum != prevDirNum)
            {
                CheckPos();
                prevDirNum = dirNum;
            }
        }
    }
    void CheckPos()
    {
        Vector2[] dirs ={ Vector2.down, Vector2.left, Vector2.up, Vector2.right };

        for (int i = 0; i < dirs.Length; i++)
        {
            checkPos[i] = dirs[(i + dirNum) % dirs.Length];
        }
    }

    void CheckNearObj(Vector2 direction, int index, Action<GameObject> callback)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, 1f);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hitCollider = hits[i].collider;
            if (hitCollider.CompareTag("Factory") && 
                hitCollider.GetComponent<SplitterCtrl>() != GetComponent<SplitterCtrl>())
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
            inObj = obj;
            if (inObj.TryGetComponent(out BeltCtrl belt) && belt.dirNum != dirNum)
            {
                belt.dirNum = dirNum;
                belt.BeltModelSet();
            }
        }
    }
    void SetOutObj(GameObject obj, int num)
    {
        if (obj.GetComponent<SolidFactoryCtrl>() != null)
        {
            if (obj.TryGetComponent(out BeltCtrl belt))
            {
                if (obj.GetComponentInParent<BeltGroupMgr>().nextObj == this.gameObject)                
                    return;                
                if (belt.beltState == BeltState.SoloBelt || belt.beltState == BeltState.StartBelt)                
                    belt.FactoryVecCheck(GetComponentInParent<SolidFactoryCtrl>());                
            }
            else
            {
                outSameList.Add(obj);
                StartCoroutine(OutCheck(obj));
            }
            outObj.Add(obj);
            FilterArr(obj, num);
        }
    }

    void FilterArr(GameObject obj, int num)
    {
        arrFilter[num].outObj = obj;
        arrFilter[num].isFilterOn = false;
        arrFilter[num].isFullFilterOn = false;
        arrFilter[num].isItemFilterOn = false;
        arrFilter[num].selItem = itemsList[0];
    }

    void FilterSet(int num, bool filterOn, bool fullFilterOn, bool itemFilterOn, Item itemNum)
    {
        arrFilter[num].isFilterOn = filterOn;
        arrFilter[num].isFullFilterOn = fullFilterOn;
        arrFilter[num].isItemFilterOn = itemFilterOn;
        arrFilter[num].selItem = itemNum;
    }

    void FilterReset(int num)
    {
        arrFilter[num].outObj = null;
        arrFilter[num].isFilterOn = false;
        arrFilter[num].isFullFilterOn = false;
        arrFilter[num].isItemFilterOn = false;
        arrFilter[num].selItem = itemsList[0];
    }

    IEnumerator OutCheck(GameObject otherObj)
    {
        yield return new WaitForSeconds(0.1f);

        SolidFactoryCtrl otherFacCtrl = otherObj.GetComponent<SolidFactoryCtrl>();
        GameObject[] outObjArray = outObj.ToArray();

        foreach (GameObject otherList in otherFacCtrl.outSameList)
        {
            if (otherList == this.gameObject)
            {
                for (int a = outObjArray.Length - 1; a >= 0; a--)
                {
                    if (otherObj == outObjArray[a])
                    {
                        for (int b = 0; b < arrFilter.Length; b++)
                        {
                            if (arrFilter[b].outObj == outObjArray[a])
                            {
                                FilterReset(b);
                            }
                        }
                        outObj.RemoveAt(a);
                        break;
                    }
                }
            }
        }
    }
    void GetItem()
    {
        itemGetDelay = true;
        if (inObj.TryGetComponent(out BeltCtrl belt) && belt.isItemStop)
        {
            OnFactoryItem(belt.itemObjList[0]);
            belt.itemObjList[0].transform.position = transform.position;
            belt.isItemStop = false;
            belt.itemObjList.RemoveAt(0);
            belt.beltGroupMgr.GroupItem.RemoveAt(0);
            belt.ItemNumCheck();
            Invoke("DelayGetItem", solidFactoryData.SendDelay);
        }
        itemGetDelay = false;
    }

    void SetItem()
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

    void FilterSetItem()
    {
        itemSetDelay = true;

        SolidFactoryCtrl outFactory = null;

        for (int i = 0; i < arrFilter.Length; i++)
        {
            int index = (i + getObjNum) % arrFilter.Length;
            Filter filter = arrFilter[index];

            if (!filter.isFilterOn) continue;

            GameObject outObject = filter.outObj;

            if (outObject == null) continue;

            outFactory = outObject.GetComponent<SolidFactoryCtrl>();

            if (outFactory == null) continue;

            if (outFactory.isFull) continue;

            if (filter.isFullFilterOn)
            {
                if (!ItemFilterFullCheck(itemList[0])) continue;
            }
            else if (filter.isItemFilterOn)
            {
                if (filter.selItem != itemList[0]) continue;
            }

            if (outObject.TryGetComponent<BeltCtrl>(out BeltCtrl beltCtrl))
            {
                ItemProps spawnItem = itemPool.Get();
                SpriteRenderer sprite = spawnItem.GetComponent<SpriteRenderer>();
                sprite.sprite = itemList[0].icon;
                spawnItem.item = itemList[0];
                spawnItem.amount = 1;
                spawnItem.transform.position = transform.position;

                outFactory.OnBeltItem(spawnItem);
            }
            else
            {
                SetFacDelay(outObject);
            }

            itemList.RemoveAt(0);
            ItemNumCheck();
            getObjNum = index + 1;
            break;
        }

        if (outFactory != null && !outFactory.isFull)
        {
            getObjNum++;
            if (getObjNum >= arrFilter.Length) getObjNum = 0;
        }

        itemSetDelay = false;
    }

    bool ItemFilterFullCheck(Item item)
    {
        for (int a = 0; a < arrFilter.Length; a++)
        {
            Filter filter = arrFilter[a];
            if (filter.outObj == null) continue;

            if (filter.isItemFilterOn && filter.selItem == item)
            {
                SolidFactoryCtrl factoryCtrl = filter.outObj.GetComponent<SolidFactoryCtrl>();
                if (factoryCtrl != null && !factoryCtrl.isFull)
                {
                    return false;
                }
            }
        }

        return true;
    }

    IEnumerator SetFacDelay(GameObject outFac)
    {
        var spawnItem = itemPool.Get();
        var sprite = spawnItem.GetComponent<SpriteRenderer>();
        sprite.color = new Color(1f, 1f, 1f, 0f);

        spawnItem.transform.position = transform.position;

        var targetPos = outFac.transform.position;
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
                var outFactory = outFac.GetComponent<SolidFactoryCtrl>();
                outFactory.OnFactoryItem(itemList[0]);

                itemList.RemoveAt(0);

                ItemNumCheck();
            }
        }

        if (spawnItem != null)
        {
            itemPool.Release(spawnItem);
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
