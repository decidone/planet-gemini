using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using Unity.Netcode;
using Unity.Netcode.Components;

// UTF-8 설정
public class ItemSpawner : LogisticsCtrl
{
    public Item itemData;

    void Start()
    {
        isMainSource = true;
        StrBuilt();
    }

    protected override void Update()
    {
        base.Update();
        if (!removeState)
        {
            //if (isSetBuildingOk)
            //{
            //    for (int i = 0; i < nearObj.Length; i++)
            //    {
            //        if (nearObj[i] == null)
            //        {
            //            CheckNearObj(checkPos[i], i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
            //        }
            //    }
            //}

            if (IsServer && !isPreBuilding && checkObj)
            {
                if (outObj.Count > 0 && !itemSetDelay)
                {
                    if (itemData.name != "EmptyFilter")
                    {
                        int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(itemData);
                        SendItem(itemIndex);
                        //SendItem(itemData);
                    }
                }
            }
            if (DelaySendList.Count > 0 && outObj.Count > 0 && !outObj[DelaySendList[0].Item2].GetComponent<Structure>().isFull)
            {
                SendDelayFunc(DelaySendList[0].Item1, DelaySendList[0].Item2, 0);
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
    public void ItemSetServerRpc(int itemIndex)
    {
        ItemSetClientRpc(itemIndex);
    }

    [ClientRpc]
    public void ItemSetClientRpc(int itemIndex)
    {
        itemData = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
    }
}
