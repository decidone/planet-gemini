using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UnderBeltCtrl : MonoBehaviour
{
    [SerializeField]
    GameObject GetBelt = null;
    [SerializeField]
    GameObject SendBelt = null;

    public bool isPreBuilding = false;

    public GameObject belt = null;
    public FactoryCtrl beltScipt = null;

    Vector2[] checkPos = new Vector2[4];

    public int dirNum = 0;

    // Update is called once per frame
    void Update()
    {// 기본적으로 send벨트이고 send벨트의 반대 방향으로 10 체크해서 다른 send벨트가 있을 때 get벨트로 변경
        if(isPreBuilding)
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

            if (factoryCollider.CompareTag("Factory") && factoryCollider.gameObject != belt)
            //&& !factoryCollider.GetComponent<FactoryCtrl>().isPreBuilding)
            {
                //FactoryCtrl factoryCtrl = factoryCollider.GetComponent<FactoryCtrl>();
                GetUnderBeltCtrl getUnderBeltCtrl = factoryCollider.GetComponent<GetUnderBeltCtrl>();
                SendUnderBeltCtrl sendUnderBeltCtrl = factoryCollider.GetComponent<SendUnderBeltCtrl>();

                if (getUnderBeltCtrl != null && getUnderBeltCtrl.dirNum == dirNum)
                {
                    SetSendUnderBelt();
                    return;
                }

                if (sendUnderBeltCtrl != null && sendUnderBeltCtrl.dirNum == dirNum)
                {
                    SetGetUnderBelt();
                    return;
                }
            }
        }

        ReturnSendBelt();
    }

    void ReturnSendBelt()
    {
        if (GetBelt.activeSelf == true)
        {
            SetSendUnderBelt();
        }
    }

    public void SetGetUnderBelt()
    {
        SendBelt.SetActive(false);
        GetBelt.SetActive(true);
        SetSlotColor(GetBelt.GetComponent<SpriteRenderer>(), Color.green, 0.35f);
        belt = GetBelt;
        beltScipt = belt.GetComponent<GetUnderBeltCtrl>();
        beltScipt.isPreBuilding = true;
        beltScipt.dirNum = dirNum;
    }

    public void SetSendUnderBelt()
    {
        GetBelt.SetActive(false);
        SendBelt.SetActive(true);
        belt = SendBelt;
        beltScipt = belt.GetComponent<SendUnderBeltCtrl>();
        beltScipt.isPreBuilding = true;
        beltScipt.dirNum = dirNum;
    }
    void SetSlotColor(SpriteRenderer sprite, Color color, float alpha)
    {
        Color slotColor = sprite.color;
        slotColor = color;
        slotColor.a = alpha;
        sprite.color = slotColor;
    }

    public void RemoveObj()
    {
        if (belt != null)
        {
            belt.transform.parent = null;
            //beltScipt.isPreBuilding = false;
        }

        Destroy(this.gameObject);
    }
}
