using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class FluidFactoryCtrl : Production
{
    [HideInInspector]
    public string fluidName;
    public float saveFluidNum;
    [HideInInspector]
    public float sendDelayTimer = 0.0f;

    public int howFarSource;
    public FluidFactoryCtrl mainSource;
    protected FluidFactoryCtrl myFluidScript;
    public bool getFluid = false;

    public bool isPreventingDuplicate = false;

    public List<FluidFactoryCtrl> fluidList = new List<FluidFactoryCtrl>();

    protected override void Awake()
    {
        GameManager gameManager = GameManager.instance;
        myFluidScript = GetComponent<FluidFactoryCtrl>();
        playerInven = gameManager.GetComponent<Inventory>();
        buildName = structureData.FactoryName;
        col = GetComponent<BoxCollider2D>();
        hp = structureData.MaxHp[level];
        hpBar.fillAmount = hp / structureData.MaxHp[level];
        repairBar.fillAmount = 0;
        mainSource = null;
        howFarSource = -1;
    }

    protected override void Update()
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
    }

    protected virtual void FluidSetOutObj(GameObject obj)
    {
        if (obj.TryGetComponent(out FluidFactoryCtrl factoryCtrl))
        {
            outObj.Add(obj);
            if (obj.GetComponent<UnderPipeCtrl>() != null)
            {
                StartCoroutine("UnderPipeConnectCheck", obj);
            }
            StartCoroutine("MainSourceCheck", factoryCtrl);
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
        foreach (GameObject obj in outObj)
        {
            if (obj.TryGetComponent(out FluidFactoryCtrl fluidFactory) && !fluidFactory.GetComponent<PumpCtrl>()
                && !fluidFactory.GetComponent<ExtractorCtrl>()
                && ((howFarSource != -1 && fluidFactory.howFarSource != -1 && howFarSource <= fluidFactory.howFarSource && getFluid)
                || fluidFactory.mainSource == null
                || (mainSource == null && fluidFactory.mainSource == null && howFarSource <= fluidFactory.howFarSource)))
            {
                if (saveFluidNum > fluidFactory.saveFluidNum && fluidFactory.structureData.MaxFulidStorageLimit == structureData.MaxFulidStorageLimit)
                {
                    SendFluidFunc(fluidFactory);
                }
                else if (fluidFactory.structureData.MaxFulidStorageLimit != structureData.MaxFulidStorageLimit)
                {
                    if (fluidFactory.structureData.MaxFulidStorageLimit > fluidFactory.saveFluidNum)
                    {
                        SendFluidFunc(fluidFactory);
                    }
                }
                if (fluidFactory.mainSource == null)
                {
                    RemoveMainSorce();
                }
            }
        }
    }

    void SendFluidFunc(FluidFactoryCtrl othFluid)
    {
        saveFluidNum -= structureData.SendFluidAmount;
        othFluid.SendFluidFunc(structureData.SendFluidAmount);
        getFluid = false;
    }

    public void SendFluidFunc(float getNum)
    {
        saveFluidNum += getNum;

        if (structureData.MaxFulidStorageLimit <= saveFluidNum)
        {
            saveFluidNum = structureData.MaxFulidStorageLimit;
        }
        getFluid = true;
    }

    public IEnumerator MainSourceFunc()
    {
        isPreventingDuplicate = true;

        yield return new WaitForSeconds(0.5f);
        fluidList.Clear();
        CheckFarSource(0, myFluidScript);

        isPreventingDuplicate = false;
    }


    public void CheckFarSource(int dis, FluidFactoryCtrl _mainSource)
    {
        if (_mainSource != myFluidScript && (mainSource == null || howFarSource > dis))
        {
            howFarSource = dis;
            if (mainSource != null && mainSource != _mainSource)
                mainSource.fluidList.Remove(myFluidScript);

            mainSource = _mainSource;
            _mainSource.fluidList.Add(myFluidScript);
        }

        foreach (GameObject obj in outObj)
        {
            obj.TryGetComponent(out FluidFactoryCtrl factoryCtrl);
            if(factoryCtrl.mainSource == null || factoryCtrl.howFarSource > dis)
            {
                factoryCtrl.CheckFarSource(dis + 1, _mainSource);
            }
        }
    }

    public void RemoveMainSorce()
    {
        if(mainSource != null)
            mainSource.FluidListReset();
        else if(GetComponent<PumpCtrl>() || GetComponent<ExtractorCtrl>())
        {
            FluidListReset();
        }
    }

    public void FluidListReset()
    {
        foreach (FluidFactoryCtrl fluid in fluidList)
        {
            fluid.mainSource = null;
            fluid.howFarSource = -1;
        }

        fluidList.Clear();

        if (!isPreventingDuplicate)
            StartCoroutine(MainSourceFunc());        
    }
}
