using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class SpinRobot : UnitAi
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
            foreach (GameObject gameObject in targetList)
            {
                float distance = Vector3.Distance(tr.position, gameObject.transform.position);
                if (distance > unitCommonData.AttackDist)
                    continue;

                if (gameObject.TryGetComponent(out MonsterAi monster))
                {
                    monster.TakeDamage(damage, 0);
                }
                else if (gameObject.TryGetComponent(out MonsterSpawner spawner))
                {
                    spawner.TakeDamage(damage, gameObject);
                }
            }
        }
    }
}
