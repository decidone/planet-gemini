using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

// UTF-8 설정
public class MergerCtrl : LogisticsCtrl
{
    void Start()
    {
        setModel = GetComponent<SpriteRenderer>();
        CheckPos();
    }

    protected override void Update()
    {
        base.Update();
        if (!removeState)
        {
            SetDirNum();
            if (isSetBuildingOk)
            {
                for (int i = 0; i < nearObj.Length; i++)
                {
                    if (nearObj[i] == null)
                    {
                        if (i == 0)
                            CheckNearObj(checkPos[0], 0, obj => StartCoroutine(SetOutObjCoroutine(obj)));
                        else if (i == 1)
                            CheckNearObj(checkPos[1], 1, obj => StartCoroutine(SetInObjCoroutine(obj)));
                        else if (i == 2)
                            CheckNearObj(checkPos[2], 2, obj => StartCoroutine(SetInObjCoroutine(obj)));
                        else if (i == 3)
                            CheckNearObj(checkPos[3], 3, obj => StartCoroutine(SetInObjCoroutine(obj)));
                    }
                }
            }

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
}
