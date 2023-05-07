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

    // Start is called before the first frame update
    void Start()
    {

    }

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
            Collider2D hitCollider = hits[i].collider;

            if (hitCollider.CompareTag("Factory") &&
                !hitCollider.GetComponent<FactoryCtrl>().isPreBuilding &&
                hitCollider.GetComponent<GetUnderBeltCtrl>() != null)
            {
                break;
            }
            else if (hitCollider.CompareTag("Factory") &&
                !hitCollider.GetComponent<FactoryCtrl>().isPreBuilding &&
                hitCollider.GetComponent<SendUnderBeltCtrl>() != null)
            {
                ConnectionDirCheck(hitCollider.gameObject);
                return;
            }
        }

        ReturnSendBelt();
    }

    void ConnectionDirCheck(GameObject obj)
    {
        if(SendBelt.activeSelf == true)
        {
            SendUnderBeltCtrl sendUnderbelt = obj.GetComponent<SendUnderBeltCtrl>();
            if (sendUnderbelt.dirNum == dirNum)
            {
                SetGetUnderBelt();
            }
            else
                return;
        }
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

    //public void SetGetUnderBelt(int beltDir)
    //{
    //    if (belt != null)
    //        Destroy(belt);

    //    isSendBelt = false;
    //    belt = Instantiate(GetBelt, this.transform.position, Quaternion.identity);
    //    belt.transform.parent = this.transform;
    //    beltScipt = belt.GetComponent<GetUnderBeltCtrl>();
    //    beltScipt.isPreBuilding = true;
    //    beltScipt.dirNum = beltDir;
    //}

    //public void SetSendUnderBelt(int beltDir)
    //{
    //    if (belt != null)
    //        Destroy(belt);

    //    isSendBelt = true;
    //    belt = Instantiate(SendBelt, this.transform.position, Quaternion.identity);
    //    belt.transform.parent = this.transform;
    //    beltScipt = belt.GetComponent<SendUnderBeltCtrl>();
    //    beltScipt.isPreBuilding = true;
    //    beltScipt.dirNum = beltDir;
    //}

    public void RemoveObj()
    {
        if (belt != null)
        {
            belt.transform.parent = null;
            beltScipt.isPreBuilding = false;
        }

        Destroy(this.gameObject);
    }
}
