using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidFactoryCtrl : Structure
{
    [SerializeField]
    public FluidFactoryData fluidFactoryData;
    protected FluidFactoryData FluidFactoryData { set { fluidFactoryData = value; } }

    public string fluidName;
    public float saveFluidNum;
    public float sendDelayTimer = 0.0f;

    BoxCollider2D box2D = null;

    protected override void Awake()
    {
        base.Awake();

        buildName = fluidFactoryData.FactoryName;
        box2D = GetComponent<BoxCollider2D>();
        hp = fluidFactoryData.MaxHp[level];
        hpBar.fillAmount = hp / fluidFactoryData.MaxHp[level];
        repairBar.fillAmount = 0;
    }

    protected virtual void Update()
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

            //if (fluidFactoryData.FullFluidNum <= saveFluidNum)
            //{
            //    saveFluidNum = fluidFactoryData.FullFluidNum;
            //}
        }
    }

    public void OnFluidSentHandler(float sentFluid)
    {
        // 유체를 받은 후 처리할 작업 수행
    }

    //public override void DisableColliders()
    //{
    //    box2D.enabled = false;
    //}

    //public override void EnableColliders()
    //{
    //    box2D.enabled = true;
    //}

    public override void ColliderTriggerOnOff(bool isOn)
    {
        if (isOn)
            box2D.isTrigger = true;
        else
            box2D.isTrigger = false;
    }

    public override void SetBuild()
    {
        unitCanvas.SetActive(true);
        hpBar.enabled = false;
        repairBar.enabled = true;
        repairGauge = 0;
        repairBar.fillAmount = repairGauge / fluidFactoryData.MaxRepairGauge;
        isSetBuildingOk = true;
    }

    protected override void RepairFunc(bool isBuilding)
    {
        repairGauge += 10.0f * Time.deltaTime;

        if (isBuilding)
        {
            repairBar.fillAmount = repairGauge / fluidFactoryData.MaxBuildingGauge;
            if (repairGauge >= fluidFactoryData.MaxRepairGauge)
            {
                isPreBuilding = false;
                repairGauge = 0.0f;
                repairBar.enabled = false;
                if (hp < fluidFactoryData.MaxHp[level])
                {
                    unitCanvas.SetActive(true);
                    hpBar.enabled = true;
                }
                else
                {
                    unitCanvas.SetActive(false);
                    //isRepair = true;
                }
                //EnableColliders();
                ColliderTriggerOnOff(false);
            }
        }
        else
        {
            repairBar.fillAmount = repairGauge / fluidFactoryData.MaxRepairGauge;
            if (repairGauge >= fluidFactoryData.MaxRepairGauge)
            {
                RepairEnd();
            }
        }
    }

    protected override void RepairEnd()
    {
        hpBar.enabled = true;

        //if (hp < solidFactoryData.MaxHp)
        //{
        //    unitCanvers.SetActive(true);
        //    hpBar.enabled = true;
        //}
        //else
        //{
        hp = fluidFactoryData.MaxHp[level];
        unitCanvas.SetActive(false);
        //}

        hpBar.fillAmount = hp / fluidFactoryData.MaxHp[level];

        repairBar.enabled = false;
        repairGauge = 0.0f;

        isRuin = false;
        isPreBuilding = false;

        //EnableColliders();
        ColliderTriggerOnOff(false);
    }

    public override void TakeDamage(float damage)
    {
        if (!isPreBuilding)
        {
            if (!unitCanvas.activeSelf)
            {
                unitCanvas.SetActive(true);
                hpBar.enabled = true;
            }
        }

        if (hp <= 0f)
            return;

        hp -= damage;
        hpBar.fillAmount = hp / fluidFactoryData.MaxHp[level];

        if (hp <= 0f)
        {
            hp = 0f;
            DieFunc();
        }
    }

    public override void HealFunc(float heal)
    {
        if (hp == fluidFactoryData.MaxHp[level])
        {
            return;
        }
        else if (hp + heal > fluidFactoryData.MaxHp[level])
        {
            hp = fluidFactoryData.MaxHp[level];
            if (!isRepair)
                unitCanvas.SetActive(false);
        }
        else
            hp += heal;

        hpBar.fillAmount = hp / fluidFactoryData.MaxHp[level];
    }

    public override void RepairSet(bool repair)
    {
        hp = fluidFactoryData.MaxHp[level];
        isRepair = repair;
        //repairBar.enabled = repair;
    }

    protected override void DieFunc()
    {
        //unitCanvers.SetActive(false);
        repairBar.enabled = true;
        hpBar.enabled = false;

        repairGauge = 0;
        repairBar.fillAmount = repairGauge / fluidFactoryData.MaxBuildingGauge;

        //DisableColliders();
        ColliderTriggerOnOff(true);

        isRuin = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.GetComponent<ItemProps>())
        {
            if (isPreBuilding)
            {
                buildingPosObj.Add(collision.gameObject);
                if (buildingPosObj.Count > 0)
                {
                    if (!collision.GetComponentInParent<PreBuilding>())
                    {
                        canBuilding = false;
                    }

                    PreBuilding preBuilding = GetComponentInParent<PreBuilding>();
                    if (preBuilding != null)
                    {
                        preBuilding.isBuildingOk = false;
                    }
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.GetComponent<ItemProps>())
        {
            if (isPreBuilding)
            {
                buildingPosObj.Remove(collision.gameObject);
                if (buildingPosObj.Count > 0)
                    canBuilding = false;
                else
                {
                    canBuilding = true;

                    PreBuilding preBuilding = GetComponentInParent<PreBuilding>();
                    if (preBuilding != null)
                        preBuilding.isBuildingOk = true;
                }
            }
        }
    }
}
