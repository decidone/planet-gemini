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

    // Start is called before the first frame update
    void Start()
    {
        if (transform.parent.gameObject != null)
            pipeGroupMgr = GetComponentInParent<PipeGroupMgr>();
    }

    // Update is called once per frame
    void Update()
    {
        ModelSet();

        if(isUp == false)
            ObjCheck(transform.up);
        if (isRight == false)
            ObjCheck(transform.right); 
        if (isDown == false)
            ObjCheck(-transform.up);
        if (isLeft == false)
            ObjCheck(-transform.right);
    }

    void ModelSet()
    {

    }

    void ObjCheck(Vector3 vec)
    {
        RaycastHit2D[] Hits = Physics2D.RaycastAll(this.gameObject.transform.position, vec, 1f);

        for (int a = 0; a < Hits.Length; a++)
        {
            if (Hits[a].collider.GetComponent<PipeCtrl>() != this.gameObject.GetComponent<PipeCtrl>())
            {
                if (Hits[a].collider.GetComponent<PipeCtrl>() != null && Hits[a].collider.CompareTag("Factory"))
                {                    
                    if (vec == transform.up)
                        isUp = true;
                    else if (vec == transform.right)
                        isRight = true;
                    else if (vec == -transform.up)
                        isDown = true;
                    else if (vec == -transform.right)
                        isLeft = true;

                    if (Hits[a].collider.GetComponent<PipeCtrl>() != null)
                        pipeGroupMgr.CheckGroup(Hits[a].collider.GetComponent<PipeCtrl>());
                    //else if (Hits[a].collider.GetComponent<PipeCtrl>() == null)
                    //{
                    //    if (Hits[a].collider.GetComponent<PumpCtrl>() == null)
                    //        pipeGroupMgr.factoryList.Add(Hits[a].collider.gameObject);
                    //}
                }
            }            
        }
    }
}
