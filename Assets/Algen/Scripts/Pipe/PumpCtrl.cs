using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PumpCtrl : FluidFactoryCtrl
{
    public bool PumpUp = false;

    float pumpFluid = 15.0f;

    [SerializeField]
    List<GameObject> factoryList = new List<GameObject>();

    GameObject[] nearObj = new GameObject[4];
    Vector2[] checkPos = new Vector2[4];

    public bool PumpIng = false;

    bool isUp = false;
    bool isRight = false;
    bool isDown = false;
    bool isLeft = false;
    // Start is called before the first frame update
    void Start()
    {
        CheckPos();
    }

    // Update is called once per frame
    void Update()
    {
        if(!isPreBuilding)
        {
            if (isUp == false)
                isUp = ObjCheck(transform.up);
            if (isRight == false)
                isRight = ObjCheck(transform.right);
            if (isDown == false)
                isDown = ObjCheck(-transform.up);
            if (isLeft == false)
                isLeft = ObjCheck(-transform.right);

            if(PumpIng == true)
            {
                sendDelayTimer += Time.deltaTime;

                if (sendDelayTimer > fluidFactoryData.SendDelay)
                {
                    Pump();
                    sendDelayTimer = 0;
                }            
            }
        }
    }

    void CheckPos()
    {
        checkPos[0] = transform.up;
        checkPos[1] = transform.right;
        checkPos[2] = -transform.up;
        checkPos[3] = -transform.right;
    }

    bool ObjCheck(Vector3 vec)
    {
        RaycastHit2D[] Hits = Physics2D.RaycastAll(this.gameObject.transform.position, vec, 1f);

        for (int a = 0; a < Hits.Length; a++)
        {
            if (Hits[a].collider.GetComponent<PipeCtrl>() != this.gameObject.GetComponent<PipeCtrl>())
            {
                if (Hits[a].collider.GetComponent<PumpCtrl>() != this.gameObject.GetComponent<PumpCtrl>())
                {
                    if (Hits[a].collider.CompareTag("Factory") && !Hits[a].collider.GetComponent<FactoryCtrl>().isPreBuilding)
                    {
                        nearObj[0] = Hits[a].collider.gameObject;
                        SetOutObj(nearObj[0]);
                        return true;
                    }
                }
            }
        }
        return false;
    }

    void SetOutObj(GameObject obj)
    { 
        if(obj.GetComponent<FluidFactoryCtrl>() != null)
        {
            factoryList.Add(obj);
            if(obj.GetComponent<PipeCtrl>() != null)
            {
                obj.GetComponent<PipeCtrl>().FactoryVecCheck(this.transform.position);
            }
        }

    }

    void Pump()
    {
        if(saveFluidNum < fluidFactoryData.FullFluidNum)
        { 
            if (saveFluidNum + pumpFluid >= fluidFactoryData.FullFluidNum)
                saveFluidNum = fluidFactoryData.FullFluidNum;
            else if (saveFluidNum + pumpFluid < fluidFactoryData.FullFluidNum)
                saveFluidNum += pumpFluid;
        }


        if (factoryList.Count > 0)
        {
            foreach (GameObject obj in factoryList)
            {
                if (obj.GetComponent<FluidFactoryCtrl>() && obj.GetComponent<FluidFactoryCtrl>().fluidIsFull == false)
                {
                    FluidFactoryCtrl fluidFactory = obj.GetComponent<FluidFactoryCtrl>();

                    fluidFactory.SendFluidFunc(fluidFactoryData.SendFluid);
                    saveFluidNum -= fluidFactoryData.SendFluid;
                }
                if (fluidFactoryData.FullFluidNum > saveFluidNum)
                    fluidIsFull = false;
                else if (fluidFactoryData.FullFluidNum <= saveFluidNum)
                    fluidIsFull = true;
            }
        }
    }
}   
