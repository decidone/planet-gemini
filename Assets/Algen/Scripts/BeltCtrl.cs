using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeltCtrl : MonoBehaviour
{
    public float dirNum = 0;
    public float modelNum = 0;

    protected Animator anim;
    protected Animator animsync;

    public float[] otherBeltNum = new float[] { 4, 4, 4, 4 };  //0 : »ó, 1 : ¿ì, 2 : ÇÏ, 3 : ÁÂ , 4 ¾øÀ½
    Vector2 pos;

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
        if (dirNum == 0 || dirNum == 2)
        {
            if((otherBeltNum[1] == 3 && otherBeltNum[3] == 1) || (otherBeltNum[1] == 4 && otherBeltNum[3] == 4))
            {
                if (otherBeltNum[0] != 4 && otherBeltNum[2] != 4)
                    modelNum = 2;
                else if (otherBeltNum[0] != 4 && otherBeltNum[2] == 4)
                    modelNum = 3;
                else if (otherBeltNum[0] == 4 && otherBeltNum[2] != 4)
                    modelNum = 1;
                else
                    modelNum = 0;
            }
            else if (otherBeltNum[1] == 3)
                modelNum = 5;
            else if (otherBeltNum[3] == 1)
                modelNum = 4;
        }
        else if (dirNum == 1 || dirNum == 3)
        {
            if ((otherBeltNum[0] == 2 && otherBeltNum[2] == 0) || (otherBeltNum[0] == 4 && otherBeltNum[2] == 4))
            {
                if (otherBeltNum[1] != 4 && otherBeltNum[3] != 4)
                    modelNum = 2;
                else if (otherBeltNum[1] == 4 && otherBeltNum[3] != 4)
                    modelNum = 3;
                else if (otherBeltNum[1] != 4 && otherBeltNum[3] == 4)
                    modelNum = 1;
                else
                    modelNum = 0;
            }
            else if (otherBeltNum[0] == 2)
                modelNum = 5;
            else if (otherBeltNum[2] == 0)
                modelNum = 4;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Belt"))
        {
            if (collision.transform.position.x == pos.x)//°°Àº x ÁÂÇ¥°í
            {
                if (collision.transform.position.y > pos.y)//y ÁÂÇ¥°¡ ³ôÀ¸¸é
                {
                    otherBeltNum[0] = collision.GetComponentInChildren<BeltCtrl>().dirNum;
                }
                else if (collision.transform.position.y < pos.y)//y ÁÂÇ¥°¡ ³·À¸¸é
                {
                    otherBeltNum[2] = collision.GetComponentInChildren<BeltCtrl>().dirNum;
                }
                else
                    return;
            }
            else if (collision.transform.position.y == pos.y)//°°Àº y ÁÂÇ¥°í
            {
                if (collision.transform.position.x > pos.x)//x ÁÂÇ¥°¡ ³ôÀ¸¸é
                {
                    otherBeltNum[1] = collision.GetComponentInChildren<BeltCtrl>().dirNum;
                }
                else if (collision.transform.position.x < pos.x)//x ÁÂÇ¥°¡ ³·À¸¸é
                {
                    otherBeltNum[3] = collision.GetComponentInChildren<BeltCtrl>().dirNum;
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
            if (collision.transform.position.x == pos.x)//°°Àº x ÁÂÇ¥°í
            {
                if (collision.transform.position.y > pos.y)//y ÁÂÇ¥°¡ ³ôÀ¸¸é
                {
                    otherBeltNum[0] = 4;
                }
                else if (collision.transform.position.y < pos.y)//y ÁÂÇ¥°¡ ³·À¸¸é
                {
                    otherBeltNum[2] = 4;
                }
                else
                    return;
            }
            else if (collision.transform.position.y == pos.y)//°°Àº y ÁÂÇ¥°í
            {
                if (collision.transform.position.x > pos.x)//x ÁÂÇ¥°¡ ³ôÀ¸¸é
                {
                    otherBeltNum[1] = 4;
                }
                else if (collision.transform.position.x < pos.x)//x ÁÂÇ¥°¡ ³·À¸¸é
                {
                    otherBeltNum[3] = 4;
                }
                else
                    return;
            }
        }
    }
}