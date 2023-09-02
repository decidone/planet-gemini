using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class SpinRobot : UnitAi
{
    protected override void AttackStart()
    {
        animator.Play("Attack", -1, 0);
        animator.SetBool("isAttack", true);


    }

    protected override void AttackEnd(string str)
    {
        base.AttackEnd(str);
        if (aggroTarget != null)
        {
            aggroTarget.GetComponent<MonsterAi>().TakeDamage(unitCommonData.Damage);
        }
    }
}
