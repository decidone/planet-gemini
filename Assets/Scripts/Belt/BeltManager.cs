using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
        List<BeltCtrl> result = new List<BeltCtrl>();

        //ÇÕÄ¡±â
        result.AddRange(fstGroupMgr.BeltList);
        result.AddRange(secGroupMgr.BeltList);

        fstGroupMgr.BeltList.Clear();
        fstGroupMgr.BeltList = result.Distinct().ToList();

        foreach (BeltCtrl belt in secGroupMgr.BeltList)
        {
            belt.transform.parent = fstGroupMgr.transform;
            belt.beltGroupMgr = fstGroupMgr;
        }

        fstGroupMgr.Reconfirm();
        Destroy(secGroupMgr);
    }
}
