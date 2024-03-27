using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

// UTF-8 설정
public class MonsterAi : UnitCommonAi
{
    public GameObject spawner;
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

    [SerializeField]
    protected bool waveState = false;
    bool waveWaiting = false;
    public bool waveArrivePos = false;
    //bool goingBase = false;
    Vector3 wavePos; // 나중에 웨이브 대상으로 변경해야함 (현 맵 중심으로 이동하게)
    bool waveFindObj = false;
    //bool goalPathBlocked = false;

    protected override void Start()
    {
        base.Start();
        spawner = GetComponentInParent<MonsterSpawner>().gameObject;
        spawnPos = spawner.transform;
    }

    protected override void FixedUpdate()
    {
        if (isScriptActive)
        {
            base.FixedUpdate();
        }
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
                    if (IsServer && idleTimeStart)
                        StartCoroutine(nameof(IdleTime));
                }
                else
                {
                    if (IsServer)
                        PatrolRandomPosSet();
                }
                break;
            case AIState.AI_Patrol:
                PatrolFunc();
                break;
            case AIState.AI_Attack:
                if (attackState == AttackState.Waiting)
                {
                    if (IsServer && aggroTarget)
                        AttackCheck();
                }
                else if (attackState == AttackState.AttackStart)
                {
                    if(IsServer)
                        Attack();
                }
                break;
            case AIState.AI_NormalTrace:
                {
                    NormalTrace();
                    if (IsServer && aggroTarget)
                        AttackCheck();
                }
                break;
            case AIState.AI_ReturnPos:
                {
                    if(!waveState)
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
        if (IsServer)
        {
            if (movePath == null)
                return;
            if (movePath.Count <= currentWaypointIndex)
                return;

            Vector3 targetWaypoint = movePath[currentWaypointIndex];

            direction = targetWaypoint - tr.position;
            direction.Normalize();

            tr.position = Vector3.MoveTowards(tr.position, targetWaypoint, Time.deltaTime * unitCommonData.MoveSpeed);
            if (Vector3.Distance(tr.position, patrolPos) <= 0.3f)
            {
                aIState = AIState.AI_Idle;
                AnimSetFloat(lastMoveDirection, false);
                return;
            }
            if (Vector3.Distance(tr.position, targetWaypoint) <= 0.3f)
            {
                currentWaypointIndex++;

                if (currentWaypointIndex >= movePath.Count)
                {
                    aIState = AIState.AI_Idle;
                    AnimSetFloat(lastMoveDirection, false);
                    return;
                }
            }

            animator.SetBool("isMove", true);

            if (direction.magnitude > 0.5f)
            {
                AnimSetFloat(direction, true);
            }
        }
        else
        {
            direction = patrolPos - tr.position;
            direction.Normalize();
            animator.SetBool("isMove", true);
            AnimSetFloat(direction, true);

            if (Vector3.Distance(tr.position, patrolPos) <= 0.5f)
            {
                aIState = AIState.AI_Idle;
                AnimSetFloat(lastMoveDirection, false);
            }
        }
    }

    protected void PatrolRandomPosSet()
    {
        Vector3 patrolMainPos;
        if (!waveState)
        {
            patrolMainPos = spawnPos.position;
        }
        else
        {
            patrolMainPos = wavePos;
        }

        if (waveWaiting)
            return;

        float spawnDis = (tr.position - patrolMainPos).magnitude;

        if (spawnDis > maxSpawnDist)
        {
            float randomAngle = Random.Range(0f, 2f * Mathf.PI);
            float randomDistance = Random.Range(0f, unitCommonData.PatrolRad);
            Vector3 randomPosition = patrolMainPos + new Vector3(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * randomDistance;

            patrolPos = randomPosition;
            if (checkPathCoroutine == null)
            {
                checkPathCoroutine = StartCoroutine(CheckPath(patrolPos, "Patrol"));
            }
        }
        else
        {
            idle = Random.Range(0, 3);
            if (idle == 0)
            {
                patrolPos = tr.position;
                animator.SetBool("isMove", false);
                aIState = AIState.AI_Idle;
                return;
            }
            else
            {
                patrolPos = Vector3.zero;

                patRandomPos = Random.insideUnitCircle * unitCommonData.PatrolRad * Random.Range(5, 10);
                patrolPos = tr.position + patRandomPos;
                if (checkPathCoroutine == null)
                {
                    checkPathCoroutine = StartCoroutine(CheckPath(patrolPos, "Patrol"));
                }
            }
        }
    }

    protected void ReturnPos()
    {
        if (movePath == null)
            return; 
        if (movePath.Count <= currentWaypointIndex)
            return;

        Vector3 targetWaypoint = movePath[currentWaypointIndex];

        direction = targetWaypoint - tr.position;
        direction.Normalize();

        tr.position = Vector3.MoveTowards(tr.position, targetWaypoint, Time.deltaTime * (unitCommonData.MoveSpeed + 7));
        if (Vector3.Distance(tr.position, targetPosition) <= 0.3f)
        {
            aIState = AIState.AI_Idle;
            return;
        }
        if (Vector3.Distance(tr.position, targetWaypoint) <= 0.3f)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= movePath.Count)
            {
                aIState = AIState.AI_Idle;
                return;
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
        if (movePath == null)
        {
            return;
        }
        if (!waveState && aggroTarget == null)
        {
            animator.SetBool("isMove", false);
            aIState = AIState.AI_Idle;
            return;
        }

        animator.SetBool("isMove", true);

        if (aggroTarget)
        {
            if (targetDist > unitCommonData.AttackDist)
            {
                if (currentWaypointIndex >= movePath.Count)
                    return;

                Vector3 targetWaypoint = movePath[currentWaypointIndex];
                direction = targetWaypoint - tr.position;
                direction.Normalize();
                AnimSetFloat(targetVec, true);

                tr.position = Vector3.MoveTowards(tr.position, targetWaypoint, Time.deltaTime * unitCommonData.MoveSpeed);

                if (Vector3.Distance(tr.position, targetWaypoint) <= 0.3f)
                {
                    currentWaypointIndex++;

                    if (currentWaypointIndex >= movePath.Count)
                        return;
                }
            }
        }
        else if(waveState)
        {
            float basePosDist = Vector3.Distance(tr.position, wavePos);
            if (basePosDist > unitCommonData.AttackDist)
            {
                if (currentWaypointIndex >= movePath.Count)
                    return;

                Vector3 targetWaypoint = movePath[currentWaypointIndex];
                direction = targetWaypoint - tr.position;
                direction.Normalize();
                AnimSetFloat(targetVec, true);

                tr.position = Vector3.MoveTowards(tr.position, targetWaypoint, Time.deltaTime * 6);
                //tr.position = Vector3.MoveTowards(tr.position, targetWaypoint, Time.deltaTime * unitCommonData.MoveSpeed);

                if (Vector3.Distance(tr.position, targetWaypoint) <= 0.3f)
                {
                    currentWaypointIndex++;

                    if (currentWaypointIndex >= movePath.Count)
                    {
                        //if (goalPathBlocked)
                        //{
                        //    goalPathBlocked = true;
                        //    DestroyMapObject(wavePos);
                        //}
                        return;
                    }
                }
            }
            else
                waveArrivePos = true;
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
                    Obj.GetComponent<UnitAi>().TakeDamageClientRpc(unitCommonData.Damage);
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

                waveFindObj = true;
            }
        }

        if(targetList.Count > 0)
        {
            battleBGM.BattleAddMonster(gameObject);
        }

        if((aIState == AIState.AI_NormalTrace || aIState == AIState.AI_Attack) && targetList.Count == 0)
        {
            if (!waveState)
            {
                idle = 0;
                aIState = AIState.AI_Idle;
                attackState = AttackState.Waiting;
                battleBGM.BattleRemoveMonster(gameObject);
                return;
            }
            else if (!waveArrivePos && waveFindObj)
            {
                WaveStart(wavePos);
            }
            else if (waveState && waveArrivePos && !waveWaiting)
            {
                battleBGM.BattleRemoveMonster(gameObject);
                checkPathCoroutine = StartCoroutine(CheckPath(patrolPos, "Patrol"));
            }
            //else if (!goingBase)
            //    WaveStart(wavePos);
            waveFindObj = false;
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
                    //goingBase = false;
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
        {
            AttackSet(aggroTarget.transform);
            soundManager.PlaySFX(gameObject, "unitSFX", "MonsterAttack");
        }
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
            //if (waveState && movePath.Count > 0 && wavePos != movePath[movePath.Count - 1])
            //    goalPathBlocked = true;
        }
        else if (moveFunc == "ReturnPos")
        {
            aIState = AIState.AI_ReturnPos;
        }

        direction = targetPos - tr.position;
        checkPathCoroutine = null;
    }

    void DestroyMapObject(Vector2 targetPos)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(targetPos, 5);

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider2D collider = colliders[i];
            GameObject target = collider.gameObject;
            string targetTag = target.tag;

            if (targetTag == "MapObj")
            {
                targetList.Add(target);
            }
        }
        WaveStart(wavePos);
    }

    protected virtual void AttackSet(Transform targetTr)
    {
        attackState = AttackState.Attacking;

        animator.SetBool("isAttack", true);
        animator.SetFloat("attackMotion", 0);
        animator.Play("Attack", -1, 0);
    }

    protected virtual void AttackMove() { }

    protected override void AttackEnd()
    {
        base.AttackEnd();
        if (aggroTarget != null)
            AttackObjCheck(aggroTarget);        
    }

    [ClientRpc]
    protected override void DieFuncClientRpc()
    {
        base.DieFuncClientRpc();

        if (!IsServer)
            return;

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
        battleBGM.BattleRemoveMonster(gameObject);
        Destroy(gameObject);
        }

    public void MonsterSpawnerSet(MonsterSpawner monsterSpawner, int type)
    {
        spawner = monsterSpawner.gameObject;
        spawnPos = spawner.transform;
        monsterType = type;
    }

    public void MonsterAStarSet(bool isHostMap)
    {
        GraphMask mask;
        if (isHostMap)
            mask = GraphMask.FromGraphName("Map1");
        else
            mask = GraphMask.FromGraphName("Map2");

        seeker.graphMask = mask;
    }

    [ClientRpc]
    public void MonsterScriptSetClientRpc(bool scriptState)
    {
        isScriptActive = scriptState;

        if(scriptState)
        {
            //enabled = true;
            animator.enabled = true;
            capsule2D.enabled = true;
        }
        else
        {
            if (IsServer)
            {
                spawnDist = Vector3.Distance(tr.position, spawnPos.position);
                if (spawnDist > maxSpawnDist)
                {
                    checkPathCoroutine = StartCoroutine(CheckPath(spawnPos.position, "ReturnPos"));
                }
                else
                {
                    //enabled = false;
                    animator.enabled = false;
                    capsule2D.enabled = false;
                }
            }
        }
    }

    public void WaveStart(Vector3 _wavePos)
    {
        waveState = true;
        waveWaiting = false;
        int x = (int)Random.Range(-5, 5);
        int y = (int)Random.Range(-5, 5);

        wavePos = _wavePos + new Vector3(x, y);
        checkPathCoroutine = StartCoroutine(CheckPath(_wavePos, "NormalTrace"));
    }

    public void ColonyAttackStart(Vector3 _wavePos)
    {
        waveState = true;

        int x = (int)Random.Range(-5, 5);
        int y = (int)Random.Range(-5, 5);

        wavePos = _wavePos + new Vector3(x, y);
        checkPathCoroutine = StartCoroutine(CheckPath(wavePos, "NormalTrace"));
    }

    public void WaveSetMoveMonster(Vector3 pos)
    {
        if (checkPathCoroutine == null)
        {
            waveState = true;
            checkPathCoroutine = StartCoroutine(CheckPath(pos, "NormalTrace"));
        }
    }

    public void WaveTeleport(Vector3 _wavePos)
    {
        if(aIState != AIState.AI_NormalTrace && aIState != AIState.AI_Attack)
        {
            int x = (int)Random.Range(-10, 10);
            int y = (int)Random.Range(-10, 10);

            Vector3 newWavePos = _wavePos + new Vector3(x, y);
            waveState = true;
            waveWaiting = true;
            transform.position = newWavePos;
        }
    }
}
