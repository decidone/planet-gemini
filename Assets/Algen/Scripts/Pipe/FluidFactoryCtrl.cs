using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidFactoryCtrl : FactoryCtrl
{
    [SerializeField]
    public FluidFactoryData fluidFactoryData;
    protected FluidFactoryData FluidFactoryData { set { fluidFactoryData = value; } }

    public float saveFluidNum;
    public float sendDelayTimer = 0.0f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SendFluidFunc(float getNum)
    {
        if(this.GetComponentInParent<PipeGroupMgr>() != null)
        {
            PipeGroupMgr pipeGroupMgr = this.GetComponentInParent<PipeGroupMgr>();
            pipeGroupMgr.GroupFluidCount(getNum);
        }
        else if (this.GetComponentInParent<PipeGroupMgr>() == null)
        {
            saveFluidNum += getNum;

            if (fluidFactoryData.FullFluidNum <= saveFluidNum)
            {
                fluidIsFull = true;
                saveFluidNum = fluidFactoryData.FullFluidNum;
            }
        }
    }

    public void OnFluidSentHandler(float sentFluid)
    {
        // 유체를 받은 후 처리할 작업 수행
    }

    public void GetFluidFunc(float getNum)
    {

        if (this.GetComponentInParent<PipeGroupMgr>() != null)
        {
            PipeGroupMgr pipeGroupMgr = this.GetComponentInParent<PipeGroupMgr>();
            if(getNum < pipeGroupMgr.groupSaveFluidNum)
                pipeGroupMgr.GroupFluidCount(-getNum);
        }
        else if (this.GetComponentInParent<PipeGroupMgr>() == null)
        {
            if(getNum < saveFluidNum)
            { 
                saveFluidNum -= getNum;

                if (fluidFactoryData.FullFluidNum > saveFluidNum)
                {
                    fluidIsFull = false;
                }
            }
        }
    }
}
