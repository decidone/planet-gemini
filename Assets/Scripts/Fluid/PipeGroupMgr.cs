using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeGroupMgr : MonoBehaviour
{
    [SerializeField]
    GameObject pipeObj;

    public List<PipeCtrl> pipeList = new List<PipeCtrl>();
    public List<GameObject> outObj = new List<GameObject>();

    public bool isPreBuilding = false;
    public float groupFullFluidNum = 0.0f;
    public float groupSaveFluidNum = 0.0f;
    float sendFluid = 1.0f;
    float sendDelayTimer = 0.0f;
    float sendDelay = 0.03f;

    void Update()
    {
        if (!isPreBuilding)
        {
            if (outObj.Count > 0)
            {
                sendDelayTimer += Time.deltaTime;

                if (sendDelayTimer > sendDelay)
                {
                    if(groupSaveFluidNum >= sendFluid)
                        SendFluid();
                    sendDelayTimer = 0;
                }
            }
        }
    }

    public void SetPipe(int pipeDir)
    {
        GameObject pipe = Instantiate(pipeObj, this.transform.position, Quaternion.identity);
        pipe.transform.parent = this.transform;
        PipeCtrl pipeCtrl = pipe.GetComponent<PipeCtrl>();
        pipeCtrl.pipeGroupMgr = this.GetComponent<PipeGroupMgr>();
        pipeList.Add(pipeCtrl);
        pipeCtrl.dirNum = pipeDir;
        GroupCheck();
        GroupFluidCount(0);

        if(pipeList.Count == 1)
        {
            sendFluid = pipeList[0].structureData.SendFluidAmount;
            sendDelay = pipeList[0].structureData.SendDelay;
        }
    }

    public void CheckGroup(PipeCtrl nextPipe)
    {
        PipeGroupMgr pipeGroupMgr = this.GetComponent<PipeGroupMgr>();

        if (nextPipe.pipeGroupMgr != null && pipeGroupMgr != nextPipe.pipeGroupMgr)
        {
            CombineFunc(pipeGroupMgr, nextPipe);
        }
    }

    void CombineFunc(PipeGroupMgr pipeGroupMgr, PipeCtrl nextPipe)
    {
        PipeManager pipeManager = this.GetComponentInParent<PipeManager>();

        pipeManager.PipeCombine(pipeGroupMgr, nextPipe.pipeGroupMgr);
    }

    public void GroupCheck()
    {
        groupFullFluidNum = 0;

        foreach (PipeCtrl pipe in pipeList)
        {
            groupFullFluidNum += pipe.structureData.MaxFulidStorageLimit;
        }

        float pipeFluid = groupSaveFluidNum / pipeList.Count;

        foreach (PipeCtrl pipe in pipeList)
        {
            pipe.saveFluidNum = pipeFluid;
        }
    }

    public void GroupFluidCount(float getNum)
    {
        float pipeFluid = (groupSaveFluidNum + getNum) / pipeList.Count;
        groupSaveFluidNum += getNum;
        if (groupFullFluidNum <= groupSaveFluidNum)
        {
            groupSaveFluidNum = groupFullFluidNum;
        }

        foreach (PipeCtrl pipe in pipeList)
        {
            pipe.saveFluidNum = pipeFluid;
        }
    }

    public void FactoryListAdd(GameObject facroty)
    {
        outObj.Add(facroty);
    }

    void SendFluid()
    {
        foreach (GameObject obj in outObj)
        {
            if (obj.TryGetComponent(out FluidFactoryCtrl fluidFactory) && obj.GetComponent<PumpCtrl>() == null)// && !obj.GetComponent<FluidFactoryCtrl>().fluidIsFull)
            {
                if(fluidFactory.structureData.MaxFulidStorageLimit > fluidFactory.saveFluidNum)
                {
                    float currentFillRatio = (float)fluidFactory.structureData.MaxFulidStorageLimit / fluidFactory.saveFluidNum;
                    float targetFillRatio = groupFullFluidNum / groupSaveFluidNum;

                    if (currentFillRatio > targetFillRatio)
                    {
                        groupSaveFluidNum -= pipeList[0].structureData.SendFluidAmount;
                        fluidFactory.SendFluidFunc(pipeList[0].structureData.SendFluidAmount);
                    }

                    GroupFluidCount(0);
                }
            }
        }
    }
}
