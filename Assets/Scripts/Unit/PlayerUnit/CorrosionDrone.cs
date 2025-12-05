using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CorrosionDrone : UnitAi
{
    [SerializeField] float debuffTimer;
    [SerializeField] float debuffInterval;
    [SerializeField] float debuffPer;

    [SerializeField] int debuffAmount;
    [SerializeField] List<MonsterAi> debuffTargetList = new List<MonsterAi>();

    protected override void Update()
    {
        base.Update();

        if (targetList.Count > 0)
        {
            debuffTimer += Time.deltaTime;
            if (debuffTimer >= debuffInterval)
            {
                DebuffFunc();
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
                    if (aggroTarget)
                        AttackCheck();
                    NormalTrace();
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
        else
        {
            if(debuffTargetList.Count < debuffAmount)
                DebuffFunc();
        }
    }

    public override void RemoveTarget(GameObject target)
    {
        if(debuffTargetList.Contains(target.GetComponent<MonsterAi>()))
        {
            debuffTargetList.Remove(target.GetComponent<MonsterAi>());
        }

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
    
    protected override void RemoveObjectsOutOfRange()
    {
        base.RemoveObjectsOutOfRange();

        var removedTargets = debuffTargetList
            .Where(target =>
            !target || Vector2.Distance(tr.position, target.transform.position) > unitCommonData.ColliderRadius)
            .ToList();

        foreach (MonsterAi mon in removedTargets)
        {
            if (mon)
                mon.isDebuffed = false;
        }

        debuffTargetList.RemoveAll(target =>
            !target || Vector2.Distance(tr.position, target.transform.position) > unitCommonData.ColliderRadius);
    }

    void DebuffFunc()
    {
        List<MonsterAi> monstersList = new List<MonsterAi>(); // 정렬용 리스트

        foreach (GameObject obj in targetList)
        {
            if (obj && obj.TryGetComponent(out MonsterAi monsterAi))
            {
                if(monsterAi && !monsterAi.isDebuffed)
                    monstersList.Add(monsterAi);
            }
        }

        var addTargets = monstersList
            .OrderBy(monster => Vector2.Distance(tr.position, monster.transform.position))
            .Take(debuffAmount - debuffTargetList.Count);

        foreach (var m in addTargets)
        {
            debuffTargetList.Add(m);
        }

        //debuffTargetList = monstersList
        //    .OrderBy(monster => Vector2.Distance(tr.position, monster.transform.position))
        //    .Take(debuffAmount)
        //    .ToList();

        foreach (MonsterAi monster in debuffTargetList)
        {
            if (monster && monster.CompareTag("Monster"))
            {
                monster.RefreshDebuffServerRpc(1, debuffPer);    // 서버, 클라이언트 상관없이 디버프 띄워주는데 데미지 계산은 서버 디버프 유무로만 계산
            }
        }
    }
}
