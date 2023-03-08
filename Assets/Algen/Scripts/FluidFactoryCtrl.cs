using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidFactoryCtrl : FactoryCtrl
{
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

            if (fullFluidNum <= saveFluidNum)
            {
                fluidIsFull = true;
                saveFluidNum = fullFluidNum;
            }
        }
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

                if (fullFluidNum > saveFluidNum)
                {
                    fluidIsFull = false;
                }
            }
        }
    }
}
