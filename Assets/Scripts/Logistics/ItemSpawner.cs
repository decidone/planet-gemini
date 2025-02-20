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
        CheckPos();
        isMainSource = true;
    }

    protected override void Update()
    {
        base.Update();
        if (!removeState)
        {
            if (isSetBuildingOk)
            {
                for (int i = 0; i < nearObj.Length; i++)
                {
                    if (nearObj[i] == null)
                    {
                        CheckNearObj(checkPos[i], i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
                    }
                }
            }

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
