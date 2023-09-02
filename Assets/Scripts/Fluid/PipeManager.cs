using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// UTF-8 설정
public class PipeManager : MonoBehaviour
{
    public void PipeCombine(PipeGroupMgr fstGroupMgr, PipeGroupMgr secGroupMgr)
    {
        List<PipeCtrl> pipeCtrl = new List<PipeCtrl>();
        List<GameObject> gameObjects = new List<GameObject>();

        //합치기
        pipeCtrl.AddRange(fstGroupMgr.pipeList);
        pipeCtrl.AddRange(secGroupMgr.pipeList);

        gameObjects.AddRange(fstGroupMgr.outObj);
        gameObjects.AddRange(secGroupMgr.outObj);

        fstGroupMgr.pipeList.Clear();
        fstGroupMgr.pipeList = pipeCtrl.Distinct().ToList();
        
        fstGroupMgr.outObj.Clear();
        fstGroupMgr.outObj = gameObjects.Distinct().ToList();

        foreach (PipeCtrl pipe in secGroupMgr.pipeList)
        {
            pipe.transform.parent = fstGroupMgr.transform;
            pipe.pipeGroupMgr = fstGroupMgr;
        }
        fstGroupMgr.GroupCheck();

        Destroy(secGroupMgr.gameObject);
    }
}
