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
        // 변경사항이 생기면 DelayNearStrBuiltCoroutine()에도 반영해야 함
        if (IsServer)
        {
            CheckPos();
            for (int i = 0; i < nearObj.Length; i++)
            {
                if (nearObj[i] == null)
                {
                    CheckNearObj(i, obj => FluidSetOutObj(obj));
                }
            }
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
                CheckNearObj(i, obj => FluidSetOutObj(obj));
            }
        }
    }
}
