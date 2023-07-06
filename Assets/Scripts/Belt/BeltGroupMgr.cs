using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeltGroupMgr : MonoBehaviour
{
    [SerializeField]
    GameObject beltObj = null;

    public List<BeltCtrl> beltList = new List<BeltCtrl>();
    public List<ItemProps> groupItem = new List<ItemProps>();

    public GameObject nextObj = null;
    bool nextCheck = true;

    public bool isPreBuilding = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!isPreBuilding)
        {
            if(nextCheck == true)
            {
                if(beltList.Count > 0)
                    nextObj = NextObjCheck();        
            }
        }
    }

    public void SetBelt(int beltDir)
    {
        GameObject belt = Instantiate(beltObj, this.transform.position, Quaternion.identity);
        belt.transform.parent = this.transform;
        BeltCtrl beltCtrl = belt.GetComponent<BeltCtrl>();
        beltCtrl.beltGroupMgr = this.GetComponent<BeltGroupMgr>();
        beltList.Add(beltCtrl);
        beltCtrl.dirNum = beltDir;
        beltCtrl.beltState = BeltState.SoloBelt;
    }
    void BeltModelSet(BeltCtrl preBelt, BeltCtrl nextBelt)
    {
        if(preBelt == beltList[0])
            preBelt.beltState = BeltState.StartBelt;
        else if (preBelt != beltList[0])        
            preBelt.beltState = BeltState.RepeaterBelt;

        nextBelt.beltState = BeltState.EndBelt;
    }

    public void Reconfirm()
    {
        int index = 0;
        foreach(BeltCtrl belt in beltList)
        {
            if (beltList.Count - 1 > index)
            {
                BeltModelSet(belt, beltList[index + 1]);
                index++;
            }
            else
                return;
        }
    }

    private GameObject NextObjCheck()
    {
        var Check = transform.up;

        BeltCtrl belt = beltList[beltList.Count - 1].GetComponent<BeltCtrl>();
        if (belt.dirNum == 0)
        {
            Check = belt.transform.up;
        }
        else if (belt.dirNum == 1)
        {
            Check = belt.transform.right;
        }
        else if (belt.dirNum == 2)
        {
            Check = -belt.transform.up;
        }
        else if (belt.dirNum == 3)
        {
            Check = -belt.transform.right;
        }

        RaycastHit2D[] raycastHits = Physics2D.RaycastAll(belt.transform.position, Check, 1f);

        for (int a = 0; a < raycastHits.Length; a++)
        {
            Collider2D collider = raycastHits[a].collider;

            if (collider.CompareTag("Factory") && !collider.GetComponent<Structure>().isPreBuilding && 
                collider.GetComponent<BeltCtrl>() != belt)
            {
                if (collider.GetComponent<BeltCtrl>() != null)
                {
                    CheckGroup(belt, collider.GetComponent<BeltCtrl>());
                }
                else
                {
                    nextCheck = false;
                }

                return collider.gameObject;
            }
        }

        return null;
    }

    void CheckGroup(BeltCtrl belt, BeltCtrl nextBelt)
    {
        BeltGroupMgr beltGroupMgr = this.GetComponent<BeltGroupMgr>();

        if (nextBelt.beltGroupMgr != null && beltGroupMgr != nextBelt.beltGroupMgr)
        {
            if (nextBelt.beltState == BeltState.StartBelt || nextBelt.beltState == BeltState.SoloBelt)
            {
                if (belt.dirNum == nextBelt.dirNum)                
                    CombineFunc(beltGroupMgr, belt, nextBelt);
                
                else if (belt.dirNum != nextBelt.dirNum)
                {
                    if (belt.dirNum % 2 == 0)
                    {
                        if (nextBelt.dirNum % 2 == 1)                        
                            CombineFunc(beltGroupMgr, belt, nextBelt);                        
                        else
                            return;
                    }
                    else if (belt.dirNum % 2 == 1)
                    {
                        if (nextBelt.dirNum % 2 == 0)                        
                            CombineFunc(beltGroupMgr, belt, nextBelt);                        
                        else
                            return;
                    }
                }
            }
        }
    }

    void CombineFunc(BeltGroupMgr beltGroupMgr, BeltCtrl belt, BeltCtrl nextBelt)
    {
        BeltManager beltManager = this.GetComponentInParent<BeltManager>();

        beltManager.BeltCombine(beltGroupMgr, nextBelt.beltGroupMgr);
        belt.nextBelt = nextBelt;
        nextBelt.preBelt = belt;
        nextBelt.BeltModelSet();
    }
}
