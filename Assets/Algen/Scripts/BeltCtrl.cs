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
public class BeltCtrl : MonoBehaviour
{
    public int dirNum = 0;    // 방향
    public int modelNum = 0;  // 모션

    public GameObject beltGroupMgr;
    GameObject beltManager = null;

    protected Animator anim;
    protected Animator animsync;

    public BeltState beltState;

    public bool isTurn = false;
    public bool isRightTurn = true;
    
    public BeltCtrl beltInSequence;
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
            beltGroupMgr = transform.parent.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetFloat("DirNum", dirNum);
        anim.SetFloat("ModelNum", modelNum);

        anim.Play(0, -1, animsync.GetCurrentAnimatorStateInfo(0).normalizedTime);
        ModelSet();

        if (beltInSequence == null)
            beltInSequence = NextBeltCheck();

        else if(beltInSequence != null)
            CheckGroup();
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
            if(isRightTurn == true)
            {
                modelNum = 5;
            }
            else if (isRightTurn == false)
            {
                modelNum = 4;
            }
        }
    }

    private BeltCtrl NextBeltCheck() 
    {
        var nextCheck = transform.up;

        if (dirNum == 0)
        {
            nextCheck = transform.up;
        }
        else if (dirNum == 1)
        {
            nextCheck = transform.right;
        }
        else if (dirNum == 2)
        {
            nextCheck = -transform.up;
        }
        else if (dirNum == 3)
        {
            nextCheck = -transform.right;
        }

        RaycastHit2D hit = Physics2D.Raycast(this.gameObject.transform.position, nextCheck, 1f);

        if (hit)
        {
            Debug.Log(hit.collider.gameObject.name);
            BeltCtrl belt = hit.collider.GetComponent<BeltCtrl>();

            if (belt != null)
            {
                return belt;
            }
        }

        return null;
    }

    void CheckGroup()
    {// && dirNum == beltInSequence.dirNum
        if(beltInSequence.beltGroupMgr != null && beltGroupMgr != beltInSequence.beltGroupMgr)
        {
            if(dirNum % 2 == 0)
            {
                if (beltInSequence.dirNum % 2 == 1)
                    beltManager.GetComponent<BeltManager>().BeltCombine(beltGroupMgr, beltInSequence.beltGroupMgr);
                else
                    return;
            }
            else if (dirNum % 2 == 1)
            {
                if (beltInSequence.dirNum % 2 == 0)
                    beltManager.GetComponent<BeltManager>().BeltCombine(beltGroupMgr, beltInSequence.beltGroupMgr);
                else
                    return;
            }

        }

    }
}
