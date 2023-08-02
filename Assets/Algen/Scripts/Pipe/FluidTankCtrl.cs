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

    void Start()
    {
        TransformCheck();
    }

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

                if (factoryList.Count > 0)
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
                    if (!factoryList.Contains(Hits[a].collider.gameObject))
                    {
                        factoryList.Add(Hits[a].collider.gameObject);
                        if (Hits[a].collider.GetComponent<PipeCtrl>() != null)
                        {
                            Hits[a].collider.GetComponent<PipeCtrl>().FactoryVecCheck(this.transform.position + startVec);
                            Hits[a].collider.GetComponentInParent<PipeGroupMgr>().FactoryListAdd(this.gameObject);
                        }
                        StartCoroutine("ObjAddCheck", Hits[a].collider.gameObject);
                        return true;
                    }
                }
            }
        }
        return false;
    }

    IEnumerator ObjAddCheck(GameObject obj)
    {
        yield return null;

        if (obj.GetComponent<UnderPipeCtrl>())
        {
            if (obj.GetComponent<UnderPipeCtrl>().otherPipe == null || obj.GetComponent<UnderPipeCtrl>().otherPipe != this.gameObject)
            {
                factoryList.Remove(obj);
            }
        }
    }

    void SendFluid()
    {
        foreach (GameObject obj in factoryList)
        {
            if (obj.TryGetComponent(out FluidFactoryCtrl fluidFactory) && fluidFactory.GetComponent<PumpCtrl>() == null)
            {
                if (fluidFactory.fluidFactoryData.FullFluidNum > fluidFactory.saveFluidNum)
                {
                    float currentFillRatio = (float)fluidFactory.fluidFactoryData.FullFluidNum / fluidFactory.saveFluidNum;
                    float targetFillRatio = (float)fluidFactoryData.FullFluidNum / saveFluidNum;

                    if (currentFillRatio > targetFillRatio)
                    {
                        saveFluidNum -= fluidFactoryData.SendFluid;
                        fluidFactory.SendFluidFunc(fluidFactoryData.SendFluid);
                    }
                }
            }
        }
    }
}
