using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PumpCtrl : FluidFactoryCtrl
{
    public bool PumpUp = false;

    float pumpFluid = 1.0f;
    float pumpDelayTimer = 0.0f;
    float pumpDelay = 0.1f;

    [SerializeField]
    List<GameObject> outObj = new List<GameObject>();
    public Dictionary<GameObject, float> notFullObj = new Dictionary<GameObject, float>();

    GameObject[] nearObj = new GameObject[4];
    Vector2[] checkPos = new Vector2[4];

    // Start is called before the first frame update
    void Start()
    {
        CheckPos();
    }

    // Update is called once per frame
    void Update()
    {
        pumpDelayTimer += Time.deltaTime;

        if (pumpDelayTimer > pumpDelay)
        {
            Pump();
            pumpDelayTimer = 0;
        }

        if (nearObj[0] == null)
            UpObjCheck();
        if (nearObj[1] == null)
            RightObjCheck();
        if (nearObj[2] == null)
            DownObjCheck();
        if (nearObj[3] == null)
            LeftObjCheck();
    }

    void CheckPos()
    {
        checkPos[0] = transform.up;
        checkPos[1] = transform.right;
        checkPos[2] = -transform.up;
        checkPos[3] = -transform.right;
    }

    void UpObjCheck()
    {
        if (nearObj[0] == null)
        {
            RaycastHit2D[] upHits = Physics2D.RaycastAll(this.gameObject.transform.position, checkPos[0], 1f);

            for (int a = 0; a < upHits.Length; a++)
            {
                if (upHits[a].collider.GetComponent<PumpCtrl>() != this.gameObject.GetComponent<PumpCtrl>())
                {
                    if (upHits[a].collider.CompareTag("Factory"))
                    {
                        nearObj[0] = upHits[a].collider.gameObject;
                        SetOutObj(nearObj[0]);
                    }
                }
            }
        }
    }

    void RightObjCheck()
    {
        if (nearObj[1] == null)
        {
            RaycastHit2D[] rightHits = Physics2D.RaycastAll(this.gameObject.transform.position, checkPos[1], 1f);

            for (int a = 0; a < rightHits.Length; a++)
            {
                if (rightHits[a].collider.GetComponent<PumpCtrl>() != this.gameObject.GetComponent<PumpCtrl>())
                {
                    if (rightHits[a].collider.CompareTag("Factory"))
                    {
                        nearObj[1] = rightHits[a].collider.gameObject;
                        SetOutObj(nearObj[1]);
                    }
                }
            }
        }
    }
    void DownObjCheck()
    {
        if (nearObj[2] == null)
        {
            RaycastHit2D[] downHits = Physics2D.RaycastAll(this.gameObject.transform.position, checkPos[2], 1f);

            for (int a = 0; a < downHits.Length; a++)
            {
                if (downHits[a].collider.GetComponent<PumpCtrl>() != this.gameObject.GetComponent<PumpCtrl>())
                {
                    if (downHits[a].collider.CompareTag("Factory"))
                    {
                        nearObj[2] = downHits[a].collider.gameObject;
                        SetOutObj(nearObj[2]);
                    }
                }
            }
        }
    }

    void LeftObjCheck()
    {
        if (nearObj[3] == null)
        {
            RaycastHit2D[] leftHits = Physics2D.RaycastAll(this.gameObject.transform.position, checkPos[3], 1f);

            for (int a = 0; a < leftHits.Length; a++)
            {
                if (leftHits[a].collider.GetComponent<PumpCtrl>() != this.gameObject.GetComponent<PumpCtrl>())
                {
                    if (leftHits[a].collider.CompareTag("Factory"))
                    {
                        nearObj[3] = leftHits[a].collider.gameObject;
                        SetOutObj(nearObj[3]);
                    }
                }
            }
        }
    }


    void SetOutObj(GameObject obj)
    {
        if (obj.GetComponent<FactoryCtrl>() != null)
        {
            if(obj.GetComponent<BeltCtrl>() == null)
                outObj.Add(obj);
        }
    }

    void Pump()
    {
        if(outObj.Count > 0)
        {
            float totalSendFluid = saveFluidNum + pumpFluid;
            saveFluidNum = 0;

            foreach (GameObject obj in outObj)
            {
                if(obj.GetComponent<FluidFactoryCtrl>())
                {
                    if (obj.GetComponent<FluidFactoryCtrl>().fluidIsFull == false)
                    {
                        notFullObj.Add(obj, obj.GetComponent<FluidFactoryCtrl>().ExtraSize());
                    }
                }
                //else if(obj.GetComponent<FluidFactoryCtrl>())//공장일 경우
                //{
                //    if (obj.GetComponent<FluidFactoryCtrl>().fluidIsFull == false)
                //        notFullObj.Add(obj);
                //}
            }

            float sendFluid = totalSendFluid / notFullObj.Count;

            foreach (KeyValuePair<GameObject, float> obj in notFullObj)
            {
                if (obj.Key.GetComponent<FluidFactoryCtrl>())
                {
                    if (obj.Value < sendFluid) 
                    {
                        obj.Key.GetComponent<FluidFactoryCtrl>().GetFluid(obj.Value);
                        totalSendFluid -= obj.Value;

                    }
                    else if (obj.Value >= sendFluid)
                    {
                        obj.Key.GetComponent<FluidFactoryCtrl>().GetFluid(sendFluid);
                        totalSendFluid -= sendFluid;
                    }
                }
                //else if (obj.GetComponent<FluidFactoryCtrl>())//공장일 경우
                //{
                //    obj.GetComponent<FluidFactoryCtrl>().GetFluid(SendFluid);
                //}
            }
            saveFluidNum += totalSendFluid;

            if (fullFluidNum <= saveFluidNum)
            {
                fluidIsFull = true;
                saveFluidNum = fullFluidNum;
            }
            notFullObj.Clear();
        }
        else if (outObj.Count == 0)
        {
            if(fluidIsFull == false)
            {
                saveFluidNum += pumpFluid;
                if (fullFluidNum <= saveFluidNum)
                {
                    fluidIsFull = true;
                    saveFluidNum = fullFluidNum;
                }
            }
        }
    }
}   
