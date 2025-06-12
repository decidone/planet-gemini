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
        maxHp = structureData.MaxHp[level];
        defense = structureData.Defense[level];
        hp = maxHp;
        getDelay = 0.01f;
        sendDelay = structureData.SendDelay[level]; 
        hpBar.fillAmount = hp / maxHp;
        repairBar.fillAmount = 0;
        repairEffect = GetComponentInChildren<RepairEffectFunc>();
        destroyInterval = structureData.RemoveGauge;
        soundManager = SoundManager.instance;
        destroyTimer = destroyInterval;
        onEffectUpgradeCheck += IncreasedStructureCheck;
        onEffectUpgradeCheck.Invoke();
        setModel = GetComponent<SpriteRenderer>();
        if (TryGetComponent(out Animator anim))
        {
            getAnim = true;
            animator = anim;
        }
        NonOperateStateSet(isOperate);
        #endregion

        #region FluidFactoryAwake
        GameManager gameManager = GameManager.instance;
        myFluidScript = GetComponent<FluidFactoryCtrl>();
        playerInven = gameManager.inventory;
        mainSource = null;
        howFarSource = -1;
        preSaveFluidNum = 0;
        uiOpened = false;
        myVision.SetActive(false);

        connectors = new List<EnergyGroupConnector>();
        conn = null;
        efficiency = 0;
        effiCooldown = 0;
        energyUse = structureData.EnergyUse[level];
        isEnergyStr = structureData.IsEnergyStr;
        energyProduction = structureData.Production;
        energyConsumption = structureData.Consumption[level];

        displaySlot = GameObject.Find("Canvas").transform.Find("StructureInfo").transform.Find("Storage")
            .transform.Find("Refinery").transform.Find("DisplaySlot").GetComponent<Slot>();
        #endregion
    }

    protected override void Start()
    {
        #region ProductionStart
        itemDic = ItemList.instance.itemDic;
        if (recipe == null)
            recipe = new Recipe();
        fluidName = "CrudeOil";

        GameManager gameManager = GameManager.instance;
        canvas = gameManager.GetComponent<GameManager>().inventoryUiCanvas;
        sInvenManager = canvas.GetComponent<StructureInvenManager>();
        rManager = canvas.GetComponent<RecipeManager>();
        GetUIFunc();
        CheckPos();
        #endregion

        displaySlot.SetInputItem(ItemList.instance.itemDic["CrudeOil"]);
        displaySlot.AddItem(ItemList.instance.itemDic["CrudeOil"], 0);
    }

    protected override void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        #region ProductionUpdate
        if (!removeState)
        {
            if (isRepair)
            {
                RepairFunc(false);
            }
            else if (isPreBuilding && isSetBuildingOk)
            {
                RepairFunc(true);
            }

            WarningStateCheck();
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
            

        if (IsServer && !isPreBuilding)
        {
            if (inObj.Count > 0 && !itemGetDelay && checkObj)
                GetItem();
        }
        if (DelayGetList.Count > 0 && inObj.Count > 0)
        {
            GetDelayFunc(DelayGetList[0], 0);
        }
        #endregion

        base.Update();

        if (!isPreBuilding)
        {
            FluidChangeCheck();

            var slot = inventory.SlotCheck(0);

            if (recipe.name != null)
            {
                if (conn != null && conn.group != null && conn.group.efficiency > 0)
                {
                    EfficiencyCheck();

                    if (saveFluidNum >= recipe.amounts[0] && (slot.amount + recipe.amounts[recipe.amounts.Count - 1]) <= maxAmount)
                    {
                        //output = itemDic[recipe.items[recipe.items.Count - 1]];

                        if (slot.item == output || slot.item == null)
                        {
                            OperateStateSet(true);
                            prodTimer += Time.deltaTime;
                            if (prodTimer > effiCooldown - ((overclockOn ? effiCooldown * overclockPer / 100 : 0) + effiCooldownUpgradeAmount))
                            {
                                saveFluidNum -= recipe.amounts[0];
                                if (IsServer)
                                {
                                    inventory.SlotAdd(0, output, recipe.amounts[recipe.amounts.Count - 1]);
                                    Overall.instance.OverallProd(output, recipe.amounts[recipe.amounts.Count - 1]);
                                }
                                soundManager.PlaySFX(gameObject, "structureSFX", "Structure");
                                prodTimer = 0;
                            }
                        }
                        else
                        {
                            OperateStateSet(false);
                            prodTimer = 0;
                        }
                    }
                    else
                    {
                        OperateStateSet(false);
                        prodTimer = 0;
                    }
                }
                else
                {
                    OperateStateSet(false);
                    prodTimer = 0;
                }
            }

            if (IsServer && slot.amount > 0 && outObj.Count > 0 && !itemSetDelay && checkObj)
            {
                int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(output);
                SendItem(itemIndex);
                //SendItem(output);
            }
            if (DelaySendList.Count > 0 && outObj.Count > 0 && !outObj[DelaySendList[0].Item2].GetComponent<Structure>().isFull)
            {
                SendDelayFunc(DelaySendList[0].Item1, DelaySendList[0].Item2, 0);
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
        base.OpenUI();
        uiOpened = true;
        displaySlot.SetItemAmount((int)saveFluidNum);

        sInvenManager.SetInven(inventory, ui);
        sInvenManager.SetProd(this);
        sInvenManager.progressBar.SetMaxProgress(effiCooldown - ((overclockOn ? effiCooldown * overclockPer / 100 : 0) + effiCooldownUpgradeAmount));
        sInvenManager.SetCooldownText(effiCooldown - ((overclockOn ? effiCooldown * overclockPer / 100 : 0) + effiCooldownUpgradeAmount));
        //sInvenManager.progressBar.SetMaxProgress(cooldown);

        rManager.recipeBtn.gameObject.SetActive(true);
        rManager.recipeBtn.onClick.RemoveAllListeners();
        rManager.recipeBtn.onClick.AddListener(OpenRecipe);

        sInvenManager.InvenInit();
        if (recipe.name != null)
            SetRecipe(recipe, recipeIndex);
    }

    public override void CloseUI()
    {
        base.CloseUI();
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

    public override void SetRecipe(Recipe _recipe, int index)
    {
        base.SetRecipe(_recipe, index);
        sInvenManager.slots[0].SetInputItem(itemDic[recipe.items[1]]);
        sInvenManager.slots[0].SetNeedAmount(recipe.amounts[1]);
        sInvenManager.slots[0].outputSlot = true;
    }

    public override void SetOutput(Recipe recipe)
    {
        output = itemDic[recipe.items[recipe.items.Count - 1]];
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

    public override void AddInvenItem()
    {
        if (isInHostMap)
            playerInven = GameManager.instance.hostMapInven;
        else
            playerInven = GameManager.instance.clientMapInven;
        var slot = inventory.SlotCheck(0);

        if (slot.item != null)
        {
            playerInven.Add(slot.item, slot.amount);
        }
    }

    public override Dictionary<Item, int> PopUpItemCheck()
    {
        if (saveFluidNum > 0 && fluidName != "")
        {
            Dictionary<Item, int> returnDic = new Dictionary<Item, int>();
            returnDic.Add(ItemList.instance.itemDic[fluidName], (int)saveFluidNum);

            var slot = inventory.SlotCheck(0);
            if (slot.item != null && slot.amount > 0)
                returnDic.Add(slot.item, slot.amount);

            return returnDic;
        }
        else
            return null;
    }

    protected override void ItemDrop()
    {
        for (int i = 0; i < inventory.space; i++)
        {
            var invenItem = inventory.SlotCheck(i);

            if (invenItem.item != null && invenItem.amount > 0)
            {
                ItemToItemProps(invenItem.item, invenItem.amount);
            }
        }
    }

    protected override void NonOperateStateSet(bool isOn)
    {
        setModel.sprite = strImg[isOn ? 1 : 0];
    }
}
