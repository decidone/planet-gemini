using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeltCtrl : MonoBehaviour
{
    public float dirNum = 0;
    public float modelNum = 0;

    bool upExist = false;
    bool downExist = false;
    bool leftExit = false;
    bool rightExit = false;

    private Animator anim;

    Vector3 Pos;

    BeltRootCtrl BRCtrl;

    // Start is called before the first frame update
    private void Awake()
    {
        anim = GetComponent<Animator>();
        BRCtrl = GetComponentInParent<BeltRootCtrl>();
        BRCtrl.AddAnimArr();
        anim.Play("BlendAnim", -1, BRCtrl.animTime % 1);             
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        anim.SetFloat("DirNum", dirNum);
        anim.SetFloat("ModelNum", modelNum);
        Pos = this.gameObject.transform.position;

        ModelSelFunc();
    }

    private void FixedUpdate()
    {
 
    }

    void ModelSelFunc()
    {
        if ((upExist == true && downExist == false) || (leftExit == false && rightExit == true))
            modelNum = 3;
        else if ((upExist == true && downExist == true) || (leftExit == true && rightExit == true))
            modelNum = 2;
        else if ((upExist == false && downExist == true) || (leftExit == true && rightExit == false))
            modelNum = 1;
        else
            modelNum = 0;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Belt"))
        {
            if ((dirNum == 0 || dirNum == 2) && collision.transform.position.x == Pos.x)//�ٿ�, ��
            {
                if (collision.transform.position.y > Pos.y)
                {
                    upExist = true;
                }
                else if (collision.transform.position.y < Pos.y)
                {
                    downExist = true;
                }
                else
                    return;
            }

            else if ((dirNum == 1 || dirNum == 3) && collision.transform.position.y == Pos.y)//��, ��
            {
                if (collision.transform.position.x > Pos.x)
                {
                    leftExit = true;
                }
                else if (collision.transform.position.x < Pos.x)
                {
                    rightExit = true;
                }
                else
                    return;
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Belt"))
        {
            if ((dirNum == 0 || dirNum == 2) && collision.transform.position.x == Pos.x)//�ٿ�, ��
            {
                if (collision.transform.position.y > Pos.y)
                {
                    upExist = false;
                }
                else if (collision.transform.position.y < Pos.y)
                {
                    downExist = false;
                }
                else
                    return;
            }

            else if ((dirNum == 1 || dirNum == 3) && collision.transform.position.y == Pos.y)//��, ��
            {
                if (collision.transform.position.x > Pos.x)
                {
                    leftExit = false;
                }
                else if (collision.transform.position.x < Pos.x)
                {
                    rightExit = false;
                }
                else
                    return;
            }
        }
    }
}