using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unloader : LogisticsCtrl
{
    public Item selectItem;

    void Start()
    {
        setModel = GetComponent<SpriteRenderer>();
        isMainSource = true;
        CheckPos();
    }

    protected override void Update()
    {
        base.Update();

        if (!removeState)
        {
            if (!isPreBuilding && checkObj)
            {
                if (inObj.Count > 0 && outObj.Count > 0 && !itemGetDelay)
                {
                    GetItem();
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
            else if (structure.isStorageBuild)
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

    protected override void GetItem()
    {
        itemGetDelay = true;
        if (getItemIndex > inObj.Count)
        {
            getItemIndex = 0;
            return;
        }
        else if (inObj[getItemIndex] == null)
        {
            getItemIndex = 0;
            return;
        }
        else if (inObj[getItemIndex].TryGetComponent(out Production production))
        {
            if (production.UnloadItem(selectItem))
            {
                SendItem(selectItem);
            }
            getItemIndex++;
            if (getItemIndex >= inObj.Count)
                getItemIndex = 0;

            Invoke("DelayGetItem", structureData.SendDelay);
        }
        else
            DelayGetItem();
    }
}