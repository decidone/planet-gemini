using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Refinery : FluidFactoryCtrl
{
    public List<GameObject> factoryList = new List<GameObject>();

    bool isUp = false;
    bool isRight = false;
    bool isDown = false;
    bool isLeft = false;

    protected override void Awake()
    {
        #region ProductionAwake
        inventory = this.GetComponent<Inventory>();
        buildName = productionData.FactoryName;
        box2D = GetComponent<BoxCollider2D>();
        hp = productionData.MaxHp[level];
        hpBar.fillAmount = hp / productionData.MaxHp[level];
        repairBar.fillAmount = 0;

        itemPool = new ObjectPool<ItemProps>(CreateItemObj, OnGetItem, OnReleaseItem, OnDestroyItem, maxSize: 20);
        #endregion
    }

    protected override void Start()
    {
        #region ProductionStart
        itemDic = ItemList.instance.itemDic;
        recipe = new Recipe();
        output = null;

        GameManager gameManager = GameManager.instance;
        canvas = gameManager.GetComponent<GameManager>().inventoryUiCanvas;
        sInvenManager = canvas.GetComponent<StructureInvenManager>();
        rManager = canvas.GetComponent<RecipeManager>();
        GetUIFunc();
        base.nearObj = new GameObject[4];

        CheckPos();
        #endregion
    }

    protected override void Update()
    {
        #region ProductionUpdate
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
        #endregion

        base.Update();

        if (!removeState)
        {
            if (!isPreBuilding)
            {
                if (isUp == false)
                    isUp = ObjCheck(transform.up);
                if (isRight == false)
                    isRight = ObjCheck(transform.right);
                if (isDown == false)
                    isDown = ObjCheck(-transform.up);
                if (isLeft == false)
                    isLeft = ObjCheck(-transform.right);
            }
        }

        if (!isPreBuilding)
        {
            var slot = inventory.SlotCheck(0);
            var slot1 = inventory.SlotCheck(1);

            if (recipe.name != null)
            {
                if (slot.amount >= recipe.amounts[0] && (slot1.amount + recipe.amounts[recipe.amounts.Count - 1]) <= maxAmount)
                {
                    output = itemDic[recipe.items[recipe.items.Count - 1]];

                    if (slot1.item == output || slot1.item == null)
                    {
                        prodTimer += Time.deltaTime;
                        if (prodTimer > cooldown)
                        {
                            inventory.Sub(0, recipe.amounts[0]);
                            inventory.SlotAdd(1, output, recipe.amounts[recipe.amounts.Count - 1]);
                            prodTimer = 0;
                        }
                    }
                    else
                    {
                        prodTimer = 0;
                    }
                }
                else
                {
                    prodTimer = 0;
                }
            }

            if (slot1.amount > 0 && outObj.Count > 0 && !itemSetDelay)
            {
                SendItem(output);
            }
        }
    }

    bool ObjCheck(Vector3 vec)
    {
        RaycastHit2D[] Hits = Physics2D.RaycastAll(this.gameObject.transform.position, vec, 1f);

        for (int a = 0; a < Hits.Length; a++)
        {
            if (Hits[a].collider.GetComponent<Refinery>() != this.gameObject.GetComponent<Refinery>())
            {
                if (Hits[a].collider.CompareTag("Factory") && !Hits[a].collider.GetComponent<Structure>().isPreBuilding)
                {
                    nearObj[0] = Hits[a].collider.gameObject;
                    SetOutObj(nearObj[0]);
                    return true;
                }
            }
        }
        return false;
    }

    void SetOutObj(GameObject obj)
    {
        if (obj.GetComponent<FluidFactoryCtrl>() != null)
        {
            factoryList.Add(obj);
            if (obj.GetComponent<PipeCtrl>() != null)
            {
                obj.GetComponent<PipeCtrl>().FactoryVecCheck(this.transform.position);
                obj.GetComponentInParent<PipeGroupMgr>().FactoryListAdd(this.gameObject);
            }
        }
    }

    public override void OpenUI()
    {
        sInvenManager.SetInven(inventory, ui);
        sInvenManager.SetProd(this);
        sInvenManager.progressBar.SetMaxProgress(cooldown);

        rManager.recipeBtn.gameObject.SetActive(true);
        rManager.recipeBtn.onClick.RemoveAllListeners();
        rManager.recipeBtn.onClick.AddListener(OpenRecipe);

        sInvenManager.InvenInit();
        if (recipe.name != null)
            SetRecipe(recipe);
    }

    public override void CloseUI()
    {
        sInvenManager.ReleaseInven();

        rManager.recipeBtn.onClick.RemoveAllListeners();
        rManager.recipeBtn.gameObject.SetActive(false);
    }

    public override void OpenRecipe()
    {
        rManager.OpenUI();
        rManager.SetRecipeUI("Refinery", this);
    }

    public override void SetRecipe(Recipe _recipe)
    {
        if (recipe.name != null && recipe != _recipe)
        {
            sInvenManager.EmptySlot();
        }
        recipe = _recipe;
        sInvenManager.ResetInvenOption();
        sInvenManager.slots[0].SetInputItem(itemDic[recipe.items[0]]);
        sInvenManager.slots[1].outputSlot = true;
        sInvenManager.progressBar.SetMaxProgress(recipe.cooldown);
    }

    public override bool CanTakeItem(Item item)
    {
        if (recipe.name != null && itemDic[recipe.items[0]] == item)
        {
            var slot = inventory.SlotCheck(0);
            return slot.amount < 99;
        }

        return false;
    }

    public override void OnFactoryItem(ItemProps itemProps)
    {
        if (itemDic[recipe.items[0]] == itemProps.item)
        {
            inventory.SlotAdd(0, itemProps.item, itemProps.amount);
        }

        OnDestroyItem(itemProps);
    }
    public override void OnFactoryItem(Item item)
    {
        if (itemDic[recipe.items[0]] == item)
        {
            inventory.SlotAdd(0, item, 1);
        }
    }

    protected override void SubFromInventory()
    {
        inventory.Sub(1, 1);
    }

    public override bool CheckOutItemNum()
    {
        var slot1 = inventory.SlotCheck(1);
        if (slot1.amount > 0)
            return true;
        else
            return false;
    }

    public override void ItemNumCheck()
    {
        var slot1 = inventory.SlotCheck(1);

        if (slot1.amount < maxAmount)
            isFull = false;
        else
            isFull = true;
    }

    public override (Item, int) QuickPullOut()
    {
        var slot1 = inventory.SlotCheck(1);
        if (slot1.amount > 0)
            inventory.Sub(1, slot1.amount);
        return slot1;
    }

    public override void GetUIFunc()
    {
        InventoryList inventoryList = canvas.GetComponent<InventoryList>();

        foreach (GameObject list in inventoryList.StructureStorageArr)
        {
            if (list.name == "Refinery")
            {
                ui = list;
            }
        }
    }
    protected override void AddInvenItem()
    {
        var slot = inventory.SlotCheck(0);
        var slot1 = inventory.SlotCheck(1);

        if (slot.item != null)
        {
            playerInven.Add(slot.item, slot.amount);
        }
        if (slot1.item != null)
        {
            playerInven.Add(slot1.item, slot1.amount);
        }
    }
}
