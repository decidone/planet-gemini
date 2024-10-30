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
        soundManager.PlaySFX(gameObject, "unitSFX", "MeleeAttack");
    }

    protected override void AttackEnd()
    {
        base.AttackEnd();
        if (IsServer && aggroTarget != null)
        {
            if (aggroTarget.TryGetComponent(out MonsterAi monster))
            {
                monster.TakeDamage(unitCommonData.Damage, 0);
            }
            else if (aggroTarget.TryGetComponent(out MonsterSpawner spawner))
            {
                spawner.TakeDamage(unitCommonData.Damage, gameObject);
            }
        }
    }
}
