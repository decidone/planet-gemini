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

                if (nearObj[2] == null)
                    CheckNearObj(checkPos[2], 2, obj => StartCoroutine(SetInObjCoroutine(obj)));
            }
        } 
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
        Debug.Log("newSend");
    }
}
