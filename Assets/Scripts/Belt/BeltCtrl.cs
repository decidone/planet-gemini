using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum BeltState
{
    SoloBelt,
    StartBelt,
    EndBelt,
    RepeaterBelt
}
public class BeltCtrl : SolidFactoryCtrl
{
    [SerializeField]
    int modelNum = 0;  // 모션

    public BeltGroupMgr beltGroupMgr;
    GameObject beltManager = null;

    protected Animator anim;
    protected Animator animsync;

    public BeltState beltState;

    public bool isTurn = false;
    public bool isRightTurn = true;

    public BeltCtrl nextBelt;
    public BeltCtrl preBelt;

    Vector2[] nextPos = new Vector2[3];

    public bool isItemStop = false;

    bool isUp = false;
    bool isRight = false;
    bool isDown = false;
    bool isLeft = false;

    void Start()
    {
        dirCount = 4;
        beltManager = GameObject.Find("BeltManager");
        animsync = beltManager.GetComponent<Animator>();
        anim = GetComponent<Animator>();
        beltState = BeltState.SoloBelt;

        //if (transform.parent.gameObject != null)
        //    beltGroupMgr = GetComponentInParent<BeltGroupMgr>();

        //if (preBelt != null)
        BeltModelSet();
    }

    protected override void Update()
    {
        base.Update();

        if (!removeState)
        {
            anim.SetFloat("DirNum", dirNum);
            anim.SetFloat("ModelNum", modelNum);

            anim.Play(0, -1, animsync.GetCurrentAnimatorStateInfo(0).normalizedTime);
            ModelSet();
        }
    }

    private void FixedUpdate()
    {
        if (itemObjList.Count > 0)
            itemMove();
        else if(itemObjList.Count == 0 && isItemStop == true)
            isItemStop = false;
    }

    void ModelSet()
    {
        if (isTurn == false)
        {
            if (beltState == BeltState.SoloBelt)
            {
                modelNum = 0;
            }
            else if (beltState == BeltState.StartBelt)
            {
                modelNum = 1;
            }
            else if (beltState == BeltState.EndBelt)
            {
                modelNum = 3;
            }
            else if (beltState == BeltState.RepeaterBelt)
            {
                modelNum = 2;
            }
        }
        else if (isTurn == true)
        {
            if (isRightTurn == true)
            {
                modelNum = 5;
            }
            else if (isRightTurn == false)
            {
                modelNum = 4;
            }
        }
        SetDirNum();
    }

    protected override void SetDirNum()
    {
        for (int a = 0; a < nextPos.Length; a++)
        {
            nextPos[a] = this.transform.position;
        }

        if (dirNum == 0)
        {
            if (modelNum != 4 && modelNum != 5)
            {
                nextPos[0] += Vector2.up * 0.34f;
                nextPos[2] += Vector2.down * 0.34f;
            }
            else if (modelNum == 4)
            {
                nextPos[0] += Vector2.up * 0.34f;
                nextPos[1] += Vector2.up * 0.1f;
                nextPos[2] += Vector2.left * 0.34f;
                nextPos[2] += Vector2.up * 0.1f;
            }
            else if (modelNum == 5)
            {
                nextPos[0] += Vector2.up * 0.34f;
                nextPos[1] += Vector2.up * 0.1f;
                nextPos[2] += Vector2.right * 0.34f;
                nextPos[2] += Vector2.up * 0.1f;
            }
        }
        else if (dirNum == 1)
        {
            if (modelNum != 4 && modelNum != 5)
            {
                nextPos[0] += Vector2.right * 0.34f;
                nextPos[2] += Vector2.left * 0.34f;
                for (int a = 0; a < nextPos.Length; a++)
                {
                    nextPos[a] += Vector2.up * 0.1f;
                }
            }
            else if (modelNum == 4)
            {
                nextPos[0] += Vector2.right * 0.34f;
                nextPos[0] += Vector2.up * 0.1f;
                nextPos[1] += Vector2.up * 0.1f;
                nextPos[2] += Vector2.up * 0.34f;
            }
            else if (modelNum == 5)
            {
                nextPos[0] += Vector2.right * 0.34f;
                nextPos[0] += Vector2.up * 0.1f;
                nextPos[1] += Vector2.up * 0.1f;
                nextPos[2] += Vector2.down * 0.34f;
            }
        }
        else if (dirNum == 2)
        {
            if (modelNum != 4 && modelNum != 5)
            {
                nextPos[0] += Vector2.down * 0.34f;
                nextPos[2] += Vector2.up * 0.34f;
            }
            else if (modelNum == 4)
            {
                nextPos[0] += Vector2.down * 0.34f;
                nextPos[1] += Vector2.up * 0.1f;
                nextPos[2] += Vector2.right * 0.34f;
                nextPos[2] += Vector2.up * 0.1f;
            }
            else if (modelNum == 5)
            {
                nextPos[0] += Vector2.down * 0.34f;
                nextPos[1] += Vector2.up * 0.1f;
                nextPos[2] += Vector2.left * 0.34f;
                nextPos[2] += Vector2.up * 0.1f;
            }
        }
        else if (dirNum == 3)
        {
            if (modelNum != 4 && modelNum != 5)
            {
                nextPos[0] += Vector2.left * 0.34f;
                nextPos[2] += Vector2.right * 0.34f;
                for (int a = 0; a < nextPos.Length; a++)
                {
                    nextPos[a] += Vector2.up * 0.1f;
                }
            }
            else if (modelNum == 4)
            {
                nextPos[0] += Vector2.left * 0.34f;
                nextPos[0] += Vector2.up * 0.1f;
                nextPos[1] += Vector2.up * 0.1f;
                nextPos[2] += Vector2.down * 0.34f;
            }
            else if (modelNum == 5)
            {
                nextPos[0] += Vector2.left * 0.34f;
                nextPos[0] += Vector2.up * 0.1f;
                nextPos[1] += Vector2.up * 0.1f;
                nextPos[2] += Vector2.up * 0.34f;
            }
        }
    }

    void itemMove()
    {
        for (int i = 0; i < itemObjList.Count; i++)
        {
            itemObjList[i].transform.position = Vector3.MoveTowards(itemObjList[i].transform.position, nextPos[i], Time.deltaTime * solidFactoryData.SendSpeed[level]);
        }

        if (Vector2.Distance(itemObjList[0].transform.position, nextPos[0]) < 0.001f)
        {
            isItemStop = true;
        }
        else
        {
            isItemStop = false;
        }

        ItemSend();
    }

    void AddNewItem(ItemProps newItem)
    {
        // 새로운 아이템을 리스트에 추가합니다.
        itemObjList.Add(newItem);

        // 새로운 아이템의 위치를 가져옵니다.
        Vector3 newItemPos = newItem.transform.position;

        // 새로운 아이템이 들어갈 위치를 찾습니다.
        int insertIndex = -1;
        float minDist = 1.0f;
        for (int i = 0; i < itemObjList.Count - 1; i++)
        {
            float dist = Vector3.Distance(newItemPos, itemObjList[i].transform.position);
            if (dist < minDist)
            {
                insertIndex = i;
                minDist = dist;
            }
        }

        // 새로운 아이템을 리스트에서 제거하고, insertIndex에 다시 추가합니다.
        itemObjList.Remove(newItem);
        itemObjList.Insert(insertIndex, newItem);

        //// 아이템의 위치를 다시 설정합니다.
        //for (int i = 0; i < itemObjList.Count; i++)
        //{
        //    itemObjList[i].transform.position = nextPos[i];
        //}
    }

    void ItemSend()
    {
        if (nextBelt != null && beltState != BeltState.EndBelt)
        {
            if (nextBelt.isFull == false && itemObjList.Count > 0)
            {
                Vector2 fstItemPos = itemObjList[0].transform.position;
                if (fstItemPos == nextPos[0])
                {
                    nextBelt.BeltGroupSendItem(itemObjList[0]);
                    itemObjList.Remove(itemObjList[0]);
                    ItemNumCheck(); 
                }
            }
        }
    }

    public void BeltModelSet()
    {
        if (preBelt == null)
        {
            isTurn = false;
        }
        else if (preBelt.dirNum != dirNum)
        {
            isTurn = true;
            if (preBelt.dirNum == 0)
            {
                if (dirNum == 1)
                {
                    isRightTurn = true;
                }
                else if (dirNum == 3)
                {
                    isRightTurn = false;
                }
            }
            else if (preBelt.dirNum == 1)
            {
                if (dirNum == 2)
                {
                    isRightTurn = true;
                }
                else if (dirNum == 0)
                {
                    isRightTurn = false;
                }
            }
            else if (preBelt.dirNum == 2)
            {
                if (dirNum == 3)
                {
                    isRightTurn = true;
                }
                else if (dirNum == 1)
                {
                    isRightTurn = false;
                }
            }
            else if (preBelt.dirNum == 3)
            {
                if (dirNum == 0)
                {
                    isRightTurn = true;
                }
                else if (dirNum == 2)
                {
                    isRightTurn = false;
                }
            }
        }
        else if(preBelt.dirNum == dirNum)
        {
            isTurn = false;
        }
    }

    public void PreBeltCombinModelSet(int tempDir)
    {
        isTurn = true;

        if ((tempDir == 0 && dirNum == 1) || (tempDir == 2 && dirNum == 3) ||
            (tempDir == 1 && dirNum == 2) || (tempDir == 3 && dirNum == 0))
        {
            isRightTurn = true;
        }
        else if ((tempDir == 0 && dirNum == 3) || (tempDir == 2 && dirNum == 1) ||
            (tempDir == 1 && dirNum == 0) || (tempDir == 3 && dirNum == 2))
        {
            isRightTurn = false;
        }
    }

    public void FactoryVecCheck(Structure factory)
    {
        if (factory.transform.position.x > this.transform.position.x)  
            isLeft = true;        
        else if (factory.transform.position.x < this.transform.position.x)
            isRight = true;        
        else if (factory.transform.position.y - 0.1f > this.transform.position.y)
            isDown = true;        
        else if (factory.transform.position.y - 0.1f < this.transform.position.y)
            isUp = true;
        
        Invoke("FactoryModelSet", 0.1f);
    }

    void FactoryModelSet()
    {
        
        if (isUp == true && isRight == false && isDown == false && isLeft == false)
        {
            if (dirNum == 1)
            {
                isTurn = true;
                isRightTurn = true;
            }
            else if (dirNum == 3)
            {
                isTurn = true;
                isRightTurn = false;
            }
        }
        else if (isUp == false && isRight == true && isDown == false && isLeft == false)
        {
            if (dirNum == 0)
            {
                isTurn = true;
                isRightTurn = false;
            }
            else if (dirNum == 2)
            {
                isTurn = true;
                isRightTurn = true;
            }            
        }
        else if (isUp == false && isRight == false && isDown == true && isLeft == false)
        {
            if (dirNum == 1)
            {
                isTurn = true;
                isRightTurn = false;
            }
            else if (dirNum == 3)
            {
                isTurn = true;
                isRightTurn = true;
            }
        }
        else if (isUp == false && isRight == false && isDown == false && isLeft == true)
        {
            if (dirNum == 0)
            {
                isTurn = true;
                isRightTurn = true;
            }
            else if (dirNum == 2)
            {
                isTurn = true;
                isRightTurn = false;
            }
        }
        else        
            isTurn = false;
    }
}
