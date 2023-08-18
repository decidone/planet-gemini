using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeCtrl : FluidFactoryCtrl
{
    public PipeGroupMgr pipeGroupMgr;

    //GameObject[] nearObj = new GameObject[4];
    public bool isUp = false;
    public bool isRight = false;
    public bool isDown = false;
    public bool isLeft = false;

    protected override void Start()
    {
        setModel = GetComponent<SpriteRenderer>();
    }

    protected override void Update()
    {
        base.Update();
        if (!removeState)
        {
            SetDirNum();
            if (!isPreBuilding)
            {
                for (int i = 0; i < nearObj.Length; i++)
                {
                    if (nearObj[i] == null)
                    {
                        CheckNearObj(checkPos[i], i, obj => ObjCheck(obj, checkPos[i]));
                    }
                }
            }
        }
    }

    protected override void CheckPos()
    {
        Vector2[] dirs = { Vector2.up, Vector2.right, Vector2.down, Vector2.left };

        for (int i = 0; i < 4; i++)
        {
            checkPos[i] = dirs[i];
        }
        ChangeModel();
    }

    void ObjCheck(GameObject game, Vector3 vec)
    {
        if (vec == transform.up)
            isUp = true;
        if (vec == transform.right)
            isRight = true;
        if (vec == -transform.up)
            isDown = true;
        if (vec == -transform.right)
            isLeft = true;

        if (game.GetComponent<PipeCtrl>() != null)
            pipeGroupMgr.CheckGroup(game.GetComponent<PipeCtrl>());
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

    public void FactoryVecCheck(Vector3 factory)
    {
        if (factory.x < this.transform.position.x)
            isLeft = true;
        else if (factory.x > this.transform.position.x)
            isRight = true;
        else if (factory.y - 0.1f < this.transform.position.y)
            isDown = true;
        else if (factory.y - 0.1f > this.transform.position.y)
            isUp = true;

        ChangeModel();
    }
}
