using System.Collections;
using System.Collections.Generic;
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
        isMainSource = true;
        CheckPos();
    }

    protected override void Update()
    {
        base.Update();

        if (!removeState)
        {
            for (int i = 0; i < nearObj.Length; i++)
            {
                if (nearObj[i] == null)
                {
                    CheckNearObj(checkPos[i], i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
                }
            }

            if (IsServer && !isPreBuilding && checkObj)
            {
                if (inObj.Count > 0 && outObj.Count > 0 && !itemGetDelay)
                {
                    GetAndSendItem();
                }
            }
            if (DelayGetList.Count > 0 && inObj.Count > 0)
            {
                GetDelayFunc(DelayGetList[0], 0);
            }
            //if (DelaySendList.Count > 0 && outObj.Count > 0 && !outObj[DelaySendList[0].Item2].GetComponent<Structure>().isFull)
            //{
            //    SendDelayFunc(DelaySendList[0].Item1, DelaySendList[0].Item2, 0);
            //}
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public override void ClientConnectSyncServerRpc()
    {
        base.ClientConnectSyncServerRpc();

        int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(selectItem);
        ClientFillterSetClientRpc(itemIndex);
    }

    [ClientRpc]
    void ClientFillterSetClientRpc(int itemIndex)
    {
        if (IsServer)
            return;

        SelectItemSetClientRpc(itemIndex);
    }

    protected override IEnumerator SetOutObjCoroutine(GameObject obj)
    {
        checkObj = false;
        yield return new WaitForSeconds(0.1f);

        if (obj.GetComponent<WallCtrl>())
            yield break;

        if (obj.TryGetComponent(out Structure structure))
        {
            if (structure.isMainSource)
            {
                checkObj = true;
            }
            else if (structure.isStorageBuilding)
            {
                inObj.Add(obj);
                checkObj = true;
            }
            else
            {
                if (obj.TryGetComponent(out BeltCtrl belt))
                {
                    if (obj.GetComponentInParent<BeltGroupMgr>().nextObj == this.gameObject)
                    {
                        checkObj = true;
                        yield break;
                    }
                    belt.FactoryPosCheck(GetComponentInParent<Structure>());
                }
                else
                {
                    outSameList.Add(obj);
                    StartCoroutine(OutCheck(obj));
                }
                outObj.Add(obj);
                StartCoroutine(UnderBeltConnectCheck(obj));
            }
        }
    }

    protected void GetAndSendItem()
    {
        itemGetDelay = true;
        Structure outFactory = outObj[sendItemIndex].GetComponent<Structure>();
        if ((inObj[getItemIndex].TryGetComponent(out Production inObjScript)
                && !inObjScript.UnloadItemCheck(GetComponent<Unloader>().selectItem))
                || outFactory.isFull)
        {
            GetItemIndexSet();
            Invoke(nameof(DelayGetItem), getDelay);
            return;
        }
        itemList.Add(selectItem);
        GetItemServerRpc(getItemIndex);
        GetItemIndexSet();
    }

    protected override void GetItemFunc(int inObjIndex)
    {
        if (inObj[inObjIndex].TryGetComponent(out Production production))
        {
            if (production.UnloadItemCheck(selectItem))
            {
                int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(selectItem);
                if (IsServer)
                {
                    production.UnloadItem(selectItem);
                    SendItem(itemIndex);
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
        selectItem = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
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
}
