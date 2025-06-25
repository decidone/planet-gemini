using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class PipeCtrl : FluidFactoryCtrl
{
    public bool isUp = false;
    public bool isRight = false;
    public bool isDown = false;
    public bool isLeft = false;

    protected override void Start()
    {
        base.Start();

        StrBuilt();
    }

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

            if (!isPreBuilding && checkObj)
            {
                if (outObj.Count > 0)
                {
                    sendDelayTimer += Time.deltaTime;

                    if (sendDelayTimer > sendDelay)
                    {
                        if (saveFluidNum >= structureData.SendFluidAmount)
                            SendFluid();
                        sendDelayTimer = 0;
                    }
                }
            }
        }
    }

    public override void NearStrBuilt()
    {
        // 건물을 지었을 때나 근처에 새로운 건물이 지어졌을 때 동작
        CheckPos();
        for (int i = 0; i < nearObj.Length; i++)
        {
            if (nearObj[i] == null)
            {
                CheckNearObj(checkPos[i], i, obj => FluidSetOutObj(obj, checkPos[i]));
            }
        }
        ChangeModel();
        setModel.sprite = modelNum[dirNum];
    }

    protected override void CheckPos()
    {
        Vector2[] dirs = { Vector2.up, Vector2.right, Vector2.down, Vector2.left };

        for (int i = 0; i < 4; i++)
        {
            checkPos[i] = dirs[i];
        }
        //ChangeModel();
    }

    protected void FluidSetOutObj(GameObject obj, Vector3 vec)
    {
        if (obj.TryGetComponent(out FluidFactoryCtrl factoryCtrl))
        {
            if (!factoryCtrl.GetComponent<UnderPipeCtrl>())
            {
                outObj.Add(obj);
            }
            if (obj.GetComponent<UnderPipeCtrl>() != null)
            {
                StartCoroutine(nameof(UnderPipeConnectCheck), obj);
            }
            StartCoroutine(nameof(MainSourceCheck), factoryCtrl);
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

    void ChangeModel()
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

    public override void ResetCheckObj(GameObject game)
    {
        checkObj = false;

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
        checkObj = true;
    }
}
