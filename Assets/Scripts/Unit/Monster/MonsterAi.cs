using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

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
    //bool waveWaiting = false;
    public bool waveArrivePos = false;
    //bool goingBase = false;
    Vector3 wavePos; // 나중에 웨이브 대상으로 변경해야함 (현 맵 중심으로 이동하게)
    bool waveFindObj = false;
    bool isWaveColonyCallCheck = true;
    //bool goalPathBlocked = false;

    public float normalTraceTimer;
    float normalTraceInterval = 10f;
    bool stopTrace;

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
                }
                else if (attackState == AttackState.AttackStart)
                {
                    Attack();
                }
                break;
            case AIState.AI_NormalTrace:
                {
                    NormalTrace();
                    if (aggroTarget)
                        AttackCheck();
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

        if (!spawnerScript.nearUserObjExist && !spawnerScript.dieCheck && !waveState)
        {
            checkPathCoroutine = StartCoroutine(CheckPath(spawnPos.position, "ReturnPos"));
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

        tr.position = Vector3.MoveTowards(tr.position, targetWaypoint, Time.deltaTime * (unitCommonData.MoveSpeed + 7) * slowSpeedPer);
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

                tr.position = Vector3.MoveTowards(tr.position, targetWaypoint, Time.deltaTime * unitCommonData.MoveSpeed * slowSpeedPer);

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
        if (targetDist > unitCommonData.AttackDist)
            return;

        else if (IsServer && targetDist <= unitCommonData.AttackDist)
        {
            if (Obj != null)
            {
                if (Obj.GetComponent<PlayerStatus>())
                    Obj.GetComponent<PlayerStatus>().TakeDamage(damage);
                else if (Obj.GetComponent<UnitAi>())
                    Obj.GetComponent<UnitAi>().TakeDamage(damage, 0);
                else if (Obj.GetComponent<TowerAi>())
                    Obj.GetComponent<TowerAi>().TakeDamage(damage);
                else if (Obj.GetComponent<Structure>())
                    Obj.GetComponent<Structure>().TakeDamage(damage);
            }
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
            //else if (waveState && waveArrivePos && !waveWaiting)
            else if (waveState && waveArrivePos)
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

    public override void TakeDamage(float damage, int attackType, float ignorePercent)
    {
        base.TakeDamage(damage, attackType, ignorePercent);
        normalTraceTimer = 0;
    }

    public override void TakeDamage(float damage, int attackType)
    {
        base.TakeDamage(damage, attackType);
        normalTraceTimer = 0;
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


    [ServerRpc]
    protected override void DieFuncServerRpc()
    {
        base.DieFuncServerRpc();
        MonsterSpawnerManager.instance.BattleRemoveMonster(gameObject);
    }

    [ClientRpc]
    protected override void DieFuncClientRpc()
    {
        base.DieFuncClientRpc();

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
                }
            }
        }
    }

    public void SpawnerCallCheck()
    {
        if (aIState == AIState.AI_Idle || aIState == AIState.AI_Patrol)
        {
            targetVec = (new Vector3(spawnPos.transform.position.x, spawnPos.transform.position.y, 0) - tr.position).normalized;

            checkPathCoroutine = StartCoroutine(CheckPath(spawnPos.transform.position, "SpawnerCall"));
            aIState = AIState.AI_SpawnerCall;
        }
    }

    public virtual void SpawnerCall()
    {
        //AnimBoolCtrl("isMove", true);
        animator.SetBool("isMove", true);

        AnimSetFloat(targetVec, true);

        targetDist = Vector3.Distance(tr.position, spawnPos.transform.position);

        if (targetDist > unitCommonData.AttackDist)
        {
            if (currentWaypointIndex >= movePath.Count)
                return;

            Vector3 targetWaypoint = movePath[currentWaypointIndex];
            direction = targetWaypoint - tr.position;
            direction.Normalize();

            tr.position = Vector3.MoveTowards(tr.position, targetWaypoint, Time.deltaTime * unitCommonData.MoveSpeed * slowSpeedPer);

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


    public void WaveStart(Vector3 _wavePos)
    {
        waveState = true;
        //waveWaiting = false;

        int x = (int)Random.Range(-5, 5);
        int y = (int)Random.Range(-5, 5);

        wavePos = _wavePos + new Vector3(x, y);

        checkPathCoroutine = StartCoroutine(CheckPath(_wavePos, "NormalTrace"));
    }

    public void ColonyAttackStart(Vector3 _wavePos)
    {
        waveState = true;
        isWaveColonyCallCheck = false;

        wavePos = _wavePos;
        checkPathCoroutine = StartCoroutine(CheckPath(wavePos, "NormalTrace"));
    }

    //public void WaveSetMoveMonster(Vector3 pos)
    //{
    //    if (checkPathCoroutine == null)
    //    {
    //        waveState = true;
    //        checkPathCoroutine = StartCoroutine(CheckPath(pos, "NormalTrace"));
    //    }
    //}

    //public void WaveTeleport(Vector3 teleportPos, Vector3 setWavePos)
    //{
    //    Map map;

    //    if (isInHostMap)
    //        map = GameManager.instance.hostMap;
    //    else
    //        map = GameManager.instance.clientMap;
    //    Vector3 newWavePos;
    //    if (aIState != AIState.AI_NormalTrace && aIState != AIState.AI_Attack)
    //    {
    //        do
    //        {
    //            int x = (int)Random.Range(-20, 20);
    //            int y = (int)Random.Range(-20, 20);

    //            newWavePos = teleportPos + new Vector3(x, y);

    //        } while (map.GetCellDataFromPos((int)newWavePos.x, (int)newWavePos.y).biome.biome == "lake" 
    //        || map.GetCellDataFromPos((int)newWavePos.x, (int)newWavePos.y).biome.biome == "cliff");

    //        wavePos = setWavePos;
    //        waveState = true;
    //        waveWaiting = true;
    //        transform.position = newWavePos;
    //    }
    //}

    public override void GameStartSet(UnitSaveData unitSaveData)
    {
        base.GameStartSet(unitSaveData);

        waveState = unitSaveData.waveState;
        //waveWaiting = unitSaveData.waveWaiting;
        monsterType = unitSaveData.monsterType;
    }

    public override UnitSaveData SaveData()
    {
        UnitSaveData data = base.SaveData();

        data.unitIndex = GeminiNetworkManager.instance.GetMonsterSOIndex(this.gameObject, monsterType, false);
        data.monsterType = monsterType;
        data.wavePos = Vector3Extensions.FromVector3(wavePos);
        data.waveState = waveState;
        //data.waveWaiting = waveWaiting;
        data.isWaveColonyCallCheck = isWaveColonyCallCheck;
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
}
