using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BeltGroupMgr : MonoBehaviour
{
    public GameObject BeltObj = null;

    public bool up = false;
    public bool down = false;
    public bool left = false;
    public bool right = false;
    
    public List<BeltCtrl> BeltList = new List<BeltCtrl>();
    public List<ItemProps> GroupItem = new List<ItemProps>();

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
    }

    void SetBelt(int beltDir)
    {
        if(BeltList.Count == 0)
        {
            GameObject belt = Instantiate(BeltObj, this.transform.position, Quaternion.identity);
            belt.transform.parent = this.transform;
            BeltCtrl beltCtrl = belt.GetComponent<BeltCtrl>();
            BeltList.Add(beltCtrl);
            beltCtrl.dirNum = beltDir;
            beltCtrl.beltState = BeltState.SoloBelt;
        }
        else if (BeltList.Count != 0)
        {
            BeltCtrl preBeltCtrl = BeltList[BeltList.Count - 1];

            if(preBeltCtrl.dirNum == 0)
            {
                if (beltDir == 0 || beltDir == 1 || beltDir == 3)
                {
                    nextPos = new Vector2(preBeltCtrl.transform.position.x , preBeltCtrl.transform.position.y + 1);
                }
                else if(beltDir == 2)
                {
                    return;
                }
            }
            else if (preBeltCtrl.dirNum == 1)
            {
                if (beltDir == 1 || beltDir == 0 || beltDir == 2)
                {
                    nextPos = new Vector2(preBeltCtrl.transform.position.x + 1, preBeltCtrl.transform.position.y);
                }
                else if (beltDir == 3)
                {
                    return;
                }
            }
            else if (preBeltCtrl.dirNum == 2)
            {
                if (beltDir == 2 || beltDir == 1 || beltDir == 3)
                {
                    nextPos = new Vector2(preBeltCtrl.transform.position.x, preBeltCtrl.transform.position.y - 1);
                }
                else if (beltDir == 0)
                {
                    return;
                }
            }
            else if (preBeltCtrl.dirNum == 3)
            {
                if (beltDir == 3 || beltDir == 0 || beltDir == 2)
                {
                    nextPos = new Vector2(preBeltCtrl.transform.position.x - 1, preBeltCtrl.transform.position.y );
                }
                else if (beltDir == 1)
                {
                    return;
                }
            }
            GameObject belt = Instantiate(BeltObj, nextPos, Quaternion.identity);
            belt.transform.parent = this.transform;
            BeltCtrl beltCtrl = belt.GetComponent<BeltCtrl>();
            BeltList.Add(beltCtrl);
            beltCtrl.dirNum = beltDir;
            BeltModelSet(preBeltCtrl, beltCtrl);
        }
    }

    void BeltModelSet(BeltCtrl preBelt, BeltCtrl nextBelt)
    {
        if(preBelt == BeltList[0])
            preBelt.beltState = BeltState.StartBelt;
        else if (preBelt != BeltList[0])        
            preBelt.beltState = BeltState.RepeaterBelt;

        if (preBelt.dirNum != nextBelt.dirNum)
        {
            BeltTurnCheck(preBelt, nextBelt);
        }

        nextBelt.beltState = BeltState.EndBelt;
    }

    void BeltTurnCheck(BeltCtrl preBelt, BeltCtrl nextBelt)
    {
        nextBelt.isTurn = true;
        if(preBelt.dirNum == 0)
        {
            if(nextBelt.dirNum == 1)
            {
                nextBelt.isRightTurn = true;
            }
            else if (nextBelt.dirNum == 3)
            {
                nextBelt.isRightTurn = false;
            }
        }
        else if (preBelt.dirNum == 1)
        {
            if (nextBelt.dirNum == 2)
            {
                nextBelt.isRightTurn = true;
            }
            else if (nextBelt.dirNum == 0)
            {
                nextBelt.isRightTurn = false;
            }
        }
        else if (preBelt.dirNum == 2)
        {
            if (nextBelt.dirNum == 3)
            {
                nextBelt.isRightTurn = true;
            }
            else if (nextBelt.dirNum == 1)
            {
                nextBelt.isRightTurn = false;
            }
        }
        else if (preBelt.dirNum == 3)
        {
            if (nextBelt.dirNum == 0)
            {
                nextBelt.isRightTurn = true;
            }
            else if (nextBelt.dirNum == 2)
            {
                nextBelt.isRightTurn = false;
            }
        }
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

    public void AddItem(ItemProps item)
    {
        GroupItem.Add(item);
    }
}
