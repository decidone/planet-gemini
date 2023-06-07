using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Pool;
using System;

public class Structure : MonoBehaviour
{
    // 건물 공용 스크립트
    // Update처럼 함수 호출하는 부분은 다 하위 클래스에 넣을 것
    // 연결된 구조물 확인 방법 1. 콜라이더, 2. 맵에서 인접 타일 체크

    public bool isFull = false;         // 건물의 아이템 슬롯별로 꽉 찼는지 체크하는 방식으로 변경되어야 함, 그러므로 건물 쪽 변수로 들어가야 할거같음
    public bool fluidIsFull = false;    // 창고 처럼 모든 칸이 구분없이 채울 수 있다면 모든 슬롯이 차있는지 체크하는 방식으로도 생각해 봐야함

    public int dirNum = 0;
    public int dirCount = 0;

    public bool isPreBuilding = false;
    public bool isSetBuildingOk = false;

    protected bool removeState = false;

    [SerializeField]
    protected GameObject unitCanvers = null;

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
    // 건물의 방향 설정
    protected virtual void CheckPos() { }
    // 근처 오브젝트 찻는 위치(상하좌우) 설정
    protected virtual void CheckNearObj(Vector2 direction, int index, Action<GameObject> callback) { }
    // CheckPos의 방향 기준으로 오브젝트 찻기
    public virtual void DisableColliders() { }
    // 콜라이더 끄기
    public virtual void EnableColliders() { }
    // 콜라이더 키기
    public virtual void SetBuild() { }
    // 건물 설치 기능

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
    //    // 연결된 컨베이어 벨트 체크
    //}

    //protected void PipeCheck()
    //{
    //    // 연결된 파이프 체크
    //}

    //protected void StructureCheck()
    //{
    //    // 연결된 건물 체크
    //}
}
