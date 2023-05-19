using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemFXCtrl : MonoBehaviour
{
    public Animator animator;
    Vector3 moveNextStep = Vector3.zero;    // ¿Ãµø πÊ«‚ ∫§≈Õ
    public Transform aggroTarget = null;   // ≈∏∞Ÿ
    int attackMotionNum;
    bool isAnimEnd = false;
    float damage = 0;

    // Update is called once per frame
    void Update()
    {        
        AttackFunc();

    }

    public void FXEnd()
    {
        isAnimEnd = true;
    }

    void AttackFunc()
    {
        if (attackMotionNum == 0)
        {
            //this.gameObject.GetComponent<BoxCollider2D>().enabled = false;
            ImgMrror();
            if (isAnimEnd == true)
            {
                Destroy(this.gameObject, 0.1f);
            }
        }
        else if (attackMotionNum == 1)
        {
            if (isAnimEnd == true)
            {
                Destroy(this.gameObject, 0.1f);
            }
        }
        else if (attackMotionNum == 2)
        {
            transform.position += moveNextStep * 2 * Time.fixedDeltaTime;
            Destroy(this.gameObject, 3f);
        }
    }

    void FXMove()
    {
        
    }//void MonsterMove()
    void ImgMrror()
    {
        if (moveNextStep.x > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (moveNextStep.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }//void ImgMrror()

    public void GetTarget(Vector3 target, int attackMotion, float getDamage)
    {
        moveNextStep = (target - transform.position).normalized;
        //moveNextStep.Normalize();
        damage = getDamage;
        attackMotionNum = attackMotion;
    }

    public void CollOn()
    {
        //this.gameObject.GetComponent<BoxCollider2D>().enabled = true;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.GetComponent<PlayerStatus>())
        {            
            if (collision.isTrigger == false)
            {
                collision.GetComponent<PlayerStatus>().TakeDamage(damage);
            }
        }
        else if (collision.GetComponent<UnitAi>())
        {
            if (collision.isTrigger == false)
            {
                collision.GetComponent<UnitAi>().TakeDamage(damage);
            }
        }
        else if (collision.GetComponent<TowerAi>())
        {
            if (collision.isTrigger == false)
            {
                collision.GetComponent<TowerAi>().TakeDamage(damage);
            }
        }
        if (attackMotionNum == 2)
        {
            Destroy(this.gameObject, 0.2f);
        }
    }//private void OnTriggerEnter2D(Collider2D collision)
}
