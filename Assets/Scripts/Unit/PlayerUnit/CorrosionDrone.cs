using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorrosionDrone : UnitAi
{
    [SerializeField] float debuffTimer;
    [SerializeField] float debuffInterval;
    [SerializeField] float debuffPer;

    protected override void Update()
    {
        base.Update();

        if (targetList.Count > 0)
        {
            debuffTimer += Time.deltaTime;
            if (debuffTimer >= debuffInterval)
            {
                foreach (GameObject obj in targetList)
                {
                    if (!obj && obj.CompareTag("Monster"))
                    {
                        if (obj.TryGetComponent(out MonsterAi mon))
                        {
                            mon.RefreshDebuffServerRpc(1, debuffPer);    // 서버, 클라이언트 상관없이 디버프 띄워주는데 데미지 계산은 서버 디버프 유무로만 계산
                        }
                    }
                }
                debuffTimer = 0f;
            }
        }
    }

    protected override void UnitAiCtrl()
    {
        switch (aIState)
        {
            case AIState.AI_Move:
                MoveFunc();
                break;
            case AIState.AI_Patrol:
                PatrolFunc();
                break;
            case AIState.AI_Attack:
                break;
            case AIState.AI_NormalTrace:
                {
                    NormalTrace();
                    AttackCheck();
                }
                break;
        }
    }

    protected override void SearchObjectsInRange()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(tr.position, unitCommonData.ColliderRadius);

        if (colliders.Length > 0)
        {
            foreach (Collider2D collider in colliders)
            {
                GameObject monster = collider.gameObject;
                if (monster.CompareTag("Monster") || monster.CompareTag("Spawner"))
                {
                    if (!targetList.Contains(monster))
                    {
                        targetList.Add(monster);
                    }

                    if (targetList.Count > 0 && !animator.GetBool("isAttack"))
                    {
                        animator.Play("Attack", -1, 0);
                        animator.SetBool("isAttack", true);
                    }
                }
            }
        }

        if (targetList.Count == 0)
        {
            animator.SetBool("isAttack", false);
        }
    }

    public override void RemoveTarget(GameObject target)
    {
        if (targetList.Contains(target))
        {
            targetList.Remove(target);
        }
        if (targetList.Count == 0)
        {
            aggroTarget = null;
        }

        if (targetList.Count == 0 && isLastStateOn)
        {
            aIState = unitLastState;

            if (aIState == AIState.AI_Move)
            {
                checkPathCoroutine = StartCoroutine(CheckPath(targetPosition, "Move"));
            }

            isLastStateOn = false;
            unitLastState = AIState.AI_Idle;
            animator.SetBool("isAttack", false);
        }
        if (isTargetSet && aggroTarget == target)
        {
            aggroTarget = null;
            isTargetSet = false;
            animator.Play("Idle", -1, 0);
            animator.SetBool("isAttack", false);
        }
    }
}
