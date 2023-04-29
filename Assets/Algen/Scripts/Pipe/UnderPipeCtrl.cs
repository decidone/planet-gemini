using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UnderPipeCtrl : FluidFactoryCtrl
{
    Vector2[] checkPos = new Vector2[2];
    [SerializeField]
    GameObject[] nearObj = new GameObject[2];

    [SerializeField]
    Sprite[] modelNum = new Sprite[4];
    SpriteRenderer setModel;

    GameObject otherPipe = null;
    [SerializeField]
    GameObject connectUnderPipe = null;
    
    // Start is called before the first frame update
    void Start()
    {
        dirCount = 4;
        setModel = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        ModelSet();
        if (!isPreBuilding)
        {
            for (int i = 0; i < nearObj.Length; i++)
            {
                if (nearObj[i] == null)
                {
                    if (i == 0)
                        CheckNearObj(checkPos[0], 0, obj => SetInObj(obj));
                    else if (i == 1)
                        CheckNearObj(checkPos[1], 1, obj => SetOutObj(obj));
                }
            }

            if (otherPipe != null && saveFluidNum >= fluidFactoryData.SendFluid)
            {
                sendDelayTimer += Time.deltaTime;

                if (sendDelayTimer > fluidFactoryData.SendDelay)
                {
                    SendFluid();
                    GetFluid();
                    sendDelayTimer = 0;
                }
            }
        }
    }
    void ModelSet()
    {
        setModel.sprite = modelNum[dirNum];

        CheckPos();
    }

    void CheckPos()
    {
        if (dirNum == 0)
        {
            checkPos[0] = -transform.up;
            checkPos[1] = transform.up;
        }
        else if (dirNum == 1)
        {
            checkPos[0] = -transform.right;
            checkPos[1] = transform.right;
        }
        else if (dirNum == 2)
        {
            checkPos[0] = transform.up;
            checkPos[1] = -transform.up;
        }
        else if (dirNum == 3)
        {
            checkPos[0] = transform.right;
            checkPos[1] = -transform.right;
        }
    }

    void CheckNearObj(Vector2 direction, int index, Action<GameObject> callback)
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
            if (hitCollider.CompareTag("Factory") && !hitCollider.GetComponent<FactoryCtrl>().isPreBuilding &&
                hitCollider.GetComponent<UnderPipeCtrl>() != GetComponent<UnderPipeCtrl>())
            {
                nearObj[index] = hits[i].collider.gameObject;
                callback(hitCollider.gameObject);
                break;
            }
        }
    }

    void SetInObj(GameObject obj)
    {
        if (obj.GetComponent<FluidFactoryCtrl>() != null)
        {
            if (obj.TryGetComponent(out UnderPipeCtrl othUnderPipe))
            {
                if (dirNum == 0 && othUnderPipe.dirNum == 2)
                {
                    connectUnderPipe = obj;
                }
                else if (dirNum == 1 && othUnderPipe.dirNum == 3)
                {
                    connectUnderPipe = obj;
                }
                else if (dirNum == 2 && othUnderPipe.dirNum == 0)
                {
                    connectUnderPipe = obj;
                }
                else if (dirNum == 3 && othUnderPipe.dirNum == 1)
                {
                    connectUnderPipe = obj;
                }
            }
        }
    }

    void SetOutObj(GameObject obj)
    {
        if (obj.GetComponent<FluidFactoryCtrl>() != null)
        {
            if (obj.GetComponent<PipeCtrl>() != null)
            {
                otherPipe = obj;
                otherPipe.GetComponent<PipeCtrl>().FactoryVecCheck(this.transform.position);
                otherPipe.GetComponentInParent<PipeGroupMgr>().FactoryListAdd(this.gameObject);
            }
            else if (obj.TryGetComponent(out UnderPipeCtrl othUnderPipe))
            {
                if (dirNum == 0 && othUnderPipe.dirNum == 2)
                {
                    otherPipe = obj;
                }
                else if (dirNum == 1 && othUnderPipe.dirNum == 3)
                {
                    otherPipe = obj;
                }
                else if (dirNum == 2 && othUnderPipe.dirNum == 0)
                {
                    otherPipe = obj;
                }
                else if (dirNum == 3 && othUnderPipe.dirNum == 1)
                {
                    otherPipe = obj;
                }
            }
        }
    }

    void SendFluid()
    {
        if (otherPipe != null && otherPipe.TryGetComponent(out FluidFactoryCtrl pipe))
        {
            if (pipe.fluidIsFull == false)
            {
                if (pipe.saveFluidNum < saveFluidNum)
                {
                    pipe.SendFluidFunc(fluidFactoryData.SendFluid);
                    saveFluidNum -= fluidFactoryData.SendFluid;
                }
            }
        }
        if (connectUnderPipe != null && connectUnderPipe.TryGetComponent(out FluidFactoryCtrl underPipe))
        {
            if (underPipe.fluidIsFull == false)
            {
                if (underPipe.saveFluidNum < saveFluidNum)
                {
                    underPipe.SendFluidFunc(fluidFactoryData.SendFluid);
                    saveFluidNum -= fluidFactoryData.SendFluid;
                }
            }
        }

        if(fluidFactoryData.FullFluidNum > saveFluidNum)        
            fluidIsFull = false;
        else if (fluidFactoryData.FullFluidNum <= saveFluidNum)
            fluidIsFull = true;
    }
    void GetFluid()
    {
        if (otherPipe != null && otherPipe.TryGetComponent(out FluidFactoryCtrl otherFac))
        {
            if (otherFac.fluidIsFull == true && fluidIsFull == false)
            {
                otherFac.GetFluidFunc(fluidFactoryData.SendFluid);
                saveFluidNum += fluidFactoryData.SendFluid;
            }
        }
        if (connectUnderPipe != null && connectUnderPipe.TryGetComponent(out FluidFactoryCtrl underPipe))
        {
            if (underPipe.fluidIsFull == true && fluidIsFull == false)
            {
                underPipe.GetFluidFunc(underPipe.fluidFactoryData.SendFluid);
                saveFluidNum += underPipe.fluidFactoryData.SendFluid;
            }
        }
    }
}
