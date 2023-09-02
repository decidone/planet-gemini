using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class StoneGolemCtrl : MonsterAi
{
    protected override void RandomAttackNum(int attackNum, Transform targetTr)
    {
        attackState = AttackState.Attacking;

        attackMotion = Random.Range(0, attackNum);
        animator.SetBool("isAttack", true);
        animator.SetFloat("attackMotion", attackMotion);
        animator.Play("Attack", -1, 0);
    }

    protected override void AttackEnd(string str)
    {
        base.AttackEnd(str);
        if (str == "false")
        {
            if (aggroTarget != null)
                AttackObjCheck(aggroTarget);
        }
    }
}
