using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum PipeState
{
    SoloPipe,
    StartPipe,
    EndPipe,
    RepeaterPipe
}
public class PipeCtrl : FluidCtrl
{
    public PipeGroupMgr pipeGroupMgr;

    public PipeState pipeState;

    public PipeCtrl nextPipe;
    public PipeCtrl prePipe;

    // Start is called before the first frame update
    void Start()
    {
        pipeState = PipeState.SoloPipe;
        if (transform.parent.gameObject != null)
            pipeGroupMgr = GetComponentInParent<PipeGroupMgr>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
