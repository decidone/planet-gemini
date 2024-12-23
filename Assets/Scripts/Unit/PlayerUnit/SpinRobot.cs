using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class SpinRobot : UnitAi
{
    protected override bool AttackStart()
    {
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
            for (int i = targetList.Count - 1; i >= 0; i--)
            {
                if (targetList[i] == null)
                {
                    Debug.Log("error Null");
                }

                float distance = Vector3.Distance(tr.position, targetList[i].transform.position);
                if (distance > unitCommonData.AttackDist)
                    continue;

                if (targetList[i].TryGetComponent(out MonsterAi monster))
                {
                    monster.TakeDamage(damage, 0);
                }
                else if (targetList[i].TryGetComponent(out MonsterSpawner spawner))
                {
                    spawner.TakeDamage(damage, targetList[i]);
                }
            }
        }
    }
}
