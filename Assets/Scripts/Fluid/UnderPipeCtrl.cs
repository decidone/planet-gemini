using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// UTF-8 설정
public class UnderPipeCtrl : FluidFactoryCtrl
{
    public Structure otherPipe = null;

    public Structure connectUnderPipe = null;
    PreBuilding preBuilding;
    bool preBuildingCheck;

    protected override void Start()
    {
        gameManager = GameManager.instance;
        preBuilding = PreBuilding.instance;
        nearObj = new Structure[2];
        checkPos = new Vector2[2];

        StrBuilt();
    }

    protected override void Update()
    {
        base.Update();
        if (!removeState)
        {
            if (gameManager.focusedStructure == null && (dirNum == 0 || dirNum == 1))
            {
                if (preBuilding.isBuildingOn && preBuilding.isUnderObj && !preBuilding.isUnderBelt)
                {
                    if (!preBuildingCheck && connectUnderPipe)
                    {
                        LineRendererSet(connectUnderPipe.transform.position);
                        preBuildingCheck = true;
                    }
                }
                else
                {
                    if (preBuildingCheck)
                    {
                        DestroyLineRenderer();
                        preBuildingCheck = false;
                    }
                }
            }
        }
    }

    public override void StrBuilt()
    {
        NearStrBuilt();

        float dist = 10;

        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, checkPos[0], dist);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hitCollider = hits[i].collider;
            if (hitCollider.TryGetComponent(out Structure str) && str != this)
            {
                if (str.TryGet(out UnderPipeCtrl othPipe) && CanConnectUnderPipe(othPipe))
                {
                    othPipe.NearStrBuilt();
                    return;
                }
            }
        }

        CheckSlotState(0);
    }

    public override void NearStrBuilt()
    {
        // 건물을 지었을 때나 근처에 새로운 건물이 지어졌을 때 동작
        // 변경사항이 생기면 DelayNearStrBuiltCoroutine()에도 반영해야 함
        if (IsServer)
        {
            CheckPos();
            for (int i = 0; i < nearObj.Length; i++)
            {
                if (nearObj[i] == null)
                {
                    if (i == 0)
                        CheckNearObj(checkPos[0], 0, obj => UnderPipeSetInObj(obj));
                    else if (i == 1)
                        CheckNearObj(checkPos[1], 1, obj => UnderPipeSetOutObj(obj));
                }
            }
            setModel.sprite = modelNum[dirNum];
        }
        else
        {
            DelayNearStrBuilt();
        }
    }

    public override void DelayNearStrBuilt()
    {
        // 동시 건설, 클라이언트 동기화 등의 이유로 딜레이를 주고 NearStrBuilt()를 실행할 때 사용
        StartCoroutine(DelayNearStrBuiltCoroutine());
    }

    protected override IEnumerator DelayNearStrBuiltCoroutine()
    {
        // 동시 건설이나 그룹핑을 따로 예외처리 하는 경우가 아니면 NearStrBuilt()를 그대로 사용
        yield return new WaitForEndOfFrame();

        CheckPos();
        for (int i = 0; i < nearObj.Length; i++)
        {
            if (nearObj[i] == null)
            {
                if (i == 0)
                    CheckNearObj(checkPos[0], 0, obj => UnderPipeSetInObj(obj));
                else if (i == 1)
                    CheckNearObj(checkPos[1], 1, obj => UnderPipeSetOutObj(obj));
            }
        }
        setModel.sprite = modelNum[dirNum];
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

    protected override void CheckNearObj(Vector2 direction, int index, Action<Structure> callback)
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
            if (hitCollider.TryGetComponent(out Structure str))
            {
                if(str.destroyStart)
                    continue; // 구조물이 파괴 중이면 무시
                else if (str != this)
                {
                    if (index == 0)
                    {
                        if (str.TryGet(out UnderPipeCtrl otherUnderPipe))
                        {
                            if (CanConnectUnderPipe(otherUnderPipe))
                            {
                                nearObj[index] = str;
                                callback(str);
                                break;
                            }
                            else
                                continue;
                        }
                    }
                    else
                    {
                        nearObj[index] = str;
                        callback(str);
                        break;
                    }
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

    void UnderPipeSetInObj(Structure obj)
    {
        if (obj && obj.TryGet(out UnderPipeCtrl othUnderPipe))
        {
            connectUnderPipe = obj;
            if (!outObj.Contains(obj))
                outObj.Add(obj);
            if (othUnderPipe.connectUnderPipe != this)
            {
                if(othUnderPipe.connectUnderPipe != null && 
                othUnderPipe.connectUnderPipe.TryGet(out UnderPipeCtrl underPipe))
                    underPipe.DisCntObj();

                othUnderPipe.DisCntObj();
                othUnderPipe.StrBuilt();
            }
        }
    }

    void DisCntObj()
    {
        outObj.Remove(connectUnderPipe);
        connectUnderPipe = null;
        nearObj[0] = null;
    }

    void UnderPipeSetOutObj(Structure obj)
    {
        if(obj)
        {
            if (obj.Has<UnderPipeCtrl>())
            {
                return;
            }

            otherPipe = obj;
            if (!outObj.Contains(obj))
                outObj.Add(obj);

            if (obj.TryGet(out PipeCtrl otherPipeCtrl))
            {
                otherPipeCtrl.FactoryVecCheck(this);
            }
        }
    }

    public override void ResetNearObj(Structure game)
    {
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
    }

    public void EndRenderer(bool isSend)
    {
        if (outObj.Count > 0)
        {
            if (outObj[0].TryGet(out UnderPipeCtrl underPipe))
            {
                underPipe.DestroyLineRenderer();
                underPipe.preBuildingCheck = false;
                if (connectUnderPipe && isSend)
                    connectUnderPipe.Get<UnderPipeCtrl>().EndRenderer(!isSend);
            }
        }
    }

    public override void Focused()
    {
        if(connectUnderPipe)
            LineRendererSet(connectUnderPipe.transform.position);
    }

    public override void DisableFocused()
    {
        DestroyLineRenderer();
    }
}
