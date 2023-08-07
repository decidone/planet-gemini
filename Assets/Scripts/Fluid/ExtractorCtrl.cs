using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtractorCtrl : FluidFactoryCtrl
{
    float pumpFluid = 15.0f;

    [SerializeField]
    List<GameObject> factoryList = new List<GameObject>();

    GameObject[] nearObj = new GameObject[4];
    Vector2[] checkPos = new Vector2[4];

    public bool PumpIng = true;

    bool isUp = false;
    bool isRight = false;
    bool isDown = false;
    bool isLeft = false;

    void Start()
    {
        CheckPos();
    }

    protected override void Update()
    {
        base.Update();

        if (!removeState)
        {
            if (!isPreBuilding)
            {
                if (isUp == false)
                    isUp = ObjCheck(transform.up);
                if (isRight == false)
                    isRight = ObjCheck(transform.right);
                if (isDown == false)
                    isDown = ObjCheck(-transform.up);
                if (isLeft == false)
                    isLeft = ObjCheck(-transform.right);

                if (PumpIng == true)
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
    }

    protected override void CheckPos()
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
            if (Hits[a].collider.GetComponent<ExtractorCtrl>() != this.gameObject.GetComponent<ExtractorCtrl>())
            {
                if (Hits[a].collider.CompareTag("Factory") && !Hits[a].collider.GetComponent<Structure>().isPreBuilding)
                {
                    nearObj[0] = Hits[a].collider.gameObject;
                    SetOutObj(nearObj[0]);
                    return true;
                }
            }
        }
        return false;
    }

    void SetOutObj(GameObject obj)
    {
        if (obj.GetComponent<FluidFactoryCtrl>() != null)
        {
            factoryList.Add(obj);
            if (obj.GetComponent<PipeCtrl>() != null)
            {
                obj.GetComponent<PipeCtrl>().FactoryVecCheck(this.transform.position);
            }
        }
    }

    void Pump()
    {
        if (saveFluidNum < fluidFactoryData.FullFluidNum)
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
                if (obj.TryGetComponent(out FluidFactoryCtrl fluidFactory) && obj.GetComponent<ExtractorCtrl>() == null)// && obj.GetComponent<FluidFactoryCtrl>().fluidIsFull == false)
                {
                    if (fluidFactory.fluidFactoryData.FullFluidNum > fluidFactory.saveFluidNum)
                    {
                        fluidFactory.SendFluidFunc(fluidFactoryData.SendFluid);
                        saveFluidNum -= fluidFactoryData.SendFluid;
                    }
                }
            }
        }
    }
}
