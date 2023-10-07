using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

// UTF-8 설정
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
    protected int invenCount;
    protected Dictionary<Item, int> invenSlotDic;

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
        buildName = structureData.FactoryName;
        col = GetComponent<BoxCollider2D>();
        hp = structureData.MaxHp[level];
        hpBar.fillAmount = hp / structureData.MaxHp[level];
        repairBar.fillAmount = 0;
        canInsertItem = false;
    }

    protected virtual void Start()
    {
        itemDic = ItemList.instance.itemDic;
        recipe = new Recipe();
        output = null;

        if (sizeOneByOne)
            nearObj = new GameObject[4];
        else
            nearObj = new GameObject[8];

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
                if (nearObj[i] == null && sizeOneByOne)
                {
                    CheckNearObj(checkPos[i], i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
                }
                else if (nearObj[i] == null && !sizeOneByOne) 
                {
                    int dirIndex = i / 2;
                    CheckNearObj(startTransform[indices[i]], directions[dirIndex], i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
                }
            }
        }
    }

    protected override IEnumerator SetOutObjCoroutine(GameObject obj)
    {
        checkObj = false;
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
                belt.FactoryPosCheck(GetComponentInParent<Structure>());
            }
            else
            {
                outSameList.Add(obj);
                StartCoroutine(OutCheck(obj));
            }
            outObj.Add(obj);
            StartCoroutine(UnderBeltConnectCheck(obj));
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
                if (belt.itemObjList.Count > 0 && CanTakeItem(belt.itemObjList[0].item))
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

                    Invoke("DelayGetItem", structureData.SendDelay);
                }
                else
                {
                    getItemIndex++;
                    if (getItemIndex >= inObj.Count)
                        getItemIndex = 0;

                    DelayGetItem();
                    return;
                }
            }
            else
            {
                getItemIndex++;
                if (getItemIndex >= inObj.Count)
                    getItemIndex = 0;

                DelayGetItem();
                return;
            }
        }
    }
}
