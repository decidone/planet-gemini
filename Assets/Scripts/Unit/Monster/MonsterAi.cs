using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

// UTF-8 설정
public class MonsterAi : UnitCommonAi
{
    Transform spawnPos; 
    Vector3 patRandomPos = Vector3.zero;
    int idle = -1;
    protected int attackMotion;
    List<string> targetTags = new List<string> { "Player", "Unit", "Tower", "Factory" };    
    bool idleTimeStart = true;

    void Start()
    {
        spawnPos = GetComponentInParent<MonsterSpawner>().gameObject.transform;
    }

    protected override void UnitAiCtrl()
    {
        switch (aIState)
        {
            case AIState.AI_Idle:                
                if(idle == 0)
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
            case AIState.AI_AggroTrace:
                {

                }
                break;
        }
    }

    protected override void AnimSetFloat(Vector3 direction, bool isNotLast)
    {
        float moveStep = unitCommonData.MoveSpeed * Time.fixedDeltaTime;
        Vector2 moveDir = direction.normalized;
        Vector3 moveNextStep = moveDir * moveStep;

        if (moveNextStep.y > 0)
            animator.SetFloat("moveNextStepY", 1.0f);
        else if (moveNextStep.y <= 0)
            animator.SetFloat("moveNextStepY", -1.0f);

        if (moveDir.x > 0.75f || moveDir.x < -0.75f)
            animator.SetFloat("moveNextStepX", 1.0f);
        else if (moveDir.x <= 0.75f && moveDir.x >= -0.75f)
            animator.SetFloat("moveNextStepX", 0.0f);

        if (isFlip)
        {
            if (direction.x > 0)
            {
                if (!unitSprite.flipX)
                {
                    unitSprite.flipX = true;
                }
            }
            else if (direction.x < 0)
            {
                if (unitSprite.flipX)
                {
                    unitSprite.flipX = false;
                }
            }
        }
        else
        {
            if (direction.x > 0)
            {
                if (unitSprite.flipX)
                {
                    unitSprite.flipX = false;
                }
            }
            else if (direction.x < 0)
            {
                if (!unitSprite.flipX)
                {
                    unitSprite.flipX = true;
                }
            }
        }
    }

    void PatrolFunc()
    {
        if (movePath.Count <= currentWaypointIndex)
            return;

        Vector3 targetWaypoint = movePath[currentWaypointIndex];

        direction = targetWaypoint - tr.position;
        direction.Normalize();

        tr.position = Vector3.MoveTowards(tr.position, targetWaypoint, Time.deltaTime * unitCommonData.MoveSpeed);
        if (Vector3.Distance(tr.position, targetPosition) <= 0.3f)
        {
            aIState = AIState.AI_Idle;
            AnimSetFloat(lastMoveDirection, false);
            SwBodyType(false);
        }
        if (Vector3.Distance(tr.position, targetWaypoint) <= 0.3f)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= movePath.Count)
            {
                aIState = AIState.AI_Idle;
                AnimSetFloat(lastMoveDirection, false);
                SwBodyType(false);
            }
        }

        animator.SetBool("isMove", true);

        if (direction.magnitude > 0.5f)
        {
            AnimSetFloat(direction, true);
        }        
    }

    void PatrolRandomPosSet()
    {
        float spawnDis = (tr.position - spawnPos.position).magnitude;

        if (spawnDis > 7)
        {
            bool hasObj = true;

            while (hasObj)
            {
                float randomAngle = Random.Range(0f, 2f * Mathf.PI);
                float randomDistance = Random.Range(0f, unitCommonData.PatrolRad);
                Vector3 randomPosition = spawnPos.position + new Vector3(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * randomDistance;

                patrolStartPos = randomPosition;

                Collider2D[] hitColliders = Physics2D.OverlapCircleAll(patrolStartPos, 0.5f, LayerMask.GetMask("Obj"));
                if (hitColliders.Length == 0)
                    hasObj = false;
            }

            if (!hasObj && checkPathCoroutine == null)
            {
                checkPathCoroutine = StartCoroutine(CheckPath(patrolStartPos, "Patrol"));
            }
        }
        else
        {
            idle = Random.Range(0, 3);
            if (idle == 0)
            {
                patrolStartPos = tr.position;
                animator.SetBool("isMove", false);
                aIState = AIState.AI_Idle;
            }
            else
            {
                patrolStartPos = Vector3.zero;
                bool hasObj = true;

                while (hasObj)
                {
                    patRandomPos = Random.insideUnitCircle * unitCommonData.PatrolRad * Random.Range(1, 4);
                    patrolStartPos = tr.position + patRandomPos;

                    Collider2D[] hitColliders = Physics2D.OverlapCircleAll(patrolStartPos, 0.5f, LayerMask.GetMask("Obj"));
                    if (hitColliders.Length == 0)
                        hasObj = false;
                }

                if (!hasObj && checkPathCoroutine == null)
                {
                    checkPathCoroutine = StartCoroutine(CheckPath(patrolStartPos, "Patrol"));
                }
            }
        }
    }

    IEnumerator IdleTime()
    {
        idleTimeStart = false;
        int RandomTime = Random.Range(5, 8);
        yield return new WaitForSeconds(RandomTime);
        if (unitCanvas.activeSelf)
            unitCanvas.SetActive(false);
        idle = -1;
        idleTimeStart = true;
    }

    protected override void NormalTrace()
    {
        if (aggroTarget == null)
        {
            animator.SetBool("isMove", false);
            aIState = AIState.AI_Idle;
            return;
        }

        animator.SetBool("isMove", true);
        AnimSetFloat(targetVec, true);

        if (targetDist > unitCommonData.AttackDist)
        {
            if (currentWaypointIndex >= movePath.Count)
                return;

            Vector3 targetWaypoint = movePath[currentWaypointIndex];
            direction = targetWaypoint - tr.position;
            direction.Normalize();

            tr.position = Vector3.MoveTowards(tr.position, targetWaypoint, Time.deltaTime * unitCommonData.MoveSpeed);

            if (Vector3.Distance(tr.position, targetWaypoint) <= 0.3f)
            {
                currentWaypointIndex++;

                if (currentWaypointIndex >= movePath.Count)
                    return;
            }
        }
    }

    protected void AttackObjCheck(GameObject Obj)
    {
        if (targetDist > unitCommonData.AttackDist)
            return;

        else if (targetDist <= unitCommonData.AttackDist)
        {
            if (Obj != null)
            {
                if (Obj.GetComponent<PlayerStatus>())
                    Obj.GetComponent<PlayerStatus>().TakeDamage(unitCommonData.Damage);
                else if (Obj.GetComponent<UnitAi>())
                    Obj.GetComponent<UnitAi>().TakeDamage(unitCommonData.Damage);
                else if (Obj.GetComponent<TowerAi>())
                    Obj.GetComponent<TowerAi>().TakeDamage(unitCommonData.Damage);
                else if (Obj.GetComponent<Structure>())
                    Obj.GetComponent<Structure>().TakeDamage(unitCommonData.Damage);
            }
        }
    }

    protected override void SearchObjectsInRange()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(tr.position, unitCommonData.ColliderRadius);
        HashSet<GameObject> targetListSet = new HashSet<GameObject>(targetList);

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider2D collider = colliders[i];
            GameObject target = collider.gameObject;
            string targetTag = target.tag;

            if (targetTags.Contains(targetTag))
            {
                Structure structure = target.GetComponent<Structure>();
                if (structure != null && structure.col.isTrigger)
                {
                    continue;
                }
                else if (!targetListSet.Contains(target))
                {
                    targetListSet.Add(target);
                    targetList.Add(target);
                }

                PlayerController player = target.GetComponent<PlayerController>();
                if (player != null && !player.circleColl)
                {
                    continue;
                }
                else if (!targetListSet.Contains(target))
                {
                    targetListSet.Add(target);
                    targetList.Add(target);
                }
            }
        }

        if(aIState == AIState.AI_NormalTrace && targetList.Count == 0)
        {
            idle = 0;
            aIState = AIState.AI_Idle;
            attackState = AttackState.Waiting;
        }
    }

    protected override void AttackTargetCheck()
    {
        float closestDistance = float.MaxValue;

        foreach (GameObject target in targetList)
        {
            if (target != null)
            {
                float distance = Vector3.Distance(tr.position, target.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    aggroTarget = target;
                }
            }
        }

        if (aggroTarget != null)
        {
            if (checkPathCoroutine == null)
                checkPathCoroutine = StartCoroutine(CheckPath(aggroTarget.transform.position, "NormalTrace"));
            else
            {
                StopCoroutine(checkPathCoroutine);
                checkPathCoroutine = StartCoroutine(CheckPath(aggroTarget.transform.position, "NormalTrace"));
            }
        }        
    }

    protected override void AttackStart()
    {
        if(aggroTarget != null)
            RandomAttackNum(unitCommonData.AttackNum, aggroTarget.transform);
    }

    protected override IEnumerator CheckPath(Vector3 targetPos, string moveFunc)
    {
        ABPath path = ABPath.Construct(tr.position, targetPos, null);
        seeker.CancelCurrentPathRequest();
        seeker.StartPath(path);

        yield return StartCoroutine(path.WaitForPath());

        currentWaypointIndex = 0;

        movePath = path.vectorPath;

        if (moveFunc == "Move")
        {
            aIState = AIState.AI_Move;
        }
        else if (moveFunc == "Patrol")
        {
            aIState = AIState.AI_Patrol;
        }
        else if (moveFunc == "NormalTrace")
        {
            aIState = AIState.AI_NormalTrace;
        }

        SwBodyType(true);
        direction = targetPos - tr.position;
        checkPathCoroutine = null;
    }


    protected virtual void RandomAttackNum(int attackNum, Transform targetTr) { }

    protected virtual void AttackMove() { }

    protected override void DieFunc()
    {
        base.DieFunc();

        foreach (GameObject target in targetList)
        {
            if (target != null)
            {
                if (target.TryGetComponent(out UnitAi unit))
                {
                    unit.RemoveTarget(this.gameObject);
                }
                else if (target.TryGetComponent(out AttackTower tower))
                {
                    tower.RemoveMonster(this.gameObject);
                }
            }
        }
        Destroy(this.gameObject, 1f);
    }
}
