using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// UTF-8 설정
public class FluidTankCtrl : FluidFactoryCtrl
{
    protected override void Update()
    {
        base.Update();

        if (!removeState)
        {
            if (!isPreBuilding)
            {
                for (int i = 0; i < nearObj.Length; i++)
                {
                    if (nearObj[i] == null)
                    {
                        int dirIndex = i / 2;
                        CheckNearObj(startTransform[indices[i]], directions[dirIndex], i, obj => FluidSetOutObj(obj));
                    }
                }

                if (outObj.Count > 0)
                {
                    sendDelayTimer += Time.deltaTime;

                    if (sendDelayTimer > structureData.SendDelay)
                    {
                        if(saveFluidNum >= structureData.SendFluidAmount)
                            SendFluid();
                        sendDelayTimer = 0;
                    }
                }
            }
        }
    }

    protected override void SendFluid()
    {
        foreach (GameObject obj in outObj)
        {
            if (obj.TryGetComponent(out FluidFactoryCtrl fluidFactory) && fluidFactory.GetComponent<PumpCtrl>() == null)
            {
                if (fluidFactory.structureData.MaxFulidStorageLimit > fluidFactory.saveFluidNum)
                {
                    float currentFillRatio = (float)fluidFactory.structureData.MaxFulidStorageLimit / fluidFactory.saveFluidNum;
                    float targetFillRatio = (float)structureData.MaxFulidStorageLimit / saveFluidNum;

                    if (currentFillRatio > targetFillRatio)
                    {
                        saveFluidNum -= structureData.SendFluidAmount;
                        fluidFactory.SendFluidFunc(structureData.SendFluidAmount);
                    }
                }
            }
        }
    }
}
