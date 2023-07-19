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

    Vector2[] checkPos = new Vector2[4];

    public int dirNum = 0;
    int tempDir = 0;
    UnderPipeCtrl underpipeCtrl;

    void Start()
    {
        SetSlotColor(underPipe.GetComponent<SpriteRenderer>(), Color.green, 0.35f);
    }

    // Update is called once per frame
    void Update()
    {// 기본적으로 send벨트이고 send벨트의 반대 방향으로 10 체크해서 다른 send벨트가 있을 때 get벨트로 변경
        if (isPreBuilding)
        {
            CheckPos();
            CheckNearObj(checkPos[0]);
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

            if (factoryCollider.CompareTag("Factory") && factoryCollider.gameObject != underPipeObj)
            {
                underpipeCtrl = factoryCollider.GetComponent<UnderPipeCtrl>();
                if (underpipeCtrl != null)
                {
                    if (underpipeCtrl.dirNum == 0)
                    {
                        pipeScipt.dirNum = 2;
                    }
                    else if (underpipeCtrl.dirNum == 1)
                    {
                        pipeScipt.dirNum = 3;
                    }
                    else if (underpipeCtrl.dirNum == 2)
                    {
                        pipeScipt.dirNum = 0;
                    }
                    else if (underpipeCtrl.dirNum == 3)
                    {
                        pipeScipt.dirNum = 1;
                    }
                }
                else if(!factoryCollider.GetComponent<UnderPipeCtrl>())
                {
                    Debug.Log("dd");
                    pipeScipt.dirNum = tempDir;

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
        tempDir = pipeScipt.dirNum;
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
