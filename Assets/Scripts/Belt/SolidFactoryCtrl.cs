using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class SolidFactoryCtrl : Structure
{
    [SerializeField]
    protected SolidFactoryData solidFactoryData;
    protected SolidFactoryData SolidFactoryData { set { solidFactoryData = value; } }

    public List<ItemProps> itemObjList = new List<ItemProps>();
    public List<Item> itemList = new List<Item>();

    List<Item> items = new List<Item>();
    //public List<GameObject> outSameList = new List<GameObject>();

    protected bool itemGetDelay = false;
    protected bool itemSetDelay = false;

    BoxCollider2D box2D = null;

    private void Awake()
    {
        box2D = GetComponent<BoxCollider2D>();
        hp = solidFactoryData.MaxHp[level];
        hpBar.fillAmount = hp / solidFactoryData.MaxHp[level];
        repairBar.fillAmount = 0;

        itemPool = new ObjectPool<ItemProps>(CreateItemObj, OnGetItem, OnReleaseItem, OnDestroyItem, maxSize: 20);
    }
    // Start is called before the first frame update

    protected virtual void Update()
    {
        if(!removeState)
        {
            if (isRuin && isRepair)
            {
                RepairFunc(false);
            }
            else if (isPreBuilding && isSetBuildingOk && !isRuin)
            {
                RepairFunc(true);
            }
        }
    }

    protected virtual void GetItem() { }
    // 벨트에서 아이템 받아오는 함수
    protected virtual void SetItem() { }
    // 벨트나 건물로 아이템 보내는 함수

    public List<Item> PlayerGetItemList()
    {
        List<Item> itemListCopy = new List<Item>(itemList); // itemList를 복사하여 itemListCopy에 저장
        itemList.Clear(); // itemList 초기화
        ItemNumCheck();

        return itemListCopy;
    }

    public override void BeltGroupSendItem(ItemProps itemObj)
    {
        itemObjList.Add(itemObj);

        if (itemObjList.Count >= solidFactoryData.FullItemNum)        
            isFull = true;        
        else
            isFull = false;
    }

    public override bool OnBeltItem(ItemProps itemObj)
    {
        if(itemObjList.Count < solidFactoryData.FullItemNum)
        {
            itemObjList.Add(itemObj);

            if (GetComponent<BeltCtrl>())
                GetComponent<BeltCtrl>().beltGroupMgr.groupItem.Add(itemObj);

            if (itemObjList.Count >= solidFactoryData.FullItemNum)
                isFull = true;
            else
                isFull = false;

            return true;
        }
        return false;
    }

    public override void OnFactoryItem(ItemProps itemProps)
    {
        itemList.Add(itemProps.item);

        OnDestroyItem(itemProps);
        if (itemList.Count >= solidFactoryData.FullItemNum)
        {
            isFull = true;
        }
    }

    public override void OnFactoryItem(Item item)
    {
        itemList.Add(item);

        if (itemList.Count >= solidFactoryData.FullItemNum)
        {
            isFull = true;
        }
    }

    public override void ItemNumCheck()
    {
        if (GetComponent<BeltCtrl>())
        {
            if (itemObjList.Count >= solidFactoryData.FullItemNum)
            {
                isFull = true;
            }
            else
                isFull = false;
        }
        else
        {
            if (itemList.Count >= solidFactoryData.FullItemNum)
            {
                isFull = true;
            }
            else
                isFull = false;
        }
    }

    //public void GetFluid(float getNum) // 나중에 유체 기능도 넣을 때 추가
    //{

    //}

    //public override void DisableColliders()
    //{
    //    box2D.enabled = false;
    //}

    //public override void EnableColliders()
    //{
    //    box2D.enabled = true;
    //}

    public override void ColliderTriggerOnOff(bool isOn)
    {
        if (isOn)
            box2D.isTrigger = true;
        else
            box2D.isTrigger = false;
    }

    public override void SetBuild() 
    {
        unitCanvas.SetActive(true);
        hpBar.enabled = false;
        repairBar.enabled = true;
        repairGauge = 0;
        repairBar.fillAmount = repairGauge / solidFactoryData.MaxRepairGauge;
        isSetBuildingOk = true;
    }

    protected override void RepairFunc(bool isBuilding)
    {
        repairGauge += 10.0f * Time.deltaTime;

        if(isBuilding)
        {
            repairBar.fillAmount = repairGauge / solidFactoryData.MaxBuildingGauge;
            if (repairGauge >= solidFactoryData.MaxRepairGauge)
            {
                isPreBuilding = false;
                repairGauge = 0.0f;
                repairBar.enabled = false;
                if (hp < solidFactoryData.MaxHp[level])
                {
                    unitCanvas.SetActive(true);
                    hpBar.enabled = true;
                }
                else
                {
                    unitCanvas.SetActive(false);
                    //isRepair = true;
                }
                //EnableColliders();
                ColliderTriggerOnOff(false);

            }
        }
        else
        {
            repairBar.fillAmount = repairGauge / solidFactoryData.MaxRepairGauge;
            if (repairGauge >= solidFactoryData.MaxRepairGauge)
            {
                RepairEnd();
            }
        }
    }

    protected override void RepairEnd()
    {
        hpBar.enabled = true;

        //if (hp < solidFactoryData.MaxHp)
        //{
        //    unitCanvers.SetActive(true);
        //    hpBar.enabled = true;
        //}
        //else
        //{
        hp = solidFactoryData.MaxHp[level];
        unitCanvas.SetActive(false);
        //}

        hpBar.fillAmount = hp / solidFactoryData.MaxHp[level];

        repairBar.enabled = false;
        repairGauge = 0.0f;

        isRuin = false;
        isPreBuilding = false;

        ColliderTriggerOnOff(false);
        //EnableColliders();
    }

    public override void TakeDamage(float damage)
    {
        if (!isPreBuilding)
        {
            if (!unitCanvas.activeSelf)
            {
                unitCanvas.SetActive(true);
                hpBar.enabled = true;
            }
        }

        if (hp <= 0f)
            return;

        hp -= damage;
        hpBar.fillAmount = hp / solidFactoryData.MaxHp[level];

        if (hp <= 0f)
        {
            hp = 0f;
            DieFunc();
        }
    }
    public override void HealFunc(float heal)
    {
        if (hp == solidFactoryData.MaxHp[level])
        {
            return;
        }
        else if (hp + heal > solidFactoryData.MaxHp[level])
        {
            hp = solidFactoryData.MaxHp[level];
            if (!isRepair)
                unitCanvas.SetActive(false);
        }
        else
            hp += heal;

        hpBar.fillAmount = hp / solidFactoryData.MaxHp[level];
    }

    public override void RepairSet(bool repair)
    {
        hp = solidFactoryData.MaxHp[level];
        isRepair = repair;
        //repairBar.enabled = repair;
    }

    protected override void DieFunc()
    {
        //unitCanvers.SetActive(false);
        repairBar.enabled = true;
        hpBar.enabled = false;

        repairGauge = 0;
        repairBar.fillAmount = repairGauge / solidFactoryData.MaxBuildingGauge;

        //DisableColliders();
        ColliderTriggerOnOff(true);

        isRuin = true;
    }

    //public virtual void AddProductionFac(GameObject obj) { }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isPreBuilding)
        {
            buildingPosObj.Add(collision.gameObject);
            
            if(buildingPosObj.Count > 0)
            {
                canBuilding = false;
          
                PreBuilding preBuilding = GetComponentInParent<PreBuilding>();
                if (preBuilding != null)
                {
                    if (collision.GetComponent<SolidFactoryCtrl>() && !collision.GetComponent<SolidFactoryCtrl>().isSetBuildingOk)
                    {
                        preBuilding.isBuildingOk = true;
                    }
                    else
                        preBuilding.isBuildingOk = false;
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (isPreBuilding)
        {
            buildingPosObj.Remove(collision.gameObject);
            if (buildingPosObj.Count > 0)
                canBuilding = false;
            else
            {
                canBuilding = true;

                PreBuilding preBuilding = GetComponentInParent<PreBuilding>();
                if (preBuilding != null)
                    preBuilding.isBuildingOk = true;
            }
        }
    }
}
