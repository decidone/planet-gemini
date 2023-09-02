using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// UTF-8 설정
public class FluidTankCtrl : FluidFactoryCtrl
{
    Vector2[] startTransform;
    Vector3[] directions;
    int[] indices;

    protected override void Start()
    {
        startTransform = new Vector2[4];
        directions = new Vector3[4];
        indices = new int[6];
        nearObj = new GameObject[8];
        CheckPos();
    }

    protected override void Update()
    {
        base.Update();

        if (!removeState)
        {
            CheckPos();

            if (!isPreBuilding)
            {
                for (int i = 0; i < nearObj.Length; i++)
                {
                    if (nearObj[i] == null)
                    {
                        int dirIndex = i / 2;
                        CheckNearObj(startTransform[indices[i]], directions[dirIndex], i, obj => FluidSetOutObj(obj));
                    }
                }

                if (outObj.Count > 0)
                {
                    sendDelayTimer += Time.deltaTime;

                    if (sendDelayTimer > structureData.SendDelay)
                    {
                        if(saveFluidNum >= structureData.SendFluidAmount)
                            SendFluid();
                        sendDelayTimer = 0;
                    }
                }
            }
        }
    }

    protected override void CheckPos()
    {
        indices = new int[] { 3, 0, 0, 1, 1, 2, 2, 3 };
        startTransform = new Vector2[] { new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f), new Vector2(-0.5f, 0.5f) };
        directions = new Vector3[] { transform.up, transform.right, -transform.up, -transform.right };
    }

    void CheckNearObj(Vector3 startVec, Vector3 endVec, int index, Action<GameObject> callback)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(this.transform.position + startVec, endVec, 1f);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hitCollider = hits[i].collider;
            if (hitCollider.CompareTag("Factory") && !hitCollider.GetComponent<Structure>().isPreBuilding &&
                hits[i].collider.gameObject != this.gameObject)
            {
                nearObj[index] = hits[i].collider.gameObject;
                callback(hitCollider.gameObject);
                break;
            }
        }
    }

    protected override void SendFluid()
    {
        foreach (GameObject obj in outObj)
        {
            if (obj.TryGetComponent(out FluidFactoryCtrl fluidFactory) && fluidFactory.GetComponent<PumpCtrl>() == null)
            {
                if (fluidFactory.structureData.MaxFulidStorageLimit > fluidFactory.saveFluidNum)
                {
                    float currentFillRatio = (float)fluidFactory.structureData.MaxFulidStorageLimit / fluidFactory.saveFluidNum;
                    float targetFillRatio = (float)structureData.MaxFulidStorageLimit / saveFluidNum;

                    if (currentFillRatio > targetFillRatio)
                    {
                        saveFluidNum -= structureData.SendFluidAmount;
                        fluidFactory.SendFluidFunc(structureData.SendFluidAmount);
                    }
                }
            }
        }
    }
}
