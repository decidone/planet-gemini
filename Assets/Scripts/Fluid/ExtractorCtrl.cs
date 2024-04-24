using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class ExtractorCtrl : FluidFactoryCtrl
{
    float pumpFluid = 15.0f;

    protected override void Start()
    {
        mainSource = myFluidScript;
        fluidName = "CrudeOil";
        CheckPos();
    }

    protected override void Update()
    {
        base.Update();

        if (!removeState)
        {
            if (isSetBuildingOk)
            {
                for (int i = 0; i < nearObj.Length; i++)
                {
                    if (nearObj[i] == null)
                    {
                        CheckNearObj(checkPos[i], i, obj => FluidSetOutObj(obj));
                    }
                }
            }

            if (!isPreBuilding && checkObj)
            {
                sendDelayTimer += Time.deltaTime;

                if (sendDelayTimer > structureData.SendDelay)
                {
                    SendFluid();
                    sendDelayTimer = 0;
                }
            }
        }
    }

    protected override void SendFluid()
    {
        if (saveFluidNum < structureData.MaxFulidStorageLimit)
        {
            if (saveFluidNum + pumpFluid >= structureData.MaxFulidStorageLimit)
                saveFluidNum = structureData.MaxFulidStorageLimit;
            else if (saveFluidNum + pumpFluid < structureData.MaxFulidStorageLimit)
                saveFluidNum += pumpFluid;
        }

        if (outObj.Count > 0)
        {
            foreach (GameObject obj in outObj)
            {
                if (obj.TryGetComponent(out FluidFactoryCtrl fluidFactory) && obj.GetComponent<PumpCtrl>() == null && !fluidFactory.isPreBuilding)
                {
                    if (fluidFactory.structureData.MaxFulidStorageLimit > fluidFactory.saveFluidNum && fluidFactory.CanTake() && fluidFactory.fluidName == fluidName)
                    {
                        fluidFactory.SendFluidFunc(structureData.SendFluidAmount);
                        saveFluidNum -= structureData.SendFluidAmount;
                    }

                    if (fluidFactory.mainSource == null && !fluidFactory.reFindMain)
                        RemoveMainSource(false);
                }
            }
        }
    }
}
