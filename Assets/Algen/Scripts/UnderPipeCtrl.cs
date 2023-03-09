using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        setModel = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        ModelSet();

        if (nearObj[0] == null)
            UpObjCheck();
        if (nearObj[1] == null)
            DownObjCheck();

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

    void UpObjCheck()
    {
        RaycastHit2D[] upHits = Physics2D.RaycastAll(this.gameObject.transform.position, checkPos[0], 10f);

        for (int a = 0; a < upHits.Length; a++)
        {
            if (upHits[a].collider.GetComponent<UnderPipeCtrl>() != this.gameObject.GetComponent<UnderPipeCtrl>())
            {
                if (upHits[a].collider.CompareTag("Factory"))
                {
                    if (upHits[a].collider.GetComponent<UnderPipeCtrl>() != null)
                    {
                        nearObj[0] = upHits[a].collider.gameObject;
                        ConnectUnder(nearObj[0]);
                    }
                }
            }
        }
    }

    void DownObjCheck()
    {
        RaycastHit2D[] downHits = Physics2D.RaycastAll(this.gameObject.transform.position, checkPos[1], 1f);

        for (int a = 0; a < downHits.Length; a++)
        {
            if (downHits[a].collider.GetComponent<UnderPipeCtrl>() != this.gameObject.GetComponent<UnderPipeCtrl>())
            {
                if (downHits[a].collider.CompareTag("Factory"))
                {
                    nearObj[1] = downHits[a].collider.gameObject;
                    ConnectOther(nearObj[1]);
                }
            }
        }
    }


    void ConnectUnder(GameObject obj)
    {
        if (obj.GetComponent<FluidFactoryCtrl>() != null)
        {
            if (obj.GetComponent<UnderPipeCtrl>() != null)
            {
                UnderPipeCtrl othUnderPipe = obj.GetComponent<UnderPipeCtrl>();

                if(dirNum == 0 && othUnderPipe.dirNum == 2)
                {
                    connectUnderPipe = obj;
                }
                else if (dirNum == 1 && othUnderPipe.dirNum == 3)
                {
                    connectUnderPipe = obj;
                }
                else if(dirNum == 2 && othUnderPipe.dirNum == 0)
                {
                    connectUnderPipe = obj;
                }
                else if(dirNum == 3 && othUnderPipe.dirNum == 1)
                {
                    connectUnderPipe = obj;
                }
            }
        }
    }

    void ConnectOther(GameObject obj)
    {
        if (obj.GetComponent<FluidFactoryCtrl>() != null)
        {
            if (obj.GetComponent<PipeCtrl>() != null)
            {
                otherPipe = obj;
                otherPipe.GetComponent<PipeCtrl>().FactoryVecCheck(this.transform.position);
                otherPipe.GetComponentInParent<PipeGroupMgr>().FactoryListAdd(this.gameObject);
            }
            else if (obj.GetComponent<UnderPipeCtrl>() != null)
            {
                UnderPipeCtrl othUnderPipe = obj.GetComponent<UnderPipeCtrl>();

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
        if (otherPipe != null && otherPipe.GetComponent<FluidFactoryCtrl>())
        {
            FluidFactoryCtrl pipe = otherPipe.GetComponent<FluidFactoryCtrl>();
            if (pipe.fluidIsFull == false)
            {
                //if (fluidIsFull == false)
                //{
                if (pipe.saveFluidNum < saveFluidNum)
                {
                    pipe.SendFluidFunc(fluidFactoryData.SendFluid);
                    saveFluidNum -= fluidFactoryData.SendFluid;
                }
            }
        }
        if (connectUnderPipe != null && connectUnderPipe.GetComponent<FluidFactoryCtrl>())
        {
            FluidFactoryCtrl underPipe = connectUnderPipe.GetComponent<FluidFactoryCtrl>();
            if (underPipe.fluidIsFull == false)
            {
                if (underPipe.saveFluidNum < saveFluidNum)
                {
                    //float checkFluid = underPipe.ExtraSize();
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
        if (otherPipe != null && otherPipe.GetComponent<FluidFactoryCtrl>())
        {
            FluidFactoryCtrl fluidFactory = otherPipe.GetComponent<FluidFactoryCtrl>();

            if (fluidFactory.fluidIsFull == true && fluidIsFull == false)
            {
                fluidFactory.GetFluidFunc(fluidFactoryData.SendFluid);
                saveFluidNum += fluidFactoryData.SendFluid;
            }
        }
        if (connectUnderPipe != null && connectUnderPipe.GetComponent<FluidFactoryCtrl>())
        {
            FluidFactoryCtrl fluidFactory = connectUnderPipe.GetComponent<FluidFactoryCtrl>();

            if (fluidFactory.fluidIsFull == true && fluidIsFull == false)
            {
                fluidFactory.GetFluidFunc(fluidFactoryData.SendFluid);
                saveFluidNum += fluidFactoryData.SendFluid;
            }
        }
    }
}
