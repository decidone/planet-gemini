using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemFXCtrl : MonoBehaviour
{
    public Animator animator;
    Vector3 moveNextStep = Vector3.zero;    // ÀÌµ¿ ¹æÇâ º¤ÅÍ
    public Transform aggroTarget = null;   // Å¸°Ù
    int attackMotionNum;
    bool isAnimEnd = false;
    float damage = 0;

    void Update()
    {        
        AttackFunc();
    }

    public void FXEnd(){ isAnimEnd = true; }

    void AttackFunc()
    {
        if (attackMotionNum == 0)
        {
            ImgMrror();
            if (isAnimEnd)
            {
                Destroy(this.gameObject, 0.1f);
            }
        }
        else if (attackMotionNum == 1)
        {
            if (isAnimEnd)
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

    public void FXMove() { }

    void ImgMrror()
    {
        if (moveNextStep.x > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (moveNextStep.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }
    
    public void GetTarget(Vector3 target, int attackMotion, float getDamage)
    {
        moveNextStep = (target - transform.position).normalized;
        damage = getDamage;
        attackMotionNum = attackMotion;
    }

    public void CollOn() { }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerStatus>())
        {            
            if (!collision.isTrigger)
            {
                collision.GetComponent<PlayerStatus>().TakeDamage(damage);
            }
        }
        else if (collision.GetComponent<UnitAi>())
        {
            if (!collision.isTrigger)
            {
                collision.GetComponent<UnitAi>().TakeDamage(damage);
            }
        }
        else if (collision.GetComponent<TowerAi>())
        {
            if (!collision.isTrigger)
            {
                collision.GetComponent<TowerAi>().TakeDamage(damage);
            }
        }
        if (attackMotionNum == 2)
        {
            Destroy(this.gameObject, 0.2f);
        }
    }
}
