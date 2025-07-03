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
            //if (isSetBuildingOk)
            //{                
            //    for (int i = 0; i < nearObj.Length; i++)
            //    {
            //        if (nearObj[i] == null)
            //        {
            //            int dirIndex = i / 2;
            //            CheckNearObj(startTransform[indices[i]], directions[dirIndex], i, obj => FluidSetOutObj(obj));
            //        }
            //    }
            //}

            //if (!isPreBuilding && checkObj)
            //{
            //    if (outObj.Count > 0)
            //    {
            //        sendDelayTimer += Time.deltaTime;

            //        if (sendDelayTimer > sendDelay)
            //        {
            //            if(saveFluidNum >= structureData.SendFluidAmount)
            //                SendFluid();
            //            sendDelayTimer = 0;
            //        }
            //    }
            //}
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
                CheckNearObj(i, obj => FluidSetOutObj(obj));
            }
        }
    }
}
