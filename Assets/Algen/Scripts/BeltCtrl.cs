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

    protected Animator anim;
    protected Animator animsync;

    Vector3 pos;

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
        anim.SetFloat("DirNum", dirNum);
        anim.SetFloat("ModelNum", modelNum);
        pos = this.gameObject.transform.position;

        anim.Play(0, -1, animsync.GetCurrentAnimatorStateInfo(0).normalizedTime);

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
            if ((dirNum == 0 || dirNum == 2) && collision.transform.position.x == pos.x)//棻遴, 機
            {
                if (collision.transform.position.y > pos.y)
                {
                    upExist = true;
                }
                else if (collision.transform.position.y < pos.y)
                {
                    downExist = true;
                }
                else
                    return;
            }

            else if ((dirNum == 1 || dirNum == 3) && collision.transform.position.y == pos.y)//謝, 辦
            {
                if (collision.transform.position.x > pos.x)
                {
                    leftExit = true;
                }
                else if (collision.transform.position.x < pos.x)
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
            if ((dirNum == 0 || dirNum == 2) && collision.transform.position.x == pos.x)//棻遴, 機
            {
                if (collision.transform.position.y > pos.y)
                {
                    upExist = false;
                }
                else if (collision.transform.position.y < pos.y)
                {
                    downExist = false;
                }
                else
                    return;
            }

            else if ((dirNum == 1 || dirNum == 3) && collision.transform.position.y == pos.y)//謝, 辦
            {
                if (collision.transform.position.x > pos.x)
                {
                    leftExit = false;
                }
                else if (collision.transform.position.x < pos.x)
                {
                    rightExit = false;
                }
                else
                    return;
            }
        }
    }
}