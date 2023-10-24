using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

// UTF-8 설정
public class Refinery : FluidFactoryCtrl
{
    public Slot displaySlot;
    int preSaveFluidNum;
    bool uiOpened;

    protected override void Awake()
    {
        #region ProductionAwake
        inventory = this.GetComponent<Inventory>();
        buildName = structureData.FactoryName;
        col = GetComponent<BoxCollider2D>();
        hp = structureData.MaxHp[level];
        hpBar.fillAmount = hp / structureData.MaxHp[level];
        repairBar.fillAmount = 0;
        #endregion
        #region FluidFactoryAwake
        GameManager gameManager = GameManager.instance;
        myFluidScript = GetComponent<FluidFactoryCtrl>();
        playerInven = gameManager.GetComponent<Inventory>();
        mainSource = null;
        howFarSource = -1;
        preSaveFluidNum = 0;
        uiOpened = false;

        Debug.Log(GameObject.Find("Canvas"));
        Debug.Log(GameObject.Find("Canvas").transform.Find("StructureInfo"));
        Debug.Log(GameObject.Find("Canvas").transform.Find("StructureInfo").transform.Find("Storage"));
        Debug.Log(GameObject.Find("Canvas").transform.Find("StructureInfo").transform.Find("Storage")
            .transform.Find("Refinery"));
        Debug.Log(GameObject.Find("Canvas").transform.Find("StructureInfo").transform.Find("Storage")
            .transform.Find("Refinery").transform.Find("DisplaySlot").GetComponent<Slot>());

        displaySlot = GameObject.Find("Canvas").transform.Find("StructureInfo").transform.Find("Storage")
            .transform.Find("Refinery").transform.Find("DisplaySlot").GetComponent<Slot>();
        #endregion
    }

    protected override void Start()
    {
        #region ProductionStart
        itemDic = ItemList.instance.itemDic;
        recipe = new Recipe();
        output = null;
        fluidName = "CrudeOil";

        GameManager gameManager = GameManager.instance;
        canvas = gameManager.GetComponent<GameManager>().inventoryUiCanvas;
        sInvenManager = canvas.GetComponent<StructureInvenManager>();
        rManager = canvas.GetComponent<RecipeManager>();
        GetUIFunc();
        nearObj = new GameObject[4];
        CheckPos();
        #endregion

        displaySlot.SetInputItem(ItemList.instance.itemDic["CrudeOil"]);
        displaySlot.AddItem(ItemList.instance.itemDic["CrudeOil"], 0);
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
        if (isSetBuildingOk)
        {
            for (int i = 0; i < nearObj.Length; i++)
            {
                if (nearObj[i] == null)
                {
                    CheckNearObj(checkPos[i], i, obj => CheckOutObjScript(obj));
                }
            }
        }
            

        if (!isPreBuilding)
        {
            if (inObj.Count > 0 && !itemGetDelay && checkObj)
                GetItem();
        }
        #endregion

        base.Update();

        if (!isPreBuilding)
        {
            FluidChangeCheck();

            var slot = inventory.SlotCheck(0);
            var slot1 = inventory.SlotCheck(1);

            if (recipe.name != null)
            {
                if (saveFluidNum >= recipe.amounts[0] && (slot1.amount + recipe.amounts[recipe.amounts.Count - 1]) <= maxAmount)
                {
                    output = itemDic[recipe.items[recipe.items.Count - 1]];

                    if (slot1.item == output || slot1.item == null)
                    {
                        prodTimer += Time.deltaTime;
                        if (prodTimer > cooldown)
                        {
                            saveFluidNum -= recipe.amounts[0];
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

            if (slot1.amount > 0 && outObj.Count > 0 && !itemSetDelay && checkObj)
            {
                SendItem(output);
            }
        }
    }

    void CheckOutObjScript(GameObject game)
    {
        StartCoroutine(SetOutObjCoroutine(game));
        if (game.TryGetComponent(out FluidFactoryCtrl factoryCtrl))
            StartCoroutine("MainSourceCheck", factoryCtrl);
    }

    void FluidChangeCheck()
    {
        if (preSaveFluidNum != (int)saveFluidNum)
        {
            preSaveFluidNum = (int)saveFluidNum;
            if (uiOpened)
                displaySlot.SetItemAmount((int)saveFluidNum);
        }
    }

    public override void OpenUI()
    {
        uiOpened = true;
        displaySlot.SetItemAmount((int)saveFluidNum);

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
        uiOpened = false;

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
        var slot1 = inventory.SlotCheck(1);

        if (slot1.item != null)
        {
            playerInven.Add(slot1.item, slot1.amount);
        }
    }

    public override Dictionary<Item, int> PopUpItemCheck()
    {
        if (saveFluidNum > 0 && fluidName != "")
        {
            Dictionary<Item, int> returnDic = new Dictionary<Item, int>();
            returnDic.Add(ItemList.instance.itemDic[fluidName], (int)saveFluidNum);

            var slot1 = inventory.SlotCheck(1);
            if (slot1.item != null && slot1.amount > 0)
                returnDic.Add(slot1.item, slot1.amount);

            return returnDic;
        }
        else
            return null;
    }
}
