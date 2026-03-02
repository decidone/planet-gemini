using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

// UTF-8 설정
public class MonsterAi : UnitCommonAi
{
    public new string name;
    MonsterSpawner spawnerScript;
    Transform spawnPos;
    float spawnDist;
    [SerializeField]
    float maxSpawnDist;

    Vector3 patRandomPos = Vector3.zero;
    protected int idle = -1;
    protected int attackMotion;
    protected bool idleTimeStart = true;

    public bool isScriptActive = false;
    public int monsterType;    // 0 : 약한, 1 : 노멀, 2 : 강함, 3 : 가디언

    [SerializeField]
    protected bool waveState = false;
    float waveSpeed = 9;

    //bool waveWaiting = false;
    public bool waveArrivePos = false;
    //bool goingBase = false;
    Vector3 wavePos; // 나중에 웨이브 대상으로 변경해야함 (현 맵 중심으로 이동하게)
    bool isWaveColonyCallCheck = true;
    //bool goalPathBlocked = false;

    public float normalTraceTimer;
    float normalTraceInterval = 15f;
    [SerializeField]
    bool stopTrace;

    public bool isDebuffed;
    float debuffTimer;
    float debuffDuration = 2f;  // 등대 디버프 지속시간 2초, 등대 범위 내에 몬스터가 있을 때 디버프 갱신 시간 1초
    float debuffRate;           // 등대가 속한 에너지 그룹의 에너지 효율 상태에 따른 디버프 배율 계산
    float reducedDefensePer;    // 방어력 감소 퍼센트

    float speedMove = 10;
    public bool targetOn;
    Transform _t;
    Effects _effects;

    protected bool spawnerPhaseOn;
    protected bool spawnerLastPhaseOn;

    Coroutine selectTargetCo;
    Coroutine mapSeekerCheckCo;

    bool waitForGraphUpdate = false;

    [SerializeField]
    GameObject bestTarget;  // 선타겟 대상

    MonsterMapSeeker monsterMapSeeker;

    protected override void Awake()
    {
        base.Awake();
        int mask =
            (1 << LayerMask.NameToLayer("Obj")) |
            (1 << LayerMask.NameToLayer("Unit")) |
            (1 << LayerMask.NameToLayer("Portal")) |
            (1 << LayerMask.NameToLayer("LocalPortal")) |
            (1 << LayerMask.NameToLayer("Tank"));

        contactFilter.SetLayerMask(mask);
        contactFilter.useLayerMask = true;
    }

    protected override void Start()
    {
        base.Start();
        monsterMapSeeker = GetComponentInChildren<MonsterMapSeeker>();

        normalTraceInterval = Random.Range(10, 15);
        _t = transform;
        _effects = Effects.instance;
        unitIndex = GeminiNetworkManager.instance.GetUnitSOIndex(gameObject, monsterType, false);

        if(waveState)
        {
            WaveStatusSet(GameManager.instance.hpMultiplier, GameManager.instance.atkMultiplier);
        }
    }

    protected override void FixedUpdate()
    {
        if (isScriptActive)
        {
            base.FixedUpdate();
        }
        else
        {
            if (!waveState)
                ReturnPos();
        }
    }

    protected override void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        if (!IsServer)
            return;

        if ((isScriptActive || !dieCheck) && !waitForGraphUpdate)
        {
            if (aIState != AIState.AI_SpawnerCall && !targetOn)
            {
                if (aIState != AIState.AI_Die)
                {
                    searchTimer += Time.deltaTime;

                    if (searchTimer >= searchInterval)
                    {
                        if(targetList.Count > 0)
                            AttackTargetDisCheck();
                        searchTimer = 0;
                    }
                }

                if (isDebuffed)
                {
                    debuffTimer += Time.deltaTime;

                    if (debuffTimer > debuffDuration)
                    {
                        isDebuffed = false;
                        reducedDefensePer = 0;
                        debuffTimer = 0f;
                    }
                }
            }
            else if (aggroTarget)
            {
                searchTimer += Time.deltaTime;
                if (searchTimer >= searchInterval)
                {
                    AttackTargetDisCheck();
                    SpawnerCallCheck(aggroTarget);
                    searchTimer = 0f;
                }
            }
            else
            {
                targetOn = false;
                aIState = AIState.AI_Idle;
            }

            if (aggroTarget && !waveState && aIState == AIState.AI_NormalTrace)
            {
                normalTraceTimer += Time.deltaTime;
                if (normalTraceTimer > normalTraceInterval)
                {
                    normalTraceInterval = Random.Range(10, 20);
                    normalTraceTimer = 0f;
                    stopTrace = true;
                    PatrolRandomPosSet();
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RefreshDebuffServerRpc(float efficiency, float reducedPer)
    {
        RefreshDebuffClientRpc(efficiency, reducedPer);
    }

    [ClientRpc]
    void RefreshDebuffClientRpc(float efficiency, float reducedPer)
    {
        RefreshDebuff(efficiency, reducedPer);
    }

    public void RefreshDebuff(float efficiency, float reducedPer)
    {
        if (!isDebuffed)
        {
            TriggerEffects("dark");
        }

        debuffRate = efficiency;
        isDebuffed = true;
        float reducedDefensePerCheck = reducedPer * efficiency;
        if (reducedDefensePer < reducedDefensePerCheck)
        {
            reducedDefensePer = reducedDefensePerCheck;
        }
        debuffTimer = 0f;
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
                {
                    PatrolRandomPosSet();
                }
                break;
            case AIState.AI_Patrol:
                PatrolFunc();
                break;
            case AIState.AI_Attack:
                if (attackState == AttackState.Waiting)
                {
                    if (aggroTarget)
                        AttackCheck();
                    else
                        aIState = AIState.AI_Idle;
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
                    if(!waveState)
                        ReturnPos();
                }
                break;
            case AIState.AI_SpawnerCall:
                {
                    SpawnerCall();
                    if (aggroTarget)
                    {
                        if (targetDist == 0)
                            return;

                        if (targetDist <= unitCommonData.AttackDist)  // 공격 범위 내로 들어왔을 때        
                        {
                            aIState = AIState.AI_Attack;
                            attackState = AttackState.AttackStart;
                        }
                    }
                }
                break;
        }
    }

    protected override void OnClientConnectedCallback(ulong clientId)
    {
        base.OnClientConnectedCallback(clientId);
        MonsterScriptSetServerRpc(isScriptActive);
        FlipSyncServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void FlipSyncServerRpc()
    {
        FlipSyncClientRpc(isFlip);
    }

    [ClientRpc]
    void FlipSyncClientRpc(bool flip)
    {
        isFlip = flip;
    }

    protected override void AnimSetFloat(Vector3 direction, bool isNotLast)
    {
        float moveStep = unitCommonData.MoveSpeed * Time.fixedDeltaTime * slowSpeedPer;
        Vector2 moveDir = direction.normalized;
        Vector3 moveNextStep = moveDir * moveStep;

        if (moveNextStep.y > 0)
        {
            if(animator.GetFloat("moveNextStepY") != 1.0f)
            {
                UnitAnimNextStepSetServerRpc("moveNextStepY", 1.0f);
                //animator.SetFloat("moveNextStepY", 1.0f);
            }
        }
        else if (moveNextStep.y <= 0)
        {
            if (animator.GetFloat("moveNextStepY") != -1.0f)
            {
                UnitAnimNextStepSetServerRpc("moveNextStepY", -1.0f);
                //animator.SetFloat("moveNextStepY", -1.0f);
            }
        }

        if (moveDir.x > 0.75f || moveDir.x < -0.75f)
        {
            if (animator.GetFloat("moveNextStepX") != 1.0f)
            {
                UnitAnimNextStepSetServerRpc("moveNextStepX", 1.0f);
                //animator.SetFloat("moveNextStepX", 1.0f);
            }
        }
        else if (moveDir.x <= 0.75f && moveDir.x >= -0.75f)
        {
            if (animator.GetFloat("moveNextStepX") != 0.0f)
            {
                UnitAnimNextStepSetServerRpc("moveNextStepX", 0.0f);
                //animator.SetFloat("moveNextStepX", 0.0f);
            }
        }

        if (isFlip)
        {
            if (direction.x > 0)
            {
                if (!unitSprite.flipX)
                {
                    //unitSprite.flipX = true;
                    UnitFlipSetServerRpc(true);
                }
            }
            else if (direction.x < 0)
            {
                if (unitSprite.flipX)
                {
                    //unitSprite.flipX = false;
                    UnitFlipSetServerRpc(false);
                }
            }
        }
        else
        {
            if (direction.x > 0)
            {
                if (unitSprite.flipX)
                {
                    //unitSprite.flipX = false;
                    UnitFlipSetServerRpc(false);
                }
            }
            else if (direction.x < 0)
            {
                if (!unitSprite.flipX)
                {
                    //unitSprite.flipX = true;
                    UnitFlipSetServerRpc(true);
                }
            }
        }
    }

    [ServerRpc]
    void UnitAnimNextStepSetServerRpc(string parameter, float num)
    {
        UnitAnimNextStepSetClientRpc(parameter, num);
    }

    [ClientRpc]
    void UnitAnimNextStepSetClientRpc(string parameter, float num)
    {
        if(parameter == "moveNextStepX")
        {
            animator.SetFloat("moveNextStepX", num); 
        }
        else
        {
            animator.SetFloat("moveNextStepY", num);
        }
    }

    [ServerRpc]
    void UnitFlipSetServerRpc(bool flipX)
    {
        UnitFlipSetClientRpc(flipX);
    }

    [ClientRpc]
    void UnitFlipSetClientRpc(bool flipX)
    {
        unitSprite.flipX = flipX;
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

            tr.position = Vector2.MoveTowards(tr.position, targetWaypoint, Time.deltaTime * unitCommonData.MoveSpeed * slowSpeedPer);
            if (Vector2.Distance(tr.position, patrolPos) <= 0.3f)
            {
                aIState = AIState.AI_Idle;
                AnimSetFloat(lastMoveDirection, false);
                stopTrace = false;
                return;
            }
            if (Vector2.Distance(tr.position, targetWaypoint) <= 0.3f)
            {
                currentWaypointIndex++;

                if (currentWaypointIndex >= movePath.Count)
                {
                    aIState = AIState.AI_Idle;
                    AnimSetFloat(lastMoveDirection, false);
                    stopTrace = false;
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

            if (Vector2.Distance(tr.position, patrolPos) <= 0.5f)
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

        //if (waveWaiting)
        //    return;

        float spawnDis = (tr.position - patrolMainPos).magnitude;

        if (waveState && !waveArrivePos)
        {
            return;
        }

        if (!spawnerScript.nearUserObjExist && !spawnerScript.dieCheck && !waveState)
        {
            checkPathCoroutine = StartCoroutine(CheckPath(spawnPos.position, "ReturnPos"));
        }
        else if (waveState)
        {
            checkPathCoroutine = StartCoroutine(CheckPath(spawnPos.position, "NormalTrace"));
        }
        else if (spawnDis > maxSpawnDist)
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

                patRandomPos = Random.insideUnitCircle * unitCommonData.PatrolRad * Random.Range(3, 7);
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
        {
            aIState = AIState.AI_Idle;
            return;
        }

        Vector3 targetWaypoint = movePath[currentWaypointIndex];

        direction = targetWaypoint - tr.position;
        direction.Normalize();

        tr.position = Vector2.MoveTowards(tr.position, targetWaypoint, Time.deltaTime * speedMove * slowSpeedPer);
        if (Vector2.Distance(tr.position, spawnPos.position) <= 0.3f)
        {
            aIState = AIState.AI_Idle;
            if (!spawnerScript.nearUserObjExist && !spawnerScript.dieCheck)
            {
                ReturnFunc();
            }
            stopTrace = false;
            return;
        }
        if (Vector2.Distance(tr.position, targetWaypoint) <= 0.3f)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= movePath.Count)
            {
                aIState = AIState.AI_Idle;
                if (!spawnerScript.nearUserObjExist && !spawnerScript.dieCheck)
                {
                    ReturnFunc();
                }
                stopTrace = false;
                return;
            }
        }

        animator.SetBool("isMove", true);

        if (direction.magnitude > 0.5f)
        {
            AnimSetFloat(direction, true);
        }
    }

    void ReturnFunc()
    {
        DieFunc();
        StopAllCoroutines();
        seeker.StopAllCoroutines();
        seeker.OnDestroy();

        if (InfoUI.instance.monster == this)
            InfoUI.instance.SetDefault();

        if (!IsServer)
            return;

        spawnerScript.ReturnMonster(this);

        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
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
        if (animator.GetBool("isAttack"))
        {
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

                tr.position = Vector2.MoveTowards(tr.position, targetWaypoint, waveState ? Time.deltaTime * waveSpeed : (Time.deltaTime * (spawnerLastPhaseOn ?  speedMove : unitCommonData.MoveSpeed) * slowSpeedPer));

                if (Vector2.Distance(tr.position, targetWaypoint) <= 0.3f)
                {
                    currentWaypointIndex++;

                    if (currentWaypointIndex >= movePath.Count)
                        return;
                }
            }
        }
        else if(waveState)
        {
            float basePosDist = Vector2.Distance(tr.position, wavePos);
            if (basePosDist > unitCommonData.AttackDist)
            {
                if (currentWaypointIndex >= movePath.Count)
                    return;

                Vector3 targetWaypoint = movePath[currentWaypointIndex];
                direction = targetWaypoint - tr.position;
                direction.Normalize();
                AnimSetFloat(targetVec, true);

                tr.position = Vector2.MoveTowards(tr.position, targetWaypoint, Time.deltaTime * waveSpeed);

                if (Vector2.Distance(tr.position, targetWaypoint) <= 0.3f)
                {
                    currentWaypointIndex++;
                    if (currentWaypointIndex >= movePath.Count)
                    {
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
            targetDist = Vector2.Distance(tr.position, aggroTarget.transform.position);
            if(spawnerScript != null)
                spawnDist = Vector2.Distance(tr.position, spawnPos.position);
        }
    }

    protected void AttackObjCheck(GameObject Obj)
    {
        if (targetDist > unitCommonData.AttackDist + 2)
            return;
        else if (IsServer)
        {
            if (Obj != null)
            {
                if (Obj.TryGetComponent(out PlayerStatus playerStatus))
                    playerStatus.TakeDamage(damage);
                else if (Obj.TryGetComponent(out UnitAi unitAi))
                    unitAi.TakeDamage(damage, 0);
                else if (Obj.TryGetComponent(out Structure structure))
                    structure.TakeDamage(damage);
            }
        }
        stopTrace = false;
        if (targetOn)
        {
            targetOn = false;
            aIState = AIState.AI_Idle;
        }
    }

    public override void SearchObjectsInRange()
    {
        int hitCount = 0;

        if (stopTrace)
        {
            hitCount = Physics2D.OverlapCircle(
                tr.position,
                unitCommonData.MinCollRad,
                contactFilter,
                targetColls
            );
        }
        else
        {
            if (waveState && !bestTarget)
            {
                hitCount = Physics2D.OverlapCircle(
                    tr.position,
                    unitCommonData.ColliderRadius + 15,
                    contactFilter,
                    targetColls
                );
            }
            else
            {
                hitCount = Physics2D.OverlapCircle(
                    tr.position,
                    unitCommonData.ColliderRadius,
                    contactFilter,
                    targetColls
                );
            }
        }

        if (hitCount == 0)
        {
            aggroTarget = null;
            if (targetList.Count > 0)
            {
                targetList.Clear();
            }
            if (aIState == AIState.AI_NormalTrace || aIState == AIState.AI_Attack)
            {
                if (!waveState)
                {
                    idle = 0;
                    aIState = AIState.AI_Idle;
                    attackState = AttackState.Waiting;
                }
                else
                {
                    if (!waveArrivePos)
                    {
                        WaveStart(wavePos);
                    }
                    else if (waveArrivePos)
                    {
                        checkPathCoroutine = StartCoroutine(CheckPath(patrolPos, "Patrol"));
                    }
                }                
            }
            if(bestTarget)
            {
                bestTarget.TryGetComponent(out Structure str);
                if (str)
                {
                    --str.monsterTargetAmount;
                }
                bestTarget = null;
            }
            return;
        }

        targetList.Clear();

        for (int i = 0; i < hitCount; i++)
        {
            GameObject target = targetColls[i].gameObject;
            targetList.Add(target);
        }

        AttackTargetCheck();
    }

    protected override void AttackTargetCheck()
    {
        if (selectTargetCo == null)
        {
            selectTargetCo = StartCoroutine(SelectTargetAndTrace());
        }
    }

    IEnumerator SelectTargetAndTrace()
    {
        // 현재 상태가 복귀 중이면 타겟 탐색을 중단
        if (aIState == AIState.AI_ReturnPos)
        {
            selectTargetCo = null;
            yield break;
        }

        // 공격 가능한 타겟 후보 리스트와, 공격 불가능한 가장 가까운 타겟을 저장
        var attackableCandidates = new List<(GameObject obj, float dist, float aggro)>();
        GameObject nearestUnattackable = null;
        float nearestUnattackDist = float.MaxValue;
        float dist = 0;

        // 모든 타겟에 대해 탐색
        foreach (var target in targetList)
        {
            if (!target) continue;

            dist = Vector3.Distance(tr.position, target.transform.position);

            // 공격 가능한 대상인지 확인
            bool isAttackable = target.TryGetComponent(out UnitAi unitAi) || target.TryGetComponent(out TowerAi towerAi) || target.TryGetComponent(out PlayerController player);
            if (!isAttackable)
            {
                //공격 불가능하지만 가까운 타겟 저장
                if (dist < nearestUnattackDist)
                {
                    nearestUnattackDist = dist;
                    nearestUnattackable = target;
                }
                continue;
            }

            // 어그로 수치 가져오기
            float aggro = 0f;
            if (target.TryGetComponent(out AggroAmount aggroAmount))
                aggro = aggroAmount.GetAggroAmount();

            attackableCandidates.Add((target, dist, aggro));
        }

        GameObject best = null;
        float bestScore = float.MinValue;

        foreach (var x in attackableCandidates)
        {
            float score = x.aggro - (x.dist * 3f);
            x.obj.TryGetComponent(out Structure str);
            if (str)
            {
                score -= str.monsterTargetAmount * 3f; // 타겟을 많이 잡고 있는 구조물은 선호도 감소
            }
            
            if (bestTarget && x.obj == bestTarget)
            {
                score += 3f; // 기존 선타겟인 경우 점수 보너스
            }

            if (score > bestScore)
            {
                bestScore = score;
                best = x.obj;
            }
        }

        if (best)
        {
            if (!bestTarget)
            {
                bestTarget = best;
                bestTarget.TryGetComponent(out Structure str);
                if (str)
                {
                    ++str.monsterTargetAmount;
                }
            }
            else if(bestTarget != best) 
            {
                bestTarget.TryGetComponent(out Structure preStr);
                if (preStr)
                {
                    --preStr.monsterTargetAmount;
                }

                bestTarget = best;

                bestTarget.TryGetComponent(out Structure nextStr);
                if (nextStr)
                {
                    ++nextStr.monsterTargetAmount;
                }
            }
        }
        else
        {
            if (nearestUnattackable)
            {
                aggroTarget = nearestUnattackable;

                if (Vector2.Distance(transform.position, aggroTarget.transform.position) > unitCommonData.AttackDist)
                {
                    if (checkPathCoroutine != null) StopCoroutine(checkPathCoroutine);
                    checkPathCoroutine = StartCoroutine(CheckPath(aggroTarget.transform.position, "NormalTrace"));
                }
                else
                {
                    aIState = AIState.AI_NormalTrace;
                    direction = aggroTarget.transform.position - tr.position;
                    checkPathCoroutine = null;
                }
                selectTargetCo = null; 
                yield break;
            }
            else
            {
                aggroTarget = null;
                selectTargetCo = null;
                yield break;
            }
        }

        ABPath pathReq = null;
        bool canAttack = false;
        bool finished = false;

        Vector3 dir = (tr.position - best.transform.position).normalized;
        Vector3 attackPos = best.transform.position + dir;
        bool pathValid = false;

        // A* 경로 요청 생성
        pathReq = ABPath.Construct(tr.position, attackPos, p =>
        {
            finished = true;

            // 경로 계산 실패하거나 유효하지 않으면 무시
            if (this == null || p.error || p.vectorPath.Count == 0)
                return;

            pathValid = true;
        });

        // A* 경로 요청 시작
        seeker.StartPath(pathReq);
        yield return new WaitUntil(() => finished); // 경로 계산이 끝날 때까지 대기

        if(!pathValid)
        {
            selectTargetCo = null;
            yield break;
        }

        if (best)
        {
            float lastMoveDist = Vector3.Distance(pathReq.vectorPath.Last(), best.transform.position);

            // 공격 사거리 안에 도달 가능한 경우
            if (lastMoveDist <= unitCommonData.AttackDist)
            {
                canAttack = true;
            }
        }
        else
        {
            selectTargetCo = null;
            yield break;
        }

        GameObject blockingObj = null;
        float mapDistance = 0f;
        float totalDistance = 0f;


        if (best && pathReq != null)
        {
            bool hasResult = false;

            for (int i = 0; i < pathReq.vectorPath.Count - 1; i++)
            {
                totalDistance += Vector3.Distance(
                    pathReq.vectorPath[i],
                    pathReq.vectorPath[i + 1]
                );
            }

            if(mapSeekerCheckCo != null) StopCoroutine(mapSeekerCheckCo);
            mapSeekerCheckCo = StartCoroutine(monsterMapSeeker.GetBlockingObjectToTarget(transform.position, attackPos, isInHostMap, (blocking, mapOnlyDistance) =>
            {
                hasResult = true;
                if (this == null) return;

                mapDistance = mapOnlyDistance;

                if (blocking)
                {
                    blockingObj = blocking;
                }
            }));
            yield return new WaitUntil(() => hasResult);
        }

        if(!best)
        {
            selectTargetCo = null;
            yield break;
        }

        float pathRatio = totalDistance / mapDistance;

        float maxRatio = 1.7f;

        if (blockingObj)
        {
            if(!canAttack)
                best = blockingObj;
            else if(pathRatio > maxRatio)
                best = blockingObj;
            else if (best && Vector3.Distance(transform.position, best.transform.position) <= unitCommonData.AttackDist)
                best = blockingObj;
        }

        // 최종 타겟 설정
        aggroTarget = best;

        // 추적 시작
        if (aggroTarget != null)
        {
            if (Vector3.Distance(transform.position, aggroTarget.transform.position) > unitCommonData.AttackDist)
            {
                if (checkPathCoroutine != null) StopCoroutine(checkPathCoroutine);
                checkPathCoroutine = StartCoroutine(CheckPath(aggroTarget.transform.position, "NormalTrace"));
            }
            else
            {
                aIState = AIState.AI_NormalTrace;
                direction = aggroTarget.transform.position - tr.position;
                checkPathCoroutine = null;
            }
        }

        selectTargetCo = null;
    }

    public override void TakeDamage(float damage, int attackType, float ignorePercent)
    {
        base.TakeDamage(damage, attackType, ignorePercent);
        normalTraceTimer = 0;
        stopTrace = false;
    }

    public override void TakeDamage(float damage, int attackType)
    {
        base.TakeDamage(damage, attackType);
        normalTraceTimer = 0;
        stopTrace = false;
    }


    [ClientRpc]
    public override void TakeDamageClientRpc(float damage, int attackType, float option)
    {
        if (!unitCanvas.activeSelf)
            unitCanvas.SetActive(true);

        float reducedDamage = damage;
        float reducedDefense = defense;

        if (isDebuffed)
        {
            reducedDefense = defense - Mathf.Floor(defense * reducedDefensePer / 100);
        }

        if (attackType == 0 || attackType == 4)
        {
            float defenseRate = defense * 0.01f; // 0 ~ 1 변환
            reducedDamage = Mathf.Max(damage * (1f - defenseRate), 5f);
            if (attackType == 4)
            {
                if (!slowDebuffOn)
                {
                    StartCoroutine(SlowDebuffDamage(option));
                    TriggerEffects("ice");
                }
            }
        }
        else if (attackType == 2)
        {
            reducedDamage = Mathf.Max(damage - (reducedDefense * (option / 100)), 5);
        }
        else if (attackType == 3)
        {
            reducedDamage = 0;
            if (!takePoisonDamgae)
            {
                StartCoroutine(PoisonDamage(damage, option));
                TriggerEffects("poison");
            }
        }

        if (!slowDebuffOn && !takePoisonDamgae && !damageEffectOn)
        {
            StartCoroutine(TakeDamageEffect());
        }

        hp -= reducedDamage;
        if (hp < 0f)
            hp = 0f;
        onHpChangedCallback?.Invoke();
        hpBar.fillAmount = hp / maxHp;

        if (IsServer && hp <= 0f && !dieCheck)
        {
            aIState = AIState.AI_Die;
            hp = 0f;
            dieCheck = true;
            DieFuncServerRpc();
            StopAllCoroutines();
            seeker.enabled = false;
            seeker.OnDestroy();
        }
    }

    protected override bool AttackStart()
    {
        bool isAttacked = false;

        if (aggroTarget != null)
        {
            isAttacked = true;
            normalTraceTimer = 0;
            AttackSet(aggroTarget.transform);
            soundManager.PlaySFX(gameObject, "unitSFX", "MonsterAttack");
        }

        return isAttacked;
    }

    protected override IEnumerator CheckPath(Vector3 targetPos, string moveFunc)
    {
        Vector3 dir = (tr.position - targetPos).normalized;
        Vector3 attackPos = targetPos + dir;

        ABPath path = ABPath.Construct(tr.position, attackPos, null);
        seeker.CancelCurrentPathRequest();
        seeker.StartPath(path);

        yield return StartCoroutine(path.WaitForPath());

        currentWaypointIndex = 1;

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
        else if (moveFunc == "SpawnerCall")
        {
            aIState = AIState.AI_SpawnerCall;
        }

        direction = targetPos - tr.position;
        checkPathCoroutine = null;
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

    public void OnGraphsUpdated()
    {
        waitForGraphUpdate = false;
    }

    [ServerRpc]
    public override void DieFuncServerRpc()
    {
        DieFuncClientRpc();
        if (bestTarget)
        {
            bestTarget.TryGetComponent(out Structure str);
            if (str)
            {
                --str.monsterTargetAmount;
            }
        }

        MonsterSpawnerManager.instance.BattleRemoveMonster(gameObject);
    }

    [ClientRpc]
    protected override void DieFuncClientRpc()
    {
        //base.DieFuncClientRpc();
        DieFunc();
        StopAllCoroutines();
        seeker.StopAllCoroutines();
        seeker.OnDestroy();

        if (InfoUI.instance.monster == this)
            InfoUI.instance.SetDefault();

        if (!IsServer)
            return;

        if(spawnerScript != null)
        {
            spawnerScript.MonsterDieChcek(gameObject, monsterType, waveState);
        }

        if (IsServer && NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
            Overall.instance.OverallCount(1);
        }
    }

    public void MonsterSpawnerSet(MonsterSpawner monsterSpawner, int type)
    {
        spawnerScript = monsterSpawner;
        spawnPos = spawnerScript.transform;
        monsterType = type;
    }

    [ServerRpc]
    public void MonsterScriptSetServerRpc(bool scriptState)
    {
        MonsterScriptSetClientRpc(scriptState);
    }

    [ClientRpc]
    public void MonsterScriptSetClientRpc(bool scriptState)
    {
        isScriptActive = scriptState;

        if(isScriptActive)
        {
            enabled = true;
            animator.enabled = true;
            capsule2D.enabled = true;
            searchManager.UnitListAdd(this);
        }
        else
        {
            if (IsServer)
            {
                if(aIState == AIState.AI_NormalTrace || aIState == AIState.AI_Attack)
                {
                    isScriptActive = true;
                    searchManager.UnitListAdd(this); 
                    return;
                }
                else if(!waveState)
                {
                    checkPathCoroutine = StartCoroutine(CheckPath(spawnPos.position, "ReturnPos"));
                    if (targetOn)
                    {
                        targetOn = false;
                        aIState = AIState.AI_Idle;
                    }
                }
            }
        }
    }

    public virtual void SpawnerCallCheck(GameObject obj)
    {
        if (obj == null)
            return;

        if (aIState == AIState.AI_Idle || aIState == AIState.AI_Patrol)
        {
            spawnerPhaseOn = true;
            aggroTarget = obj;
            targetVec = (new Vector3(aggroTarget.transform.position.x, aggroTarget.transform.position.y, 0) - tr.position).normalized;

            aIState = AIState.AI_SpawnerCall;
            targetOn = true;
            checkPathCoroutine = StartCoroutine(CheckPath(obj.transform.position, "SpawnerCall"));
        }
    }

    public virtual void SpawnerCall()
    {
        //AnimBoolCtrl("isMove", true);
        animator.SetBool("isMove", true);

        AnimSetFloat(targetVec, true);

        if (currentWaypointIndex >= movePath.Count)
            return;

        Vector3 targetWaypoint = movePath[currentWaypointIndex];
        direction = targetWaypoint - tr.position;
        direction.Normalize();

        tr.position = Vector2.MoveTowards(tr.position, targetWaypoint, Time.deltaTime * speedMove * slowSpeedPer);

        if (Vector2.Distance(tr.position, targetWaypoint) <= 0.3f)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= movePath.Count)
            {
                //aIState = AIState.AI_NormalTrace;
                return;
            }
        }       
    }

    public void LoadGameWaveSet(Vector3 wavePos)
    {
        StartCoroutine(LoadGameWaveSetCo(wavePos));
    }

    IEnumerator LoadGameWaveSetCo(Vector3 wavePos)
    {
        while (MonsterBaseMapCheck.instance == null || MonsterBaseMapCheck.instance.wavePath.vectorPath.Count == 0)
        {
            yield return null;
        }

        WaveStart(wavePos);
    }
    

    public void WaveStart(Vector3 _wavePos)
    {
        waveState = true;

        if (MonsterBaseMapCheck.instance.wavePath.vectorPath.Count > 0)
        {
            if (waveMovePath.Count == 0)
                waveMovePath = new List<Vector3>(MonsterBaseMapCheck.instance.wavePath.vectorPath); // 복사

            checkPathCoroutine = null;
            StartCoroutine(ReturnToWavePath());
        }
    }
    
    public void WaveStatusSet(float hpMultiplier, float damageMultiplier)
    {
        WaveStatusSetSyncServerRpc(hpMultiplier, damageMultiplier);
    }

    [ServerRpc]
    void WaveStatusSetSyncServerRpc(float hpMultiplier, float damageMultiplier)
    {
        WaveStatusSetSyncClientRpc(hpMultiplier, damageMultiplier);
    }

    [ClientRpc]
    void WaveStatusSetSyncClientRpc(float hpMultiplier, float damageMultiplier)
    {
        maxHp = Mathf.FloorToInt(unitCommonData.MaxHp * hpMultiplier);
        hp = maxHp;
        damage = Mathf.FloorToInt(unitCommonData.Damage * damageMultiplier);
    }

    protected IEnumerator ReturnToWavePath()
    {
        Vector3 closestPoint = Vector3.zero;
        int closestIndex = -1;
        float minDistance = float.MaxValue;

        for (int i = 0; i < waveMovePath.Count; i++)
        {
            float distance = Vector2.Distance(tr.position, waveMovePath[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
                closestPoint = waveMovePath[i];
            }
        }

        ABPath wavePath = ABPath.Construct(tr.position, closestPoint, null);
        seeker.CancelCurrentPathRequest();
        seeker.StartPath(wavePath);

        yield return StartCoroutine(wavePath.WaitForPath());
        movePath = wavePath.vectorPath;
        movePath.AddRange(waveMovePath.GetRange(closestIndex, waveMovePath.Count - closestIndex));
        currentWaypointIndex = 1;
        aIState = AIState.AI_NormalTrace;
    }

    public override void GameStartSet(UnitSaveData unitSaveData)
    {
        base.GameStartSet(unitSaveData);

        waveState = unitSaveData.waveState;
        monsterType = unitSaveData.monsterType;
        spawnerLastPhaseOn = unitSaveData.spawnerLastPhaseOn;
    }

    public override UnitSaveData SaveData()
    {
        UnitSaveData data = base.SaveData();

        data.monsterType = monsterType;
        data.wavePos = Vector3Extensions.FromVector3(wavePos);
        data.waveState = waveState;
        data.isWaveColonyCallCheck = isWaveColonyCallCheck;
        data.spawnerLastPhaseOn = spawnerLastPhaseOn;
        return data;
    }

    public override void AStarSet(bool isHostMap)
    {
        GraphMask mask;
        if (isHostMap)
            mask = GraphMask.FromGraphName("Map1MonsterUnit");
        else
            mask = GraphMask.FromGraphName("Map2MonsterUnit");

        isInHostMap = isHostMap;
        seeker.graphMask = mask;
    }

    public void TriggerEffects(string effect)
    {
        switch (effect)
        {
            case ("dark"):
                _effects.TriggerDark(this.gameObject);
                break;
            case ("ice"):
                _effects.TriggerIce(this.gameObject);
                break;
            case ("poison"):
                _effects.TriggerPoison(this.gameObject);
                break;
        }
    }

    public void LastPhase()
    {
        spawnerLastPhaseOn = true;
    }
}
