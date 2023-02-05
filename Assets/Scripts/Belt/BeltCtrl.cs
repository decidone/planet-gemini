using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum BeltState
{
    SoloBelt,
    StartBelt,
    EndBelt,
    RepeaterBelt,
}
public class BeltCtrl : FactoryCtrl
{
    public int modelNum = 0;  // ¸ð¼Ç

    public BeltGroupMgr beltGroupMgr;
    GameObject beltManager = null;

    protected Animator anim;
    protected Animator animsync;

    public BeltState beltState;

    public bool isTurn = false;
    public bool isRightTurn = true;

    public float beltSpeed = 3f;

    public BeltCtrl nextBelt;
    public BeltCtrl preBelt;

    Vector2[] nextPos = new Vector2[3];

    public bool isItemStop = false;

    // Start is called before the first frame update
    private void Awake()
    {
        beltManager = GameObject.Find("BeltManager");
        animsync = beltManager.GetComponent<Animator>();
        anim = GetComponent<Animator>();
        beltState = BeltState.SoloBelt;
    }
    void Start()
    {
        if (transform.parent.gameObject != null)
            beltGroupMgr = GetComponentInParent<BeltGroupMgr>();

        if (preBelt != null)
            BeltModelSet();
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetFloat("DirNum", dirNum);
        anim.SetFloat("ModelNum", modelNum);

        anim.Play(0, -1, animsync.GetCurrentAnimatorStateInfo(0).normalizedTime);
        ModelSet();
    }

    private void FixedUpdate()
    {
        if (itemList.Count > 0)
            itemMove();
        else if(itemList.Count == 0 && isItemStop == true)
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
        SetPos();
    }

    void SetPos()
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
        if (itemList.Count == 1)
        {
            itemList[0].transform.position = Vector3.MoveTowards(itemList[0].transform.position, nextPos[0], Time.deltaTime * beltSpeed);
        }
        else if (itemList.Count == 2)
        {
            itemList[0].transform.position = Vector3.MoveTowards(itemList[0].transform.position, nextPos[0], Time.deltaTime * beltSpeed);
            itemList[1].transform.position = Vector3.MoveTowards(itemList[1].transform.position, nextPos[1], Time.deltaTime * beltSpeed);
        }
        else if(itemList.Count == 3)
        {
            itemList[0].transform.position = Vector3.MoveTowards(itemList[0].transform.position, nextPos[0], Time.deltaTime * beltSpeed);
            itemList[1].transform.position = Vector3.MoveTowards(itemList[1].transform.position, nextPos[1], Time.deltaTime * beltSpeed);
            itemList[2].transform.position = Vector3.MoveTowards(itemList[2].transform.position, nextPos[2], Time.deltaTime * beltSpeed);
        }

        Vector2 fstItemPos = itemList[0].transform.position;

        if (fstItemPos == nextPos[0])
        {
            isItemStop = true;
        }
        else if (fstItemPos != nextPos[0])
        {
            isItemStop = false;
        }

        ItemSend();
    }

    void ItemSend()
    {
        if (nextBelt != null && beltState != BeltState.EndBelt)
        {
            if (nextBelt.isFull == false && itemList.Count > 0)
            {
                Vector2 fstItemPos = itemList[0].transform.position;
                if (fstItemPos == nextPos[0])
                {
                    nextBelt.AddItem(itemList[0]);
                    itemList.Remove(itemList[0]);
                    ItemNumCheck(); ;
                }
            }
        }
    }

    public void BeltModelSet()
    {
        if (preBelt.dirNum != dirNum)
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
    }
}
