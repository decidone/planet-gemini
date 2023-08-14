using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Pool;
using UnityEngine.Rendering;
using System;

public class Structure : MonoBehaviour
{
    // �ǹ� ���� ��ũ��Ʈ
    // Updateó�� �Լ� ȣ���ϴ� �κ��� �� ���� Ŭ������ ���� ��
    // ����� ������ Ȯ�� ��� 1. �ݶ��̴�, 2. �ʿ��� ���� Ÿ�� üũ

    [HideInInspector]
    public bool isFull = false;         // �ǹ��� ������ ���Ժ��� �� á���� üũ�ϴ� ������� ����Ǿ�� ��, �׷��Ƿ� �ǹ� �� ������ ���� �ҰŰ���
    //public bool fluidIsFull = false;    // â�� ó�� ��� ĭ�� ���о��� ä�� �� �ִٸ� ��� ������ ���ִ��� üũ�ϴ� ������ε� ������ ������

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

    // HpBar ����
    [SerializeField]
    protected Image hpBar;
    protected float hp = 200.0f;
    [HideInInspector]
    public bool isRuin = false;

    // Repair ����
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

    protected Coroutine setFacDelayCoroutine; // ���� ���� �ڷ�ƾ�� �����ϴ� ����

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
    // �ǹ��� ���� ����
    protected virtual void CheckPos()
    {
        Vector2[] dirs = { Vector2.up, Vector2.right, Vector2.down, Vector2.left };

        for (int i = 0; i < 4; i++)
        {
            checkPos[i] = dirs[(dirNum + i) % 4];
        }
    }
    // ��ó ������Ʈ ���� ��ġ(�����¿�) ����
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
    // �ǹ� ��ġ ���

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
                // ���嵥���� �۾� ��
                // �� ����ó�� SendDelay���� �����ͼ� �����ؾ���                
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
            // ���嵥���� �۾� ��
            // �� ����ó�� SendDelay���� �����ͼ� �����ؾ���    
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

    // �ݶ��̴� Ű��
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
