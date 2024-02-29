using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [HideInInspector]
    public GameObject preBuildingObj;
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
        hp = structureData.MaxHp[level];
        hpBar.fillAmount = hp / structureData.MaxHp[level];
        repairBar.fillAmount = 0;
        #endregion
        #region FluidFactoryAwake
        gameManager = GameManager.instance;
        myFluidScript = GetComponent<FluidFactoryCtrl>();
        playerInven = gameManager.GetComponent<Inventory>();
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
        preBuildingObj = gameManager.preBuildingObj;
        prodTimer = cooldown;

        displaySlot.SetInputItem(ItemList.instance.itemDic["Water"]);
        displaySlot.AddItem(ItemList.instance.itemDic["Water"], 0);
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
            if (preBuildingObj.activeSelf)
            {
                if (!preBuildingCheck)
                {
                    preBuildingCheck = true;
                    preBuildingStr = preBuildingObj.GetComponentInChildren<Structure>();
                    if (preBuildingStr != null && (preBuildingStr.energyUse || preBuildingStr.isEnergyStr))
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
                inventory.SubServerRpc(0, 1);
                fuel += 50;
            }

            prodTimer += Time.deltaTime;
            if (prodTimer > cooldown)
            {
                if (saveFluidNum >= waterRequirement && fuel >= fuelRequirement)
                {
                    saveFluidNum -= waterRequirement;
                    fuel -= fuelRequirement;
                    isOperate = true;
                    prodTimer = 0;
                }
                else
                {
                    isOperate = false;
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

    public override void RemoveObj()
    {
        connector.RemoveFromGroup();
        base.RemoveObj();
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
        if (FuelItem == itemProps.item)
            inventory.SlotAdd(0, itemProps.item, itemProps.amount);

        itemProps.itemPool.Release(itemProps.gameObject);
    }

    public override void OnFactoryItem(Item item)
    {
        if (FuelItem == item)
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
}
