using System.Collections;
using UnityEngine;
using Unity.Netcode;

// UTF-8 설정
public class SendUnderBeltCtrl : LogisticsCtrl
{
    void Start()
    {
        StrBuilt();
    }

    protected override void Update()
    {
        base.Update();
        if (!removeState)
        {
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
                }
            }
        } 
    }

    public void EndRenderer()
    {
        if (outObj.Count > 0)
        {
            outObj[0].TryGet(out GetUnderBeltCtrl get);
            get.EndRenderer();
        }
    }

    [ClientRpc]
    public override void UpgradeFuncClientRpc()
    {
        //base.UpgradeFuncClientRpc();
        UpgradeFunc();

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
            if (hitCollider.TryGetComponent(out GetUnderBeltCtrl getUnderBelt) && hitCollider.gameObject != this.gameObject)
            {
                if (getUnderBelt.dirNum == dirNum)
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

    protected override IEnumerator SetInObjCoroutine(Structure obj)
    {
        yield return new WaitForSeconds(0.1f);

        if (obj)
        {
            if (obj.TryGet<BeltCtrl>(out var belt))
            {
                if (belt.beltGroupMgr.nextObj != this)
                {
                    yield break;
                }
                belt.FactoryPosCheck(this);
            }
            inObj.Add(obj);
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

        if (!outObj[0].isFull)
        {
            SendFacDelay(outObj[0], item);
        }

        outObj[0].takeItemDelay = false;
        Invoke(nameof(DelaySetItem), sendDelay);
    }

    public void SetOutObj(Structure obj)
    {
        if (outObj.Count > 0)
        {
            outObj[0].Get<GetUnderBeltCtrl>().ResetInObj();
            outObj.Remove(outObj[0]);
        }
        nearObj[0] = obj;
        if (!outObj.Contains(obj))
            outObj.Add(obj);
    }

    [ClientRpc]
    public override void SettingClientRpc(int _level, int _beltDir, int objHeight, int objWidth, bool isHostMap, int index)
    {
        level = _level;
        dirNum = _beltDir;
        height = objHeight;
        width = objWidth;
        buildingIndex = index;
        isInHostMap = isHostMap;
        settingEndCheck = true;
        SetBuild();
        ColliderTriggerOnOff(true);

        if (col != null)
        {
            // 3. A* 그래프 업데이트 (해당 영역을 길막으로 인식시킴)
            Bounds b = col.bounds;
            AstarPath.active.UpdateGraphs(b);
        }
        //gameObject.AddComponent<DynamicGridObstacle>();
        myVision.SetActive(true);
        DataSet();

        if (energyUse)
        {
            GameObject TriggerObj = new GameObject("Trigger");
            CircleCollider2D coll = TriggerObj.AddComponent<CircleCollider2D>();
            coll.isTrigger = true;
            TriggerObj.transform.position = Vector3.zero;
            StartCoroutine(Move(TriggerObj));
        }
        soundManager.PlaySFX(gameObject, "structureSFX", "BuildingSound");
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
            if (outObj[0].isFull || outObj[0].takeItemDelay || outObj[0].destroyStart || outObj[0].isPreBuilding)
            {
                SendItemIndexSet();
                Invoke(nameof(ItemSetDelayReset), 0.05f);
                return;
            }
            else if (outObj[0].TryGet(out Production production))
            {
                Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
                if (!production.CanTakeItem(item))
                {
                    SendItemIndexSet();
                    Invoke(nameof(ItemSetDelayReset), 0.05f);
                    return;
                }
            }
            else if (!outObj[0].canTakeItem)
            {
                SendItemIndexSet();
                Invoke(nameof(ItemSetDelayReset), 0.05f);
                return;
            }
            outObj[0].takeItemDelay = true;
        }

        SendItemServerRpc(itemIndex, sendItemIndex);
        SendItemIndexSet();
    }

    public override void ColliderTriggerOnOff(bool isOn)
    {
        col.isTrigger = true;
    }

    public override void Focused()
    {
        if (outObj.Count > 0)
        {
            outObj[0].Get<GetUnderBeltCtrl>().StartRenderer();
        }
    }

    public override void DisableFocused()
    {
        if (outObj.Count > 0)
        {
            outObj[0].Get<GetUnderBeltCtrl>().EndRenderer();
        }
    }

    [ClientRpc]
    public override void RemoveObjClientRpc()
    {
        StopAllCoroutines();

        if (InfoUI.instance.str == this)
            InfoUI.instance.SetDefault();

        for (int i = 0; i < nearObj.Length; i++)
        {
            if (nearObj[i])
            {
                nearObj[i].ResetNearObj(this);
                if (nearObj[i].TryGet(out BeltCtrl belt))
                {
                    BeltGroupMgr beltGroup = belt.beltGroupMgr;
                    beltGroup.nextCheck = true;
                    beltGroup.preCheck = true;
                }
            }
        }

        EndRenderer();
        
        if (GameManager.instance.focusedStructure == this)
        {
            GameManager.instance.focusedStructure = null;
        }

        DestroyFuncServerRpc();
    }
}
