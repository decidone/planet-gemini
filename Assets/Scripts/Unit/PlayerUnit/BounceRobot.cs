using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

// UTF-8 설정
public class BounceRobot : UnitAi
{
    protected override bool AttackStart()
    {
        Debug.Log("aggroTarget: " + aggroTarget);
        bool isAttacked = false;

        if (aggroTarget != null)
        {
            isAttacked = true;
            animator.Play("Attack", -1, 0);
            animator.SetBool("isAttack", true);
            soundManager.PlaySFX(gameObject, "unitSFX", "MeleeAttack");
        }

        return isAttacked;
    }

    protected override void AttackEnd()
    {
        base.AttackEnd();
        if (IsServer && aggroTarget != null)
        {
            if (aggroTarget.TryGetComponent(out MonsterAi monster))
            {
                monster.TakeDamage(damage, 0);
            }
            else if (aggroTarget.TryGetComponent(out MonsterSpawner spawner))
            {
                spawner.TakeDamage(damage, gameObject);
            }
        }
    }
}
