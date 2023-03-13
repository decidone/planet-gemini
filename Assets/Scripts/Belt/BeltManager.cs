using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeltManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BeltCombine(BeltGroupMgr fstGroupMgr, BeltGroupMgr secGroupMgr)
    {
        fstGroupMgr.BeltList.AddRange(secGroupMgr.BeltList);

        foreach (BeltCtrl belt in secGroupMgr.BeltList)
        {
            belt.transform.parent = fstGroupMgr.transform;
            belt.beltGroupMgr = fstGroupMgr;
        }

        fstGroupMgr.Reconfirm();
        Destroy(secGroupMgr.gameObject);
    }
}
