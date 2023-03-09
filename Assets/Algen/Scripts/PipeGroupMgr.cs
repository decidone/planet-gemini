using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeGroupMgr : MonoBehaviour
{
    [SerializeField]
    GameObject PipeObj = null;

    public bool up = false;
    public bool down = false;
    public bool left = false;
    public bool right = false;

    public List<PipeCtrl> pipeList = new List<PipeCtrl>();
    public List<GameObject> factoryList = new List<GameObject>();
    //public Dictionary<GameObject, float> notFullObj = new Dictionary<GameObject, float>();

    Vector2 nextPos;

    public float groupFullFluidNum = 0.0f;
    public float groupSaveFluidNum = 0.0f;

    public bool groupIsFull = false;

    float sendFluid = 1.0f;
    float sendDelayTimer = 0.0f;
    float sendDelay = 0.03f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (up == true)
        {
            SetPipe(0);
            up = false;
        }
        else if (down == true)
        {
            SetPipe(2);
            down = false;
        }
        else if (left == true)
        {
            SetPipe(3);
            left = false;
        }
        else if (right == true)
        {
            SetPipe(1);
            right = false;
        }

        if (factoryList.Count > 0 && groupSaveFluidNum >= sendFluid)
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

    void SetPipe(int pipeDir)
    {
        if (pipeList.Count == 0)
        {
            GameObject pipe = Instantiate(PipeObj, this.transform.position, Quaternion.identity);
            pipe.transform.parent = this.transform;
            PipeCtrl pipeCtrl = pipe.GetComponent<PipeCtrl>();
            pipeList.Add(pipeCtrl);
            GroupCheck();
        }
        else if (pipeList.Count != 0)
        {
            PipeCtrl prePipeCtrl = pipeList[pipeList.Count - 1];

            if(pipeDir == 0)
                nextPos = new Vector2(prePipeCtrl.transform.position.x, prePipeCtrl.transform.position.y + 1);
            if (pipeDir == 1)
                nextPos = new Vector2(prePipeCtrl.transform.position.x + 1, prePipeCtrl.transform.position.y);
            if (pipeDir == 2)
                nextPos = new Vector2(prePipeCtrl.transform.position.x, prePipeCtrl.transform.position.y - 1);
            if (pipeDir == 3)
                nextPos = new Vector2(prePipeCtrl.transform.position.x - 1, prePipeCtrl.transform.position.y);

            GameObject pipe = Instantiate(PipeObj, nextPos, Quaternion.identity);
            pipe.transform.parent = this.transform;
            PipeCtrl pipeCtrl = pipe.GetComponent<PipeCtrl>();
            pipeList.Add(pipeCtrl);
            GroupCheck();
            GroupFluidCount(0);
        }
    }

   public void CheckGroup(PipeCtrl nextPipe)
    {
        PipeGroupMgr pipeGroupMgr = this.GetComponent<PipeGroupMgr>();

        if (nextPipe.pipeGroupMgr != null && pipeGroupMgr != nextPipe.pipeGroupMgr)
        {
            CombineFunc(pipeGroupMgr, nextPipe);         
        }
    }
    void CombineFunc(PipeGroupMgr pipeGroupMgr, PipeCtrl nextPipe)
    {
        PipeManager pipeManager = this.GetComponentInParent<PipeManager>();

        pipeManager.PipeCombine(pipeGroupMgr, nextPipe.pipeGroupMgr);
    }

    public void GroupCheck()
    {
        groupIsFull = false;
        groupFullFluidNum = 0;

        foreach (PipeCtrl pipe in pipeList)
        {
            groupFullFluidNum += pipe.fluidFactoryData.FullFluidNum;
        }

        float SendFluid = groupSaveFluidNum / pipeList.Count;

        foreach (PipeCtrl pipe in pipeList)
        {
            pipe.saveFluidNum = SendFluid;
        }
    }

    public void GroupFluidCount(float getNum)
    {
        float SendFluid = (groupSaveFluidNum + getNum) / pipeList.Count;
        groupSaveFluidNum += getNum;

        if (groupFullFluidNum <= groupSaveFluidNum)
        {
            groupIsFull = true;
            groupSaveFluidNum = groupFullFluidNum;
        }
        else if (groupFullFluidNum > groupSaveFluidNum)
        {
            groupIsFull = false;
        }

        foreach (PipeCtrl pipe in pipeList)
        {
            pipe.saveFluidNum = SendFluid;
            if (pipe.saveFluidNum >= pipe.fluidFactoryData.FullFluidNum)
                pipe.fluidIsFull = true;
            else if (pipe.saveFluidNum < pipe.fluidFactoryData.FullFluidNum)
                pipe.fluidIsFull = false;
        }
    }

    public void FactoryListAdd(GameObject facroty)
    {
        factoryList.Add(facroty);
    }

    void SendFluid()
    {
        foreach (GameObject obj in factoryList)
        {
            if (obj.GetComponent<FluidFactoryCtrl>() && obj.GetComponent<FluidFactoryCtrl>().fluidIsFull == false)
            {
                FluidFactoryCtrl fluidFactory = obj.GetComponent<FluidFactoryCtrl>();

                if (fluidFactory.saveFluidNum < pipeList[0].saveFluidNum)
                {
                    fluidFactory.SendFluidFunc(sendFluid);
                    groupSaveFluidNum -= sendFluid;
                }

                GroupFluidCount(0);
            }            
        }
    }
    void GetFluid()
    {
        foreach (GameObject obj in factoryList)
        {
            if (obj.GetComponent<FluidFactoryCtrl>())
            {
                FluidFactoryCtrl fluidFactory = obj.GetComponent<FluidFactoryCtrl>();
                if (fluidFactory.fluidIsFull == true && groupIsFull == false)
                {
                    fluidFactory.GetFluidFunc(sendFluid);
                    groupSaveFluidNum += sendFluid;
                }
            }
        }
    }
}
