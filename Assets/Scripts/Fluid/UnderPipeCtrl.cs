using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// UTF-8 설정
public class UnderPipeCtrl : FluidFactoryCtrl
{
    public GameObject otherPipe = null;

    public GameObject connectUnderPipe = null;

    protected override void Start()
    {
        setModel = GetComponent<SpriteRenderer>();
        nearObj = new GameObject[2];
        checkPos = new Vector2[2];
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
                            CheckNearObj(checkPos[0], 0, obj => UnderPipeSetInObj(obj));
                        else if (i == 1)
                            CheckNearObj(checkPos[1], 1,  obj => UnderPipeSetOutObj(obj));
                    }
                }
            }

            if (!isPreBuilding && checkObj)
            {
                if (outObj.Count > 0)
                {
                    sendDelayTimer += Time.deltaTime;

                    if (sendDelayTimer > structureData.SendDelay[level])
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
        if (dirNum == 0)
        {
            checkPos[0] = transform.up;
            checkPos[1] = -transform.up;
        }
        else if (dirNum == 1)
        {
            checkPos[0] = transform.right;
            checkPos[1] = -transform.right;
        }
        else if (dirNum == 2)
        {
            checkPos[0] = -transform.up;
            checkPos[1] = transform.up;
        }
        else if (dirNum == 3)
        {
            checkPos[0] = -transform.right;
            checkPos[1] = transform.right;
        }
    }

    protected override void CheckNearObj(Vector2 direction, int index, Action<GameObject> callback)
    {
        float dist = 0;

        if (index == 0)
            dist = 10;
        else
            dist = 1;

        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, dist);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hitCollider = hits[i].collider;
            if (hitCollider.CompareTag("Factory") && hitCollider.GetComponent<Structure>().isSetBuildingOk &&
                hits[i].collider.gameObject != this.gameObject)
            {
                if (index == 0)
                {
                    if (hitCollider.TryGetComponent(out UnderPipeCtrl otherUnderPipe))
                    {
                        if (CanConnectUnderPipe(otherUnderPipe))
                        {
                            nearObj[index] = hits[i].collider.gameObject;
                            callback(hitCollider.gameObject);
                            break;
                        }
                        else
                            break;
                    }
                }
                else
                {                
                    nearObj[index] = hits[i].collider.gameObject;
                    callback(hitCollider.gameObject);
                    break;
                }
            }
        }
    }

    bool CanConnectUnderPipe(UnderPipeCtrl othUnderPipe)
    {
        if (dirNum == 0 && othUnderPipe.dirNum == 2)
        {
            return true;
        }
        else if (dirNum == 1 && othUnderPipe.dirNum == 3)
        {
            return true;
        }
        else if (dirNum == 2 && othUnderPipe.dirNum == 0)
        {
            return true;
        }
        else if (dirNum == 3 && othUnderPipe.dirNum == 1)
        {
            return true;
        }
        else
            return false;
    }

    void UnderPipeSetInObj(GameObject obj)
    {
        if (obj.TryGetComponent(out FluidFactoryCtrl factoryCtrl))
        {
            if (obj.TryGetComponent(out UnderPipeCtrl othUnderPipe))
            {
                connectUnderPipe = obj;
                outObj.Add(obj);
                if (othUnderPipe.connectUnderPipe != this.gameObject)
                {
                    if(othUnderPipe.connectUnderPipe != null)
                        othUnderPipe.connectUnderPipe.GetComponent<UnderPipeCtrl>().DisCntObj();
                    othUnderPipe.DisCntObj();
                }
                StartCoroutine("MainSourceCheck", factoryCtrl);
            }
        }
    }

    public void DisCntObj()
    {
        outObj.Remove(connectUnderPipe);
        connectUnderPipe = null;
        nearObj[0] = null;
    }

    void UnderPipeSetOutObj(GameObject obj)
    {
        if (obj.TryGetComponent(out FluidFactoryCtrl factoryCtrl))
        {
            if (obj.TryGetComponent(out UnderPipeCtrl othUnderPipe))
            {
                if (!CanConnectUnderPipe(othUnderPipe))
                    return;
            }

            otherPipe = obj;
            outObj.Add(obj);
            if (obj.GetComponent<PipeCtrl>() != null)
            {
                otherPipe.GetComponent<PipeCtrl>().FactoryVecCheck(this.gameObject);
            }
            StartCoroutine("MainSourceCheck", factoryCtrl);
        }
    }

    public override void ResetCheckObj(GameObject game)
    {
        checkObj = false;

        if(otherPipe == game)
        {
            otherPipe = null;
        }
        else if(connectUnderPipe == game)
        {
            connectUnderPipe = null;
        }
        if (outObj.Contains(game))
        {
            outObj.Remove(game);
        }

        for (int i = 0; i < nearObj.Length; i++)
        {
            if (nearObj[i] != null && nearObj[i] == game)
            {
                nearObj[i] = null;
            }
        }

        checkObj = true;
    }
}
