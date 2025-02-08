using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

// UTF-8 설정
public class GetUnderBeltCtrl : LogisticsCtrl
{
    //void Start()
    //{
    //    setModel = GetComponent<SpriteRenderer>();
    //}

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
                        else if (i == 2) 
                            CheckNearObj(checkPos[2], 2, obj => StartCoroutine(SetInObjCoroutine(obj)));
                    }
                }
            }                

            if (IsServer && !isPreBuilding && checkObj)
            {
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
        }
    }

    protected override void SetDirNum()
    {
        setModel.sprite = modelNum[dirNum + (level * 4)];
        CheckPos();
    }

    protected override void CheckNearObj(Vector2 direction, int index, Action<GameObject> callback)
    {
        float dist = 0;

        if (index == 2)
            dist = 10;
        else
            dist = 1;

        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, dist);

        for (int i = 0; i < hits.Length; i++)
        {
            if (index != 2)
            {
                Collider2D hitCollider = hits[i].collider;
                if (hitCollider.CompareTag("Factory") &&
                    hitCollider.GetComponent<Structure>().isSetBuildingOk &&
                    hits[i].collider.gameObject != this.gameObject)
                {
                    nearObj[index] = hits[i].collider.gameObject;
                    callback(hitCollider.gameObject);
                    break;
                }
            }
            else
            {
                Collider2D hitCollider = hits[i].collider;
                if (hitCollider.CompareTag("Factory") &&
                    hitCollider.GetComponent<Structure>().isSetBuildingOk &&
                    hitCollider.GetComponent<GetUnderBeltCtrl>() != GetComponent<GetUnderBeltCtrl>())
                {
                    if (hitCollider.TryGetComponent(out GetUnderBeltCtrl othGet) && othGet.dirNum == dirNum)                    
                    {
                        break;
                    }
                    else if(hitCollider.TryGetComponent(out SendUnderBeltCtrl sendUnderBelt))
                    {
                        if (sendUnderBelt.dirNum == dirNum)
                        {
                            nearObj[index] = hits[i].collider.gameObject;
                            callback(hitCollider.gameObject);
                            break;
                        }
                    }
                }
            }
        }
    }
    protected override IEnumerator SetOutObjCoroutine(GameObject obj)
    {
        checkObj = false;
        yield return new WaitForSeconds(0.1f);
        
        if (obj.GetComponent<WallCtrl>())
            yield break;

        if (obj.GetComponent<Structure>() != null)
        {
            if ((obj.GetComponent<ItemSpawner>() && GetComponent<ItemSpawner>())
                || obj.GetComponent<Unloader>())
            {
                checkObj = true;
                yield break;
            }

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
        //checkObj = true;
    }

    protected override IEnumerator SetInObjCoroutine(GameObject obj)
    {
        checkObj = false;
        yield return new WaitForSeconds(0.1f);

        SendUnderBeltCtrl sendUnderbelt = obj.GetComponent<SendUnderBeltCtrl>();

        if (sendUnderbelt.dirNum == dirNum)
        {
            inObj.Add(obj);
            sendUnderbelt.SetOutObj(this.gameObject);
        }
        checkObj = true;
    }

    public void ResetInObj()
    {
        nearObj[2] = null;
        inObj.Clear();
    }

    public override StructureSaveData SaveData()
    {
        StructureSaveData data = base.SaveData();
        data.sideObj = true;
        return data;
    }
}
