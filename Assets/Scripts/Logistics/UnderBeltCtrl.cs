using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// UTF-8 설정
public class UnderBeltCtrl : MonoBehaviour
{
    public GameObject getBelt;
    public GameObject sendBelt;

    public bool isPreBuilding = false;

    public GameObject underBelt;
    public bool isSendBelt = true;
    public Structure beltScipt;

    Vector2[] checkPos = new Vector2[4];

    public int dirNum;
    GetUnderBeltCtrl getUnderBeltCtrl;
    SendUnderBeltCtrl sendUnderBeltCtrl;
    public bool buildEnd = false;

    void Start()
    {
        SetSlotColor(getBelt.GetComponent<SpriteRenderer>(), Color.green, 0.35f);
        SetSlotColor(sendBelt.GetComponent<SpriteRenderer>(), Color.green, 0.35f);
    }

    void Update()
    {
        // 기본적으로 send벨트이고 send벨트의 반대 방향으로 10 체크해서 다른 send벨트가 있을 때 get벨트로 변경
        if(isPreBuilding)
        {
            CheckPos();
            if (!buildEnd)
            {
                CheckNearObj(checkPos[0]);
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

            if (factoryCollider.CompareTag("Factory") && factoryCollider.gameObject != underBelt
                && factoryCollider.gameObject.transform.position != underBelt.transform.position)
            {
                getUnderBeltCtrl = factoryCollider.GetComponent<GetUnderBeltCtrl>();
                sendUnderBeltCtrl = factoryCollider.GetComponent<SendUnderBeltCtrl>();
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
        if (getBelt.activeSelf)
        {
            SetSendUnderBelt();
        }
    }
    public void BuildingSetting(int _level, int _height, int _width, int _dirCount)
    {
        getBelt.SetActive(true);
        sendBelt.SetActive(true);
        Structure getObj = getBelt.GetComponent<Structure>();
        Structure sendObj = sendBelt.GetComponent<Structure>();
        getObj.BuildingSetting(_level, _height, _width, _dirCount);
        sendObj.BuildingSetting(_level, _height, _width, _dirCount);
        getBelt.SetActive(false);
        sendBelt.SetActive(false);
    }

    public void SetGetUnderBelt()
    {
        sendBelt.SetActive(false);
        getBelt.SetActive(true);
        underBelt = getBelt;
        isSendBelt = false;
        beltScipt = underBelt.GetComponent<GetUnderBeltCtrl>();
        ColliderTriggerOnOff(true);
        beltScipt.isPreBuilding = true;
        beltScipt.dirNum = dirNum;
    }

    public void SetSendUnderBelt()
    {
        getBelt.SetActive(false);
        sendBelt.SetActive(true);
        underBelt = sendBelt;
        isSendBelt = true;
        beltScipt = underBelt.GetComponent<SendUnderBeltCtrl>();
        ColliderTriggerOnOff(true);
        beltScipt.isPreBuilding = true;
        beltScipt.dirNum = dirNum;
    }

    public void SetColor(Color color)
    {
        SpriteRenderer getRen = getBelt.GetComponent<SpriteRenderer>();
        SpriteRenderer senRen = sendBelt.GetComponent<SpriteRenderer>();

        SetSlotColor(getRen, color, 0.35f);
        SetSlotColor(senRen, color, 0.35f);
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
        beltScipt.ColliderTriggerOnOff(isOn);
    }

    public void RemoveObj()
    {
        if (underBelt != null)
        {
            underBelt.transform.parent = null;
        }

        Destroy(this.gameObject);
    }
}
