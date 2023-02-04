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

    public float beltSpeed = 3f;

    public GameObject nextObj;
    public GameObject preObj;

    public BeltCtrl nextBelt;
    //public ItemSpawner itemSpawner;

    public List<ItemProps> itemList = new List<ItemProps>();

    public bool isFull = false;
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
            beltGroupMgr = transform.parent.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetFloat("DirNum", dirNum);
        anim.SetFloat("ModelNum", modelNum);

        anim.Play(0, -1, animsync.GetCurrentAnimatorStateInfo(0).normalizedTime);
        ModelSet();

        if (beltState == BeltState.SoloBelt || beltState == BeltState.EndBelt)
        {
            if (nextBelt != null)
                CheckGroup();
        }
        if(beltState == BeltState.SoloBelt || beltState == BeltState.StartBelt)
        {
            if (preObj == null)
                preObj = PreObjCheck();
        }

        if (nextObj == null)
            nextObj = NextObjCheck();

        if (itemList.Count < 3)
            isFull = false;
        else if (itemList.Count >= 3)
            isFull = true;
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

    private GameObject NextObjCheck()
    {
        var Check = transform.up;

        if (dirNum == 0)
        {
            Check = transform.up;
        }
        else if (dirNum == 1)
        {
            Check = transform.right;
        }
        else if (dirNum == 2)
        {
            Check = -transform.up;
        }
        else if (dirNum == 3)
        {
            Check = -transform.right;
        }

        LayerMask mask = LayerMask.GetMask("Factory");
        RaycastHit2D[] raycastHits = Physics2D.RaycastAll(this.gameObject.transform.position, Check, 1f, mask);

        for (int a = 0; a < raycastHits.Length; a++)
        {

            if (raycastHits[a].collider.GetComponent<BeltCtrl>() != this.gameObject.GetComponent<BeltCtrl>())
            {
                Debug.Log(raycastHits[a].collider.name);

                if (raycastHits[a].collider.GetComponent<BeltCtrl>() != null)
                    nextBelt = raycastHits[a].collider.GetComponent<BeltCtrl>();

                return ObjCheck(raycastHits[a]);
            }
        }

        //RaycastHit2D hit = Physics2D.Raycast(this.gameObject.transform.position, Check, 1f);
        //if(hit)
        //{
        //    if (hit.collider.GetComponent<BeltCtrl>() != null)
        //        nextBelt = hit.collider.GetComponent<BeltCtrl>();

        //    return ObjCheck(hit);
        //}
        return null;
    }
    private GameObject PreObjCheck()
    {
        var Check = transform.up;

        if (dirNum == 0)
        {
            Check = -transform.up;
        }
        else if (dirNum == 1)
        {
            Check = -transform.right;
        }
        else if (dirNum == 2)
        {
            Check = transform.up;
        }
        else if (dirNum == 3)
        {
            Check = transform.right;
        }

        LayerMask mask = LayerMask.GetMask("Factory");
        RaycastHit2D[] raycastHits = Physics2D.RaycastAll(this.gameObject.transform.position, Check, 1f, mask);
            

        for (int a = 0; a < raycastHits.Length; a++)
        {
            Debug.Log(raycastHits[a].collider.name);

            if (raycastHits[a].collider.GetComponent<BeltCtrl>() != this.GetComponent<BeltCtrl>())
                return ObjCheck(raycastHits[a]);
        }
        //RaycastHit2D hit = Physics2D.Raycast(this.gameObject.transform.position, Check, 1f);

        //if (hit)
        //    return ObjCheck(hit); 

        return null;
    }

    GameObject ObjCheck(RaycastHit2D hit)
    {
        if (hit.collider.GetComponent<BeltCtrl>() != null)
        {
            BeltCtrl belt = hit.collider.GetComponent<BeltCtrl>();
            return hit.collider.gameObject;
        }
        else if (hit.collider.GetComponent<ItemSpawner>() != null)
        {
            if (beltState == BeltState.SoloBelt || beltState == BeltState.StartBelt)
                hit.collider.GetComponent<ItemSpawner>().AddBeltList(this.GetComponent<BeltCtrl>());
            return hit.collider.gameObject;
        }
        else if (hit.collider.GetComponent<MergerCtrl>() != null)
        {
            MergerCtrl merger = hit.collider.GetComponent<MergerCtrl>();
            merger.SetBelt(GetComponent<BeltCtrl>());

            return hit.collider.gameObject;
        }
        else if (hit.collider.GetComponent<SplitterCtrl>() != null)
        {
            SplitterCtrl splitter = hit.collider.GetComponent<SplitterCtrl>();
            splitter.SetBelt(GetComponent<BeltCtrl>());

            return hit.collider.gameObject;
        }
        else
            return null;
    }

    void CheckGroup()
    {
        if (nextBelt.beltGroupMgr != null && beltGroupMgr != nextBelt.beltGroupMgr)
        {
            if (nextBelt.beltState == BeltState.StartBelt || nextBelt.beltState == BeltState.SoloBelt)
            {
                if (dirNum == nextBelt.dirNum)
                {
                    beltManager.GetComponent<BeltManager>().BeltCombine(beltGroupMgr, nextBelt.beltGroupMgr);
                }
                else if (dirNum != nextBelt.dirNum)
                {
                    if (dirNum % 2 == 0)
                    {
                        if (nextBelt.dirNum % 2 == 1)
                            beltManager.GetComponent<BeltManager>().BeltCombine(beltGroupMgr, nextBelt.beltGroupMgr);
                        else
                            return;
                    }
                    else if (dirNum % 2 == 1)
                    {
                        if (nextBelt.dirNum % 2 == 0)
                            beltManager.GetComponent<BeltManager>().BeltCombine(beltGroupMgr, nextBelt.beltGroupMgr);
                        else
                            return;
                    }
                }
            }
        }
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

    public void AddItem(ItemProps item)
    {
        itemList.Add(item);
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
                }
            }
        }
    }
}
