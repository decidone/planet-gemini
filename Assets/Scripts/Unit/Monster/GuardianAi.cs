using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// UTF-8 설정
public class GuardianAi : MonsterAi
{
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
                    if (aggroTarget)
                        AttackCheck();
                }
                else if (attackState == AttackState.AttackStart)
                {
                    Attack();
                }
                break;
            case AIState.AI_NormalTrace:
                {
                    if (aggroTarget)
                        AttackCheck();
                    NormalTrace();
                }
                break;
            case AIState.AI_ReturnPos:
                {
                    ReturnPos();
                }
                break;
            case AIState.AI_SpawnerCall:
                {
                    SpawnerCall();
                    if (aggroTarget)
                        AttackCheck();
                }
                break;
        }
    }

    public override void SpawnerCallCheck(WorldObj obj)
    {
        if (obj == null)
            return;

        if (aIState == AIState.AI_Idle || aIState == AIState.AI_Patrol)
        {
            spawnerPhaseOn = true;
            aggroTarget = obj;
            targetVec = (new Vector3(aggroTarget.transform.position.x, aggroTarget.transform.position.y, 0) - tr.position).normalized;

            checkPathCoroutine = StartCoroutine(CheckPath(obj.transform.position, "SpawnerCall"));
            aIState = AIState.AI_SpawnerCall;
        }
    }
}
