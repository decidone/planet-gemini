using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using System;

public abstract class Production : Structure
{
    // 연료(석탄, 전기), 작업 시간, 작업량, 재료, 생산품, 아이템 슬롯
    [SerializeField]
    protected GameObject ui;
    [SerializeField]
    protected StructureInvenManager sInvenManager;
    [SerializeField]
    protected RecipeManager rManager;
    [SerializeField]
    protected int maxAmount;
    [SerializeField]
    protected float cooldown;

    protected GameObject canvas;

    protected Inventory inventory;
    protected Dictionary<string, Item> itemDic;
    protected float prodTimer;
    protected int fuel;
    protected int maxFuel;
    protected Item output;
    protected Recipe recipe;
    protected List<Recipe> recipes;

    [SerializeField]
    protected ProductionData productionData;
    protected ProductionData ProductionData { set { productionData = value; } }

    protected bool itemGetDelay = false;
    protected bool itemSetDelay = false;

    [HideInInspector]
    public BoxCollider2D box2D = null;

    //[SerializeField]
    //Sprite[] modelNum = new Sprite[4];
    //SpriteRenderer setModel;
    //private int prevDirNum = -1; // 이전 방향 값을 저장할 변수
    protected List<GameObject> inObj = new List<GameObject>();
    protected List<GameObject> outObj = new List<GameObject>();

    GameObject[] nearObj = new GameObject[4];

    protected int getObjNum = 0;
    protected int sendObjNum = 0;

    Vector2[] checkPos = new Vector2[4];

    protected Coroutine setFacDelayCoroutine; // 실행 중인 코루틴을 저장하는 변수

    public abstract void OpenUI();
    public abstract void CloseUI();

    protected virtual void Awake()
    {
        inventory = this.GetComponent<Inventory>();

        box2D = GetComponent<BoxCollider2D>();
        hp = productionData.MaxHp[level];
        hpBar.fillAmount = hp / productionData.MaxHp[level];
        repairBar.fillAmount = 0;

        itemPool = new ObjectPool<ItemProps>(CreateItemObj, OnGetItem, OnReleaseItem, OnDestroyItem, maxSize: 20);
    }

    protected virtual void Start()
    {
        itemDic = ItemList.instance.itemDic;  
        recipe = new Recipe();
        output = null;

        GameManager gameManager = GameManager.instance;
        canvas = gameManager.GetComponent<GameManager>().inventoryUiCanvas;
        sInvenManager = canvas.GetComponent<StructureInvenManager>();
        rManager = canvas.GetComponent<RecipeManager>();
        GetUIFunc();

        CheckPos();
    }

    protected virtual void Update()
    {
        //SetDirNum();
        if (!removeState)
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
        if (!isPreBuilding)
        {
            if (inObj.Count > 0 && !itemGetDelay)
                GetItem();

            for (int i = 0; i < nearObj.Length; i++)
            {
                if (nearObj[i] == null)
                {
                    CheckNearObj(checkPos[i], i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
                }
            }
        }
    }

    public virtual void SetRecipe(Recipe _recipe) { }
    public virtual float GetProgress() { return prodTimer; }
    public virtual float GetFuel() { return fuel; }
    public virtual void OpenRecipe() { }
    public virtual void GetUIFunc() { }

    protected override void CheckPos()
    {
        Vector2[] dirs = { Vector2.up, Vector2.right, Vector2.down, Vector2.left };

        for (int i = 0; i < 4; i++)
        {
            checkPos[i] = dirs[i];
        }
    }

    protected override void CheckNearObj(Vector2 direction, int index, Action<GameObject> callback)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, 1f);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hitCollider = hits[i].collider;
            if (hitCollider.CompareTag("Factory") && !hitCollider.GetComponent<Structure>().isPreBuilding &&
                hitCollider.GetComponent<Structure>() != GetComponent<Structure>())
            {
                nearObj[index] = hits[i].collider.gameObject;
                callback(hitCollider.gameObject);
                break;
            }
        }
    }

    protected IEnumerator SetInObjCoroutine(GameObject obj)
    {
        yield return new WaitForSeconds(0.1f);

        if (obj.GetComponent<Structure>() != null)
        {
            inObj.Add(obj);
        }
    }

    protected IEnumerator SetOutObjCoroutine(GameObject obj)
    {
        yield return new WaitForSeconds(0.1f);

        if (obj.GetComponent<Structure>() != null)
        {
            if (obj.TryGetComponent(out BeltCtrl belt))
            {
                if (obj.GetComponentInParent<BeltGroupMgr>().nextObj == this.gameObject)
                {
                    StartCoroutine(SetInObjCoroutine(obj));
                    yield break;
                }
                if (belt.beltState == BeltState.SoloBelt || belt.beltState == BeltState.StartBelt)
                    belt.FactoryVecCheck(GetComponentInParent<Structure>());
            }
            else
            {
                outSameList.Add(obj);
                StartCoroutine(OutCheck(obj));
            }
            outObj.Add(obj);
        }
    }

    protected IEnumerator OutCheck(GameObject otherObj)
    {
        yield return new WaitForSeconds(0.1f);

        if(otherObj.TryGetComponent(out Structure otherFacCtrl))
        {
            if (otherObj.GetComponent<Production>())
                yield break;
            foreach (GameObject otherList in otherFacCtrl.outSameList)
            {
                if (otherList == this.gameObject)
                {
                    for (int a = outObj.Count - 1; a >= 0; a--)
                    {
                        if (otherObj == outObj[a])
                        {
                            StartCoroutine(SetInObjCoroutine(otherObj));
                            outObj.RemoveAt(a);

                            //if(!GetComponentInChildren<Miner>() && otherObj.GetComponent<SolidFactoryCtrl>())
                            //{
                            //    otherObj.GetComponent<SolidFactoryCtrl>().AddProductionFac(gameObject);
                            //}

                            Invoke("RemoveSameOutList", 0.1f);
                            StopCoroutine("SetFacDelay");
                            break;
                        }
                    }
                }
            }
        }        
    }

    public virtual bool CanTakeItem(Item item) { return new bool(); }

    public virtual (Item, int) QuickPullOut() { return  (new Item(), new int()); }

    protected void GetItem()
    {
        itemGetDelay = true;

        if (!GetComponentInChildren<Miner>())
        {
            if (inObj[getObjNum].TryGetComponent(out BeltCtrl belt) && belt.isItemStop)
            {
                if (CanTakeItem(belt.itemObjList[0].item))
                {
                    OnFactoryItem(belt.itemObjList[0]);
                    belt.itemObjList[0].transform.position = this.transform.position;
                    belt.isItemStop = false;
                    belt.itemObjList.RemoveAt(0);
                    belt.beltGroupMgr.GroupItem.RemoveAt(0);
                    belt.ItemNumCheck();

                    getObjNum++;
                    if (getObjNum >= inObj.Count)
                        getObjNum = 0;

                    Invoke("DelayGetItem", productionData.SendDelay);
                    itemGetDelay = false;
                }
                else
                {
                    getObjNum++;
                    if (getObjNum >= inObj.Count)
                        getObjNum = 0;

                    itemGetDelay = false;
                    return;
                }
            }
            else
            {
                getObjNum++;
                if (getObjNum >= inObj.Count)
                    getObjNum = 0;

                itemGetDelay = false;
                return;
            }
        }
    }

    protected virtual void SubFromInventory() { }

    protected void SetItem()
    {
        if (setFacDelayCoroutine != null)
        {
            return;
        }

        itemSetDelay = true;

        Structure outFactory = outObj[sendObjNum].GetComponent<Structure>();

        if (outFactory.isFull == false)
        {
            if (outObj[sendObjNum].GetComponent<BeltCtrl>())
            {
                ItemProps spawnItem = itemPool.Get();
                if (outFactory.OnBeltItem(spawnItem))
                {
                    SpriteRenderer sprite = spawnItem.GetComponent<SpriteRenderer>();
                    sprite.sprite = output.icon;
                    spawnItem.item = output;
                    spawnItem.amount = 1;
                    spawnItem.transform.position = transform.position;

                    SubFromInventory();
                }
                else
                {
                    OnDestroyItem(spawnItem);   
                    itemSetDelay = false;
                    return;
                }
            }
            else if (outObj[sendObjNum].GetComponent<SolidFactoryCtrl>()) 
            {
                StartCoroutine("SetFacDelay", outObj[sendObjNum]);
            }
            else if (outObj[sendObjNum].TryGetComponent(out Production production))
            {
                if(production.CanTakeItem(output))
                {
                    StartCoroutine("SetFacDelay", outObj[sendObjNum]);
                }
            }

            sendObjNum++;
            if (sendObjNum >= outObj.Count)
            {
                sendObjNum = 0;
            }
            Invoke("DelaySetItem", productionData.SendDelay);
        }
        else
        {
            sendObjNum++;
            if (sendObjNum >= outObj.Count)
            {
                sendObjNum = 0;
            }

            itemSetDelay = false;
        }
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
            var t = Mathf.Clamp01(elapsed / (distance / productionData.SendSpeed[level]));
            spawnItem.transform.position = Vector3.Lerp(spawnItem.transform.position, targetPos, t);

            yield return null;
        }

        if (spawnItem != null && spawnItem.transform.position == targetPos)
        {
            if (CheckOutItemNum())
            {
                var outFactory = outFac.GetComponent<Structure>();
                outFactory.OnFactoryItem(output);

                SubFromInventory();

                //ItemNumCheck();
            }
        }

        if (spawnItem != null)
        {
            sprite.color = new Color(1f, 1f, 1f, 1f);
            itemPool.Release(spawnItem);
        }
    }

    protected void DelaySetItem()
    {
        itemSetDelay = false;
    }
    protected void DelayGetItem()
    {
        itemGetDelay = false;
    }

    //public void GetFluid(float getNum) // 나중에 유체 기능도 넣을 때 추가
    //{

    //}

    public override void DisableColliders()
    {
        box2D.enabled = false;
    }

    public override void EnableColliders()
    {
        box2D.enabled = true;
    }

    public override void SetBuild()
    {
        unitCanvas.SetActive(true);
        hpBar.enabled = false;
        repairBar.enabled = true;
        repairGauge = 0;
        repairBar.fillAmount = repairGauge / productionData.MaxRepairGauge;
        isSetBuildingOk = true;
    }

    protected override void RepairFunc(bool isBuilding)
    {
        repairGauge += 10.0f * Time.deltaTime;

        if (isBuilding)
        {
            repairBar.fillAmount = repairGauge / productionData.MaxBuildingGauge;
            if (repairGauge >= productionData.MaxRepairGauge)
            {
                isPreBuilding = false;
                repairGauge = 0.0f;
                repairBar.enabled = false;
                if (hp < productionData.MaxHp[level])
                {
                    unitCanvas.SetActive(true);
                    hpBar.enabled = true;
                }
                else
                {
                    unitCanvas.SetActive(false);
                }
                EnableColliders();
            }
        }
        else
        {
            repairBar.fillAmount = repairGauge / productionData.MaxRepairGauge;
            if (repairGauge >= productionData.MaxRepairGauge)
            {
                RepairEnd();
            }
        }
    }

    protected override void RepairEnd()
    {
        hpBar.enabled = true;

        hp = productionData.MaxHp[level];
        unitCanvas.SetActive(false);

        hpBar.fillAmount = hp / productionData.MaxHp[level];

        repairBar.enabled = false;
        repairGauge = 0.0f;

        isRuin = false;
        isPreBuilding = false;

        EnableColliders();
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
        hpBar.fillAmount = hp / productionData.MaxHp[level];

        if (hp <= 0f)
        {
            hp = 0f;
            DieFunc();
        }
    }
    public override void HealFunc(float heal)
    {
        if (hp == productionData.MaxHp[level])
        {
            return;
        }
        else if (hp + heal > productionData.MaxHp[level])
        {
            hp = productionData.MaxHp[level];
            if (!isRepair)
                unitCanvas.SetActive(false);
        }
        else
            hp += heal;

        hpBar.fillAmount = hp / productionData.MaxHp[level];
    }

    public override void RepairSet(bool repair)
    {
        hp = productionData.MaxHp[level];
        isRepair = repair;
    }

    protected override void DieFunc()
    {
        repairBar.enabled = true;
        hpBar.enabled = false;

        repairGauge = 0;
        repairBar.fillAmount = repairGauge / productionData.MaxBuildingGauge;

        DisableColliders();

        isRuin = true;
    }
}
