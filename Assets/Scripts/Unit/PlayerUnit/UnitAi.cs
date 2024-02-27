using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

// UTF-8 설정
public class UnitAi : UnitCommonAi
{
    // 이동 관련
    float moveRadi;
    Vector3 lastPosition;
    bool isMoveCheckCoroutine = false;
    bool isNewPosSet = false;
    float movedDistance;

    // 페트롤 관련
    bool isPatrolMove = true;

    // 홀드 관련
    bool isHold = false;

    // 유닛 상태 관련
    bool isLastStateOn = false;

    AIState unitLastState = AIState.AI_Idle; // 시작 시 패트롤 상태
    public SpriteRenderer unitSelImg;
    bool unitSelect = false;
    UnitGroupCtrl unitGroupCtrl;
    // 공격 관련 변수
    List<Vector3> aggroPath = new List<Vector3>();    
    private int aggropointIndex; // 현재 이동 중인 경로 점 인덱스
    bool isTargetSet = false;
    bool isAttackMove = true;

    public float selfHealingAmount;
    public float selfHealInterval;
    float selfHealTimer;

    protected override void Start()
    {
        base.Start();
        unitGroupCtrl = GameManager.instance.GetComponent<UnitGroupCtrl>();
        selfHealInterval = 5;
        selfHealingAmount = 5f;
    }

    protected override void Update()
    {
        base.Update();
        if (hp != unitCommonData.MaxHp && aIState != AIState.AI_Die)
        {
            selfHealTimer += Time.deltaTime;

            if (selfHealTimer >= selfHealInterval)
            {
                hp += selfHealingAmount;
                hpBar.fillAmount = hp / unitCommonData.MaxHp;
                if(hp >= unitCommonData.MaxHp)                
                    unitCanvas.SetActive(false);                
                selfHealTimer = 0f;
            }
        }
    }

    protected override void UnitAiCtrl()
    {
        switch (aIState)
        {
            case AIState.AI_Idle:
                IdleFunc();
                break;            
            case AIState.AI_Move:
                MoveFunc();
                break;
            case AIState.AI_Patrol:
                PatrolFunc(isPatrolMove);
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
        }
    }

    void IdleFunc()
    {
        animator.SetBool("isMove", false);
        isMoveCheckCoroutine = false;
    }

    public void MovePosSet(Vector2 dir, float radi, bool isAttack)
    {
        isNewPosSet = true;
        isHold = false;
        isAttackMove = isAttack;
        currentWaypointIndex = 0;
        isMoveCheckCoroutine = false;
        isTargetSet = false;
        targetPosition = dir;
        moveRadi = radi;

        lastMoveDirection = (targetPosition - tr.position).normalized;

        if (checkPathCoroutine == null)
            checkPathCoroutine = StartCoroutine(CheckPath(targetPosition, "Move"));
        else
        {
            StopCoroutine(checkPathCoroutine);
            checkPathCoroutine = StartCoroutine(CheckPath(targetPosition, "Move"));
        }
        aIState = AIState.AI_Move;
    }

    protected override IEnumerator CheckPath(Vector3 targetPos, string moveFunc)
    {
        ABPath path = ABPath.Construct(tr.position, targetPos, null);
        seeker.CancelCurrentPathRequest();
        seeker.StartPath(path);

        yield return StartCoroutine(path.WaitForPath());

        currentWaypointIndex = 0;
        aggropointIndex = 0;

        if (moveFunc == "Move")
        {
            movePath = path.vectorPath;
            aIState = AIState.AI_Move;
        }
        else if (moveFunc == "Patrol")
        {
            movePath = path.vectorPath;
            isPatrolMove = true;
            aIState = AIState.AI_Patrol;
        }
        else if (moveFunc == "NormalTrace")
        {
            aggroPath = path.vectorPath;

            if (!isLastStateOn)
            {
                unitLastState = aIState;
                isLastStateOn = true;
            }

            aIState = AIState.AI_NormalTrace;
        }

        direction = targetPos - tr.position;
        checkPathCoroutine = null;
    }

    protected override void AnimSetFloat(Vector3 direction, bool isNotLast)
    {
        float verticalValue = 0f;
        if (direction.y > 0)
        {
            verticalValue = 1f;
        }
        else if (direction.y < 0)
        {
            verticalValue = -1f;
        }
         
        if (isNotLast)
        {
            animator.SetFloat("Vertical", verticalValue);
        }
        else
        {
            animator.SetFloat("lastMoveY", verticalValue);
        }

        if(isFlip)
        {
           if (direction.x > 0)
            {
                if(!unitSprite.flipX)
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

    void MoveFunc()
    {
        if (movePath == null)
            return;
        else if (movePath.Count <= currentWaypointIndex)
            return;

        Vector3 targetWaypoint = movePath[currentWaypointIndex];

        direction = targetWaypoint - tr.position;
        direction.Normalize();

        if (!isHold)
        {
            tr.position = Vector3.MoveTowards(tr.position, targetWaypoint, Time.deltaTime * unitCommonData.MoveSpeed);
            if (Vector3.Distance(tr.position, targetPosition) <= moveRadi / 2)
            {
                if (!isMoveCheckCoroutine)
                    StartCoroutine(UnitMoveCheck());
            }
            if (Vector3.Distance(tr.position, targetWaypoint) <= moveRadi / 2)
            {
                currentWaypointIndex++;

                if (currentWaypointIndex >= movePath.Count)
                {
                    AnimSetFloat(lastMoveDirection, false);
                    isAttackMove = true;
                    aIState = AIState.AI_Idle;
                }
            }

            animator.SetBool("isMove", true);

            if (direction.magnitude > 0.5f)
            {
                AnimSetFloat(direction, true);
            }
        }
        else
            animator.SetBool("isMove", false);
    }

    public void PatrolPosSet(Vector2 dir)
    {
        isHold = false;
        isAttackMove = true;
        isMoveCheckCoroutine = false;
        isTargetSet = false;

        targetPosition = dir;
        patrolStartPos = tr.position;

        lastMoveDirection = (targetPosition - tr.position).normalized;
        aIState = AIState.AI_Patrol;

        if (checkPathCoroutine == null)
            checkPathCoroutine = StartCoroutine(CheckPath(targetPosition, "Patrol"));
        else
        {
            StopCoroutine(checkPathCoroutine);
            checkPathCoroutine = StartCoroutine(CheckPath(targetPosition, "Patrol"));
        }
        direction = targetPosition - tr.position;
    }

    void PatrolFunc(bool isGo)
    {
        if (movePath == null)
            return;
        else if (movePath.Count <= currentWaypointIndex)
            return;
        else if (currentWaypointIndex < 0)
            return;

        Vector3 targetWaypoint = movePath[currentWaypointIndex];

        direction = targetWaypoint - tr.position;
        direction.Normalize();

        if (!isHold)
        {
            tr.position = Vector3.MoveTowards(tr.position, targetWaypoint, Time.deltaTime * unitCommonData.MoveSpeed);

            if (Vector3.Distance(tr.position, targetWaypoint) <= 0.5f)
            {
                if (isGo)
                {
                    currentWaypointIndex++;
                    if (currentWaypointIndex >= movePath.Count)
                    {
                        isPatrolMove = !isPatrolMove;
                        currentWaypointIndex = movePath.Count - 1;
                    }
                }
                else
                {
                    currentWaypointIndex--;
                    if (currentWaypointIndex < 0)
                    {
                        isPatrolMove = !isPatrolMove;
                        currentWaypointIndex = 0;
                    }
                }
            }

            animator.SetBool("isMove", true);
            if (direction.magnitude > 0.5f)
            {
                AnimSetFloat(direction, true);
            }
        }
        else
            animator.SetBool("isMove", false);
    }

    IEnumerator UnitMoveCheck()
    {
        isMoveCheckCoroutine = true;
        yield return new WaitForSeconds(0.05f);

        movedDistance = Vector3.Distance(tr.position, lastPosition);
        lastPosition = tr.position;

        if (movedDistance < 0.1f)
        {
            if (!isNewPosSet)
            {
                if(aIState == AIState.AI_Move)
                    aIState = AIState.AI_Idle;
                isAttackMove = true;
                AnimSetFloat(lastMoveDirection, false);
                isMoveCheckCoroutine = false;
                yield break;        
            }
            else if (isNewPosSet)
            {
                isNewPosSet = false;
                isMoveCheckCoroutine = false;
                yield break;
            }
        }
        isMoveCheckCoroutine = false;
    }

    public void UnitSelImg(bool isOn)
    {
        if (isOn)
        {
            unitSelImg.color = new Color(1f, 1f, 1f, 1f);
        }
        else
        {
            unitSelImg.color = new Color(1f, 1f, 1f, 0f);
        }
        unitSelect = isOn;
    }

    public void HoldFunc()
    {
        isHold = true;
        isAttackMove = true;
        isTargetSet = false;
        AnimSetFloat(direction, false);
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
                }
            }
        }
    }

    protected override void AttackTargetCheck()
    {
        if (!isTargetSet)
        {
            float closestDistance = float.MaxValue;

            foreach (GameObject monster in targetList)
            {
                if (monster != null)
                {
                    float distance = Vector3.Distance(tr.position, monster.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        aggroTarget = monster;
                    }
                }
            }

            if (aggroTarget != null)
            {
                if ((aIState == AIState.AI_Move && !isAttackMove) || isHold)
                    return;

                if (checkPathCoroutine == null)
                    checkPathCoroutine = StartCoroutine(CheckPath(aggroTarget.transform.position, "NormalTrace"));
                else
                {
                    StopCoroutine(checkPathCoroutine);
                    checkPathCoroutine = StartCoroutine(CheckPath(aggroTarget.transform.position, "NormalTrace"));
                }
            }
        }
    }

    public void TargetSet(GameObject obj)
    {
        isTargetSet = true;
        aggroTarget = obj;
        aIState = AIState.AI_NormalTrace;
        attackState = AttackState.Waiting;
    }

    protected override void NormalTrace()
    {
        if (isHold || aggroTarget == null)
        {
            animator.SetBool("isMove", false);
            return;
        }

        animator.SetBool("isMove", true);
        AnimSetFloat(targetVec, true);

        if (targetDist > unitCommonData.AttackDist)
        {
            if (aggropointIndex >= aggroPath.Count)
                return;

            Vector3 targetWaypoint = aggroPath[aggropointIndex];
            direction = targetWaypoint - tr.position;
            direction.Normalize();

            tr.position = Vector3.MoveTowards(tr.position, targetWaypoint, Time.deltaTime * unitCommonData.MoveSpeed);

            if (Vector3.Distance(tr.position, targetWaypoint) <= 0.3f)
            {
                aggropointIndex++;

                if (aggropointIndex >= aggroPath.Count)
                    return;
            }
        }
    }
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        selfHealTimer = 0;
    }
    protected override void DieFunc()
    {
        base.DieFunc();

        unitSelImg.color = new Color(1f, 1f, 1f, 0f);

        foreach (GameObject monster in targetList)
        {
            if (monster != null && monster.TryGetComponent(out MonsterAi monsterAi)) 
            {
                monsterAi.RemoveTarget(this.gameObject);
            }
        }

        if (unitSelect)
        {
            unitGroupCtrl.DieUnitCheck(this.gameObject);
        }

        Destroy(this.gameObject);
    }

    public override void RemoveTarget(GameObject target)
    {
        base.RemoveTarget(target);
        if (targetList.Count == 0 && isLastStateOn)
        {
            aIState = unitLastState;

            if (aIState == AIState.AI_Move)
            {
                checkPathCoroutine = StartCoroutine(CheckPath(targetPosition, "Move"));
            }

            isLastStateOn = false;
        } 
    }
}
