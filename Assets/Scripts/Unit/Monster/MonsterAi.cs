using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class MonsterAi : UnitCommonAi
{
    GameObject spawner;
    Transform spawnPos;
    float spawnDist;
    [SerializeField]
    float maxSpawnDist;

    Vector3 patRandomPos = Vector3.zero;
    protected int idle = -1;
    protected int attackMotion;
    List<string> targetTags = new List<string> { "Player", "Unit", "Tower", "Factory" };
    protected bool idleTimeStart = true;

    protected bool isScriptActive = false;
    protected int monsterType;    // 0 : 노멀, 1 : 강함, 2 : 가디언

    protected override void FixedUpdate()
    {
        if (isScriptActive)
            base.FixedUpdate();
        else
            ReturnPos();
    }

    protected override void Update()
    {
        if (isScriptActive)
            base.Update();
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
            case AIState.AI_ReturnPos:
                {
                    ReturnPos();
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

    protected void PatrolFunc()
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

    protected void PatrolRandomPosSet()
    {
        float spawnDis = (tr.position - spawnPos.position).magnitude;

        if (spawnDis > maxSpawnDist)
        {
            float randomAngle = Random.Range(0f, 2f * Mathf.PI);
            float randomDistance = Random.Range(0f, unitCommonData.PatrolRad);
            Vector3 randomPosition = spawnPos.position + new Vector3(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * randomDistance;

            patrolStartPos = randomPosition;


            if (checkPathCoroutine == null)
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

                patRandomPos = Random.insideUnitCircle * unitCommonData.PatrolRad * Random.Range(5, 10);
                patrolStartPos = tr.position + patRandomPos;

                if (checkPathCoroutine == null)
                {
                    checkPathCoroutine = StartCoroutine(CheckPath(patrolStartPos, "Patrol"));
                }
            }
        }
    }

    protected void ReturnPos()
    {
        if (movePath.Count <= currentWaypointIndex)
            return;

        Vector3 targetWaypoint = movePath[currentWaypointIndex];

        direction = targetWaypoint - tr.position;
        direction.Normalize();

        tr.position = Vector3.MoveTowards(tr.position, targetWaypoint, Time.deltaTime * (unitCommonData.MoveSpeed + 7));
        if (Vector3.Distance(tr.position, targetPosition) <= 0.3f)
        {
            MonsterScriptSet(false);
        }
        if (Vector3.Distance(tr.position, targetWaypoint) <= 0.3f)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= movePath.Count)
            {
                MonsterScriptSet(false);
            }
        }

        animator.SetBool("isMove", true);

        if (direction.magnitude > 0.5f)
        {
            AnimSetFloat(direction, true);
        }
    }

    protected IEnumerator IdleTime()
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

    protected override void AttackTargetDisCheck()
    {
        if (aggroTarget)
        {
            targetVec = (new Vector3(aggroTarget.transform.position.x, aggroTarget.transform.position.y, 0) - tr.position).normalized;
            targetDist = Vector3.Distance(tr.position, aggroTarget.transform.position);
            spawnDist = Vector3.Distance(tr.position, spawnPos.position);
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

        if((aIState == AIState.AI_NormalTrace || aIState == AIState.AI_Attack) && targetList.Count == 0)
        {
            idle = 0;
            aIState = AIState.AI_Idle;
            attackState = AttackState.Waiting;
        }
    }

    protected override void AttackTargetCheck()
    {
        if (aIState == AIState.AI_ReturnPos)
            return;

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
            RandomAttackNum(aggroTarget.transform);
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
        else if (moveFunc == "ReturnPos")
        {
            aIState = AIState.AI_ReturnPos;
        }

        SwBodyType(true);
        direction = targetPos - tr.position;
        checkPathCoroutine = null;
    }


    protected virtual void RandomAttackNum(Transform targetTr)
    {
        attackState = AttackState.Attacking;

        animator.SetBool("isAttack", true);
        animator.SetFloat("attackMotion", 0);
        animator.Play("Attack", -1, 0);
    }

    protected virtual void AttackMove() { }

    protected override void AttackEnd(string str)
    {
        base.AttackEnd(str);
        if (str == "false")
        {
            if (aggroTarget != null)
                AttackObjCheck(aggroTarget);
        }
    }

    protected override void DieFunc()
    {
        base.DieFunc();

        foreach (GameObject target in targetList)
        {
            if (target != null)
            {
                if (target.TryGetComponent(out UnitAi unit))
                {
                    unit.RemoveTarget(gameObject);
                }
                else if (target.TryGetComponent(out AttackTower tower))
                {
                    tower.RemoveMonster(gameObject);
                }
            }
        }

        spawner.GetComponent<MonsterSpawner>().MonsterDieChcek(gameObject, monsterType);

        Destroy(gameObject);
    }

    public void MonsterSpawnerSet(MonsterSpawner monsterSpawner, int type)
    {
        spawner = monsterSpawner.gameObject;
        spawnPos = spawner.transform;
        monsterType = type;
    }

    public void MonsterScriptSet(bool scriptState)
    {
        isScriptActive = scriptState;

        if(scriptState) //스크립트 애니메이션 콜라이더 세팅
        {
            enabled = true;
            animator.enabled = true;
            capsule2D.enabled = true;
        }
        else
        {
            spawnDist = Vector3.Distance(tr.position, spawnPos.position);
            if (spawnDist > maxSpawnDist)
            {
                checkPathCoroutine = StartCoroutine(CheckPath(spawnPos.position, "ReturnPos"));
                return;
            }
            else
            {
                enabled = false;
                animator.enabled = false;
                capsule2D.enabled = false;
            }
        }
    }
}
