using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinRobot : UnitAi
{
    protected override void AttackStart()
    {
        animator.Play("Attack", -1, 0);
        animator.SetBool("isAttack", true);

        if (aggroTarget != null)
        {
            aggroTarget.GetComponent<MonsterAi>().TakeDamage(unitData.Damage);
        }
    }

}
