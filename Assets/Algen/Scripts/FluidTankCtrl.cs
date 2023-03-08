using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidTankCtrl : FluidFactoryCtrl
{

    bool isUp1 = false;
    bool isUp2 = false;
    bool isRight1 = false;
    bool isRight2 = false;
    bool isDown1 = false;
    bool isDown2 = false;
    bool isLeft1 = false;
    bool isLeft2 = false;

    Vector2[] startTransform = new Vector2[4];

    public List<GameObject> factoryList = new List<GameObject>();
    //public Dictionary<GameObject, float> notFullObj = new Dictionary<GameObject, float>();

    float sendFluid = 1.0f;
    float sendDelayTimer = 0.0f;
    float sendDelay = 0.03f;

    // Start is called before the first frame update
    void Start()
    {
        TransformCheck();
    }

    // Update is called once per frame
    void Update()
    {
        if (isUp1 == false)
            isUp1 = ObjCheck(startTransform[3], transform.up);
        if (isUp2 == false)
            isUp2 = ObjCheck(startTransform[0], transform.up);
        if (isRight1 == false)
            isRight1 = ObjCheck(startTransform[0], transform.right);
        if (isRight2 == false)
            isRight2 = ObjCheck(startTransform[1], transform.right);
        if (isDown1 == false)
            isDown1 = ObjCheck(startTransform[1], -transform.up);
        if (isDown2 == false)
            isDown2 = ObjCheck(startTransform[2], -transform.up);
        if (isLeft1 == false)
            isLeft1 = ObjCheck(startTransform[2], -transform.right);
        if (isLeft2 == false)
            isLeft2 = ObjCheck(startTransform[3], -transform.right);

        if (factoryList.Count > 0 && saveFluidNum >= sendFluid)
        {
            sendDelayTimer += Time.deltaTime;

            if (sendDelayTimer > sendDelay)
            {
                SendFluid();            
                GetFluid();
                sendDelayTimer = 0;
            }
        }
    }

    void TransformCheck()
    {
        startTransform[0] = new Vector2(0.5f, 0.5f);
        startTransform[1] = new Vector2(0.5f, -0.5f);
        startTransform[2] = new Vector2(-0.5f, -0.5f);
        startTransform[3] = new Vector2(-0.5f, 0.5f);
    }

    bool ObjCheck(Vector3 startVec, Vector3 endVec)
    {
        RaycastHit2D[] Hits = Physics2D.RaycastAll(this.transform.position + startVec, endVec, 1f);

        for (int a = 0; a < Hits.Length; a++)
        {
            if (Hits[a].collider.GetComponent<FluidTankCtrl>() != this.gameObject.GetComponent<FluidTankCtrl>())
            {
                if (Hits[a].collider.CompareTag("Factory"))
                {
                    factoryList.Add(Hits[a].collider.gameObject);
                    if (Hits[a].collider.GetComponent<PipeCtrl>() != null)
                    {
                        Hits[a].collider.GetComponent<PipeCtrl>().FactoryVecCheck(this.transform.position + startVec);
                        Hits[a].collider.GetComponentInParent<PipeGroupMgr>().FactoryListAdd(this.gameObject);
                    }
                    return true;
                }
            }
        }
        return false;
    }

    void SendFluid()
    {
        foreach (GameObject obj in factoryList)
        {
            if (obj.GetComponent<FluidFactoryCtrl>() && obj.GetComponent<FluidFactoryCtrl>().fluidIsFull == false)
            {
                FluidFactoryCtrl fluidFactory = obj.GetComponent<FluidFactoryCtrl>();
                if (fluidFactory.saveFluidNum < saveFluidNum)
                {
                    fluidFactory.SendFluidFunc(sendFluid);
                    saveFluidNum -= sendFluid;
                }          
            }
            if (fullFluidNum > saveFluidNum)
                fluidIsFull = false;
            else if (fullFluidNum <= saveFluidNum)
                fluidIsFull = true;
        }
    }

    void GetFluid()
    {
        foreach (GameObject obj in factoryList)
        {
            if (obj.GetComponent<FluidFactoryCtrl>())
            {
                FluidFactoryCtrl fluidFactory = obj.GetComponent<FluidFactoryCtrl>();
                if (fluidFactory.fluidIsFull == true && fluidIsFull == false)
                {
                    fluidFactory.GetFluidFunc(sendFluid);
                    saveFluidNum += sendFluid;
                }
            }
        }
    }
}
