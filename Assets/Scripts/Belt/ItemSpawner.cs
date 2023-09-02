using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

// UTF-8 설정
public class ItemSpawner : LogisticsCtrl
{
    public Item itemData;

    void Start()
    {
        dirCount = 4;
        CheckPos();
    }

    protected override void Update()
    {
        base.Update();
        if (!removeState)
        {
            if (!isPreBuilding && checkObj)
            {
                if (outObj.Count > 0 && !itemSetDelay)
                {
                    if (itemData.name != "EmptyFilter")
                        SendItem(itemData);
                }

                for (int i = 0; i < nearObj.Length; i++)
                {
                    if (nearObj[i] == null)
                    {
                        CheckNearObj(checkPos[i], i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
                    }
                }
            }
        } 
    }
}
