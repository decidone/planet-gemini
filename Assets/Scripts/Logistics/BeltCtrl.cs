using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Netcode;
using Pathfinding;

// UTF-8 설정
public enum BeltState
{
    SoloBelt,
    StartBelt,
    EndBelt,
    RepeaterBelt
}

public class BeltCtrl : LogisticsCtrl
{
    int modelMotion = 0;  // 모션
    int preMotion = -1;
    public BeltGroupMgr beltGroupMgr;
    GameObject beltManager = null;

    protected Animator anim;
    protected Animator animsync;

    public BeltState beltState;

    bool isTurn = false;
    bool isRightTurn = true;

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
        beltManager = GameObject.Find("BeltManager");
        beltGroupMgr = GetComponentInParent<BeltGroupMgr>();
        animsync = beltManager.GetComponent<Animator>();
        anim = GetComponent<Animator>();
        BeltModelSet();
    }

    protected override void Update()
    {
        base.Update();

        if (!removeState)
        {
            anim.SetFloat("DirNum", dirNum);
            anim.SetFloat("ModelNum", modelMotion);
            anim.SetFloat("Level", level);

            anim.Play(0, -1, animsync.GetCurrentAnimatorStateInfo(0).normalizedTime);
            //if (IsServer)
                ModelSet();
        }
    }

    private void FixedUpdate()
    {
        //if (!IsServer)
        //    return;

        if (itemObjList.Count > 0)
            ItemMove();
        else if(itemObjList.Count == 0 && isItemStop)
            isItemStop = false;
    }

    void ModelSet()
    {
        if (!isTurn)
        {
            if (beltState == BeltState.SoloBelt)
            {
                modelMotion = 0;
            }
            else if (beltState == BeltState.StartBelt)
            {
                modelMotion = 1;
            }
            else if (beltState == BeltState.EndBelt)
            {
                modelMotion = 3;
            }
            else if (beltState == BeltState.RepeaterBelt)
            {
                modelMotion = 2;
            }
        }
        else if (isTurn)
        {
            if (isRightTurn)
            {
                modelMotion = 5;
            }
            else if (!isRightTurn)
            {
                modelMotion = 4;
            }
        }

        if (preMotion != modelMotion)
        {
            BeltModelMotionSetClientRpc(modelMotion);
            preMotion = modelMotion;
        }

        SetItemDir();
    }

    protected void SetItemDir()
    {
        for (int a = 0; a < nextPos.Length; a++)
        {
            nextPos[a] = this.transform.position;
        }

        if (dirNum == 0)
        {
            if (modelMotion != 4 && modelMotion != 5)
            {
                nextPos[0] += Vector2.up * 0.34f;
                nextPos[2] += Vector2.down * 0.34f;
            }
            else if (modelMotion == 4)
            {
                nextPos[0] += Vector2.up * 0.34f;
                nextPos[1] += Vector2.up * 0.1f;
                nextPos[2] += Vector2.left * 0.34f;
                nextPos[2] += Vector2.up * 0.1f;
            }
            else if (modelMotion == 5)
            {
                nextPos[0] += Vector2.up * 0.34f;
                nextPos[1] += Vector2.up * 0.1f;
                nextPos[2] += Vector2.right * 0.34f;
                nextPos[2] += Vector2.up * 0.1f;
            }
        }
        else if (dirNum == 1)
        {
            if (modelMotion != 4 && modelMotion != 5)
            {
                nextPos[0] += Vector2.right * 0.34f;
                nextPos[2] += Vector2.left * 0.34f;
                for (int a = 0; a < nextPos.Length; a++)
                {
                    nextPos[a] += Vector2.up * 0.1f;
                }
            }
            else if (modelMotion == 4)
            {
                nextPos[0] += Vector2.right * 0.34f;
                nextPos[0] += Vector2.up * 0.1f;
                nextPos[1] += Vector2.up * 0.1f;
                nextPos[2] += Vector2.up * 0.34f;
            }
            else if (modelMotion == 5)
            {
                nextPos[0] += Vector2.right * 0.34f;
                nextPos[0] += Vector2.up * 0.1f;
                nextPos[1] += Vector2.up * 0.1f;
                nextPos[2] += Vector2.down * 0.34f;
            }
        }
        else if (dirNum == 2)
        {
            if (modelMotion != 4 && modelMotion != 5)
            {
                nextPos[0] += Vector2.down * 0.34f;
                nextPos[2] += Vector2.up * 0.34f;
            }
            else if (modelMotion == 4)
            {
                nextPos[0] += Vector2.down * 0.34f;
                nextPos[1] += Vector2.up * 0.1f;
                nextPos[2] += Vector2.right * 0.34f;
                nextPos[2] += Vector2.up * 0.1f;
            }
            else if (modelMotion == 5)
            {
                nextPos[0] += Vector2.down * 0.34f;
                nextPos[1] += Vector2.up * 0.1f;
                nextPos[2] += Vector2.left * 0.34f;
                nextPos[2] += Vector2.up * 0.1f;
            }
        }
        else if (dirNum == 3)
        {
            if (modelMotion != 4 && modelMotion != 5)
            {
                nextPos[0] += Vector2.left * 0.34f;
                nextPos[2] += Vector2.right * 0.34f;
                for (int a = 0; a < nextPos.Length; a++)
                {
                    nextPos[a] += Vector2.up * 0.1f;
                }
            }
            else if (modelMotion == 4)
            {
                nextPos[0] += Vector2.left * 0.34f;
                nextPos[0] += Vector2.up * 0.1f;
                nextPos[1] += Vector2.up * 0.1f;
                nextPos[2] += Vector2.down * 0.34f;
            }
            else if (modelMotion == 5)
            {
                nextPos[0] += Vector2.left * 0.34f;
                nextPos[0] += Vector2.up * 0.1f;
                nextPos[1] += Vector2.up * 0.1f;
                nextPos[2] += Vector2.up * 0.34f;
            }
        }
    }

    void ItemMove()
    {
        for (int i = 0; i < itemObjList.Count; i++)
        {
            itemObjList[i].transform.position = Vector3.MoveTowards(itemObjList[i].transform.position, nextPos[i], Time.deltaTime * structureData.SendSpeed[level]);
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
            if (!nextBelt.isFull && itemObjList.Count > 0)
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
            return;
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

    public void FactoryPosCheck(Structure factory)
    {
        float xDiff = factory.transform.position.x - this.transform.position.x;
        float yDiff = factory.transform.position.y - this.transform.position.y;

        if (factory.sizeOneByOne) 
        {
            if (xDiff > 0)
            {
                isLeft = true;
                nearObj[3] = factory.gameObject;
            }
            else if (xDiff < 0)
            {
                isRight = true;
                nearObj[1] = factory.gameObject;
            }
            else if (yDiff > 0)
            {
                isDown = true;
                nearObj[2] = factory.gameObject;
            }
            else if (yDiff < 0)
            {
                isUp = true;
                nearObj[0] = factory.gameObject;
            }
        }
        else
        {
            if (xDiff > 0)
            {
                if(Mathf.Abs(xDiff) > Mathf.Abs(yDiff))
                {
                    isLeft = true;
                    nearObj[3] = factory.gameObject;
                }
                else
                {
                    if (yDiff > 0) 
                    {
                        isDown = true;
                        nearObj[2] = factory.gameObject;
                    }
                    else
                    {
                        isUp = true;
                        nearObj[0] = factory.gameObject;
                    }
                }
            }
            else
            {
                if (Mathf.Abs(xDiff) > Mathf.Abs(yDiff))
                {
                    isRight = true;
                    nearObj[1] = factory.gameObject;
                }
                else
                {
                    if (yDiff > 0)
                    {
                        isDown = true;
                        nearObj[2] = factory.gameObject;
                    }
                    else
                    {
                        isUp = true;
                        nearObj[0] = factory.gameObject;
                    }
                }
            }
        }

        if (beltState == BeltState.StartBelt || beltState == BeltState.SoloBelt)
            Invoke(nameof(FactoryModelSet), 0.1f);
    }

    public void FactoryModelSet()
    {
        if (isUp && !isRight && !isDown && !isLeft)
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
        else if (!isUp && isRight && !isDown && !isLeft)
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
        else if (!isUp && !isRight && isDown && !isLeft)
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
        else if (!isUp && !isRight && !isDown && isLeft)
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

    public override void ResetCheckObj(GameObject game)
    {
        for (int i = 0; i < nearObj.Length; i++)
        {
            if (nearObj[i] != null && nearObj[i] == game)
            {
                nearObj[i] = null;
                if(i == 0)
                {
                    isUp = false;
                }
                else if (i == 1)
                {
                    isRight = false;
                }
                else if (i == 2)
                {
                    isDown = false;
                }
                else if (i == 3)
                {
                    isLeft = false;
                }
            }
        }
        BeltModelSet();
    }

    public List<ItemProps> PlayerRootItemCheck()
    {
        List<ItemProps> sendItemList = new List<ItemProps>(itemObjList);
        
        if (itemObjList.Count >= structureData.MaxItemStorageLimit)
            isFull = true;
        else
            isFull = false;

        return sendItemList;
    }

    public void PlayerRootFunc(ItemProps item)
    {
        if (itemObjList.Contains(item))
        {
            itemObjList.Remove(item);
            item.itemPool.Release(item.gameObject);
        }
    }

    public override Dictionary<Item, int> PopUpItemCheck()
    {
        if (itemObjList.Count > 0) 
        {
            Dictionary<Item, int> returnDic = new Dictionary<Item, int>();
            foreach (ItemProps itemProps in itemObjList)
            {
                Item item = itemProps.item;
                int amounts = itemProps.amount;

                if (!returnDic.ContainsKey(item))
                    returnDic.Add(item, amounts);
                else
                {
                    int currentValue = returnDic[item];
                    int newValue = currentValue + amounts;
                    returnDic[item] = newValue;
                }
            }
            return returnDic;
        }                
        else
            return null;
    }

    [ClientRpc]
    public override void SettingClientRpc(int _level, int _beltDir, int objHeight, int objWidth)
    {
        beltGroupMgr = GetComponentInParent<BeltGroupMgr>();
        beltGroupMgr.beltList.Add(this);
        level = _level;
        dirNum = _beltDir;
        height = objHeight;
        width = objWidth;
        beltState = BeltState.SoloBelt;
        SetBuild();
        ColliderTriggerOnOff(false);
        gameObject.AddComponent<DynamicGridObstacle>();
        myVision.SetActive(true);
    }

    [ClientRpc]
    public void BeltStateSetClientRpc(int beltStateNum)
    {
        beltState = (BeltState)beltStateNum;
    }

    [ClientRpc]
    public void BeltDirSetClientRpc(int num)
    {
        dirNum = num;
    }

    [ClientRpc]
    public void BeltModelMotionSetClientRpc(int modelNum)
    {
        modelMotion = modelNum;
    }
}
