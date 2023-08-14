using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

public class GetUnderBeltCtrl : SolidFactoryCtrl
{
    void Start()
    {
        dirCount = 4;
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
                if (itemList.Count > 0 && outObj.Count > 0 && !itemSetDelay)
                {
                    SendItem(itemList[0]);
                }

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
        }
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
                    !hitCollider.GetComponent<Structure>().isPreBuilding &&
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
                    !hitCollider.GetComponent<Structure>().isPreBuilding &&
                    hitCollider.GetComponent<GetUnderBeltCtrl>() != GetComponent<GetUnderBeltCtrl>())
                {
                    if (hitCollider.GetComponent<GetUnderBeltCtrl>())                    
                    {
                        break;
                    }
                    else if(hitCollider.GetComponent<SendUnderBeltCtrl>() != null)
                    {
                        nearObj[index] = hits[i].collider.gameObject;
                        callback(hitCollider.gameObject);
                        break;
                    }
                }
            }
        }
    }

    protected override IEnumerator SetInObjCoroutine(GameObject obj)
    {
        yield return new WaitForSeconds(0.1f);

        SendUnderBeltCtrl sendUnderbelt = obj.GetComponent<SendUnderBeltCtrl>();
        if (sendUnderbelt.dirNum == dirNum)
        {
            inObj.Add(obj);
            sendUnderbelt.SetOutObj(this.gameObject);
        }
        else
            yield break;
    }

    public void ResetInObj()
    {
        nearObj[0] = null;
        inObj.Clear();
    }

    IEnumerator SetFacDelay()
    {
        var spawnItem = itemPool.Get();
        SpriteRenderer sprite = spawnItem.GetComponent<SpriteRenderer>();
        sprite.color = new Color(1f, 1f, 1f, 0f);
        CircleCollider2D coll = spawnItem.GetComponent<CircleCollider2D>();
        coll.enabled = false;

        spawnItem.transform.position = this.transform.position;

        var targetPos = outObj[sendItemIndex].transform.position;
        var startTime = Time.time;
        var distance = Vector3.Distance(spawnItem.transform.position, targetPos);

        while (spawnItem != null && spawnItem.transform.position != targetPos)
        {
            var elapsed = Time.time - startTime;
            var t = Mathf.Clamp01(elapsed / (distance / solidFactoryData.SendSpeed[level]));
            spawnItem.transform.position = Vector3.Lerp(spawnItem.transform.position, targetPos, t);

            yield return null;
        }

        if (spawnItem != null && spawnItem.transform.position == targetPos)
        {
            if (itemList.Count > 0)
            {
                if (checkObj && outObj.Count > 0 && outObj[sendItemIndex] != null)
                {
                    if (outObj[sendItemIndex].TryGetComponent(out Structure outFactory))
                    {
                        outFactory.OnFactoryItem(itemList[0]);
                    }
                }
                else
                {
                    spawnItem.item = itemList[0];
                    spawnItem.amount = 1; 
                    playerInven.Add(spawnItem.item, spawnItem.amount);
                    sprite.color = new Color(1f, 1f, 1f, 1f);
                    coll.enabled = true;
                    itemPool.Release(spawnItem);
                    spawnItem = null;
                }
                itemList.RemoveAt(0);
                ItemNumCheck();
            }
        }

        if (spawnItem != null)
        {
            sprite.color = new Color(1f, 1f, 1f, 1f);
            coll.enabled = true;
            setFacDelayCoroutine = null;
            itemPool.Release(spawnItem);
        }
        else
            setFacDelayCoroutine = null;
    }
}
