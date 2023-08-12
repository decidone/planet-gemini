using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Pool;
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

    protected virtual void SetDirNum() { }
    // �ǹ��� ���� ����
    protected virtual void CheckPos() { }
    // ��ó ������Ʈ ���� ��ġ(�����¿�) ����
    protected virtual void CheckNearObj(Vector2 direction, int index, Action<GameObject> callback) { }
    // CheckPos�� ���� �������� ������Ʈ ����
    //public virtual void DisableColliders() { }
    // �ݶ��̴� ����
    //public virtual void EnableColliders() { }
    public virtual void SetBuild() { }
    // �ǹ� ��ġ ���

    public virtual void BeltGroupSendItem(ItemProps itemObj) { }
    public virtual bool OnBeltItem(ItemProps itemObj) { return new bool(); }
    public virtual void OnFactoryItem(ItemProps itemObj) { }
    public virtual void OnFactoryItem(Item item) { }
    public virtual void ItemNumCheck() { }


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

    public virtual void ResetCheckObj(GameObject game)
    {
        for(int i = 0; i < nearObj.Length; i++)
        {
            if (nearObj[i] != null && nearObj[i] == game) 
            {
                nearObj[i] = null;
            }
        }
    }

    public virtual void RemoveObj() 
    {
        removeState = true;
        ColliderTriggerOnOff(true); 
        StopAllCoroutines();

        Structure structure = null;

        for (int i = 0; i < nearObj.Length; i++)
        {
            if(nearObj[i] != null && nearObj[i].TryGetComponent(out structure))
            {
                structure.checkObj = false;
                structure.ResetCheckObj(this.gameObject);
            }
        }

        for (int i = 0; i < nearObj.Length; i++)
        {
            if (nearObj[i] != null && nearObj[i].TryGetComponent(out structure))
            {
                structure.checkObj = true;
                if (structure.GetComponentInParent<BeltGroupMgr>())
                {
                    BeltGroupMgr beltGroup = structure.GetComponentInParent<BeltGroupMgr>();
                    beltGroup.nextCheck = true;
                    beltGroup.preCheck = true;
                }
            }
        }

        if(GetComponentInParent<BeltManager>() && GetComponentInParent<BeltGroupMgr>())
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
