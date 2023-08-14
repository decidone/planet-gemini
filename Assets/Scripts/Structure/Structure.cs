using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Pool;
using UnityEngine.Rendering;
using System;

public class Structure : MonoBehaviour
{
    // 건물 공용 스크립트
    // Update처럼 함수 호출하는 부분은 다 하위 클래스에 넣을 것
    // 연결된 구조물 확인 방법 1. 콜라이더, 2. 맵에서 인접 타일 체크

    [HideInInspector]
    public bool isFull = false;         // 건물의 아이템 슬롯별로 꽉 찼는지 체크하는 방식으로 변경되어야 함, 그러므로 건물 쪽 변수로 들어가야 할거같음
    //public bool fluidIsFull = false;    // 창고 처럼 모든 칸이 구분없이 채울 수 있다면 모든 슬롯이 차있는지 체크하는 방식으로도 생각해 봐야함

    [HideInInspector]
    public int dirNum = 0;
    [HideInInspector]
    public int dirCount = 0;
    public int level = 0;
    public string buildName;

    [HideInInspector]
    public bool isPreBuilding = false;
    [HideInInspector]
    public bool isSetBuildingOk = false;

    protected bool removeState = false;

    [SerializeField]
    protected GameObject unitCanvas;

    // HpBar 관련
    [SerializeField]
    protected Image hpBar;
    protected float hp = 200.0f;
    [HideInInspector]
    public bool isRuin = false;

    // Repair 관련
    [HideInInspector]
    public bool isRepair = false;
    [SerializeField]
    protected Image repairBar;
    protected float repairGauge = 0.0f;

    [SerializeField]
    GameObject itemPref;
    protected IObjectPool<ItemProps> itemPool;
    [HideInInspector] 
    public List<Item> itemList = new List<Item>();
    [HideInInspector]
    public List<GameObject> outSameList = new List<GameObject>();

    public bool canBuilding = true;
    protected List<GameObject> buildingPosObj = new List<GameObject>();

    protected GameObject[] nearObj = new GameObject[4];
    protected Vector2[] checkPos = new Vector2[4];
    protected bool checkObj = true;

    protected Inventory playerInven = null;

    protected bool itemGetDelay = false;
    protected bool itemSetDelay = false;
    protected BoxCollider2D box2D = null;

    public List<GameObject> inObj = new List<GameObject>();
    public List<GameObject> outObj = new List<GameObject>();

    protected int getItemIndex = 0;
    protected int sendItemIndex = 0;

    protected Coroutine setFacDelayCoroutine; // 실행 중인 코루틴을 저장하는 변수

    [SerializeField]
    protected Sprite[] modelNum;
    protected SpriteRenderer setModel;

    protected ItemProps CreateItemObj()
    {
        ItemProps item = Instantiate(itemPref).GetComponent<ItemProps>();
        item.SetPool(itemPool);
        return item;
    }

    protected void OnGetItem(ItemProps item)
    {
        item.gameObject.SetActive(true);
    }
    protected void OnReleaseItem(ItemProps item)
    {
        item.gameObject.SetActive(false);
    }
    protected void OnDestroyItem(ItemProps item)
    {
        item.DestroyItem();
        //Destroy(item.gameObject, 0.4f);
    }
    public virtual bool CheckOutItemNum()  { return new bool(); }

    protected virtual void SetDirNum()
    {
        if (dirNum < 4)
        {
            setModel.sprite = modelNum[dirNum];
            CheckPos();
        }
    }
    // 건물의 방향 설정
    protected virtual void CheckPos()
    {
        Vector2[] dirs = { Vector2.up, Vector2.right, Vector2.down, Vector2.left };

        for (int i = 0; i < 4; i++)
        {
            checkPos[i] = dirs[(dirNum + i) % 4];
        }
    }
    // 근처 오브젝트 찻는 위치(상하좌우) 설정
    protected virtual void CheckNearObj(Vector2 direction, int index, Action<GameObject> callback)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, 1f);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hitCollider = hits[i].collider;
            if (hitCollider.CompareTag("Factory") && !hitCollider.GetComponent<Structure>().isPreBuilding &&
                hits[i].collider.gameObject != this.gameObject)
            {
                nearObj[index] = hits[i].collider.gameObject;
                callback(hitCollider.gameObject);
                break;
            }
        }
    }

    public virtual void SetBuild() { }
    // 건물 설치 기능

    public virtual void BeltGroupSendItem(ItemProps itemObj) { }
    public virtual bool OnBeltItem(ItemProps itemObj) { return new bool(); }
    public virtual void OnFactoryItem(ItemProps itemObj) { }
    public virtual void OnFactoryItem(Item item) { }
    public virtual void ItemNumCheck() { }

    protected virtual void GetItem()
    {
        itemGetDelay = true;

        if (inObj[getItemIndex] == null)
        {
            getItemIndex = 0;
            return;
        }
        else if (inObj[getItemIndex].TryGetComponent(out BeltCtrl belt) && belt.isItemStop)
        {
            if (belt.itemObjList.Count > 0)
            {
                OnFactoryItem(belt.itemObjList[0]);
                belt.itemObjList[0].transform.position = this.transform.position;
                belt.isItemStop = false;
                belt.itemObjList.RemoveAt(0);
                belt.beltGroupMgr.groupItem.RemoveAt(0);
                belt.ItemNumCheck();

                getItemIndex++;
                if (getItemIndex >= inObj.Count)
                    getItemIndex = 0;

                //Invoke("DelayGetItem", solidFactoryData.SendDelay); 
                // 공장데이터 작업 후
                // 위 형식처럼 SendDelay값을 가져와서 수정해야함                
                Invoke("DelayGetItem", 0.5f);
            }
            else if (belt.isItemStop == false)
            {
                getItemIndex++;
                if (getItemIndex >= inObj.Count)
                    getItemIndex = 0;

                itemGetDelay = false;
                return;
            }
        }
        else
        {
            getItemIndex++;
            if (getItemIndex >= inObj.Count)
                getItemIndex = 0;

            itemGetDelay = false;
        }
    }
    protected virtual void SendItem(Item item) 
    {
        if (setFacDelayCoroutine != null)
        {
            return;
        }

        itemSetDelay = true;

        Structure outFactory = outObj[sendItemIndex].GetComponent<Structure>();

        if (outFactory.isFull == false)
        {
            if (outObj[sendItemIndex].TryGetComponent(out BeltCtrl beltCtrl))
            {
                ItemProps spawnItem = itemPool.Get();
                if (outFactory.OnBeltItem(spawnItem))
                {
                    SpriteRenderer sprite = spawnItem.GetComponent<SpriteRenderer>();
                    sprite.sprite = item.icon;
                    spawnItem.item = item;
                    spawnItem.GetComponent<SortingGroup>().sortingOrder = 2;
                    spawnItem.amount = 1;
                    spawnItem.transform.position = transform.position;
                    spawnItem.isOnBelt = true;
                    spawnItem.setOnBelt = beltCtrl.GetComponent<BeltCtrl>();

                    if (GetComponent<Production>())
                    {
                        SubFromInventory();
                    }
                    else if(GetComponent<SolidFactoryCtrl>() && !GetComponent<ItemSpawner>())
                    {
                        itemList.RemoveAt(0);
                        ItemNumCheck();
                    }
                }
                else
                {
                    OnDestroyItem(spawnItem);
                    itemSetDelay = false;
                    return;
                }
            }
            else if (outObj[sendItemIndex].GetComponent<SolidFactoryCtrl>())
            {
                setFacDelayCoroutine = StartCoroutine("SetFacDelay", outObj[sendItemIndex]);
            }
            else if (outObj[sendItemIndex].TryGetComponent(out Production production))
            {
                if (production.CanTakeItem(item))
                {
                    setFacDelayCoroutine = StartCoroutine("SetFacDelay", outObj[sendItemIndex]);
                }
            }

            sendItemIndex++;
            if (sendItemIndex >= outObj.Count)
                sendItemIndex = 0;

            Invoke("DelaySetItem", 0.5f);
            //Invoke("DelaySetItem", solidFactoryData.SendDelay);
            // 공장데이터 작업 후
            // 위 형식처럼 SendDelay값을 가져와서 수정해야함    
        }
        else
        {
            sendItemIndex++;
            if (sendItemIndex >= outObj.Count)
                sendItemIndex = 0;

            itemSetDelay = false;
        }
    }
    protected virtual void SubFromInventory() { }

    public virtual void TakeDamage(float damage) { }
    protected virtual void DieFunc() { }
    public virtual void HealFunc(float heal) { }
    public virtual void RepairSet(bool repair) { }
    protected virtual void RepairFunc(bool isBuilding) { }
    protected virtual void RepairEnd() { }

    // 콜라이더 키기
    public virtual void ColliderTriggerOnOff(bool isOn)
    {
        if (isOn)
            box2D.isTrigger = true;
        else
            box2D.isTrigger = false;
    }

    protected void RemoveSameOutList()
    {
        outSameList.Clear();
    }

    protected virtual IEnumerator SetInObjCoroutine(GameObject obj) 
    { 
        yield return new WaitForSeconds(0.1f);

        if (obj.GetComponent<Structure>() != null)
        {
            if (obj.TryGetComponent(out BeltCtrl belt))
            {
                if (belt.GetComponentInParent<BeltGroupMgr>().nextObj != this.gameObject)
                {
                    yield break;
                }
                else if (belt.beltState == BeltState.SoloBelt || belt.beltState == BeltState.StartBelt)
                    belt.FactoryPosCheck(GetComponentInParent<Structure>());
            }
            inObj.Add(obj);
        }
    }

    protected virtual IEnumerator SetOutObjCoroutine(GameObject obj)
    {
        yield return new WaitForSeconds(0.1f);

        if (obj.GetComponent<Structure>() != null)
        {
            if (obj.TryGetComponent(out BeltCtrl belt))
            {
                if (obj.GetComponentInParent<BeltGroupMgr>().nextObj == this.gameObject)
                    yield break;
                if (belt.beltState == BeltState.SoloBelt || belt.beltState == BeltState.StartBelt)
                    belt.FactoryPosCheck(GetComponentInParent<Structure>());
            }
            else
            {
                outSameList.Add(obj);
                StartCoroutine(OutCheck(obj));
            }
            outObj.Add(obj);
        }
    }

    protected virtual IEnumerator OutCheck(GameObject otherObj)
    {
        yield return new WaitForSeconds(0.1f);

        if (otherObj.TryGetComponent(out Structure otherFacCtrl))
        {
            if (otherObj.GetComponent<Production>())
                yield break;

            if (otherFacCtrl.outSameList.Contains(this.gameObject) && outSameList.Contains(otherObj))
            {
                outObj.Remove(otherObj);
                Invoke("RemoveSameOutList", 0.1f);
                StopCoroutine("SetFacDelay");
            }
        }
    }

    public virtual void ResetCheckObj(GameObject game)
    {
        checkObj = false;

        if (inObj.Contains(game))
        {
            inObj.Remove(game);
            getItemIndex = 0;
        }
        if (outObj.Contains(game))
        {
            outObj.Remove(game);
            sendItemIndex = 0;
        }

        for (int i = 0; i < nearObj.Length; i++)
        {
            if (nearObj[i] != null && nearObj[i] == game) 
            {
                nearObj[i] = null;
            }
        }

        checkObj = true;
    }

    public virtual void RemoveObj() 
    {
        removeState = true;
        ColliderTriggerOnOff(true); 
        StopAllCoroutines();

        for (int i = 0; i < nearObj.Length; i++)
        {
            if(nearObj[i] != null && nearObj[i].TryGetComponent(out Structure structure))
            {
                structure.ResetCheckObj(this.gameObject);
                if (structure.GetComponentInParent<BeltGroupMgr>())
                {
                    BeltGroupMgr beltGroup = structure.GetComponentInParent<BeltGroupMgr>();
                    beltGroup.nextCheck = true;
                    beltGroup.preCheck = true;
                }
            }
        }

        if (gameObject.GetComponent<BeltCtrl>() && GetComponentInParent<BeltManager>() && GetComponentInParent<BeltGroupMgr>())
        {
            BeltManager beltManager = GetComponentInParent<BeltManager>();
            BeltGroupMgr beltGroup = GetComponentInParent<BeltGroupMgr>();
            beltManager.BeltDivide(beltGroup, this.gameObject);
        }

        AddInvenItem();
        Destroy(this.gameObject);
    }

    protected virtual void AddInvenItem() { }

    protected void DelaySetItem()
    {
        itemSetDelay = false;
    }

    protected void DelayGetItem()
    {
        itemGetDelay = false;
    }
}
