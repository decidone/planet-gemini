using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// UTF-8 설정
public class GuardianAi : MonsterAi
{
    protected override void Update()
    {
        if (aIState != AIState.AI_GuardianAggro)
        {
            base.Update();
        }
    }

    protected override void UnitAiCtrl()
    {
        switch (aIState)
        {
            case AIState.AI_Idle:
                if (idle == 0)
                {
                    if (idleTimeStart)
                        StartCoroutine(nameof(IdleTime));
                }
                else
                    PatrolRandomPosSet();
                break;
            case AIState.AI_Patrol:
                PatrolFunc();
                break;
            case AIState.AI_Attack:
                if (attackState == AttackState.Waiting)
                {
                    AttackCheck();
                }
                else if (attackState == AttackState.AttackStart)
                {
                    Attack();
                }
                break;
            case AIState.AI_NormalTrace:
                {
                    NormalTrace();
                    AttackCheck();
                }
                break;
            case AIState.AI_ReturnPos:
                {
                    ReturnPos();
                }
                break;
            case AIState.AI_GuardianAggro:
                {
                    GuardianAggro();
                }
                break;
        }
    }

    void GuardianAggro() 
    {
        animator.SetBool("isMove", true);
        AnimSetFloat(targetVec, true);

        targetDist = Vector3.Distance(tr.position, aggroTarget.transform.position);
 
        if (targetDist > unitCommonData.AttackDist)
        {
            if (currentWaypointIndex >= movePath.Count)
                return;

            Vector3 targetWaypoint = movePath[currentWaypointIndex];
            direction = targetWaypoint - tr.position;
            direction.Normalize();

            tr.position = Vector3.MoveTowards(tr.position, targetWaypoint, Time.deltaTime * (unitCommonData.MoveSpeed + 10));

            if (Vector3.Distance(tr.position, targetWaypoint) <= 0.3f)
            {
                currentWaypointIndex++;

                if (currentWaypointIndex >= movePath.Count)
                    return;
            }
        }
        else
        {
            aIState = AIState.AI_NormalTrace;
        }
    } 

    public void SpawnerCollCheck(GameObject obj)
    {
        if (obj == null)
            return;
        aggroTarget = obj;
        targetVec = (new Vector3(aggroTarget.transform.position.x, aggroTarget.transform.position.y, 0) - tr.position).normalized;

        checkPathCoroutine = StartCoroutine(CheckPath(obj.transform.position, ""));
        aIState = AIState.AI_GuardianAggro;
    }
}