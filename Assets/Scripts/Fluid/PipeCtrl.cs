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
                if (isUp == false)
                    isUp = ObjCheck(transform.up);
                if (isRight == false)
                    isRight = ObjCheck(transform.right);
                if (isDown == false)
                    isDown = ObjCheck(-transform.up);
                if (isLeft == false)
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
        if ((isUp == true && isRight == false && isDown == false && isLeft == false)
            || (isUp == false && isRight == false && isDown == true && isLeft == false)
            || (isUp == true && isRight == false && isDown == true && isLeft == false)) 
        {
            dirNum = 0;
        }
        else if ((isUp == false && isRight == true && isDown == false && isLeft == false) 
            || (isUp == false && isRight == false && isDown == false && isLeft == true)
            || (isUp == false && isRight == true && isDown == false && isLeft == true))
        {
            dirNum = 1;
        }
        else if(isUp == true && isRight == true && isDown == false && isLeft == false)
        {
            dirNum = 2;
        }
        else if (isUp == true && isRight == false && isDown == false && isLeft == true)
        {
            dirNum = 3;
        }
        else if (isUp == false && isRight == false && isDown == true && isLeft == true)
        {
            dirNum = 4;
        }
        else if (isUp == false && isRight == true && isDown == true && isLeft == false)
        {
            dirNum = 5;
        }
        else if (isUp == true && isRight == true && isDown == true && isLeft == false)
        {
            dirNum = 6;
        }
        else if (isUp == true && isRight == false && isDown == true && isLeft == true)
        {
            dirNum = 7;
        }
        else if (isUp == false && isRight == true && isDown == true && isLeft == true)
        {
            dirNum = 8;
        }
        else if (isUp == true && isRight == true && isDown == false && isLeft == true)
        {
            dirNum = 9;
        }
        else if (isUp == true && isRight == true && isDown == true && isLeft == true)
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
