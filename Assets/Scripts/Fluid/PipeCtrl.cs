using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeCtrl : FluidFactoryCtrl
{
    public PipeGroupMgr pipeGroupMgr;

    //GameObject[] nearObj = new GameObject[4];
    bool isUp = false;
    bool isRight = false;
    bool isDown = false;
    bool isLeft = false;

    protected override void Start()
    {
        setModel = GetComponent<SpriteRenderer>();
        //if (transform.parent.gameObject != null)
        //    pipeGroupMgr = GetComponentInParent<PipeGroupMgr>();
    }

    protected override void Update()
    {
        base.Update();

        ModelSet();
        if (!removeState)
        {
            if (!isPreBuilding)
            {
                if (!isUp)
                    isUp = ObjCheck(transform.up);
                if (!isRight)
                    isRight = ObjCheck(transform.right);
                if (!isDown)
                    isDown = ObjCheck(-transform.up);
                if (!isLeft)
                    isLeft = ObjCheck(-transform.right);
            }
        }
    }

    void ModelSet()
    {
        setModel.sprite = modelNum[dirNum];

        ChangeModel();
    }

    bool ObjCheck(Vector3 vec)
    {
        RaycastHit2D[] Hits = Physics2D.RaycastAll(this.gameObject.transform.position, vec, 1f);

        for (int a = 0; a < Hits.Length; a++)
        {
            if (Hits[a].collider.GetComponent<PipeCtrl>() != this.gameObject.GetComponent<PipeCtrl>())
            {
                if (Hits[a].collider.GetComponent<PipeCtrl>() != null && Hits[a].collider.CompareTag("Factory") && 
                    !Hits[a].collider.GetComponent<Structure>().isPreBuilding)
                {
                    if (Hits[a].collider.GetComponent<PipeCtrl>() != null)
                        pipeGroupMgr.CheckGroup(Hits[a].collider.GetComponent<PipeCtrl>());

                    return true;
                }
            }
        }
        return false;
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
        else if(isUp && isRight && !isDown && !isLeft)
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
