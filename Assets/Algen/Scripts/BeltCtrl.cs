using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeltCtrl : MonoBehaviour
{
    public float dirNum = 0;    // 방향
    public float modelNum = 0;  // 모션

    protected Animator anim;
    protected Animator animsync;

    public float[] otherBeltNum = new float[] { 4, 4, 4, 4 };  //0 : 상, 1 : 우, 2 : 하, 3 : 좌 , 4 없음
    Vector2 pos;    // 현 위치

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
        if (dirNum == 0 || dirNum == 2) // 방향이 위 아래 일 때 
        {
            if((otherBeltNum[1] == 3 && otherBeltNum[3] == 1) || (otherBeltNum[1] == 4 && otherBeltNum[3] == 4))
            {   // 오른쪽, 왼쪽 둘다 벨트가 있거나 없을 때
                if (otherBeltNum[0] != 4 && otherBeltNum[2] != 4)       // 위랑 아래에 밸트가 있을 때
                    modelNum = 2;
                else if (otherBeltNum[0] != 4 && otherBeltNum[2] == 4)  // 위에만 벨트가 있을 때
                    modelNum = 3;
                else if (otherBeltNum[0] == 4 && otherBeltNum[2] != 4)  // 아래만 벨트가 있을 때
                    modelNum = 1;
                else
                    modelNum = 0;
            }
            else if (otherBeltNum[1] == 3)  // 오른쪽(otherBeltNum[1])에 왼쪽으로 가는 벨트(3)가 있을 때
                modelNum = 4;
            else if (otherBeltNum[3] == 1)  // 왼쪽(otherBeltNum[3])에 오른쪽으로 가는 벨트(1)가 있을 때
                modelNum = 5;
        }
        else if (dirNum == 1 || dirNum == 3)    // 방향이 오른쪽 왼쪽 일 때 
        {
            if ((otherBeltNum[0] == 2 && otherBeltNum[2] == 0) || (otherBeltNum[0] == 4 && otherBeltNum[2] == 4))
            {   // 위, 아래 둘다 벨트가 있거나 없을 때
                if (otherBeltNum[1] != 4 && otherBeltNum[3] != 4)       // 오른쪽랑 왼쪽에 밸트가 있을 때
                    modelNum = 2;
                else if (otherBeltNum[1] == 4 && otherBeltNum[3] != 4)  // 왼쪽에만 벨트가 있을 때
                    modelNum = 3;
                else if (otherBeltNum[1] != 4 && otherBeltNum[3] == 4)  // 오른쪽에만 벨트가 있을 때
                    modelNum = 1;
                else
                    modelNum = 0;
            }
            else if (otherBeltNum[0] == 2)  // 위(otherBeltNum[0])에 아래로 가는 벨트(2)가 있을 때
                modelNum = 4;
            else if (otherBeltNum[2] == 0)  // 아래(otherBeltNum[2])에 위로 가는 벨트(0)가 있을 때
                modelNum = 5;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Belt"))
        {
            if (collision.transform.position.x == pos.x)//같은 x 좌표고
            {
                if (collision.transform.position.y > pos.y)//y 좌표가 높으면
                {
                    otherBeltNum[0] = collision.GetComponentInChildren<BeltCtrl>().dirNum;
                }
                else if (collision.transform.position.y < pos.y)//y 좌표가 낮으면
                {
                    otherBeltNum[2] = collision.GetComponentInChildren<BeltCtrl>().dirNum;
                }
                else
                    return;
            }
            else if (collision.transform.position.y == pos.y)//같은 y 좌표고
            {
                if (collision.transform.position.x > pos.x)//x 좌표가 높으면
                {
                    otherBeltNum[1] = collision.GetComponentInChildren<BeltCtrl>().dirNum;
                }
                else if (collision.transform.position.x < pos.x)//x 좌표가 낮으면
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
            if (collision.transform.position.x == pos.x)//같은 x 좌표고
            {
                if (collision.transform.position.y > pos.y)//y 좌표가 높으면
                {
                    otherBeltNum[0] = 4;
                }
                else if (collision.transform.position.y < pos.y)//y 좌표가 낮으면
                {
                    otherBeltNum[2] = 4;
                }
                else
                    return;
            }
            else if (collision.transform.position.y == pos.y)//같은 y 좌표고
            {
                if (collision.transform.position.x > pos.x)//x 좌표가 높으면
                {
                    otherBeltNum[1] = 4;
                }
                else if (collision.transform.position.x < pos.x)//x 좌표가 낮으면
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