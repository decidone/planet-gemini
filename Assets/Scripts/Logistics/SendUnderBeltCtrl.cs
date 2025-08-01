using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;

// UTF-8 설정
public class SendUnderBeltCtrl : LogisticsCtrl
{
    void Start()
    {
        //setModel = GetComponent<SpriteRenderer>();
        StrBuilt();
    }

    protected override void Update()
    {
        base.Update();
        if (!removeState)
        {
            //SetDirNum();
            //if (isSetBuildingOk && nearObj[2] == null)
            //    CheckNearObj(checkPos[2], 2, obj => StartCoroutine(SetInObjCoroutine(obj)));

            if (IsServer && !isPreBuilding)
            {
                if (inObj.Count > 0 && !isFull && !itemGetDelay)
                {
                    GetItem();
                }
                if (itemList.Count > 0 && outObj.Count > 0 && !itemSetDelay)
                {
                    int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(itemList[0]);
                    SendItem(itemIndex);
                    //SendItem(itemList[0]);
                }
            }

            if (DelaySendList.Count > 0 && outObj.Count > 0 && !outObj[DelaySendList[0].Item2].GetComponent<Structure>().isFull)
            {
                SendDelayFunc(DelaySendList[0].Item1, DelaySendList[0].Item2, 0);
            }
            if (DelayGetList.Count > 0 && inObj.Count > 0)
            {
                GetDelayFunc(DelayGetList[0], 0);
            }
        } 
    }

    public void EndRenderer()
    {
        if (outObj.Count > 0)
        {
            outObj[0].TryGetComponent(out GetUnderBeltCtrl get);
            get.EndRenderer();
        }
    }

    [ClientRpc]
    public override void UpgradeFuncClientRpc()
    {
        base.UpgradeFuncClientRpc();
        setModel.sprite = modelNum[dirNum + (level * 4)];
    }

    public override void StrBuilt()
    {
        base.StrBuilt();

        float dist = 10;

        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, checkPos[0], dist);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hitCollider = hits[i].collider;
            if (hitCollider.CompareTag("Factory") && hitCollider.gameObject != this.gameObject)
            {
                if (hitCollider.TryGetComponent(out GetUnderBeltCtrl getUnderBelt) && getUnderBelt.dirNum == dirNum)
                {
                    getUnderBelt.NearStrBuilt();
                    return;
                }
            }
        }
    }

    public override void NearStrBuilt()
    {
        // 건물을 지었을 때나 근처에 새로운 건물이 지어졌을 때 동작
        // 변경사항이 생기면 DelayNearStrBuiltCoroutine()에도 반영해야 함
        if (IsServer)
        {
            CheckPos();
            if (!nearObj[2])
                CheckNearObj(checkPos[2], 2, obj => StartCoroutine(SetInObjCoroutine(obj)));
            setModel.sprite = modelNum[dirNum + (level * 4)];
        }
        else
        {
            DelayNearStrBuilt();
        }
    }

    public override void DelayNearStrBuilt()
    {
        // 동시 건설, 클라이언트 동기화 등의 이유로 딜레이를 주고 NearStrBuilt()를 실행할 때 사용
        StartCoroutine(DelayNearStrBuiltCoroutine());
    }

    protected override IEnumerator DelayNearStrBuiltCoroutine()
    {
        // 동시 건설이나 그룹핑을 따로 예외처리 하는 경우가 아니면 NearStrBuilt()를 그대로 사용
        yield return new WaitForEndOfFrame();

        CheckPos();
        if (!nearObj[2])
            CheckNearObj(checkPos[2], 2, obj => StartCoroutine(SetInObjCoroutine(obj)));
        setModel.sprite = modelNum[dirNum + (level * 4)];
    }

    protected override IEnumerator SetInObjCoroutine(GameObject obj)
    {
        yield return new WaitForSeconds(0.1f);

        if (obj.GetComponent<Structure>() != null)
        {
            if (obj.TryGetComponent(out BeltCtrl belt))
            {
                if (belt.GetComponentInParent<BeltGroupMgr>().nextObj != this.gameObject)
                {
                    yield break;
                }
                belt.FactoryPosCheck(GetComponentInParent<Structure>());
            }
            inObj.Add(obj);
        }
    }

    void GetUnderBeltNearStrBuild(Vector2 direction)
    {
        float dist = 10;

        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, dist);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hitCollider = hits[i].collider;
            if (hitCollider.CompareTag("Factory") && hitCollider.GetComponent<SendUnderBeltCtrl>() != this)
            {
                if (hitCollider.TryGetComponent(out SendUnderBeltCtrl othGet) && othGet.dirNum == dirNum)
                {
                    return;
                }
                else if (hitCollider.TryGetComponent(out GetUnderBeltCtrl getUnderBeltCtrl))
                {
                    if (getUnderBeltCtrl.dirNum == dirNum)
                    {
                        getUnderBeltCtrl.NearStrBuilt();
                        return;
                    }
                }
            }
        }
    }

    [ServerRpc]
    protected override void SendItemServerRpc(int itemIndex, int outObjIndex)
    {
        SendItemClientRpc(itemIndex, outObjIndex);
    }

    protected override void SendItemFunc(int itemIndex, int outObjIndex)
    {
        Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);

        Structure outFactory = outObj[0].GetComponent<Structure>();

        if (!outFactory.isFull)
        {
            SendFacDelay(outObj[0], item);
        }

        outFactory.takeItemDelay = false;
        Invoke(nameof(DelaySetItem), sendDelay);
    }

    public void SetOutObj(GameObject obj)
    {
        if (outObj.Count > 0)
        {
            outObj[0].GetComponent<GetUnderBeltCtrl>().ResetInObj();
            outObj.Remove(outObj[0]);
        }
        nearObj[0] = obj;
        if (!outObj.Contains(obj))
            outObj.Add(obj);
    }

    protected override void SendItem(int itemIndex)
    {
        if (itemIndex < 0) return;

        itemSetDelay = true;

        if (outObj.Count <= sendItemIndex)
        {
            SendItemIndexSet();
            itemSetDelay = false;
            return;
        }
        else
        {
            Structure outFactory = outObj[sendItemIndex].GetComponent<Structure>();

            if (outFactory.isFull || outFactory.takeItemDelay || outFactory.destroyStart || outFactory.isPreBuilding)
            {
                SendItemIndexSet();
                itemSetDelay = false;
                return;
            }
            else if (outFactory.TryGetComponent(out Production production))
            {
                Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
                if (!production.CanTakeItem(item))
                {
                    SendItemIndexSet();
                    itemSetDelay = false;
                    return;
                }
            }
            else if (outFactory.isMainSource)
            {
                SendItemIndexSet();
                itemSetDelay = false;
                return;
            }
            outFactory.takeItemDelay = true;
        }

        SendItemServerRpc(itemIndex, sendItemIndex);
        SendItemIndexSet();
    }
}
