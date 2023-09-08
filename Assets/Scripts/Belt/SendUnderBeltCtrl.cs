using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// UTF-8 설정
public class SendUnderBeltCtrl : LogisticsCtrl
{
    void Start()
    {
        setModel = GetComponent<SpriteRenderer>();
    }

    protected override void Update()
    {
        base.Update();
        if (!removeState)
        {
            SetDirNum();
            if (!isPreBuilding && checkObj)
            {

                if (inObj.Count > 0 && !isFull && !itemGetDelay)
                {
                    GetItem();
                }
                if (itemList.Count > 0 && outObj.Count > 0 && !itemSetDelay)
                {
                    SendItem(itemList[0]);
                }

                for (int i = 1; i < nearObj.Length; i++)
                {
                    if (nearObj[i] == null)
                    {
                        if (i == 0)
                            CheckNearObj(checkPos[0], 0, obj => { });
                        if (i == 2)
                            CheckNearObj(checkPos[2], 2, obj => StartCoroutine(SetInObjCoroutine(obj)));
                    }
                }
            }
        } 
    }

    protected override void SendItem(Item item)
    {
        if (setFacDelayCoroutine != null)
        {
            return;
        }

        itemSetDelay = true;

        Structure outFactory = outObj[0].GetComponent<Structure>();

        if (!outFactory.isFull)
        {
            setFacDelayCoroutine = StartCoroutine(SendFacDelayArguments(outObj[0], item));
        }

        Invoke("DelaySetItem", structureData.SendDelay);
        itemSetDelay = false;
    }

    public void SetOutObj(GameObject Obj)
    {
        if (outObj.Count > 0)
        {
            outObj[0].GetComponent<GetUnderBeltCtrl>().ResetInObj();
            outObj.Remove(outObj[0]);
        }

        outObj.Add(Obj);
    }
}
