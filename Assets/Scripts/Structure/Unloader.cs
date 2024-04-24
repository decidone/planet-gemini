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
        setModel = GetComponent<SpriteRenderer>();
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
                    GetItem();
                }
            }
            if (DelayGetList.Count > 0 && inObj.Count > 0)
            {
                GetDelayFunc(DelayGetList[0], 0);
            }
        }
    }

    protected override IEnumerator SetOutObjCoroutine(GameObject obj)
    {
        checkObj = false;
        yield return new WaitForSeconds(0.1f);

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

    //[ClientRpc]
    //protected override void GetItemClientRpc(int inObjIndex)
    //{
    //    itemGetDelay = true;
    //    if (inObj[inObjIndex].TryGetComponent(out Production production))
    //    {
    //        if (production.UnloadItem(selectItem))
    //        {
    //            int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(selectItem);
    //            SendItem(itemIndex);
    //        }
    //        DelayGetItem();
    //    }
    //}

    protected override void GetItemFunc(int inObjIndex)
    {
        if (inObj[inObjIndex].TryGetComponent(out Production production))
        {
            if (production.UnloadItem(selectItem))
            {
                int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(selectItem);
                SendItem(itemIndex);
            }
            DelayGetItem();
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
}
