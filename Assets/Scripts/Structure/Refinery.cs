using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

// UTF-8 설정
public class Refinery : FluidFactoryCtrl
{
    public Slot displaySlot;
    int preSaveFluidNum;

    protected override void Awake()
    {
        #region ProductionAwake
        inventory = this.GetComponent<Inventory>();
        if (inventory != null)
        {
            inventory.onItemChangedCallback += CheckSlotState;
            inventory.onItemChangedCallback += CheckInvenIsFull;
            CheckSlotState(0);
            CheckInvenIsFull(0);
        }
        buildName = structureData.FactoryName;
        col = GetComponent<BoxCollider2D>();
        maxHp = structureData.MaxHp[level];
        defense = structureData.Defense[level];
        hp = maxHp;
        getDelay = 0.05f;
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
        WarningStateCheck();
        #endregion

        #region FluidFactoryAwake
        GameManager gameManager = GameManager.instance;
        myFluidScript = GetComponent<FluidFactoryCtrl>();
        playerInven = gameManager.inventory;
        mainSource = null;
        howFarSource = -1;
        preSaveFluidNum = 0;
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
        fluidManager = FluidManager.instance;
        #endregion
    }

    protected override void Start()
    {
        #region ProductionStart
        itemDic = ItemList.instance.itemDic;
        if (recipe == null)
            recipe = new Recipe();
        fluidName = "CrudeOil";
        isConsumeSource = true;
        consumeSource = this;
        GameManager gameManager = GameManager.instance;
        canvas = gameManager.GetComponent<GameManager>().inventoryUiCanvas;
        sInvenManager = canvas.GetComponent<StructureInvenManager>();
        rManager = canvas.GetComponent<RecipeManager>();
        GetUIFunc();
        StrBuilt();
        #endregion

        displaySlot.SetInputItem(ItemList.instance.itemDic["CrudeOil"]);
        displaySlot.AddItem(ItemList.instance.itemDic["CrudeOil"], 0);
        StartCoroutine(EfficiencyCheck());
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
            else if (isPreBuilding)
            {
                RepairFunc(true);
            }
        }
        //if (isSetBuildingOk)
        //{
        //    for (int i = 0; i < nearObj.Length; i++)
        //    {
        //        if (nearObj[i] == null)
        //        {
        //            CheckNearObj(checkPos[i], i, obj => CheckOutObjScript(obj));
        //        }
        //    }
        //}
            

        if (IsServer && !isPreBuilding)
        {
            if (inObj.Count > 0 && !itemGetDelay)
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

            if (recipe.name != null)
            {
                if (conn != null && conn.group != null && conn.group.efficiency > 0)
                {
                    if (saveFluidNum >= recipe.amounts[0] && (slot.Item2 + recipe.amounts[recipe.amounts.Count - 1]) <= maxAmount)
                    {
                        if (slot.Item1 == output || slot.Item1 == null)
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

            if (IsServer && slot.Item2 > 0 && outObj.Count > 0 && !itemSetDelay)
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

    public override void CheckSlotState(int slotindex)
    {
        // update에서 검사해야 하는 특정 슬롯들 상태를 인벤토리 콜백이 있을 때 미리 저장
        slot = inventory.SlotCheck(0);
    }

    public override void NearStrBuilt()
    {
        // 건물을 지었을 때나 근처에 새로운 건물이 지어졌을 때 동작
        // 변경사항이 생기면 DelayNearStrBuiltCoroutine()에도 반영해야 함
        if (IsServer)
        {
            CheckPos();
            for (int i = 0; i < nearObj.Length; i++)
            {
                if (nearObj[i] == null)
                {
                    CheckNearObj(checkPos[i], i, obj => CheckOutObjScript(obj));
                }
            }
            fluidManager.ConsumeSourceGroupAdd(this);
        }
        else
        {
            DelayNearStrBuilt();
        }
    }

    public override void DelayNearStrBuilt()
    {
        // 동시 건설, 클라이언트 동기화 등의 이유로 딜레이를 주고 NearStrBuilt()를 실행할 때 사용
        StartCoroutine(DelayNearStrBuiltCoroutine());
    }

    protected override IEnumerator DelayNearStrBuiltCoroutine()
    {
        // 동시 건설이나 그룹핑을 따로 예외처리 하는 경우가 아니면 NearStrBuilt()를 그대로 사용
        yield return new WaitForEndOfFrame();

        CheckPos();
        for (int i = 0; i < nearObj.Length; i++)
        {
            if (nearObj[i] == null)
            {
                CheckNearObj(checkPos[i], i, obj => CheckOutObjScript(obj));
            }
        }
        fluidManager.ConsumeSourceGroupAdd(this);
    }

    void CheckOutObjScript(GameObject game)
    {
        StartCoroutine(SetOutObjCoroutine(game));
        //if (game.TryGetComponent(out FluidFactoryCtrl factoryCtrl))
        //    StartCoroutine("MainSourceCheck", factoryCtrl);
    }

    void FluidChangeCheck()
    {
        if (preSaveFluidNum != (int)saveFluidNum)
        {
            preSaveFluidNum = (int)saveFluidNum;
            if (isUIOpened)
                displaySlot.SetItemAmount((int)saveFluidNum);
        }
    }

    public override void OpenUI()
    {
        base.OpenUI();
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
        
        if (slot.Item1 != null)
        {
            playerInven.Add(slot.Item1, slot.Item2);
        }
    }

    public override Dictionary<Item, int> PopUpItemCheck()
    {
        if (saveFluidNum > 0 && fluidName != "")
        {
            Dictionary<Item, int> returnDic = new Dictionary<Item, int>();
            returnDic.Add(ItemList.instance.itemDic[fluidName], (int)saveFluidNum);

            if (slot.Item1 != null && slot.Item2 > 0)
                returnDic.Add(slot.Item1, slot.Item2);

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

    public override void ConsumeGroupSendFluid()
    {
        foreach (GameObject obj in outObj)
        {
            if (obj.TryGetComponent(out FluidFactoryCtrl fluidFactory) && !fluidFactory.isMainSource && !fluidFactory.isConsumeSource)
            {
                fluidFactory.ShouldUpdate(this, howFarSource + 1, false);
            }
        }
    }
}
