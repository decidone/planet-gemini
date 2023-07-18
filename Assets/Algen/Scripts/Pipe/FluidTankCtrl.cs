using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidTankCtrl : FluidFactoryCtrl
{
    bool[] checkArray = new bool[8];

    Vector2[] startTransform = new Vector2[4];
    Vector3[] directions = new Vector3[4];
    int[] indices = new int[6];
    public List<GameObject> factoryList = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        TransformCheck();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (!removeState)
        {
            if (!isPreBuilding)
            {
                for (int i = 0; i < 4; i++)
                {
                    int index = i * 2;
                    if (!checkArray[index])
                        checkArray[index] = CheckNearObj(startTransform[indices[index]], directions[i]);
                    if (!checkArray[index + 1])
                        checkArray[index + 1] = CheckNearObj(startTransform[indices[index + 1]], directions[i]);
                }

                if (factoryList.Count > 0 && saveFluidNum >= fluidFactoryData.SendFluid)
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
    }

    void TransformCheck()
    {
        indices = new int[] { 3, 0, 0, 1, 1, 2, 2, 3 };
        startTransform = new Vector2[] { new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f), new Vector2(-0.5f, 0.5f) };
        directions = new Vector3[] { transform.up, transform.right, -transform.up, -transform.right };
    }

    bool CheckNearObj(Vector3 startVec, Vector3 endVec)
    {
        RaycastHit2D[] Hits = Physics2D.RaycastAll(this.transform.position + startVec, endVec, 1f);

        for (int a = 0; a < Hits.Length; a++)
        {
            if (Hits[a].collider.GetComponent<FluidTankCtrl>() != this.gameObject.GetComponent<FluidTankCtrl>())
            {
                if (Hits[a].collider.CompareTag("Factory") && !Hits[a].collider.GetComponent<Structure>().isPreBuilding)
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
            if (obj.TryGetComponent(out FluidFactoryCtrl fluidFactory) && fluidFactory.fluidIsFull == false)
            {
                if (fluidFactory.saveFluidNum < saveFluidNum)
                {
                    fluidFactory.SendFluidFunc(fluidFactoryData.SendFluid);
                    saveFluidNum -= fluidFactoryData.SendFluid;
                }
            }
            if (fluidFactoryData.FullFluidNum > saveFluidNum)
                fluidIsFull = false;
            else if (fluidFactoryData.FullFluidNum <= saveFluidNum)
                fluidIsFull = true;
        }   
    }

    void GetFluid()
    {
        foreach (GameObject obj in factoryList)
        {
            if (obj.TryGetComponent(out FluidFactoryCtrl fluidFactory))
            {
                if (fluidFactory.fluidIsFull == true && fluidIsFull == false)
                {
                    fluidFactory.GetFluidFunc(fluidFactory.fluidFactoryData.SendFluid);
                    saveFluidNum += fluidFactory.fluidFactoryData.SendFluid;
                }
            }
        }
    }
}
