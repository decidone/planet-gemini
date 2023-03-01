using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnderPipeCtrl : FluidFactoryCtrl
{
    Vector2[] checkPos = new Vector2[2];
    [SerializeField]
    GameObject[] nearObj = new GameObject[2];

    [SerializeField]
    GameObject otherPipe = null;
    [SerializeField]
    GameObject connectUnderPipe = null;
    
    public Dictionary<GameObject, float> notFullObj = new Dictionary<GameObject, float>();

    float sendFluid = 1.0f;
    float sendDelayTimer = 0.0f;
    float sendDelay = 0.1f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ModelSet();

        if (nearObj[0] == null)
            UpObjCheck();
        if (nearObj[1] == null)
            DownObjCheck();

        if(otherPipe != null)
        {
            sendDelayTimer += Time.deltaTime;

            if (sendDelayTimer > sendDelay)
            {
                SendFluid();
                sendDelayTimer = 0;
            }
        }

    }
    void ModelSet()
    {
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
                obj.GetComponentInParent<PipeGroupMgr>().factoryList.Add(this.gameObject);
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
        if (otherPipe.GetComponent<FluidFactoryCtrl>())
        {
            if (otherPipe.GetComponent<FluidFactoryCtrl>().fluidIsFull == false)
            {
                if (otherPipe.GetComponent<FluidFactoryCtrl>().saveFluidNum < saveFluidNum)
                    notFullObj.Add(otherPipe, otherPipe.GetComponent<FluidFactoryCtrl>().ExtraSize());
            }
        }
        if (connectUnderPipe.GetComponent<FluidFactoryCtrl>())
        {
            if (connectUnderPipe.GetComponent<FluidFactoryCtrl>().fluidIsFull == false)
            {
                if(fluidIsFull == false)
                {
                    if (connectUnderPipe.GetComponent<FluidFactoryCtrl>().saveFluidNum < saveFluidNum)
                        notFullObj.Add(connectUnderPipe, connectUnderPipe.GetComponent<FluidFactoryCtrl>().ExtraSize());
                }
                else if(fluidIsFull == true)
                {
                    notFullObj.Add(connectUnderPipe, connectUnderPipe.GetComponent<FluidFactoryCtrl>().ExtraSize());
                }
            }
        }

        foreach (KeyValuePair<GameObject, float> obj in notFullObj)
        {
            if (saveFluidNum - sendFluid < 0)
            {
                notFullObj.Clear();
                return;
            }
            else if (saveFluidNum - sendFluid >= 0)
            {
                if (obj.Key.GetComponent<FluidFactoryCtrl>())
                {
                    if (obj.Value < sendFluid)
                    {
                        obj.Key.GetComponent<FluidFactoryCtrl>().GetFluid(obj.Value);
                        saveFluidNum -= obj.Value;

                    }
                    else if (obj.Value >= sendFluid)
                    {
                        obj.Key.GetComponent<FluidFactoryCtrl>().GetFluid(sendFluid);
                        saveFluidNum -= sendFluid;
                    }
                }
            }
        }
        if (fullFluidNum > saveFluidNum)
            fluidIsFull = false;

        notFullObj.Clear();
    }
}
