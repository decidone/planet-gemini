using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneGolemCtrl : MonsterAi
{
    protected override void RandomAttackNum(int attackNum, Transform targetTr)
    {
        attackState = MonsterAttackState.Attacking;

        attackMotion = Random.Range(0, attackNum);
        animator.SetBool("isAttack", true);
        animator.SetFloat("attackMotion", attackMotion);
        animator.Play("Attack", -1, 0);
    }

    void AttackEnd(string str)
    {
        if (str == "false")
        {
            animator.SetBool("isAttack", false);
            attackState = MonsterAttackState.AttackEnd;
            if (aggroTarget != null)
            {
                if (aggroTarget.GetComponent<PlayerStatus>())                
                    aggroTarget.GetComponent<PlayerStatus>().TakeDamage(getMonsterData.monsteData.Damage);                
                else if (aggroTarget.GetComponent<UnitAi>())
                    aggroTarget.GetComponent<UnitAi>().TakeDamage(getMonsterData.monsteData.Damage);
                else if (aggroTarget.GetComponent<TowerAi>())
                    aggroTarget.GetComponent<TowerAi>().TakeDamage(getMonsterData.monsteData.Damage);
            }
            StartCoroutine("AttackDelay");
        }
    }
}
