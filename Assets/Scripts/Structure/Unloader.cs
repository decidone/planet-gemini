using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class Unloader : LogisticsCtrl
{
    public Item selectItem;
    LogisticsClickEvent clickEvent;

    void Start()
    {
        //setModel = GetComponent<SpriteRenderer>();
        clickEvent = GetComponent<LogisticsClickEvent>();
        StrBuilt();
    }

    protected override void Update()
    {
        base.Update();

        if (!removeState)
        {
            //for (int i = 0; i < nearObj.Length; i++)
            //{
            //    if (nearObj[i] == null)
            //    {
            //        CheckNearObj(checkPos[i], i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
            //    }
            //}

            if (IsServer && !isPreBuilding)
            {
                if (inObj.Count > 0 && !isFull && !itemGetDelay)
                    GetItem();
                if (itemList.Count > 0 && outObj.Count > 0 && !itemSetDelay)
                {
                    int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(itemList[0]);
                    SendItem(itemIndex);
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
            for (int i = 0; i < nearObj.Length; i++)
            {
                if (nearObj[i] == null)
                {
                    CheckNearObj(checkPos[i], i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
                }
            }
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
        for (int i = 0; i < nearObj.Length; i++)
        {
            if (nearObj[i] == null)
            {
                CheckNearObj(checkPos[i], i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public override void ClientConnectSyncServerRpc()
    {
        //base.ClientConnectSyncServerRpc();
        ClientConnectSync();

        int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(selectItem);
        ClientFillterSetClientRpc(itemIndex);
    }

    [ClientRpc]
    void ClientFillterSetClientRpc(int itemIndex)
    {
        if (IsServer)
            return;

        if (itemIndex >= 0)
        {
            SelectItemSetClientRpc(itemIndex);
        }
    }

    protected override IEnumerator SetOutObjCoroutine(Structure obj)
    {
        yield return new WaitForSeconds(0.1f);

        if (!obj || !obj.canTakeItem)
            yield break;


        if (obj.isStorageBuilding)
        {
            inObj.Add(obj);
        }
        else
        {
            if (obj.TryGet<BeltCtrl>(out var belt))
            {
                if (belt.beltGroupMgr.nextObj == this)
                {
                    yield break;
                }
                belt.FactoryPosCheck(this);
            }
            else
            {
                outSameList.Add(obj);
                StartCoroutine(OutCheck(obj));
            }
            if (!outObj.Contains(obj))
                outObj.Add(obj);
            StartCoroutine(UnderBeltConnectCheck(obj));
        }        
    }

    protected override void GetItemFunc(int inObjIndex)
    {
        if (inObj[inObjIndex].TryGet(out Production production))
        {
            if (production.UnloadItemCheck(selectItem))
            {
                int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(selectItem);
                if (IsServer)
                {
                    OnFactoryItem(selectItem);
                    production.UnloadItem(selectItem);
                }
            }
            Invoke(nameof(DelayGetItem), getDelay);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SelectItemSetServerRpc(int itemIndex)
    {
        SelectItemSetClientRpc(itemIndex);
    }
     
    [ClientRpc]
    public void SelectItemSetClientRpc(int itemIndex)
    {
        if (itemIndex != -1)
        {
            selectItem = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
        }
        UIReset();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SelectItemResetServerRpc()
    {
        SelectItemResetClientRpc();
    }

    [ClientRpc]
    public void SelectItemResetClientRpc()
    {
        selectItem = null;
        UIReset();
    }

    void UIReset()
    {
        if (clickEvent.unloaderManager != null)
            clickEvent.unloaderManager.UIReset();
    }

    public override StructureSaveData SaveData()
    {
        StructureSaveData data = base.SaveData();

        FilterSaveData filterSaveData = new FilterSaveData();
        filterSaveData.filterItemIndex = GeminiNetworkManager.instance.GetItemSOIndex(selectItem);
        data.filters.Add(filterSaveData);        

        return data;
    }

    public void GameStartFillterSet(int itemIndex)
    {
        selectItem = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
    }

    protected override void FactoryOverlay()
    {
        if (!gameManager.overlayOn)
        {
            overlay.UIReset();
        }
        else
        {
            if (selectItem)
                overlay.UISet(selectItem);
        }
    }
}
