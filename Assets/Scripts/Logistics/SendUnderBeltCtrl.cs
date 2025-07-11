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

            if (IsServer && !isPreBuilding && checkObj)
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

    public override void NearStrBuilt()
    {
        // 건물을 지었을 때나 근처에 새로운 건물이 지어졌을 때 동작
        // 변경사항이 생기면 DelayNearStrBuiltCoroutine()에도 반영해야 함
        if (IsServer)
        {
            CheckPos();
            if (nearObj[2] == null)
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
        if (nearObj[2] == null)
            CheckNearObj(checkPos[2], 2, obj => StartCoroutine(SetInObjCoroutine(obj)));
        setModel.sprite = modelNum[dirNum + (level * 4)];
    }

    protected override IEnumerator SetInObjCoroutine(GameObject obj)
    {
        checkObj = false;
        yield return new WaitForSeconds(0.1f);

        if (obj.GetComponent<Structure>() != null)
        {
            if (obj.TryGetComponent(out BeltCtrl belt))
            {
                if (belt.GetComponentInParent<BeltGroupMgr>().nextObj != this.gameObject)
                {
                    checkObj = true;
                    yield break;
                }
                belt.FactoryPosCheck(GetComponentInParent<Structure>());
            }
            inObj.Add(obj);
        }
        checkObj = true;
    }

    [ServerRpc]
    protected override void SendItemServerRpc(int itemIndex, int outObjIndex)
    {
        if (IsServer)
        {
            SendItemFunc(itemIndex, outObjIndex);
        }
        else if (settingEndCheck)
        {
            SendDelaySet(itemIndex, outObjIndex);
        }
    }

    protected override void SendItemFunc(int itemIndex, int outObjIndex)
    {
        Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);

        Structure outFactory = outObj[0].GetComponent<Structure>();

        if (!outFactory.isFull)
        {
            SendFacDelay(outObj[0], item);
        }

        Invoke(nameof(DelaySetItem), sendDelay);
    }

    public void SetOutObj(GameObject Obj)
    {
        if (outObj.Count > 0)
        {
            outObj[0].GetComponent<GetUnderBeltCtrl>().ResetInObj();
            outObj.Remove(outObj[0]);
        }
        nearObj[0] = Obj;
        outObj.Add(Obj);
    }

    protected override void SendItem(int itemIndex)
    {
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

            if (outFactory.isFull || outFactory.destroyStart || outFactory.isPreBuilding)
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
