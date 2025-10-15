using System.Collections;
using Unity.Netcode;
using UnityEngine;

// UTF-8 설정
public class PipeCtrl : FluidFactoryCtrl
{
    public bool isUp = false;
    public bool isRight = false;
    public bool isDown = false;
    public bool isLeft = false;

    protected override void Update()
    {
        base.Update();
        if (!removeState)
        {
            //SetDirNum();
            //if (isSetBuildingOk)
            //{                
            //    for (int i = 0; i < nearObj.Length; i++)
            //    {
            //        if (nearObj[i] == null)
            //        {
            //            CheckNearObj(checkPos[i], i, obj => FluidSetOutObj(obj, checkPos[i]));
            //        }
            //    }
            //}

            //if (!isPreBuilding && checkObj)
            //{
            //    if (outObj.Count > 0)
            //    {
            //        sendDelayTimer += Time.deltaTime;

            //        if (sendDelayTimer > sendDelay)
            //        {
            //            if (saveFluidNum >= structureData.SendFluidAmount)
            //                SendFluid();
            //            sendDelayTimer = 0;
            //        }
            //    }
            //}
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public override void ClientConnectSyncServerRpc()
    {
        //base.ClientConnectSyncServerRpc();
        ClientConnectSync();

        DirSyncClientRpc(isUp, isRight, isDown, isLeft);
    }

    [ClientRpc]
    void DirSyncClientRpc(bool up, bool right, bool down, bool left)
    {
        if(!IsServer)
        {
            isUp = up;
            isRight = right;
            isDown = down;
            isLeft = left;
            ChangeModel();
        }
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
                    CheckNearObj(checkPos[i], i, obj => FluidSetOutObj(obj, checkPos[i]));
                }
            }
            ChangeModel();
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
                CheckNearObj(checkPos[i], i, obj => FluidSetOutObj(obj, checkPos[i]));
            }
        }
        ChangeModel();
    }

    protected override void CheckPos()
    {
        Vector2[] dirs = { Vector2.up, Vector2.right, Vector2.down, Vector2.left };

        for (int i = 0; i < 4; i++)
        {
            checkPos[i] = dirs[i];
        }
    }

    protected void FluidSetOutObj(GameObject obj, Vector3 vec)
    {
        if (obj.TryGetComponent(out FluidFactoryCtrl factoryCtrl))
        {
            if (!outObj.Contains(obj))
                outObj.Add(obj);
            if (obj.TryGetComponent(out UnderPipeCtrl underPipe))
            {
                UnderPipeConnectCheck(obj);
            }

            //StartCoroutine(nameof(MainSourceCheck), factoryCtrl);
        }
        ObjCheck(obj, vec);
    }

    void ObjCheck(GameObject game, Vector3 vec)
    {
        if(!game.GetComponent<UnderPipeCtrl>() && game.GetComponent<FluidFactoryCtrl>())
        {
            if (vec == transform.up)
                isUp = true;
            if (vec == transform.right)
                isRight = true;
            if (vec == -transform.up)
                isDown = true;
            if (vec == -transform.right)
                isLeft = true;
        }
    }

    public void ChangeModel()
    {
        if ((isUp && !isRight && !isDown && !isLeft)
            || (!isUp && !isRight && isDown && !isLeft)
            || (isUp && !isRight && isDown && !isLeft))
        {
            dirNum = 0;
        }
        else if ((!isUp && isRight && !isDown && !isLeft)
            || (!isUp && !isRight && !isDown && isLeft)
            || (!isUp && isRight && !isDown && isLeft))
        {
            dirNum = 1;
        }
        else if (isUp && isRight && !isDown && !isLeft)
        {
            dirNum = 2;
        }
        else if (isUp && !isRight && !isDown && isLeft)
        {
            dirNum = 3;
        }
        else if (!isUp && !isRight && isDown && isLeft)
        {
            dirNum = 4;
        }
        else if (!isUp && isRight && isDown && !isLeft)
        {
            dirNum = 5;
        }
        else if (isUp && isRight && isDown && !isLeft)
        {
            dirNum = 6;
        }
        else if (isUp && !isRight && isDown && isLeft)
        {
            dirNum = 7;
        }
        else if (!isUp && isRight && isDown && isLeft)
        {
            dirNum = 8;
        }
        else if (isUp && isRight && !isDown && isLeft)
        {
            dirNum = 9;
        }
        else if (isUp && isRight && isDown && isLeft)
        {
            dirNum = 10;
        }

        setModel.sprite = modelNum[dirNum];
    }

    public void FactoryVecCheck(GameObject factory)
    {
        if (factory.transform.position.x < this.transform.position.x)
        {
            nearObj[3] = factory;
            isLeft = true;
        }
        else if (factory.transform.position.x > this.transform.position.x)
        {
            nearObj[1] = factory;
            isRight = true;
        }
        else if (factory.transform.position.y - 0.1f < this.transform.position.y)
        {
            nearObj[2] = factory;
            isDown = true;

        }
        else if (factory.transform.position.y - 0.1f > this.transform.position.y)
        {
            nearObj[0] = factory;
            isUp = true;
        }
        if(!outObj.Contains(factory))
            outObj.Add(factory);

        ChangeModel();
    }

    public override void ResetNearObj(GameObject game)
    {
        if (outObj.Contains(game))
        {
            outObj.Remove(game);
            InOutObjIndexResetClientRpc(false);
        }

        for (int i = 0; i < nearObj.Length; i++)
        {
            if (nearObj[i] != null && nearObj[i] == game)
            {
                nearObj[i] = null;
                if (i == 0)
                {
                    isUp = false;
                }
                else if (i == 1)
                {
                    isRight = false;
                }
                else if (i == 2)
                {
                    isDown = false;
                }
                else if (i == 3)
                {
                    isLeft = false;
                }
            }
        }

        ChangeModel();
    }
}
