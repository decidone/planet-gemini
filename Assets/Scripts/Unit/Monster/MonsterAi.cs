using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using Unity.Netcode.Components;
using UnityEngine.Networking;
using Mono.CSharp;
using Unity.VisualScripting;
using static UnityEngine.GraphicsBuffer;
using System.IO;
using Unity.VisualScripting.FullSerializer;

// UTF-8 설정
public class MonsterAi : UnitCommonAi
{
    public new string name;
    MonsterSpawner spawnerScript;
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

    public bool isScriptActive = false;
    public int monsterType;    // 0 : 약한, 1 : 노멀, 2 : 강함, 3 : 가디언

    [SerializeField]
    protected bool waveState = false;
    [SerializeField]
    protected bool waveStateEnd = false;
    float waveSpeed = 9;

    //bool waveWaiting = false;
    public bool waveArrivePos = false;
    //bool goingBase = false;
    Vector3 wavePos; // 나중에 웨이브 대상으로 변경해야함 (현 맵 중심으로 이동하게)
    bool waveFindObj = false;
    bool isWaveColonyCallCheck = true;
    //bool goalPathBlocked = false;

    public float normalTraceTimer;
    float normalTraceInterval = 15f;
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

    public float justTraceTimer = 0f;
    [SerializeField]
    public float justTraceInterval;

    protected bool spawnerPhaseOn;
    protected bool spawnerLastPhaseOn;

    Coroutine selectTargetCo;
    Coroutine checkPathCo;

    protected override void Start()
    {
        base.Start();
        normalTraceInterval = Random.Range(10, 20);
        _t = transform;
        _effects = Effects.instance;
        unitIndex = GeminiNetworkManager.instance.GetUnitSOIndex(gameObject, monsterType, false);
    }

    protected override void FixedUpdate()
    {
        if (isScriptActive)
        {
            base.FixedUpdate();
            if(aIState == AIState.AI_NormalTrace)
            {
                normalTraceTimer += Time.deltaTime;
                if (normalTraceTimer > normalTraceInterval)
                {
                    normalTraceInterval = Random.Range(10, 20);
                    normalTraceTimer = 0f;
                    stopTrace = true;
                    TargetListReset();
                    SearchObjectsInRange();
                    PatrolRandomPosSet();
                }
            }
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

        if (isScriptActive)
        {
            if (aIState != AIState.AI_SpawnerCall && !targetOn)
            {
                if (aIState != AIState.AI_Die)
                {
                    searchTimer += Time.deltaTime;

                    if (searchTimer >= searchInterval)
                    {
                        SearchObjectsInRange();
                        searchTimer = 0f; // 탐색 후 타이머 초기화
                        tarDisCheckTime = tarDisCheckInterval; // 대상 거리 체크 즉시 수행
                    }

                    if (targetList.Count > 0)
                    {
                        tarDisCheckTime += Time.deltaTime;
                        if (tarDisCheckTime > tarDisCheckInterval)
                        {
                            tarDisCheckTime = 0f;
                            RemoveObjectsOutOfRange();
                            AttackTargetCheck();
                        }
                        AttackTargetDisCheck();
                    }
                    else if (!aggroTarget)
                    {
                        targetDist = 0;
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
                tarDisCheckTime += Time.deltaTime;
                if (tarDisCheckTime > tarDisCheckInterval)
                {
                    tarDisCheckTime = 0f;
                    AttackTargetDisCheck();
                    SpawnerCallCheck(aggroTarget);
                }
            }
            else
            {
                targetOn = false;
                aIState = AIState.AI_Idle;
            }

            if (!waveState && justTraceTimer < justTraceInterval)
            {
                justTraceTimer += Time.deltaTime;
                if (justTraceTimer >= justTraceInterval)
                {
                    stopTrace = true;
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

            tr.position = Vector3.MoveTowards(tr.position, targetWaypoint, Time.deltaTime * unitCommonData.MoveSpeed * slowSpeedPer);
            if (Vector3.Distance(tr.position, patrolPos) <= 0.3f)
            {
                aIState = AIState.AI_Idle;
                AnimSetFloat(lastMoveDirection, false);
                stopTrace = false;
                return;
            }
            if (Vector3.Distance(tr.position, targetWaypoint) <= 0.3f)
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

        //if (waveWaiting)
        //    return;

        float spawnDis = (tr.position - patrolMainPos).magnitude;

        if (waveState && !waveStateEnd && !waveArrivePos)
        {
            return;
        }

        if (!spawnerScript.nearUserObjExist && !spawnerScript.dieCheck && !waveState && !waveStateEnd)
        {
            checkPathCoroutine = StartCoroutine(CheckPath(spawnPos.position, "ReturnPos"));
        }
        else if (waveState && waveStateEnd)
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

        tr.position = Vector3.MoveTowards(tr.position, targetWaypoint, Time.deltaTime * speedMove * slowSpeedPer);
        if (Vector3.Distance(tr.position, spawnPos.position) <= 0.3f)
        {
            aIState = AIState.AI_Idle;
            if (!spawnerScript.nearUserObjExist && !spawnerScript.dieCheck)
            {
                isScriptActive = false;
                enabled = false;
                animator.enabled = false;
                capsule2D.enabled = false;
            }
            stopTrace = false;
            return;
        }
        if (Vector3.Distance(tr.position, targetWaypoint) <= 0.3f)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= movePath.Count)
            {
                aIState = AIState.AI_Idle;
                if (!spawnerScript.nearUserObjExist && !spawnerScript.dieCheck)
                {
                    isScriptActive = false;
                    enabled = false;
                    animator.enabled = false;
                    capsule2D.enabled = false;
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

                tr.position = Vector3.MoveTowards(tr.position, targetWaypoint, waveState ? Time.deltaTime * waveSpeed : (Time.deltaTime * (spawnerLastPhaseOn ?  speedMove : unitCommonData.MoveSpeed) * slowSpeedPer));

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
            if (!waveStateEnd)
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

                    tr.position = Vector3.MoveTowards(tr.position, targetWaypoint, Time.deltaTime * waveSpeed);

                    if (Vector3.Distance(tr.position, targetWaypoint) <= 0.3f)
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
            else
            {
                float basePosDist = Vector3.Distance(tr.position, spawnPos.position);
                if (basePosDist > unitCommonData.AttackDist)
                {
                    if (currentWaypointIndex >= movePath.Count)
                        return;

                    Vector3 targetWaypoint = movePath[currentWaypointIndex];
                    direction = targetWaypoint - tr.position;
                    direction.Normalize();
                    AnimSetFloat(targetVec, true);

                    tr.position = Vector3.MoveTowards(tr.position, targetWaypoint, Time.deltaTime * 6);

                    if (Vector3.Distance(tr.position, targetWaypoint) <= 0.3f)
                    {
                        currentWaypointIndex++;

                        if (currentWaypointIndex >= movePath.Count)
                        {
                            return;
                        }
                    }
                }
                else
                {
                    MonsterSpawnerManager.instance.BattleRemoveMonster(gameObject);
                    spawnerScript.ReturnMonster(this);
                    waveState = false;
                    waveStateEnd = false;
                }
            }
        }
    }

    protected override void AttackTargetDisCheck()
    {
        if (aggroTarget)
        {
            targetVec = (new Vector3(aggroTarget.transform.position.x, aggroTarget.transform.position.y, 0) - tr.position).normalized;
            targetDist = Vector3.Distance(tr.position, aggroTarget.transform.position);
            if(spawner != null)
                spawnDist = Vector3.Distance(tr.position, spawnPos.position);
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
                if (Obj.GetComponent<PlayerStatus>())
                    Obj.GetComponent<PlayerStatus>().TakeDamage(damage);
                else if (Obj.GetComponent<UnitAi>())
                    Obj.GetComponent<UnitAi>().TakeDamage(damage, 0);
                else if (Obj.GetComponent<Structure>())
                    Obj.GetComponent<Structure>().TakeDamage(damage);
            }
        }
        justTraceTimer = 0;
        stopTrace = false;
        if (targetOn)
        {
            targetOn = false;
            aIState = AIState.AI_Idle;
        }
    }

    void TargetListReset()
    {
        targetList.Clear();
    }

    protected override void SearchObjectsInRange()
    {
        Collider2D[] colliders;
        if (stopTrace)
        {
            colliders = Physics2D.OverlapCircleAll(tr.position, unitCommonData.MinCollRad);
        }
        else
        {
            colliders = Physics2D.OverlapCircleAll(tr.position, unitCommonData.ColliderRadius);
        }

        HashSet<GameObject> targetListSet = new HashSet<GameObject>(targetList);

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider2D collider = colliders[i];
            GameObject target = collider.gameObject;
            string targetTag = target.tag;

            if (targetTags.Contains(targetTag))
            {
                Structure structure = target.GetComponent<Structure>();
                if (structure && !targetListSet.Contains(target))
                {
                    targetListSet.Add(target);
                    targetList.Add(target);
                }

                PlayerController player = target.GetComponent<PlayerController>();
                if (player && !targetListSet.Contains(target))
                {
                    targetListSet.Add(target);
                    targetList.Add(target);
                }

                UnitAi unit = target.GetComponent<UnitAi>();
                if (unit && !targetListSet.Contains(target))
                {
                    targetListSet.Add(target);
                    targetList.Add(target);
                }

                waveFindObj = true;
            }
        }

        if (targetList.Count == 0)
        {
            aggroTarget = null;
        }

        if ((aIState == AIState.AI_NormalTrace || aIState == AIState.AI_Attack) && targetList.Count == 0)
        {
            if (!waveState)
            {
                idle = 0;
                aIState = AIState.AI_Idle;
                attackState = AttackState.Waiting;
                return;
            }
            else if (!waveArrivePos && waveFindObj)
            {
                WaveStart(wavePos);
            }
            else if (waveStateEnd)
            {
                checkPathCoroutine = StartCoroutine(CheckPath(spawnPos.position, "NormalTrace"));
            }
            else if (waveArrivePos)
            {
                checkPathCoroutine = StartCoroutine(CheckPath(patrolPos, "Patrol"));
            }
            //else if (!goingBase)
            //    WaveStart(wavePos);
            waveFindObj = false;
        }
    }

    protected override void AttackTargetCheck()
    {
        if (waveState)
        {
            if (targetList.Count > 0)
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
            }
            else
            {
                aggroTarget = null;
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
        else
        {
            if (selectTargetCo != null) StopCoroutine(selectTargetCo);
            selectTargetCo = StartCoroutine(SelectTargetAndTrace());
        }
    }

    IEnumerator SelectTargetAndTrace()
    {
        // 현재 상태가 복귀 중이면 타겟 탐색을 중단
        if (aIState == AIState.AI_ReturnPos) yield break;

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
            bool isAttackable = target.GetComponent<UnitAi>() || target.GetComponent<TowerAi>() || target.GetComponent<PlayerController>();
            if (!isAttackable)
            {
                // 공격 불가능하지만 가까운 타겟 저장
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

        // 가까운 공격 가능 타겟 최대 3개만 추출 (거리 기준 오름차순)
        //var nearestAttackables = attackableCandidates
        //    .OrderByDescending(x => x.aggro * 1.5f - x.dist * 0.5f) // 가중치 조절 가능
        //    .Take(4)
        //    .Select(x => x.obj)
        //    .ToList();
        var nearestAttackables = attackableCandidates
            .OrderByDescending(x => x.aggro * 1.5f - x.dist * 1.5f) // 가중치 조절 가능
            .Take(1)
            .Select(x => x.obj)
            .FirstOrDefault();
        ABPath pathReq = null;
        GameObject targetFind = null;

        float margin = 4.0f; // fallback 선택 기준 거리 마진
        float targetToDis = 2; // 실제 위치와 경로상의 차이
        float bestFallbackDist = float.MaxValue;
        float secondFallbackDist = float.MaxValue;
        GameObject fallbackTarget = null;
        bool cantFindAttack = false;
        bool fallbackTargetCanAttack = false;
        bool isFirst = true;

        // 공격 가능 후보 3개에 대해 탐색 경로 시도
        if (nearestAttackables)
        {
            bool finished = false;
            bool canAttack = false;

            // A* 경로 요청 생성
            pathReq = ABPath.Construct(tr.position, nearestAttackables.transform.position, p =>
            {
                finished = true;

                // 경로 계산 실패하거나 유효하지 않으면 무시
                if (!nearestAttackables || p.error || p.vectorPath.Count == 0) return;

                Vector3 last = p.vectorPath[^1]; // 마지막 지점
                float lastMoveDist = Vector3.Distance(last, nearestAttackables.transform.position);

                // 공격 사거리 안에 도달 가능한 경우
                if (lastMoveDist <= unitCommonData.AttackDist)
                {
                    targetFind = nearestAttackables;         // 실제로 도달 가능한 공격 타겟
                    cantFindAttack = true;
                    canAttack = true;
                    return;
                }

                // fallback 후보 저장 (가장 가까운 타겟을 보조적으로 선택)
                float dist = Vector3.Distance(transform.position, nearestAttackables.transform.position);

                if (isFirst)
                {
                    bestFallbackDist = dist;
                    fallbackTarget = nearestAttackables;
                    fallbackTargetCanAttack = canAttack;
                    isFirst = false;
                }
                else if (dist < bestFallbackDist)
                {
                    secondFallbackDist = bestFallbackDist;
                    bestFallbackDist = dist;
                    fallbackTarget = nearestAttackables;
                    fallbackTargetCanAttack = canAttack;
                }
                else if (dist < secondFallbackDist)
                {
                    secondFallbackDist = dist;
                }
            });

            // A* 경로 요청 시작
            seeker.StartPath(pathReq);
            yield return new WaitUntil(() => finished); // 경로 계산이 끝날 때까지 대기
        }

        bool hasResult = false;

        // fallback 대상이 있는 경우 조건에 따라 사용 여부 결정
        if (fallbackTarget)
        {
            if (secondFallbackDist - bestFallbackDist >= margin)
            {
                if (fallbackTargetCanAttack)
                {
                    targetFind = fallbackTarget;
                }
                else
                {
                    StartCoroutine(MonsterBaseMapCheck.instance.GetBlockingObjectToTarget(transform.position, fallbackTarget.transform.position, isInHostMap, (blockingObj) => {
                        if (blockingObj)
                        {
                            targetFind = blockingObj;
                        }
                        else
                        {
                            cantFindAttack = false;
                        }
                        hasResult = true;
                    }));
                    yield return new WaitUntil(() => hasResult);
                }
            }
        }

        // 도달 가능한 공격 대상이 있다면 거리 비교로 재검증
        if (targetFind)
        {
            if (pathReq != null)
            {
                float totalDistance = 0f;

                // 경로상의 총 거리 계산
                for (int i = 0; i < pathReq.vectorPath.Count - 1; i++)
                {
                    totalDistance += Vector3.Distance(pathReq.vectorPath[i], pathReq.vectorPath[i + 1]);
                }

                dist = Vector3.Distance(tr.position, targetFind.transform.position);

                // 실제 위치와 경로상의 차이가 너무 크면 도달 불가로 판단
                if (dist + targetToDis < totalDistance)
                {
                    StartCoroutine(MonsterBaseMapCheck.instance.GetBlockingObjectToTarget(transform.position, targetFind.transform.position, isInHostMap, (blockingObj) => {
                        if (blockingObj)
                        {
                            targetFind = blockingObj;
                        }
                        else
                        {
                            cantFindAttack = false;
                        }
                        hasResult = true;
                    }));
                    yield return new WaitUntil(() => hasResult);
                }
            }
        }

        // 적절한 공격 대상을 찾지 못했을 경우, 가장 가까운 공격 불가능한 대상으로 fallback
        if (!cantFindAttack || (!targetFind && nearestUnattackable))
        {
            targetFind = nearestUnattackable;
        }

        // 최종 타겟 설정
        aggroTarget = targetFind;

        // 추적 시작
        if (aggroTarget != null)
        {
            if (checkPathCo != null) StopCoroutine(checkPathCo);
            checkPathCo = StartCoroutine(CheckPath(aggroTarget.transform.position, "NormalTrace"));
        }
    }

    public override void TakeDamage(float damage, int attackType, float ignorePercent)
    {
        base.TakeDamage(damage, attackType, ignorePercent);
        normalTraceTimer = 0;
        justTraceTimer = 0;
        stopTrace = false;
    }

    public override void TakeDamage(float damage, int attackType)
    {
        base.TakeDamage(damage, attackType);
        normalTraceTimer = 0;
        justTraceTimer = 0;
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
            reducedDamage = Mathf.Max(damage - reducedDefense, 5);
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
        ABPath path = ABPath.Construct(tr.position, targetPos, null);
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

    [ServerRpc]
    public override void DieFuncServerRpc()
    {
        //base.DieFuncServerRpc();
        DieFuncClientRpc();

        MonsterSpawnerManager.instance.BattleRemoveMonster(gameObject);
    }

    [ClientRpc]
    protected override void DieFuncClientRpc()
    {
        //base.DieFuncClientRpc();
        DieFunc();

        if (InfoUI.instance.monster == this)
            InfoUI.instance.SetDefault();

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

        if(spawner != null)
        {
            spawner.GetComponent<MonsterSpawner>().MonsterDieChcek(gameObject, monsterType, waveState);
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
        spawner = monsterSpawner.gameObject;
        spawnPos = spawner.transform;
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
        if(scriptState)
        {
            enabled = true;
            animator.enabled = true;
            capsule2D.enabled = true;
            isScriptActive = scriptState;
        }
        else
        {
            if (IsServer)
            {
                if(aIState == AIState.AI_NormalTrace || aIState == AIState.AI_Attack)
                {
                    isScriptActive = true;
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

        if (aIState == AIState.AI_Idle || aIState == AIState.AI_Patrol || (justTraceTimer >= justTraceInterval && aggroTarget))
        {
            spawnerPhaseOn = true;
            aggroTarget = obj;
            targetVec = (new Vector3(aggroTarget.transform.position.x, aggroTarget.transform.position.y, 0) - tr.position).normalized;

            aIState = AIState.AI_SpawnerCall;
            targetOn = true;
            checkPathCoroutine = StartCoroutine(CheckPath(obj.transform.position, "SpawnerCall"));

            justTraceTimer = 0;
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

        tr.position = Vector3.MoveTowards(tr.position, targetWaypoint, Time.deltaTime * speedMove * slowSpeedPer);

        if (Vector3.Distance(tr.position, targetWaypoint) <= 0.3f)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= movePath.Count)
            {
                //aIState = AIState.AI_NormalTrace;
                return;
            }
        }       
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
    
    protected IEnumerator ReturnToWavePath()
    {
        Vector3 closestPoint = Vector3.zero;
        int closestIndex = -1;
        float minDistance = float.MaxValue;

        for (int i = 0; i < waveMovePath.Count; i++)
        {
            float distance = Vector3.Distance(tr.position, waveMovePath[i]);
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

    public void ColonyAttackStart(Vector3 _wavePos)
    {
        waveState = true;
        isWaveColonyCallCheck = false;

        wavePos = _wavePos;
        checkPathCoroutine = StartCoroutine(CheckPath(wavePos, "NormalTrace"));
    }

    public void WaveEnd()
    {
        waveStateEnd = true;
        checkPathCoroutine = StartCoroutine(CheckPath(spawnPos.position, "NormalTrace"));
    }

    public override void GameStartSet(UnitSaveData unitSaveData)
    {
        base.GameStartSet(unitSaveData);

        waveState = unitSaveData.waveState;
        waveStateEnd = unitSaveData.waveStateEnd;
        //waveWaiting = unitSaveData.waveWaiting;
        monsterType = unitSaveData.monsterType;
        spawnerLastPhaseOn = unitSaveData.spawnerLastPhaseOn;
    }

    public override UnitSaveData SaveData()
    {
        UnitSaveData data = base.SaveData();

        data.monsterType = monsterType;
        data.wavePos = Vector3Extensions.FromVector3(wavePos);
        data.waveState = waveState;
        data.waveStateEnd = waveStateEnd;
        //data.waveWaiting = waveWaiting;
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

        //StartCoroutine(Test());
    }

    IEnumerator Test()
    {
        yield return new WaitForSeconds(.333f);
        while (true)
        {
            _effects.TriggerDark(this.gameObject);
            yield return new WaitForSeconds(Random.Range(.5f, 1f));
        }
    }

    public void LastPhase()
    {
        spawnerLastPhaseOn = true;
    }
}
