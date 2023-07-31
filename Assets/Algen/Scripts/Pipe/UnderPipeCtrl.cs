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

    public GameObject otherPipe = null;

    public GameObject connectUnderPipe = null;

    // Start is called before the first frame update
    void Start()
    {
        dirCount = 4;
        setModel = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        ModelSet();
        if (!removeState)
        {
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

                if (otherPipe != null)
                {
                    sendDelayTimer += Time.deltaTime;

                    if (sendDelayTimer > fluidFactoryData.SendDelay)
                    {
                        if(saveFluidNum >= fluidFactoryData.SendFluid)
                            SendFluid();
                        sendDelayTimer = 0;
                    }
                }
            }
        }
    }
    void ModelSet()
    {
        setModel.sprite = modelNum[dirNum];

        CheckPos();
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
            otherPipe = obj;
        }
    }

    void SendFluid()
    {
        if (otherPipe != null && otherPipe.TryGetComponent(out FluidFactoryCtrl othObj) && otherPipe.GetComponent<PumpCtrl>() == null)
        {
            if (othObj.fluidFactoryData.FullFluidNum > othObj.saveFluidNum)
            {
                float currentFillRatio = (float)othObj.fluidFactoryData.FullFluidNum / othObj.saveFluidNum;
                float targetFillRatio = (float)fluidFactoryData.FullFluidNum / saveFluidNum;

                if (currentFillRatio > targetFillRatio)
                {
                    saveFluidNum -= fluidFactoryData.SendFluid;
                    othObj.SendFluidFunc(fluidFactoryData.SendFluid);
                }
            }           
        }
        if (connectUnderPipe != null && connectUnderPipe.TryGetComponent(out FluidFactoryCtrl underPipe))
        {
            if (underPipe.fluidFactoryData.FullFluidNum > underPipe.saveFluidNum)
            {
                float currentFillRatio = (float)underPipe.fluidFactoryData.FullFluidNum / underPipe.saveFluidNum;
                float targetFillRatio = (float)fluidFactoryData.FullFluidNum / saveFluidNum;

                if (currentFillRatio > targetFillRatio)
                {
                    saveFluidNum -= fluidFactoryData.SendFluid;
                    underPipe.SendFluidFunc(fluidFactoryData.SendFluid);
                }
            }
        }
    }
}
