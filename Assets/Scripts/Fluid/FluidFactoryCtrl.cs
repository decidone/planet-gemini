using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

// UTF-8 설정
public class FluidFactoryCtrl : Production
{
    [HideInInspector]
    public string fluidName;
    //[HideInInspector]
    public float saveFluidNum;
    [HideInInspector]
    public float sendDelayTimer = 0.0f;

    public int howFarSource;
    public FluidFactoryCtrl mainSource;
    protected FluidFactoryCtrl myFluidScript;

    public bool isPreventingDuplicate = false;

    public bool reFindMain = false;

    public List<FluidFactoryCtrl> fluidList = new List<FluidFactoryCtrl>();

    bool findNewObj = false;
    public bool alredyCheck = false;

    protected override void Awake()
    {
        GameManager gameManager = GameManager.instance;
        myFluidScript = GetComponent<FluidFactoryCtrl>();
        playerInven = gameManager.inventory;
        buildName = structureData.FactoryName;
        col = GetComponent<BoxCollider2D>();
        maxHp = structureData.MaxHp[level];
        hp = maxHp;
        getDelay = 0.01f;
        sendDelay = structureData.SendDelay[level]; 
        hpBar.fillAmount = hp / maxHp;
        repairBar.fillAmount = 0;
        mainSource = null;
        howFarSource = -1;
        myVision.SetActive(false);

        connectors = new List<EnergyGroupConnector>();
        conn = null;
        efficiency = 0;
        effiCooldown = 0;
        energyUse = structureData.EnergyUse[level];
        isEnergyStr = structureData.IsEnergyStr;
        energyProduction = structureData.Production;
        energyConsumption = structureData.Consumption[level];
        destroyInterval = structureData.RemoveGauge;
        soundManager = SoundManager.instance;
        repairEffect = GetComponentInChildren<RepairEffectFunc>();
        destroyTimer = destroyInterval;
        onEffectUpgradeCheck += IncreasedStructureCheck;
        onEffectUpgradeCheck.Invoke();
    }

    protected override void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

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
    }

    protected override void OnClientConnectedCallback(ulong clientId)
    {
        base.OnClientConnectedCallback(clientId);
        FluidSyncServerRpc();
    }

    IEnumerator lateSync()
    {
        yield return new WaitForSeconds(0.2f);
        FluidSyncServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void FluidSyncServerRpc()
    {
        FluidSyncClientRpc(saveFluidNum, fluidName);
    }

    [ClientRpc]
    void FluidSyncClientRpc(float fluidNum, string fluidNameSync)
    {
        saveFluidNum = fluidNum;
        fluidName = fluidNameSync;
    }


    protected virtual void FluidSetOutObj(GameObject obj)
    {
        if (obj.TryGetComponent(out FluidFactoryCtrl factoryCtrl))
        {
            outObj.Add(obj);
            if (obj.GetComponent<UnderPipeCtrl>() != null)
            {
                StartCoroutine(nameof(UnderPipeConnectCheck), obj);
            }
            if (!findNewObj)
            {
                findNewObj = true;
                StartCoroutine(nameof(MainSourceCheck), factoryCtrl);
            }
        }
    }

    IEnumerator MainSourceCheck(FluidFactoryCtrl factoryCtrl)
    {
        yield return new WaitForSeconds(0.5f);

        if(GetComponent<PumpCtrl>() || GetComponent<ExtractorCtrl>())
        {
            if(!isPreventingDuplicate)
                StartCoroutine(MainSourceFunc());
        }
        else
        {
            if(factoryCtrl.mainSource != null && !factoryCtrl.mainSource.isPreventingDuplicate)
            {
                factoryCtrl.mainSource.StartCoroutine(factoryCtrl.mainSource.MainSourceFunc());
            }
        }
        findNewObj = false;
    }

    protected virtual IEnumerator UnderPipeConnectCheck(GameObject obj)
    {
        yield return null;

        if (obj.GetComponent<UnderPipeCtrl>())
        {
            if (obj.GetComponent<UnderPipeCtrl>().otherPipe == null || obj.GetComponent<UnderPipeCtrl>().otherPipe != this.gameObject)
            {
                outObj.Remove(obj);
            }
        }
    }

    protected virtual void SendFluid()
    {
        FluidFactoryCtrl lowestObj = null;
        float lowestSaveFluidNum = float.MaxValue; // 초기값 설정

        foreach (GameObject obj in outObj)
        {
            if (obj.TryGetComponent(out FluidFactoryCtrl fluidCtrl) && !fluidCtrl.GetComponent<PumpCtrl>() && !fluidCtrl.GetComponent<ExtractorCtrl>())
            {
                float saveFluidNum = fluidCtrl.saveFluidNum;
                if (saveFluidNum < lowestSaveFluidNum && fluidCtrl.CanTake())
                {
                    lowestSaveFluidNum = saveFluidNum;
                    lowestObj = fluidCtrl;
                }

                if (fluidCtrl.mainSource == null && !fluidCtrl.reFindMain && !fluidCtrl.isPreBuilding)
                {
                    if (fluidName != "" && (fluidCtrl.fluidName == fluidName || fluidCtrl.fluidName == ""))
                    {
                        if (mainSource != null)
                            RemoveMainSource(false);
                        if (fluidCtrl.CanTake() && CheckFluidAmount(fluidCtrl))
                        {
                            SendFluidFunc(fluidCtrl);
                            fluidCtrl.SetFluidName(fluidName);
                        }
                    }
                }
            }
        }

        if (lowestObj)
        {
            bool canSendFluid = false;

            if (mainSource == lowestObj.mainSource)
            {
                if (howFarSource <= lowestObj.howFarSource ||
                    (howFarSource > lowestObj.howFarSource &&
                    structureData.MaxFulidStorageLimit == lowestObj.structureData.MaxFulidStorageLimit &&
                    saveFluidNum - 2 > lowestObj.saveFluidNum))
                {
                    canSendFluid = true;
                }
            }
            else if (lowestObj.fluidName == fluidName)
            {
                canSendFluid = true;
            }

            if (canSendFluid && CheckFluidAmount(lowestObj))
            {
                SendFluidFunc(lowestObj);
            }
        }        
    }

    protected bool CheckFluidAmount(FluidFactoryCtrl othFluid)
    {
        bool canSend = false;

        if (saveFluidNum >= othFluid.saveFluidNum && othFluid.structureData.MaxFulidStorageLimit == structureData.MaxFulidStorageLimit)
        {
            canSend = true;
        }
        else if (othFluid.structureData.MaxFulidStorageLimit != structureData.MaxFulidStorageLimit)
        {
            if (othFluid.structureData.MaxFulidStorageLimit > structureData.MaxFulidStorageLimit)
            {
                if (saveFluidNum >= othFluid.saveFluidNum)
                {
                    canSend = true;

                }
                else if (mainSource != null && structureData.MaxFulidStorageLimit == saveFluidNum && othFluid.structureData.MaxFulidStorageLimit > othFluid.saveFluidNum)
                {
                    canSend = true;
                }
            }
            else
            {
                if (saveFluidNum >= othFluid.saveFluidNum)
                {
                    canSend = true;
                }
            }
        }        

        return canSend;
    }

    protected void SendFluidFunc(FluidFactoryCtrl othFluid)
    {
        saveFluidNum -= structureData.SendFluidAmount;

        if (othFluid.GetComponent<Refinery>())
        {
            if (IsServer)
            {
                othFluid.SendFluidFuncServerRpc(structureData.SendFluidAmount);
            }
        }
        else
        {
            othFluid.SendFluidFunc(structureData.SendFluidAmount);
        }
    }

    public bool CanTake()
    {
        if (saveFluidNum + structureData.SendFluidAmount <= structureData.MaxFulidStorageLimit)
            return true;

        return false;
    }

    public void SendFluidFunc(float getNum)
    {
        saveFluidNum += getNum;

        if (structureData.MaxFulidStorageLimit <= saveFluidNum)
        {
            saveFluidNum = structureData.MaxFulidStorageLimit;
        }
    }

    [ServerRpc]
    public void SendFluidFuncServerRpc(float getNum)
    {
        SendFluidFuncClientRpc(getNum);
    }

    [ClientRpc]
    void SendFluidFuncClientRpc(float getNum)
    {
        saveFluidNum += getNum;

        if (structureData.MaxFulidStorageLimit <= saveFluidNum)
        {
            saveFluidNum = structureData.MaxFulidStorageLimit;
        }
    }

    public IEnumerator MainSourceFunc()
    {
        isPreventingDuplicate = true;
        yield return new WaitForSeconds(0.5f);

        foreach (FluidFactoryCtrl fluid in fluidList)
        {
            fluid.alredyCheck = false;
        }

        fluidList.Clear();

        CheckFarSource(0, myFluidScript);

        isPreventingDuplicate = false;
    }


    public void CheckFarSource(int dis, FluidFactoryCtrl _mainSource)
    {
        if (ShouldUpdate(_mainSource, dis))
        {
            UpdateFluidProperties(_mainSource, dis);
        }

        alredyCheck = true;

        if (!GetComponent<Refinery>() && !GetComponent<SteamGenerator>())
        {
            List<GameObject> uniqueObjects = outObj.Distinct().ToList();

            foreach (GameObject obj in uniqueObjects)
            {
                if (obj.TryGetComponent(out FluidFactoryCtrl factoryCtrl)
                    && (factoryCtrl.mainSource == null || (factoryCtrl.mainSource == _mainSource && !factoryCtrl.alredyCheck)
                    || (factoryCtrl.mainSource != _mainSource && factoryCtrl.howFarSource > dis))
                    && (factoryCtrl.fluidName == fluidName || (factoryCtrl.fluidName == "" && factoryCtrl.saveFluidNum == 0)))
                {
                    factoryCtrl.CheckFarSource(dis + 1, _mainSource);
                }
            }
        }
    }

    private bool ShouldUpdate(FluidFactoryCtrl newSource, int dis)
    {
        bool isNewSource = mainSource != newSource;

        return (howFarSource > dis || mainSource == null) || (mainSource == null || fluidName == ""
            || (mainSource != null && (fluidName == newSource.fluidName || (fluidName != newSource.fluidName && saveFluidNum == 0))))
            || isNewSource;
    }

    private void UpdateFluidProperties(FluidFactoryCtrl newSource, int dis)
    {
        howFarSource = dis;

        if (mainSource != null && mainSource != newSource)
            mainSource.fluidList.Remove(myFluidScript);

        mainSource = newSource;
        fluidName = newSource.fluidName;
        reFindMain = false;

        SetFluidName(newSource.fluidName);

        if (!newSource.fluidList.Contains(myFluidScript))
            newSource.fluidList.Add(myFluidScript);
    }

    public void SetFluidName(string _fluidName)
    {
        fluidName = _fluidName;

        //if (GetComponent<PipeCtrl>() || GetComponent<UnderPipeCtrl>() || GetComponent<FluidTankCtrl>())
        //{
        //    SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        //    sprite.color = _fluidName == "CrudeOil" ? Color.black : Color.blue;
        //}
    }

    public void RemoveMainSource(bool isRemoveMain)
    {
        if(mainSource != null)
            mainSource.FluidListReset(isRemoveMain);
        else if(GetComponent<PumpCtrl>() || GetComponent<ExtractorCtrl>())
        {
            FluidListReset(isRemoveMain);
        }
    }

    public void FluidListReset(bool isRemoveMain)
    {
        foreach (FluidFactoryCtrl fluid in fluidList)
        {
            fluid.mainSource = null;
            fluid.howFarSource = -1;
            if(!isRemoveMain)
                fluid.reFindMain = true;
        }

        fluidList.Clear();
        if (!isPreventingDuplicate)
            StartCoroutine(MainSourceFunc());
    }

    public override Dictionary<Item, int> PopUpItemCheck()
    {
        if(saveFluidNum > 0 && fluidName != "")  
        {
            Dictionary<Item, int> returnDic = new Dictionary<Item, int>();
            returnDic.Add(ItemList.instance.itemDic[fluidName], (int)saveFluidNum);

            return returnDic;
        }
        else
            return null;
    }

    public override (Item, int) QuickPullOut()
    {
        return (null, 0);
    }

    protected override void ItemDrop()
    {
        if (itemList.Count > 0)
        {
            foreach (Item item in itemList)
            {
                ItemToItemProps(item, 1);
            }
        }

        if (itemObjList.Count > 0)
        {
            foreach (ItemProps itemProps in itemObjList)
            {
                itemProps.ResetItemProps();
            }
        }
    }

    public override void AddInvenItem() { }

    public override StructureSaveData SaveData()
    {
        StructureSaveData data = base.SaveData();

        if (fluidName == "")
            data.fluidType = -1;
        else if(fluidName == "Water")
            data.fluidType = 0;
        else if (fluidName == "CrudeOil")
            data.fluidType = 1;

        data.storedFluid = saveFluidNum;

        return data;
    }
}
