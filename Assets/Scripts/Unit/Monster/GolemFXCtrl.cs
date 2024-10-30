using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

// UTF-8 설정
public class GolemFXCtrl : NetworkBehaviour
{
    public Animator animator;
    public Transform aggroTarget = null;   // 타겟
    bool isAnimEnd = false;
    float damage = 0;

    void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        AttackFunc();
    }

    public void FXEnd(){ isAnimEnd = true; }

    void AttackFunc()
    {
        if (isAnimEnd)
        {
            Destroy(this.gameObject);
        }        
    }

    public void FXMove() { }

    public void TargetPosAndDamage(float getDamage)
    {
        damage = getDamage;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer)
            return;
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
                collision.GetComponent<UnitAi>().TakeDamage(damage, 0);
            }
        }
        else if (collision.GetComponent<TowerAi>())
        {
            if (!collision.isTrigger)
            {
                collision.GetComponent<TowerAi>().TakeDamage(damage);
            }
        }
    }
}
