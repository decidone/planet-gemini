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

    public List<PipeCtrl> PipeList = new List<PipeCtrl>();

    public FluidCtrl nextFluidObj = null;
    public FactoryCtrl nextFacObj = null;
    bool nextCheck = true;

    Vector2 nextPos;

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
    }

    void SetPipe(int pipeDir)
    {
        if (PipeList.Count == 0)
        {
            GameObject pipe = Instantiate(PipeObj, this.transform.position, Quaternion.identity);
            pipe.transform.parent = this.transform;
            PipeCtrl pipeCtrl = pipe.GetComponent<PipeCtrl>();
            PipeList.Add(pipeCtrl);
            pipeCtrl.dirNum = pipeDir;
            pipeCtrl.pipeState = PipeState.SoloPipe;
        }
        else if (PipeList.Count != 0)
        {
            PipeCtrl prePipeCtrl = PipeList[PipeList.Count - 1];

            if (prePipeCtrl.dirNum == 0)
            {
                if (pipeDir == 0 || pipeDir == 1 || pipeDir == 3)
                {
                    nextPos = new Vector2(prePipeCtrl.transform.position.x, prePipeCtrl.transform.position.y + 1);
                }
                else if (pipeDir == 2)
                {
                    return;
                }
            }
            else if (prePipeCtrl.dirNum == 1)
            {
                if (pipeDir == 1 || pipeDir == 0 || pipeDir == 2)
                {
                    nextPos = new Vector2(prePipeCtrl.transform.position.x + 1, prePipeCtrl.transform.position.y);
                }
                else if (pipeDir == 3)
                {
                    return;
                }
            }
            else if (prePipeCtrl.dirNum == 2)
            {
                if (pipeDir == 2 || pipeDir == 1 || pipeDir == 3)
                {
                    nextPos = new Vector2(prePipeCtrl.transform.position.x, prePipeCtrl.transform.position.y - 1);
                }
                else if (pipeDir == 0)
                {
                    return;
                }
            }
            else if (prePipeCtrl.dirNum == 3)
            {
                if (pipeDir == 3 || pipeDir == 0 || pipeDir == 2)
                {
                    nextPos = new Vector2(prePipeCtrl.transform.position.x - 1, prePipeCtrl.transform.position.y);
                }
                else if (pipeDir == 1)
                {
                    return;
                }
            }
            GameObject pipe = Instantiate(PipeObj, nextPos, Quaternion.identity);
            pipe.transform.parent = this.transform;
            PipeCtrl pipeCtrl = pipe.GetComponent<PipeCtrl>();
            PipeList.Add(pipeCtrl);
            pipeCtrl.dirNum = pipeDir;
            pipeCtrl.prePipe = PipeList[PipeList.Count - 2];
            PipeList[PipeList.Count - 2].nextPipe = pipeCtrl;
        }
    }
}
