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

    public void GetFluid(float getNum)
    {
        if(this.GetComponentInParent<PipeGroupMgr>() != null)
        {
            PipeGroupMgr pipeGroupMgr = this.GetComponentInParent<PipeGroupMgr>();
            pipeGroupMgr.GroupFluidCount(getNum);
        }
        else if (this.GetComponentInParent<PipeGroupMgr>() == null)
        {
            //float addFluidNum = saveFluidNum + getNum;
            saveFluidNum += getNum;

            if (fullFluidNum <= saveFluidNum)
            {
                fluidIsFull = true;
                saveFluidNum = fullFluidNum;
            }
        }
    }
    public float ExtraSize()
    {
        if (this.GetComponentInParent<PipeGroupMgr>() != null)
        {
            PipeGroupMgr pipeGroupMgr = this.GetComponentInParent<PipeGroupMgr>();
            return pipeGroupMgr.groupFullFluidNum - pipeGroupMgr.groupSaveFluidNum;
        }
        else if(this.GetComponentInParent<PipeGroupMgr>() == null)
        {
            return fullFluidNum - saveFluidNum;
        }
        else
            return 0;
    }
}
