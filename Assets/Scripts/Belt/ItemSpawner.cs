using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

public class ItemSpawner : SolidFactoryCtrl
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
                    if (itemData.name != "emptyFilter")
                        SendItem(itemData);
                }

                for (int i = 0; i < nearObj.Length; i++)
                {
                    if (nearObj[i] == null)
                    {
                        if (i == 0)
                            CheckNearObj(checkPos[0], 0, obj => StartCoroutine(SetOutObjCoroutine(obj)));
                        else if (i == 1)
                            CheckNearObj(checkPos[1], 1, obj => StartCoroutine(SetOutObjCoroutine(obj)));
                        else if (i == 2)
                            CheckNearObj(checkPos[2], 2, obj => StartCoroutine(SetOutObjCoroutine(obj)));
                        else if (i == 3)
                            CheckNearObj(checkPos[3], 3, obj => StartCoroutine(SetOutObjCoroutine(obj)));
                    }
                }
            }
        } 
    }

    IEnumerator SetFacDelay(GameObject outFac)
    {
        var spawnItem = itemPool.Get();
        var sprite = spawnItem.GetComponent<SpriteRenderer>();
        sprite.color = new Color(1f, 1f, 1f, 0f);
        CircleCollider2D coll = spawnItem.GetComponent<CircleCollider2D>();
        coll.enabled = false;

        spawnItem.transform.position = transform.position;

        var targetPos = outFac.transform.position;
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
            if (checkObj && outFac != null)
            {
                if ( outFac.TryGetComponent(out Structure outFactory))
                {
                    outFactory.OnFactoryItem(itemData);
                }
            }
            else
            {
                spawnItem.item = itemData;
                spawnItem.amount = 1;
                playerInven.Add(spawnItem.item, spawnItem.amount);
                sprite.color = new Color(1f, 1f, 1f, 1f);
                coll.enabled = true;
                itemPool.Release(spawnItem);
                spawnItem = null;
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
