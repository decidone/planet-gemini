using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

public class MergerCtrl : SolidFactoryCtrl
{
    void Start()
    {
        dirCount = 4;
        setModel = GetComponent<SpriteRenderer>();
        CheckPos();
    }

    protected override void Update()
    {
        base.Update();
        SetDirNum();
        if (!removeState)
        {
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
        }
    }

    IEnumerator SetFacDelay()
    {
        var spawnItem = itemPool.Get();
        var sprite = spawnItem.GetComponent<SpriteRenderer>();
        sprite.color = new Color(1f, 1f, 1f, 0f);
        CircleCollider2D coll = spawnItem.GetComponent<CircleCollider2D>();
        coll.enabled = false;

        spawnItem.transform.position = this.transform.position;

        var targetPos = outObj[0].transform.position;
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
                if (checkObj && outObj.Count > 0 && outObj[0] != null)
                {
                    if (outObj[0].TryGetComponent(out Structure outFactory))
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
