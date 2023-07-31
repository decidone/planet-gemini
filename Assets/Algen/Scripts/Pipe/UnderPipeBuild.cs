using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnderPipeBuild : MonoBehaviour
{
    [SerializeField]
    GameObject underPipe = null;

    public bool isPreBuilding = false;

    public GameObject underPipeObj = null;
    public Structure pipeScipt = null;
    public bool isSendPipe = true;

    Vector2[] checkPos = new Vector2[4];
    Vector2[] dirs = { Vector2.down, Vector2.left, Vector2.up, Vector2.right };
    public int dirNum = 0;
    int tempDir = 0;
    UnderPipeCtrl underpipeCtrl;
    PreBuilding preBuilding;
    public bool buildEnd = false;
    void Start()
    {
        SetSlotColor(underPipe.GetComponent<SpriteRenderer>(), Color.green, 0.35f);
        preBuilding = PreBuilding.instance;
        tempDir = pipeScipt.dirNum;
    }

    // Update is called once per frame
    void Update()
    {// 기본적으로 send벨트이고 send벨트의 반대 방향으로 10 체크해서 다른 send벨트가 있을 때 get벨트로 변경
        if (isPreBuilding)
        {
            CheckPos();
            if (preBuilding !=null && preBuilding.isDrag)
            {
                if (!buildEnd)
                {
                    CheckNearObj(checkPos[0]);
                }
            }
        }
    }

    void CheckPos()
    {
        Vector2[] dirs = { Vector2.down, Vector2.left, Vector2.up, Vector2.right };

        for (int i = 0; i < 4; i++)
        {
            checkPos[i] = dirs[(dirNum + i) % 4];
        }
    }

    void CheckNearObj(Vector2 direction)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, 10);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D factoryCollider = hits[i].collider;

            if (factoryCollider.CompareTag("Factory") && factoryCollider.gameObject != underPipeObj && factoryCollider.gameObject.transform.position != underPipeObj.transform.position)
            {
                underpipeCtrl = factoryCollider.GetComponent<UnderPipeCtrl>();
                if (underpipeCtrl != null && underpipeCtrl.isPreBuilding)
                {
                    if (underpipeCtrl != null)
                    {
                        TurnDir(underpipeCtrl.dirNum);
                        return;
                    }
                    else if (underpipeCtrl != null)
                    {
                        isSendPipe = true;
                        return;
                    }    
                }            
            }
        }
    }

    public void SetUnderPipe()
    {
        underPipeObj = underPipe;
        pipeScipt = underPipeObj.GetComponent<UnderPipeCtrl>();
        ColliderTriggerOnOff(true);
        //DisableColliders();
        pipeScipt.isPreBuilding = true;
        pipeScipt.dirNum = dirNum;
    }

    void TurnDir(int preDir)
    {
        if (pipeScipt.dirNum == 0 || pipeScipt.dirNum == 2)
        {
            if (preDir == 0)
            {
                pipeScipt.dirNum = 2;
            }
            else if (preDir == 2)
            {
                pipeScipt.dirNum = 0;
            }
        }
        if (pipeScipt.dirNum == 1 || pipeScipt.dirNum == 3)
        {
            if (preDir == 1)
            {
                pipeScipt.dirNum = 3;
            }
            else if (preDir == 3)
            {
                pipeScipt.dirNum = 1;
            }
        }
        if (tempDir == pipeScipt.dirNum)
            isSendPipe = true;
        else
            isSendPipe = false;

        dirNum = pipeScipt.dirNum;
    }

    void SetSlotColor(SpriteRenderer sprite, Color color, float alpha)
    {
        Color slotColor = sprite.color;
        slotColor = color;
        slotColor.a = alpha;
        sprite.color = slotColor;
    }

    public void ColliderTriggerOnOff(bool isOn)
    {
        pipeScipt.ColliderTriggerOnOff(isOn);
    }

    public void RemoveObj()
    {
        if (underPipeObj != null)
        {
            underPipeObj.transform.parent = null;
        }
        Destroy(this.gameObject);
    }
}
