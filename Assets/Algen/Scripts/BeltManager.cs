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

    public void BeltCombine(GameObject fstBelt, GameObject secBelt)
    {
        BeltGroupMgr fstGroupMgr = fstBelt.GetComponent<BeltGroupMgr>();
        BeltGroupMgr secGroupMgr = secBelt.GetComponent<BeltGroupMgr>();

        List<GameObject> result = new List<GameObject>();

        //??ġ??
        result.AddRange(fstGroupMgr.BeltList);
        result.AddRange(secGroupMgr.BeltList);

        fstGroupMgr.BeltList.Clear();
        fstGroupMgr.BeltList = result.Distinct().ToList();

        foreach (GameObject belt in secGroupMgr.BeltList)
        {
            belt.transform.parent = fstGroupMgr.transform;
            belt.GetComponent<BeltCtrl>().beltGroupMgr = fstBelt;
        }

        fstGroupMgr.Reconfirm();
        Destroy(secBelt);
    }
}
