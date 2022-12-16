using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeltCtrl : MonoBehaviour
{
    public float dirNum = 0;    // ����
    public float modelNum = 0;  // ���

    protected Animator anim;
    protected Animator animsync;

    public float[] otherBeltNum = new float[] { 4, 4, 4, 4 };  //0 : ��, 1 : ��, 2 : ��, 3 : �� , 4 ����
    Vector2 pos;    // �� ��ġ

    // Start is called before the first frame update
    private void Awake()
    {
        animsync = GameObject.Find("BeltAnimSync").GetComponent<Animator>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        pos = this.gameObject.transform.position;

        anim.SetFloat("DirNum", dirNum);
        anim.SetFloat("ModelNum", modelNum);

        anim.Play(0, -1, animsync.GetCurrentAnimatorStateInfo(0).normalizedTime);
    }

    void ModelSelFunc()
    {
        if (dirNum == 0 || dirNum == 2) // ������ �� �Ʒ� �� �� 
        {
            if((otherBeltNum[1] == 3 && otherBeltNum[3] == 1) || (otherBeltNum[1] == 4 && otherBeltNum[3] == 4))
            {   // ������, ���� �Ѵ� ��Ʈ�� �ְų� ���� ��
                if (otherBeltNum[0] != 4 && otherBeltNum[2] != 4)       // ���� �Ʒ��� ��Ʈ�� ���� ��
                    modelNum = 2;
                else if (otherBeltNum[0] != 4 && otherBeltNum[2] == 4)  // ������ ��Ʈ�� ���� ��
                    modelNum = 3;
                else if (otherBeltNum[0] == 4 && otherBeltNum[2] != 4)  // �Ʒ��� ��Ʈ�� ���� ��
                    modelNum = 1;
                else
                    modelNum = 0;
            }
            else if (otherBeltNum[1] == 3)  // ������(otherBeltNum[1])�� �������� ���� ��Ʈ(3)�� ���� ��
                modelNum = 4;
            else if (otherBeltNum[3] == 1)  // ����(otherBeltNum[3])�� ���������� ���� ��Ʈ(1)�� ���� ��
                modelNum = 5;
        }
        else if (dirNum == 1 || dirNum == 3)    // ������ ������ ���� �� �� 
        {
            if ((otherBeltNum[0] == 2 && otherBeltNum[2] == 0) || (otherBeltNum[0] == 4 && otherBeltNum[2] == 4))
            {   // ��, �Ʒ� �Ѵ� ��Ʈ�� �ְų� ���� ��
                if (otherBeltNum[1] != 4 && otherBeltNum[3] != 4)       // �����ʶ� ���ʿ� ��Ʈ�� ���� ��
                    modelNum = 2;
                else if (otherBeltNum[1] == 4 && otherBeltNum[3] != 4)  // ���ʿ��� ��Ʈ�� ���� ��
                    modelNum = 3;
                else if (otherBeltNum[1] != 4 && otherBeltNum[3] == 4)  // �����ʿ��� ��Ʈ�� ���� ��
                    modelNum = 1;
                else
                    modelNum = 0;
            }
            else if (otherBeltNum[0] == 2)  // ��(otherBeltNum[0])�� �Ʒ��� ���� ��Ʈ(2)�� ���� ��
                modelNum = 4;
            else if (otherBeltNum[2] == 0)  // �Ʒ�(otherBeltNum[2])�� ���� ���� ��Ʈ(0)�� ���� ��
                modelNum = 5;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Belt"))
        {
            if (collision.transform.position.x == pos.x)//���� x ��ǥ��
            {
                if (collision.transform.position.y > pos.y)//y ��ǥ�� ������
                {
                    otherBeltNum[0] = collision.GetComponentInChildren<BeltCtrl>().dirNum;
                }
                else if (collision.transform.position.y < pos.y)//y ��ǥ�� ������
                {
                    otherBeltNum[2] = collision.GetComponentInChildren<BeltCtrl>().dirNum;
                }
                else
                    return;
            }
            else if (collision.transform.position.y == pos.y)//���� y ��ǥ��
            {
                if (collision.transform.position.x > pos.x)//x ��ǥ�� ������
                {
                    otherBeltNum[1] = collision.GetComponentInChildren<BeltCtrl>().dirNum;
                }
                else if (collision.transform.position.x < pos.x)//x ��ǥ�� ������
                {
                    otherBeltNum[3] = collision.GetComponentInChildren<BeltCtrl>().dirNum;
                }
                else
                    return;
            }
            ModelSelFunc();
        }
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Belt"))
        {
            if (collision.transform.position.x == pos.x)//���� x ��ǥ��
            {
                if (collision.transform.position.y > pos.y)//y ��ǥ�� ������
                {
                    otherBeltNum[0] = 4;
                }
                else if (collision.transform.position.y < pos.y)//y ��ǥ�� ������
                {
                    otherBeltNum[2] = 4;
                }
                else
                    return;
            }
            else if (collision.transform.position.y == pos.y)//���� y ��ǥ��
            {
                if (collision.transform.position.x > pos.x)//x ��ǥ�� ������
                {
                    otherBeltNum[1] = 4;
                }
                else if (collision.transform.position.x < pos.x)//x ��ǥ�� ������
                {
                    otherBeltNum[3] = 4;
                }
                else
                    return;
            }
            ModelSelFunc();
        }
    }
}