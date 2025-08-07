using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SteamGenerator : FluidFactoryCtrl
{
    GameManager gameManager;
    public Slot displaySlot;
    int preSaveFluidNum;

    public EnergyGroupConnector connector;
    public Item FuelItem;
    [SerializeField]
    SpriteRenderer view;
    bool isBuildDone;
    PreBuilding preBuilding;
    Structure preBuildingStr;
    bool preBuildingCheck;
    public float waterRequirement;
    public int fuelRequirement;

    protected override void Awake()
    {
        #region ProductionAwake
        inventory = this.GetComponent<Inventory>();
        if (inventory != null)
        {
            inventory.onItemChangedCallback += CheckSlotState;
            inventory.onItemChangedCallback += CheckInvenIsFull;
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
        gameManager = GameManager.instance;
        myFluidScript = GetComponent<FluidFactoryCtrl>();
        playerInven = gameManager.inventory;
        mainSource = null;
        howFarSource = -1;
        preSaveFluidNum = 0;
        myVision.SetActive(false);

        displaySlot = GameObject.Find("Canvas").transform.Find("StructureInfo").transform.Find("Storage")
            .transform.Find("SteamGenerator").transform.Find("DisplaySlot").GetComponent<Slot>();
        fluidManager = FluidManager.instance;
        #endregion
    }

    protected override void Start()
    {
        #region ProductionStart
        itemDic = ItemList.instance.itemDic;
        if (recipe == null)
            recipe = new Recipe();
        fluidName = "Water";
        consumeSource = this;
        isConsumeSource = true;
        canvas = gameManager.GetComponent<GameManager>().inventoryUiCanvas;
        sInvenManager = canvas.GetComponent<StructureInvenManager>();
        rManager = canvas.GetComponent<RecipeManager>();
        GetUIFunc();
        StrBuilt();
        #endregion

        maxFuel = 100;
        isBuildDone = false;
        preBuildingCheck = false;
        preBuilding = PreBuilding.instance;
        prodTimer = cooldown;

        displaySlot.SetInputItem(ItemList.instance.itemDic["Water"]);
        displaySlot.AddItem(ItemList.instance.itemDic["Water"], 0);
        view.enabled = false;
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
        //            int dirIndex = i / 2;
        //            CheckNearObj(startTransform[indices[i]], directions[dirIndex], i, obj => FluidSetOutObj(obj));
        //            CheckNearObj(startTransform[indices[i]], directions[dirIndex], i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
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

        if (destroyStart)
        {
            if (GameManager.instance.debug)
                destroyTimer -= (Time.deltaTime * 10);
            else
                destroyTimer -= Time.deltaTime;
            repairBar.fillAmount = destroyTimer / destroyInterval;

            if (destroyTimer <= 0)
            {
                ObjRemoveFunc();
                destroyStart = false;
            }
        }
        #endregion

        base.Update();

        if (gameManager.focusedStructure == null)
        {
            if (preBuilding.isBuildingOn && !removeState)
            {
                if (!preBuildingCheck)
                {
                    preBuildingCheck = true;
                    if (preBuilding.isEnergyUse || preBuilding.isEnergyStr)
                    {
                        view.enabled = true;
                    }
                }
            }
            else
            {
                if (preBuildingCheck)
                {
                    preBuildingCheck = false;
                    view.enabled = false;
                }
            }
        }

        if (!isPreBuilding)
        {
            FluidChangeCheck();

            if (!isBuildDone)
            {
                connector.Init();
                isBuildDone = true;
            }

            if (fuel <= 50 && slot.Item1 == FuelItem && slot.Item2 > 0)
            {
                if (IsServer)
                {
                    Overall.instance.OverallConsumption(slot.Item1, 1);
                    inventory.SlotSubServerRpc(0, 1);
                }
                fuel += 50;
            }

            prodTimer += Time.deltaTime;
            if (prodTimer > cooldown)
            {
                if (saveFluidNum >= waterRequirement && fuel >= fuelRequirement)
                {
                    saveFluidNum -= waterRequirement;
                    fuel -= fuelRequirement;
                    OperateStateSet(true);
                    prodTimer = 0;
                }
                else
                {
                    OperateStateSet(false);
                }
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
                    //CheckNearObj(i, obj => FluidSetOutObj(obj));
                    CheckNearObj(i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
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
                //CheckNearObj(i, obj => FluidSetOutObj(obj));
                CheckNearObj(i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
            }
        }
        fluidManager.ConsumeSourceGroupAdd(this);
    }

    protected override IEnumerator CheckWarning()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(1f);

            if (!isPreBuilding && !removeState)
            {
                if (fuel > 0)
                {
                    if (warningIconCheck)
                    {
                        if (warning != null)
                            StopCoroutine(warning);
                        warningIconCheck = false;
                        warningIcon.enabled = false;
                    }
                }
                else
                {
                    if (!warningIconCheck)
                    {
                        if (warning != null)
                            StopCoroutine(warning);
                        warning = FlickeringIcon();
                        StartCoroutine(warning);
                        warningIconCheck = true;
                    }
                }
            }
        }
    }

    public override float GetProgress() { return fuel; }

    public override void Focused()
    {
        if (connector.group != null)
        {
            connector.group.TerritoryViewOn();
        }
    }

    public override void DisableFocused()
    {
        if (connector.group != null)
        {
            connector.group.TerritoryViewOff();
        }
    }

    //void CheckOutObjScript(GameObject game)
    //{
    //    StartCoroutine(SetOutObjCoroutine(game));
    //    if (game.TryGetComponent(out FluidFactoryCtrl factoryCtrl))
    //        StartCoroutine(nameof(MainSourceCheck), factoryCtrl);
    //}

    void FluidChangeCheck()
    {
        if (preSaveFluidNum != (int)saveFluidNum)
        {
            preSaveFluidNum = (int)saveFluidNum;
            if (isUIOpened)
                displaySlot.SetItemAmount((int)saveFluidNum);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public override void RemoveObjServerRpc()
    {
        DisableFocused();
        connector.RemoveFromGroup();
        base.RemoveObjServerRpc();
    }

    public override bool CanTakeItem(Item item)
    {
        if (isInvenFull) return false;

        if (FuelItem == item && slot.Item2 < 99)
            return true;

        return false;
    }

    public override void OnFactoryItem(ItemProps itemProps)
    {
        if (IsServer && FuelItem == itemProps.item)
            inventory.SlotAdd(0, itemProps.item, itemProps.amount);

        itemProps.itemPool.Release(itemProps.gameObject);
    }

    public override void OnFactoryItem(Item item)
    {
        if (IsServer && FuelItem == item)
            inventory.SlotAdd(0, item, 1);
    }

    public override void OpenUI()
    {
        base.OpenUI();
        displaySlot.SetItemAmount((int)saveFluidNum);

        sInvenManager.SetInven(inventory, ui);
        sInvenManager.SetProd(this);
        sInvenManager.progressBar.SetMaxProgress(100);
        sInvenManager.slots[0].SetInputItem(FuelItem);
        //sInvenManager.InvenInit();
    }

    public override void CloseUI()
    {
        base.CloseUI();
        sInvenManager.ReleaseInven();
    }

    public override void GetUIFunc()
    {
        InventoryList inventoryList = canvas.GetComponent<InventoryList>();

        foreach (GameObject list in inventoryList.StructureStorageArr)
        {
            if (list.name == "SteamGenerator")
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

    public override (bool, bool, bool, EnergyGroup, float) PopUpEnergyCheck()
    {
        if (connector != null && connector.group != null)
        {
            return (energyUse, isEnergyStr, false, connector.group, energyProduction);
        }

        return (false, false, false, null, 0);
    }

    protected override void NonOperateStateSet(bool isOn)
    {
        setModel.sprite = strImg[isOn ? 1 : 0];
        smokeCtrl.SetSmokeActive(isOn);
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
