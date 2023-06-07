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

    public bool isFull = false;         // �ǹ��� ������ ���Ժ��� �� á���� üũ�ϴ� ������� ����Ǿ�� ��, �׷��Ƿ� �ǹ� �� ������ ���� �ҰŰ���
    public bool fluidIsFull = false;    // â�� ó�� ��� ĭ�� ���о��� ä�� �� �ִٸ� ��� ������ ���ִ��� üũ�ϴ� ������ε� ������ ������

    public int dirNum = 0;
    public int dirCount = 0;

    public bool isPreBuilding = false;
    public bool isSetBuildingOk = false;

    protected bool removeState = false;

    [SerializeField]
    protected GameObject unitCanvers = null;

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
    public List<GameObject> outSameList = new List<GameObject>();

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
    public virtual void DisableColliders() { }
    // �ݶ��̴� ����
    public virtual void EnableColliders() { }
    // �ݶ��̴� Ű��
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

    protected void RemoveSameOutList()
    {
        outSameList.Clear();
    }

    public virtual void RemoveObj() 
    {
        removeState = true;
        StopAllCoroutines();
    }

    //[SerializeField]
    //protected int maxHp;
    //[SerializeField]
    //protected int hp;

    //protected void ConveyorCheck()
    //{
    //    // ����� �����̾� ��Ʈ üũ
    //}

    //protected void PipeCheck()
    //{
    //    // ����� ������ üũ
    //}

    //protected void StructureCheck()
    //{
    //    // ����� �ǹ� üũ
    //}
}
