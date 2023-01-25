using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMonsterCtrl : MonsterAi
{
    protected override void RandomAttackNum(int attackNum, Transform targetTr)
    {
        attackState = AttackState.Attacking;

        animator.SetBool("isAttack", true);
        animator.SetFloat("attackMotion", 0);
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
