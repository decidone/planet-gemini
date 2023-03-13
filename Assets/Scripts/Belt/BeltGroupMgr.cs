using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeltGroupMgr : MonoBehaviour
{
    [SerializeField]
    GameObject BeltObj = null;

    public bool up = false;
    public bool down = false;
    public bool left = false;
    public bool right = false;
    
    public List<BeltCtrl> BeltList = new List<BeltCtrl>();
    public List<ItemProps> GroupItem = new List<ItemProps>();

    public GameObject nextObj = null;
    bool nextCheck = true;

    Vector2 nextPos;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(up == true)
        {
            SetBelt(0);
            up = false;
        }
        else if (down == true)
        {
            SetBelt(2);
            down = false;
        }
        else if (left == true)
        {
            SetBelt(3);
            left = false;
        }
        else if (right == true)
        {
            SetBelt(1);
            right = false;
        }

        if(nextCheck == true)
        {
            if(BeltList.Count > 0)
                nextObj = NextObjCheck();        
        }
    }

    void SetBelt(int beltDir)
    {
        if (BeltList.Count == 0)
        {
            GameObject belt = Instantiate(BeltObj, this.transform.position, Quaternion.identity);
            belt.transform.parent = this.transform;
            BeltCtrl beltCtrl = belt.GetComponent<BeltCtrl>();
            beltCtrl.beltGroupMgr = this.GetComponent<BeltGroupMgr>();
            BeltList.Add(beltCtrl);
            beltCtrl.dirNum = beltDir;
            beltCtrl.beltState = BeltState.SoloBelt;
        }
        else
        {
            BeltCtrl preBeltCtrl = BeltList[BeltList.Count - 1];
            CalculateNextPos(preBeltCtrl, beltDir, out Vector2 nextPos);

            if (nextPos == Vector2.zero)
            {
                return;
            }

            GameObject belt = Instantiate(BeltObj, nextPos, Quaternion.identity);
            belt.transform.parent = this.transform;
            BeltCtrl beltCtrl = belt.GetComponent<BeltCtrl>();
            beltCtrl.beltGroupMgr = this.GetComponent<BeltGroupMgr>();
            BeltList.Add(beltCtrl);
            beltCtrl.dirNum = beltDir;
            beltCtrl.preBelt = preBeltCtrl;
            BeltList[BeltList.Count - 2].nextBelt = beltCtrl;
            BeltModelSet(preBeltCtrl, beltCtrl);
        }
    }

    void CalculateNextPos(BeltCtrl preBeltCtrl, int beltDir, out Vector2 nextPos)
    {
        nextPos = Vector2.zero;

        switch (preBeltCtrl.dirNum)
        {
            case 0:
                if (beltDir == 0 || beltDir == 1 || beltDir == 3)
                {
                    nextPos = new Vector2(preBeltCtrl.transform.position.x, preBeltCtrl.transform.position.y + 1);
                }
                break;
            case 1:
                if (beltDir == 1 || beltDir == 0 || beltDir == 2)
                {
                    nextPos = new Vector2(preBeltCtrl.transform.position.x + 1, preBeltCtrl.transform.position.y);
                }
                break;
            case 2:
                if (beltDir == 2 || beltDir == 1 || beltDir == 3)
                {
                    nextPos = new Vector2(preBeltCtrl.transform.position.x, preBeltCtrl.transform.position.y - 1);
                }
                break;
            case 3:
                if (beltDir == 3 || beltDir == 0 || beltDir == 2)
                {
                    nextPos = new Vector2(preBeltCtrl.transform.position.x - 1, preBeltCtrl.transform.position.y);
                }
                break;
            default:
                break;
        }
    }
    void BeltModelSet(BeltCtrl preBelt, BeltCtrl nextBelt)
    {
        if(preBelt == BeltList[0])
            preBelt.beltState = BeltState.StartBelt;
        else if (preBelt != BeltList[0])        
            preBelt.beltState = BeltState.RepeaterBelt;

        nextBelt.beltState = BeltState.EndBelt;
    }

    public void Reconfirm()
    {
        int index = 0;
        foreach(BeltCtrl belt in BeltList)
        {
            if (BeltList.Count - 1 > index)
            {
                BeltModelSet(belt, BeltList[index + 1]);
                index++;
            }
            else
                return;
        }
    }

    private GameObject NextObjCheck()
    {
        var Check = transform.up;

        BeltCtrl belt = BeltList[BeltList.Count - 1].GetComponent<BeltCtrl>();
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

            if (collider.CompareTag("Factory") && collider.GetComponent<BeltCtrl>() != belt)
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
