using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeGroupMgr : MonoBehaviour
{
    [SerializeField]
    GameObject pipeObj = null;

    //public bool up = false;
    //public bool down = false;
    //public bool left = false;
    //public bool right = false;

    public List<PipeCtrl> pipeList = new List<PipeCtrl>();
    public List<GameObject> factoryList = new List<GameObject>();
    //public Dictionary<GameObject, float> notFullObj = new Dictionary<GameObject, float>();

    //Vector2 nextPos;

    public bool isPreBuilding = false;

    public float groupFullFluidNum = 0.0f;
    public float groupSaveFluidNum = 0.0f;

    //public bool groupIsFull = false;

    float sendFluid = 1.0f;
    float sendDelayTimer = 0.0f;
    float sendDelay = 0.03f;

    // Update is called once per frame
    void Update()
    {
        if (!isPreBuilding)
        {
            if (factoryList.Count > 0)
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
            sendFluid = pipeList[0].fluidFactoryData.SendFluid;
            sendDelay = pipeList[0].fluidFactoryData.SendDelay;
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
        //groupIsFull = false;
        groupFullFluidNum = 0;

        foreach (PipeCtrl pipe in pipeList)
        {
            groupFullFluidNum += pipe.fluidFactoryData.FullFluidNum;
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
            //groupIsFull = true;
            groupSaveFluidNum = groupFullFluidNum;
        }

        foreach (PipeCtrl pipe in pipeList)
        {
            pipe.saveFluidNum = pipeFluid;
            //if (pipe.saveFluidNum >= pipe.fluidFactoryData.FullFluidNum)
            //    pipe.fluidIsFull = true;
            //else if (pipe.saveFluidNum < pipe.fluidFactoryData.FullFluidNum)
            //    pipe.fluidIsFull = false;
        }
    }

    public void FactoryListAdd(GameObject facroty)
    {
        factoryList.Add(facroty);
    }

    void SendFluid()
    {
        foreach (GameObject obj in factoryList)
        {
            if (obj.TryGetComponent(out FluidFactoryCtrl fluidFactory) && obj.GetComponent<PumpCtrl>() == null)// && obj.GetComponent<FluidFactoryCtrl>().fluidIsFull == false)
            {
                if(fluidFactory.fluidFactoryData.FullFluidNum > fluidFactory.saveFluidNum)
                {
                    float currentFillRatio = (float)fluidFactory.fluidFactoryData.FullFluidNum / fluidFactory.saveFluidNum;
                    float targetFillRatio = groupFullFluidNum / groupSaveFluidNum;

                    if (currentFillRatio > targetFillRatio)
                    {
                        groupSaveFluidNum -= pipeList[0].fluidFactoryData.SendFluid;
                        fluidFactory.SendFluidFunc(pipeList[0].fluidFactoryData.SendFluid);
                    }

                    GroupFluidCount(0);
                }
            }            
        }
    }
}
