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

    public BeltCtrl nextBelt;
    public ItemSpawner itemSpawner;

    public List<BeltItemCtrl> itemList = new List<BeltItemCtrl>();

    public bool isFull = false;
    Vector2[] nextPos = new Vector2[3];

    //bool restartItem = false;
    //Vector2 nextPos;
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

        if (nextBelt == null)
            nextBelt = NextBeltCheck();
        if (beltState == BeltState.SoloBelt || beltState == BeltState.EndBelt)
            if (nextBelt != null)
                CheckGroup();

        if (itemSpawner == null)
            itemSpawner = SpawnerCheck();

        //if (itemList.Count >= 3)
        //    isFull = true;

        //else if (itemList.Count < 3)
        //    isFull = false;

        //if (itemList.Count > 0 && nextBelt != null && restartItem == false)
        //{
        //    if (nextBelt.isFull == false)
        //    {
        //        restartItem = true;
        //        RestartItem();
        //    }
        //}

        //if (itemList.Count > 0 && nextBelt != null)
        //{
        //    //ItemStop();
        //}
    }

    private void FixedUpdate()
    {
        if(itemList.Count > 0)
            itemMove();
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

        RaycastHit2D hit = Physics2D.Raycast(this.gameObject.transform.position, nextCheck, 1, 1 << LayerMask.NameToLayer("Belt"));

        if (hit)
        {
            BeltCtrl belt = hit.collider.GetComponent<BeltCtrl>();

            if (belt != null)
            {
                return belt;
            }
        }

        return null;
    }
    private ItemSpawner SpawnerCheck()
    {
        var SpwanerCheck = transform.up;

        if (dirNum == 0)
        {
            SpwanerCheck = -transform.up;
        }
        else if (dirNum == 1)
        {
            SpwanerCheck = -transform.right;
        }
        else if (dirNum == 2)
        {
            SpwanerCheck = transform.up;
        }
        else if (dirNum == 3)
        {
            SpwanerCheck = transform.right;
        }

        RaycastHit2D hit = Physics2D.Raycast(this.gameObject.transform.position, SpwanerCheck, 1f);

        if (hit)
        {
            ItemSpawner spawner = hit.collider.GetComponent<ItemSpawner>();

            if (spawner != null)
            {
                if (beltState == BeltState.SoloBelt || beltState == BeltState.StartBelt)
                    spawner.AddBeltList(this.GetComponent<BeltCtrl>());
                return spawner;
            }
        }

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

    public void AddItem(BeltItemCtrl item)
    {
        itemList.Add(item);
        //item.isStop = false;
        if (itemList.Count >= 3)
            isFull = true;
        //ItemMove(item);
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
        SetNextBelt();
    }

    void SetNextBelt()
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
                    if (itemList.Count < 3)
                        isFull = false;
                }
            }
        }
    }

    //public void SetNextPos(BeltItemCtrl item)
    //{
    //    if (nextBelt != null && beltState != BeltState.EndBelt)
    //    {
    //        if (nextBelt.isFull == false)
    //        {
    //            isFull = false;
    //            Vector2 itemPos = item.transform.position;
    //            if (itemPos == nextPos[0])
    //            {
    //                nextBelt.AddItem(item);
    //                itemList.Remove(item);

    //                //if (itemList.Count < 3)
    //                //    isFull = false;

    //                //if (itemList.Count > 0)
    //                //{
    //                //    foreach (BeltItemCtrl nextitem in itemList)
    //                //        ItemMove(nextitem);
    //                //}

    //            }
    //            //RestartItem();

    //        }
    //        else if(nextBelt.isFull == true)
    //        {
    //            Vector2 itemPos = item.transform.position;
    //            if (itemPos == nextPos[0])
    //            {
    //                item.isStop = true;
    //            }
    //        }
    //    }
    //    else if (beltState == BeltState.EndBelt || beltState == BeltState.SoloBelt)
    //    {
    //        Vector2 itemPos = item.transform.position;
    //        if (itemPos == nextPos[0])
    //        {
    //            item.isStop = true;
    //        }
    //    }
    //}

    //public void ItemMove(BeltItemCtrl item)
    //{
    //    if (isTurn == true)
    //    {
    //        if (item == itemList[0])
    //        {
    //            item.GetPos(nextPos[1], this.GetComponent<BeltCtrl>());
    //            item.GetPos(nextPos[0], this.GetComponent<BeltCtrl>());
    //        }
    //        else if (item == itemList[1])
    //            item.GetPos(nextPos[1], this.GetComponent<BeltCtrl>());
    //        else if (item == itemList[2])
    //            item.GetPos(nextPos[2], this.GetComponent<BeltCtrl>());
    //    }
    //    else if (isTurn == false)
    //    {
    //        if (item == itemList[0])
    //            item.GetPos(nextPos[0], this.GetComponent<BeltCtrl>());
    //        else if (item == itemList[1])
    //            item.GetPos(nextPos[1], this.GetComponent<BeltCtrl>());
    //        else if (item == itemList[2])
    //            item.GetPos(nextPos[2], this.GetComponent<BeltCtrl>());
    //    }
    //}

    ////void RestartItem()
    ////{
    ////    for (int index = 0; index < itemList.Count; index++)
    ////    {
    ////        Vector2 itemPos = itemList[index].transform.position;
    ////        if (itemPos == nextPos[0])
    ////        {
    ////            StartCoroutine("ItemSort", index);
    ////        }
    ////    }
    ////}

    ////IEnumerator ItemSort(int index)
    ////{
    ////    yield return new WaitForSecondsRealtime(0.01f);

    ////    nextBelt.AddItem(itemList[index]);
    ////    itemList.Remove(itemList[index]);

    ////    if (itemList.Count < 3)
    ////        isFull = false;

    ////    foreach (BeltItemCtrl item in itemList)
    ////        ItemMove(item);

    ////    restartItem = false;
    ////}
}
