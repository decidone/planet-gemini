using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class FluidFactoryCtrl : Production
{
    [HideInInspector]
    public string fluidName;
    [HideInInspector]
    public float saveFluidNum;
    [HideInInspector]
    public float sendDelayTimer = 0.0f;

    protected override void Awake()
    {
        GameManager gameManager = GameManager.instance;
        playerInven = gameManager.GetComponent<Inventory>();
        buildName = structureData.FactoryName;
        col = GetComponent<BoxCollider2D>();
        hp = structureData.MaxHp[level];
        hpBar.fillAmount = hp / structureData.MaxHp[level];
        repairBar.fillAmount = 0;
    }

    protected override void Update()
    {
        if (!removeState)
        {
            if (isRuin && isRepair)
            {
                RepairFunc(false);
            }
            else if (isPreBuilding && isSetBuildingOk && !isRuin)
            {
                RepairFunc(true);
            }
        }
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

            if (structureData.MaxFulidStorageLimit <= saveFluidNum)
            {
                saveFluidNum = structureData.MaxFulidStorageLimit;
            }
        }
    }
}
