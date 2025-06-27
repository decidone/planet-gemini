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
        fluidName = "Water";
        StrBuilt();
    }

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
            //            CheckNearObj(checkPos[i], i, obj => FluidSetOutObj(obj));
            //        }
            //    }
            //}

            if (!isPreBuilding && checkObj)
            {
                sendDelayTimer += Time.deltaTime;
                if (sendDelayTimer > sendDelay)
                {
                    SendFluid();
                    sendDelayTimer = 0;
                }

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
        // 변경사항이 생기면 DelayNearStrBuiltCoroutine()에도 반영해야 함
        if (IsServer)
        {
            CheckPos();
            for (int i = 0; i < nearObj.Length; i++)
            {
                if (nearObj[i] == null)
                {
                    CheckNearObj(checkPos[i], i, obj => FluidSetOutObj(obj));
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
                CheckNearObj(checkPos[i], i, obj => FluidSetOutObj(obj));
            }
        }
    }

    void PumpUp()
    {
        if (saveFluidNum + pumpFluid >= structureData.MaxFulidStorageLimit)
            saveFluidNum = structureData.MaxFulidStorageLimit;
        else if (saveFluidNum + pumpFluid < structureData.MaxFulidStorageLimit)
            saveFluidNum += pumpFluid;
    }

    protected override void SendFluid()
    {
        if (outObj.Count > 0 && saveFluidNum > 0)
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
