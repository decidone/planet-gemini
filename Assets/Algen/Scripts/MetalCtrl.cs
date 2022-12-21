using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetalCtrl : MonsterAi
{
    protected override void RandomAttackNum(int attackNum, Transform targetTr)
    {
        attackState = AttackState.Attacking;

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
            attackState = AttackState.AttackEnd;

            StartCoroutine("AttackDelay");
        }
    }
}
