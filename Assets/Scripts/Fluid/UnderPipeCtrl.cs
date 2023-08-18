using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UnderPipeCtrl : FluidFactoryCtrl
{
    public GameObject otherPipe = null;

    public GameObject connectUnderPipe = null;

    protected override void Start()
    {
        dirCount = 4;
        setModel = GetComponent<SpriteRenderer>();
        nearObj = new GameObject[2];
        checkPos = new Vector2[2];
    }

    protected override void Update()
    {
        base.Update();
        if (!removeState)
        {
            SetDirNum();
            if (!isPreBuilding)
            {
                for (int i = 0; i < nearObj.Length; i++)
                {
                    if (nearObj[i] == null)
                    {
                        if (i == 0)
                            CheckNearObj(checkPos[0], 0, obj => UnderPipeSetInObj(obj));
                        else if (i == 1)
                            CheckNearObj(checkPos[1], 1,  obj => UnderPipeSetOutObj(obj));
                    }
                }

                if (otherPipe != null)
                {
                    sendDelayTimer += Time.deltaTime;

                    if (sendDelayTimer > structureData.SendDelay)
                    {
                        if(saveFluidNum >= structureData.SendFluidAmount)
                            SendFluid();
                        sendDelayTimer = 0;
                    }
                }
            }
        }
    }

    protected override void CheckPos()
    {
        if (dirNum == 0)
        {
            checkPos[0] = transform.up;
            checkPos[1] = -transform.up;
        }
        else if (dirNum == 1)
        {
            checkPos[0] = transform.right;
            checkPos[1] = -transform.right;
        }
        else if (dirNum == 2)
        {
            checkPos[0] = -transform.up;
            checkPos[1] = transform.up;
        }
        else if (dirNum == 3)
        {
            checkPos[0] = -transform.right;
            checkPos[1] = transform.right;
        }
    }

    protected override void CheckNearObj(Vector2 direction, int index, Action<GameObject> callback)
    {
        float dist = 0;

        if (index == 0)
            dist = 10;
        else
            dist = 1;

        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, dist);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hitCollider = hits[i].collider;
            if (hitCollider.CompareTag("Factory") && !hitCollider.GetComponent<Structure>().isPreBuilding &&
                hits[i].collider.gameObject != this.gameObject)
            {
                if (index == 0)
                {
                    if (hitCollider.TryGetComponent(out UnderPipeCtrl otherUnderPipe))
                    {
                        if (CanConnectUnderPipe(otherUnderPipe))
                        {
                            nearObj[index] = hits[i].collider.gameObject;
                            callback(hitCollider.gameObject);
                            break;
                        }
                        else
                            break;
                    }
                }
                else
                {                
                    nearObj[index] = hits[i].collider.gameObject;
                    callback(hitCollider.gameObject);
                    break;
                }
            }
        }
    }

    bool CanConnectUnderPipe(UnderPipeCtrl othUnderPipe)
    {
        if (dirNum == 0 && othUnderPipe.dirNum == 2)
        {
            return true;
        }
        else if (dirNum == 1 && othUnderPipe.dirNum == 3)
        {
            return true;
        }
        else if (dirNum == 2 && othUnderPipe.dirNum == 0)
        {
            return true;
        }
        else if (dirNum == 3 && othUnderPipe.dirNum == 1)
        {
            return true;
        }
        else
            return false;
    }

    void UnderPipeSetInObj(GameObject obj)
    {
        if (obj.GetComponent<FluidFactoryCtrl>() != null)
        {
            if (obj.TryGetComponent(out UnderPipeCtrl othUnderPipe))
            {
                connectUnderPipe = obj;
                if (othUnderPipe.connectUnderPipe != this.gameObject)
                {
                    if(othUnderPipe.connectUnderPipe != null)
                        othUnderPipe.connectUnderPipe.GetComponent<UnderPipeCtrl>().DisCntObj();
                    othUnderPipe.DisCntObj();
                }
            }
        }
    }

    public void DisCntObj()
    {
        connectUnderPipe = null;
        nearObj = new GameObject[2];
    }

    void UnderPipeSetOutObj(GameObject obj)
    {
        if (obj.GetComponent<FluidFactoryCtrl>() != null)
        {
            otherPipe = obj;
            if (obj.GetComponent<PipeCtrl>() != null)
            {
                otherPipe.GetComponent<PipeCtrl>().FactoryVecCheck(this.transform.position);
                otherPipe.GetComponentInParent<PipeGroupMgr>().FactoryListAdd(this.gameObject);
            }
        }
    }

    protected override void SendFluid()
    {
        if (otherPipe != null && otherPipe.TryGetComponent(out FluidFactoryCtrl othObj) && otherPipe.GetComponent<PumpCtrl>() == null)
        {
            if (othObj.structureData.MaxFulidStorageLimit > othObj.saveFluidNum)
            {
                float currentFillRatio = (float)othObj.structureData.MaxFulidStorageLimit / othObj.saveFluidNum;
                float targetFillRatio = (float)structureData.MaxFulidStorageLimit / saveFluidNum;

                if (currentFillRatio > targetFillRatio)
                {
                    saveFluidNum -= structureData.SendFluidAmount;
                    othObj.SendFluidFunc(structureData.SendFluidAmount);
                }
            }
        }
        if (connectUnderPipe != null && connectUnderPipe.TryGetComponent(out FluidFactoryCtrl underPipe))
        {
            if (underPipe.structureData.MaxFulidStorageLimit > underPipe.saveFluidNum)
            {
                float currentFillRatio = (float)underPipe.structureData.MaxFulidStorageLimit / underPipe.saveFluidNum;
                float targetFillRatio = (float)structureData.MaxFulidStorageLimit / saveFluidNum;

                if (currentFillRatio > targetFillRatio)
                {
                    saveFluidNum -= structureData.SendFluidAmount;
                    underPipe.SendFluidFunc(structureData.SendFluidAmount);
                }
            }
        }
    }
}
