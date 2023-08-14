using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering;
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

    public virtual void OpenUI() { }
    public virtual void CloseUI() { }
    public virtual void SetRecipe(Recipe _recipe) { }
    public virtual float GetProgress() { return prodTimer; }
    public virtual float GetFuel() { return fuel; }
    public virtual void OpenRecipe() { }
    public virtual void GetUIFunc() { }

    protected virtual void Awake()
    {
        GameManager gameManager = GameManager.instance;
        playerInven = gameManager.GetComponent<Inventory>();
        inventory = this.GetComponent<Inventory>();
        buildName = productionData.FactoryName;
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
            if (inObj.Count > 0 && !itemGetDelay && checkObj)
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

    protected override IEnumerator SetOutObjCoroutine(GameObject obj)
    {
        yield return new WaitForSeconds(0.1f);

        if (obj.GetComponent<Structure>() != null && !obj.GetComponent<Miner>() && !obj.GetComponent<ItemSpawner>())
        {
            if (obj.TryGetComponent(out BeltCtrl belt))
            {
                if (obj.GetComponentInParent<BeltGroupMgr>().nextObj == this.gameObject)
                {
                    StartCoroutine(SetInObjCoroutine(obj));
                    yield break;
                }
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

    protected override IEnumerator OutCheck(GameObject otherObj)
    {
        yield return new WaitForSeconds(0.1f);

        if (otherObj.TryGetComponent(out Structure otherFacCtrl))
        {
            if (otherObj.GetComponent<Production>())
                yield break;

            if (otherFacCtrl.outSameList.Contains(this.gameObject) && outSameList.Contains(otherObj))
            {
                StartCoroutine(SetInObjCoroutine(otherObj));
                outObj.Remove(otherObj);
                outSameList.Remove(otherObj);
                Invoke("RemoveSameOutList", 0.1f);
                StopCoroutine("SetFacDelay");
            }
        }
    }

    public virtual bool CanTakeItem(Item item) { return new bool(); }

    public virtual (Item, int) QuickPullOut() { return (new Item(), new int()); }

    protected override void GetItem()
    {
        itemGetDelay = true;

        if (!GetComponentInChildren<Miner>())
        {
            if (inObj[getItemIndex].TryGetComponent(out BeltCtrl belt) && belt.isItemStop)
            {
                if (CanTakeItem(belt.itemObjList[0].item))
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

                    Invoke("DelayGetItem", productionData.SendDelay);
                    itemGetDelay = false;
                }
                else
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
                return;
            }
        }
    }

    IEnumerator SetFacDelay(GameObject outFac)
    {
        var spawnItem = itemPool.Get();
        var sprite = spawnItem.GetComponent<SpriteRenderer>();
        sprite.color = new Color(1f, 1f, 1f, 0f);
        CircleCollider2D coll = spawnItem.GetComponent<CircleCollider2D>();
        coll.enabled = false;

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
                if (checkObj && outFac != null)
                {
                    if (outFac.TryGetComponent(out Structure outFactory))
                    {
                        outFactory.OnFactoryItem(output);
                    }
                }
                else
                {
                    sprite.sprite = output.icon;
                    spawnItem.item = output; 
                    playerInven.Add(spawnItem.item, spawnItem.amount);
                    sprite.color = new Color(1f, 1f, 1f, 1f);
                    coll.enabled = true;
                    itemPool.Release(spawnItem);
                    spawnItem = null;
                }

                SubFromInventory();
            }
        }

        if (spawnItem != null)
        {
            sprite.color = new Color(1f, 1f, 1f, 1f);
            coll.enabled = true; 
            itemPool.Release(spawnItem);
        }
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

                ColliderTriggerOnOff(false);
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
        ColliderTriggerOnOff(false);
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

        ColliderTriggerOnOff(true);
        isRuin = true;
    }

    public override void ResetCheckObj(GameObject game)
    {
        base.ResetCheckObj(game);

        for (int i = 0; i < outObj.Count; i++)
        {
            if (outObj[i] == game)
                outObj.Remove(game);
        }
        for (int i = 0; i < inObj.Count; i++)
        {
            if (inObj[i] == game)
                inObj.Remove(game);
        }
        sendItemIndex = 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.GetComponent<ItemProps>())
        {
            if (isPreBuilding)
            {
                buildingPosObj.Add(collision.gameObject);
                if (buildingPosObj.Count > 0)
                {
                    if (!collision.GetComponentInParent<PreBuilding>())
                    {
                        canBuilding = false;
                    }
                    PreBuilding preBuilding = GetComponentInParent<PreBuilding>();
                    if (preBuilding != null)
                    {
                        preBuilding.isBuildingOk = false;
                    }
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.GetComponent<ItemProps>())
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
}
