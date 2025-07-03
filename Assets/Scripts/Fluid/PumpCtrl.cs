using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class PumpCtrl : FluidFactoryCtrl
{
    public float pumpFluid = 20.0f;
    float pumpTimer;
    public float pumpInterval = 3;

    protected override void Start()
    {
        mainSource = myFluidScript;
        isMainSource = true;
        fluidName = "Water";
        StrBuilt();
    }

    protected override void Update()
    {
        base.Update();

        if (!removeState)
        {
            if (!isPreBuilding && checkObj)
            {
                //if (outObj.Count > 0)
                //{
                //    sendDelayTimer += Time.deltaTime;
                //    if (sendDelayTimer > sendDelay)
                //    {
                //        SendFluid();
                //        sendDelayTimer = 0;
                //    }
                //}

                pumpTimer += Time.deltaTime;
                if (pumpTimer > pumpInterval)
                {
                    PumpUp();
                    pumpTimer = 0;
                }
            }
        }
    }

    public override void NearStrBuilt()
    { 
        // 건물을 지었을 때나 근처에 새로운 건물이 지어졌을 때 동작
        CheckPos();
        for (int i = 0; i < nearObj.Length; i++)
        {
            if (nearObj[i] == null)
            {
                CheckNearObj(checkPos[i], i, obj => FluidSetOutObj(obj));
            }
        }
        fluidManager.MainSourceGroupAdd(this);
    }

    void PumpUp()
    {
        if (saveFluidNum + pumpFluid >= structureData.MaxFulidStorageLimit)
            saveFluidNum = structureData.MaxFulidStorageLimit;
        else if (saveFluidNum + pumpFluid < structureData.MaxFulidStorageLimit)
            saveFluidNum += pumpFluid;
    }
    

    public override void SendFluid()
    {
        if (saveFluidNum > 0)
        {
            foreach (GameObject obj in outObj)
            {
                if (obj.TryGetComponent(out FluidFactoryCtrl fluidFactory) && !fluidFactory.isMainSource)
                {
                    fluidFactory.ShouldUpdate(this, howFarSource + 1, true);

                    if (fluidFactory.CanTake() && fluidFactory.fluidName == fluidName)
                    {
                        float amount =  fluidFactory.CanTakeAmount();
                        if (amount > saveFluidNum / outObj.Count)
                        {
                            amount = saveFluidNum / outObj.Count;
                        }
                        fluidFactory.SendFluidFunc(amount);
                        saveFluidNum -= amount;
                    }
                }
            }
        }
    }
}   
