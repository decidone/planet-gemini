using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

// UTF-8 설정
public class FluidFactoryCtrl : Production
{
    [HideInInspector]
    public string fluidName;
    //[HideInInspector]
    public float saveFluidNum;
    [HideInInspector]
    public float sendDelayTimer = 0.0f;
    public bool isConsumeSource;
    public int howFarSource;
    public FluidFactoryCtrl mainSource;
    public FluidFactoryCtrl consumeSource;
    protected FluidFactoryCtrl myFluidScript;

    //public List<FluidFactoryCtrl> fluidList = new List<FluidFactoryCtrl>(); // FluidManager로 옮겨야

    public FluidManager fluidManager;

    private FluidFactoryCtrl lastSource;
    private int lastDistance = -1;

    protected override void Awake()
    {
        gameManager = GameManager.instance;
        myFluidScript = GetComponent<FluidFactoryCtrl>();
        playerInven = gameManager.inventory;
        buildName = structureData.FactoryName;
        col = GetComponent<BoxCollider2D>();
        maxHp = structureData.MaxHp[level];
        defense = structureData.Defense[level];
        hp = maxHp;
        getDelay = 0.05f;
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
        setModel = GetComponent<SpriteRenderer>();
        fluidManager = FluidManager.instance;
        if (TryGetComponent(out Animator anim))
        {
            getAnim = true;
            animator = anim;
        }
        NonOperateStateSet(isOperate);
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
            else if (isPreBuilding)
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

    [ServerRpc(RequireOwnership = false)]
    public void FluidSyncServerRpc()
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
            if (!outObj.Contains(obj))
                outObj.Add(obj);
            if (obj.GetComponent<UnderPipeCtrl>() != null)
            {
                UnderPipeConnectCheck(obj);
            }
        }
    }

    protected virtual void UnderPipeConnectCheck(GameObject obj)
    {
        obj.TryGetComponent(out UnderPipeCtrl underPipeCtrl);
        if (underPipeCtrl.otherPipe == null || underPipeCtrl.otherPipe != this.gameObject)
        {
            outObj.Remove(obj);
        }

        if(TryGetComponent(out PipeCtrl pipe))
            pipe.ChangeModel();
    }

    public virtual void SendFluid()
    {
        if ((saveFluidNum <= 0.1f || outObj.Count <= 0) && !mainSource)
            return;

        Dictionary<FluidFactoryCtrl, (float, float, bool)> canSendDic = new Dictionary<FluidFactoryCtrl, (float, float, bool)>(); // (저장량, 최대 저장량, 역류가능성)
        foreach (GameObject obj in outObj)
        {
            if (obj.TryGetComponent(out FluidFactoryCtrl fluidCtrl) && !fluidCtrl.isMainSource)
            {
                fluidCtrl.ShouldUpdate(mainSource, howFarSource + 1, true);

                var (canSend, refluxCheck) = CheckCanSend(fluidCtrl);

                if (!canSend)
                    continue;

                if (!canSendDic.ContainsKey(fluidCtrl))
                    canSendDic.Add(fluidCtrl, (fluidCtrl.saveFluidNum, fluidCtrl.structureData.MaxFulidStorageLimit, refluxCheck));
            }
        }

        if (canSendDic.Count == 0)
            return;

        float totalFluidAmount = 0f;
        float canSendAmount = saveFluidNum / (canSendDic.Count + 1);

        foreach (var outFluid in canSendDic)
        {
            float outFluidAmount = outFluid.Value.Item2 - outFluid.Value.Item1;
            float sendAmoun = canSendAmount;
            if (outFluid.Value.Item3)
            {
                sendAmoun = canSendAmount / 1.5f;
            }

            if (outFluidAmount < sendAmoun)
            {
                outFluid.Key.saveFluidNum += outFluidAmount;
                totalFluidAmount += outFluidAmount;
            }
            else
            {
                outFluid.Key.saveFluidNum += sendAmoun;
                totalFluidAmount += sendAmoun;
            }
        }

        saveFluidNum -= totalFluidAmount;
    }

    public virtual void ConsumeGroupSendFluid()
    {
        if ((saveFluidNum <= 0.1f || outObj.Count <= 0) && !consumeSource)
            return;

        Dictionary<FluidFactoryCtrl, (float, float)> canSendDic = new Dictionary<FluidFactoryCtrl, (float, float)>(); // (저장량, 최대 저장량)
        foreach (GameObject obj in outObj)
        {
            if (obj.TryGetComponent(out FluidFactoryCtrl fluidCtrl) && !fluidCtrl.mainSource && !fluidCtrl.isMainSource)
            {
                if(!fluidCtrl.isConsumeSource)
                    fluidCtrl.ShouldUpdate(consumeSource, howFarSource + 1, false);

                var canSend = ConsumeGroupCheckCanSend(fluidCtrl);

                if (!canSend)
                    continue;

                if (!canSendDic.ContainsKey(fluidCtrl))
                    canSendDic.Add(fluidCtrl, (fluidCtrl.saveFluidNum, fluidCtrl.structureData.MaxFulidStorageLimit));
            }
        }

        if (canSendDic.Count == 0)
            return;

        float totalFluidAmount = 0f;
        float canSendAmount = saveFluidNum / (canSendDic.Count + 1);

        foreach (var outFluid in canSendDic)
        {
            float outFluidAmount = outFluid.Value.Item2 - outFluid.Value.Item1;
            float sendAmoun = canSendAmount;

            if (outFluidAmount < sendAmoun)
            {
                outFluid.Key.saveFluidNum += outFluidAmount;
                totalFluidAmount += outFluidAmount;
            }
            else
            {
                outFluid.Key.saveFluidNum += sendAmoun;
                totalFluidAmount += sendAmoun;
            }
        }

        saveFluidNum -= totalFluidAmount;
    } 

    protected (bool, bool) CheckCanSend(FluidFactoryCtrl othFluid)
    {
        bool canSend = false;
        bool refluxCheck = false;
        if (fluidName != othFluid.fluidName || !othFluid.CanTake())
            return (false, false);

        bool farSourceCheck = howFarSource <= othFluid.howFarSource; // 자신보다 거리 수치가 높은 경우
        bool refluxSendCheck = saveFluidNum / 2 > othFluid.saveFluidNum; // 역류 기능 활성화 조건: 자신의 저장량이 상대의 저장량의 2배 이상인 경우
        bool saveFluidCheck = saveFluidNum > othFluid.saveFluidNum; // 자신보다 저장량이 낮은 경우
        bool maxStorageLargeSizeCheck = structureData.MaxFulidStorageLimit < othFluid.structureData.MaxFulidStorageLimit; // 상대 구조물의 최대 저장량이 자신의 최대 저장량보다 큰지 확인
        bool othMainSoure = mainSource != othFluid.mainSource;

        if (farSourceCheck)
        {
            if (saveFluidCheck)
            {
                canSend = true;
            }
            else if (maxStorageLargeSizeCheck && structureData.MaxFulidStorageLimit == saveFluidNum)
            {
                canSend = true;
            }
        }
        else
        {
            if (othMainSoure)
            {
                if (saveFluidCheck)
                {
                    canSend = true;
                }
                else if (maxStorageLargeSizeCheck && structureData.MaxFulidStorageLimit == saveFluidNum)
                {
                    canSend = true;
                }
            }
            else if (saveFluidCheck && refluxSendCheck)
            {
                canSend = true;
                refluxCheck = true; // 역류 기능 활성화
            }
        }
        return (canSend, refluxCheck);
    }

    protected bool ConsumeGroupCheckCanSend(FluidFactoryCtrl othFluid)
    {
        bool canSend = false;
        if (fluidName != othFluid.fluidName || !othFluid.CanTake())
            return false;

        bool farSourceCheck = howFarSource >= othFluid.howFarSource; // 자신보다 거리 수치가 낮은 경우

        if (farSourceCheck)
        {
            canSend = true;
        }

        return (canSend);
    }

    public float CanTakeAmount()
    {
        if (saveFluidNum < structureData.MaxFulidStorageLimit)
        {
            return structureData.MaxFulidStorageLimit - saveFluidNum;
        }
        else
        {
            return 0f;
        }
    }

    public bool CanTake()
    {
        if (saveFluidNum < structureData.MaxFulidStorageLimit)
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

    public void ShouldUpdate(FluidFactoryCtrl newSource, int dis, bool isSend)
    {
        if (lastSource == newSource && lastDistance == dis)
            return;

        lastSource = newSource;
        lastDistance = dis;

        bool isMainSourceNull = mainSource == null;
        bool isFarther = (howFarSource == -1 || howFarSource > dis);
        bool isSameFluidName = fluidName == newSource.fluidName || fluidName == "";
        bool isFluidEmpty = saveFluidNum == 0;
        bool shouldUpdate;

        if (isSend)
        {
            shouldUpdate =
                (isMainSourceNull && (isSameFluidName || isFluidEmpty)) || // 메인 소스가 없고 같은 유체를 사용하거나 유체가 비어있는 경우
                (!isMainSourceNull && isSameFluidName && isFarther); // 메인 소스가 있고 같은 유체를 사용하고 거리가 먼경우
        }
        else
        {
            shouldUpdate = (isMainSourceNull && (isSameFluidName || isFluidEmpty) && isFarther);
            // 메인 소스가 없고 같은 유체를 사용하거나 유체가 비어있는 경우
        }

        if (shouldUpdate)
        {
            howFarSource = dis;

            if (GetComponent<FluidTankCtrl>())
                howFarSource++;

            if (isSend)
            {
                if (mainSource != null && mainSource != newSource)
                    fluidManager.MainSourceGroupListRemove(mainSource, myFluidScript);
                mainSource = newSource;

                fluidName = newSource.fluidName;
                fluidManager.MainSourceGroupAdd(newSource, this);
            }
            else
            {
                if (consumeSource != null && consumeSource != newSource)
                    fluidManager.ConsumeSourceGroupListRemove(consumeSource, myFluidScript);
                consumeSource = newSource;

                fluidName = newSource.fluidName;
                fluidManager.ConsumeSourceGroupAdd(newSource, this);
            }
        }
    }

    public void ResetSource()
    {
        mainSource = null;
        consumeSource = null;
        howFarSource = -1;
        lastDistance = -1;
        lastSource = null;
    }

    public void RemoveMainSource()
    {
        if(mainSource)
            fluidManager.MainSourceGroupRemove(mainSource, this);
        else if(consumeSource)
            fluidManager.ConsumeSourceGroupRemove(consumeSource, this);
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
