using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SteamGenerator : FluidFactoryCtrl
{
    GameManager gameManager;
    public Slot displaySlot;
    int preSaveFluidNum;
    bool uiOpened;

    public EnergyGroupConnector connector;
    public Item FuelItem;
    [SerializeField]
    SpriteRenderer view;
    bool isBuildDone;
    bool isPlaced;
    PreBuilding preBuilding;
    Structure preBuildingStr;
    bool preBuildingCheck;
    public float waterRequirement;
    public int fuelRequirement;

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
        gameManager = GameManager.instance;
        myFluidScript = GetComponent<FluidFactoryCtrl>();
        playerInven = gameManager.inventory;
        mainSource = null;
        howFarSource = -1;
        preSaveFluidNum = 0;
        uiOpened = false;
        myVision.SetActive(false);

        displaySlot = GameObject.Find("Canvas").transform.Find("StructureInfo").transform.Find("Storage")
            .transform.Find("SteamGenerator").transform.Find("DisplaySlot").GetComponent<Slot>();
        #endregion
    }

    protected override void Start()
    {
        #region ProductionStart
        itemDic = ItemList.instance.itemDic;
        if (recipe == null)
            recipe = new Recipe();
        output = null;
        fluidName = "Water";

        canvas = gameManager.GetComponent<GameManager>().inventoryUiCanvas;
        sInvenManager = canvas.GetComponent<StructureInvenManager>();
        rManager = canvas.GetComponent<RecipeManager>();
        GetUIFunc();
        CheckPos();
        #endregion

        maxFuel = 100;
        isBuildDone = false;
        isPlaced = false;
        preBuildingCheck = false;
        preBuilding = PreBuilding.instance;
        prodTimer = cooldown;

        displaySlot.SetInputItem(ItemList.instance.itemDic["Water"]);
        displaySlot.AddItem(ItemList.instance.itemDic["Water"], 0);
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
                    int dirIndex = i / 2;
                    CheckNearObj(startTransform[indices[i]], directions[dirIndex], i, obj => FluidSetOutObj(obj));
                    CheckNearObj(startTransform[indices[i]], directions[dirIndex], i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
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

        if (destroyStart)
        {
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

        if (!isPlaced)
        {
            if (isSetBuildingOk)
            {
                view.enabled = false;
                isPlaced = true;
            }
        }

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

            var slot = inventory.SlotCheck(0);
            if (fuel <= 50 && slot.item == FuelItem && slot.amount > 0)
            {
                if (IsServer)
                {
                    inventory.SlotSubServerRpc(0, 1);
                    Overall.instance.OverallConsumption(slot.item, 1);
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

    public override void WarningStateCheck()
    {
        if (!isPreBuilding && warningIcon != null)
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

    [ServerRpc(RequireOwnership = false)]
    public override void RemoveObjServerRpc()
    {
        DisableFocused();
        connector.RemoveFromGroup();
        base.RemoveObjServerRpc();
    }

    public override bool CanTakeItem(Item item)
    {
        var slot = inventory.SlotCheck(0);
        if (FuelItem == item && slot.amount < 99)
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
        uiOpened = true;
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
        uiOpened = false;
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
    }
}
