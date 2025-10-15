using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Unity.Netcode;

// UTF-8 설정
public class UnitAi : UnitCommonAi
{
    public new string name;

    // 이동 관련
    float moveRadi;
    Vector3 lastPosition;
    float movedDistance;

    // 페트롤 관련
    bool isPatrolMove = true;

    // 홀드 관련
    bool isHold = false;

    // 유닛 상태 관련
    protected bool isLastStateOn = false;

    public AIState unitLastState = AIState.AI_Idle;
    public SpriteRenderer unitSelImg;
    bool unitSelect = false;
    UnitGroupCtrl unitGroupCtrl;
    // 공격 관련 변수
    List<Vector3> aggroPath = new List<Vector3>();
    private int aggropointIndex; // 현재 이동 중인 경로 점 인덱스
    protected bool isTargetSet = false;
    bool isAttackMove = true;
    GameObject selectTarget;
    public float selfHealingAmount;
    public float selfHealInterval;
    protected float selfHealTimer;
    public float visionRadius;
    protected float fogTimer;
    protected RepairEffectFunc repairEffect;

    public delegate void OnEffectUpgradeCheck();
    public OnEffectUpgradeCheck onEffectUpgradeCheck;
    protected bool hostClientUnitIn = false;
    protected bool[] increasedUnit = new bool[4];
    // 0 Hp, 1 데미지, 2 공격속도, 3 방어력

    protected AggroAmount aggroAmount;

    [HideInInspector]
    public bool playerUnitPortalIn;

    public int unitLevel = 0;
    [SerializeField]
    UnitCommonData[] unitLevelData;

    [SerializeField]
    Sprite[] unitIcon;

    protected override void Start()
    {
        base.Start();   // 테스트용 위치 변경 해야함
        aggroAmount = GetComponent<AggroAmount>();
        repairEffect = GetComponentInChildren<RepairEffectFunc>();
        unitGroupCtrl = GameManager.instance.GetComponent<UnitGroupCtrl>();
        selfHealInterval = 5;
        selfHealingAmount = 5f;
        onEffectUpgradeCheck += IncreasedStructureCheck;
        onEffectUpgradeCheck.Invoke();
        unitIndex = GeminiNetworkManager.instance.GetUnitSOIndex(gameObject, 0, true);
    }

    protected override void Update()
    {
        fogTimer += Time.deltaTime;
        if (fogTimer > MapGenerator.instance.fogCheckCooldown)
        {
            MapGenerator.instance.RemoveFogTile(transform.position, visionRadius);
            fogTimer = 0;
        }

        if (!IsServer)
            return;

        base.Update();

        if (targetList.Count == 0 && hp != maxHp && aIState != AIState.AI_Die)
        {
            selfHealTimer += Time.deltaTime;

            if (selfHealTimer >= selfHealInterval)
            {
                SelfHealingServerRpc();
                selfHealTimer = 0f;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public override void ClientConnectSyncServerRpc()
    {
        //base.ClientConnectSyncServerRpc();
        ClientConnectSyncClientRpc(hp);

        if (playerUnitPortalIn)
        {
            PortalUnitInFuncClientRpc(hostClientUnitIn);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    protected void SelfHealingServerRpc()
    {
        hp += selfHealingAmount;
        SelfHealingClientRpc(hp);
    }

    [ClientRpc]
    void SelfHealingClientRpc(float hostHp)
    {
        hp = hostHp;

        if (hp > maxHp)
            hp = maxHp;
        onHpChangedCallback?.Invoke();
        hpBar.fillAmount = hp / maxHp;
        repairEffect.EffectStart();

        if (hp >= maxHp)
            unitCanvas.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        NetworkObjManager.instance.NetObjAdd(gameObject);
        if(!GetComponent<TankCtrl>())
            GameManager.instance.PlayerUnitCount(1);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        NetworkObjManager.instance.NetObjRemove(gameObject);
        if (!GetComponent<TankCtrl>())
            GameManager.instance.PlayerUnitCount(-1);
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
            case AIState.AI_Idle:
                {
                    if (aggroTarget)
                        AttackCheck();
                }
                break;
        }
    }

    void IdleFunc()
    {
        aIState = AIState.AI_Idle;
        animator.SetBool("isMove", false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void MovePosSetServerRpc(Vector2 dir, float radi, bool isAttack)
    {
        isHold = false;
        isAttackMove = isAttack;
        isTargetSet = false;
        targetPosition = dir;
        moveRadi = radi;
        isPatrolMove = true;
        lastMoveDirection = (targetPosition - tr.position).normalized;
        //aIState = AIState.AI_Move;
        NomalTraceCheck(AIState.AI_Move);
        if (checkPathCoroutine == null)
            checkPathCoroutine = StartCoroutine(CheckPath(targetPosition, "Move"));
        else
        {
            StopCoroutine(checkPathCoroutine);
            checkPathCoroutine = StartCoroutine(CheckPath(targetPosition, "Move"));
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void PortalUnitInServerRpc(bool portalIn)
    {
        playerUnitPortalIn = portalIn;
    }

    void NomalTraceCheck(AIState ai)
    {
        if (aIState == AIState.AI_NormalTrace || aIState == AIState.AI_Attack)
        {
            unitLastState = ai;
        }
    }

    protected override IEnumerator CheckPath(Vector3 targetPos, string moveFunc)
    {
        ABPath path = ABPath.Construct(tr.position, targetPos, null);
        seeker.CancelCurrentPathRequest();
        seeker.StartPath(path);

        yield return StartCoroutine(path.WaitForPath());

        currentWaypointIndex = 1;
        aggropointIndex = 1;

        if (moveFunc == "Move")
        {
            movePath = path.vectorPath;
            aIState = AIState.AI_Move;
        }
        else if (moveFunc == "Patrol")
        {
            movePath = path.vectorPath;
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

        //direction = targetPos - tr.position;
        checkPathCoroutine = null;
    }

    protected override void AnimSetFloat(Vector3 direction, bool isNotLast)
    {
        float verticalValueY = 0f;
        float verticalValueX = 0f;
        if (direction.y > 0)
        {
            verticalValueY = 1f;
        }
        else if (direction.y <= 0)
        {
            verticalValueY = -1f;
        }

        if (direction.x > 0)
        {
            verticalValueX = 1f;
        }
        else if (direction.x <= 0)
        {
            verticalValueX = -1f;
        }

        if (isNotLast)
        {
            animator.SetFloat("Horizontal", verticalValueX);
            animator.SetFloat("Vertical", verticalValueY);
        }
        else
        {
            animator.SetFloat("lastMoveX", verticalValueX);
            animator.SetFloat("lastMoveY", verticalValueY);
        }

        //Debug.Log("x : " + verticalValueX + " : y : "+ verticalValueY);
    }

    protected void MoveFunc()
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
            tr.position = Vector3.MoveTowards(tr.position, targetWaypoint, Time.deltaTime * unitCommonData.MoveSpeed * slowSpeedPer);

            if(playerUnitPortalIn)
            {
                if (Vector3.Distance(tr.position, targetWaypoint) <= 0.5f)
                {
                    currentWaypointIndex++;

                    if (currentWaypointIndex >= movePath.Count)
                    {
                        AnimSetFloat(lastMoveDirection, false);
                        isAttackMove = true;
                        IdleFunc();
                        return;
                    }
                }
            }
            else
            {
                if (Vector3.Distance(tr.position, targetWaypoint) <= moveRadi / 2)
                {
                    currentWaypointIndex++;

                    if (currentWaypointIndex >= movePath.Count)
                    {
                        AnimSetFloat(lastMoveDirection, false);
                        isAttackMove = true;
                        IdleFunc();
                        return;
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
        {
            animator.SetBool("isMove", false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void PatrolPosSetServerRpc(Vector2 dir)
    {
        isHold = false;
        isAttackMove = true;
        isTargetSet = false;
        playerUnitPortalIn = false;
        targetPosition = dir;
        patrolPos = tr.position;

        lastMoveDirection = (targetPosition - tr.position).normalized;
        //aIState = AIState.AI_Patrol;
        NomalTraceCheck(AIState.AI_Patrol);

        if (checkPathCoroutine == null)
            checkPathCoroutine = StartCoroutine(CheckPath(targetPosition, "Patrol"));
        else
        {
            StopCoroutine(checkPathCoroutine);
            checkPathCoroutine = StartCoroutine(CheckPath(targetPosition, "Patrol"));
        }
        //direction = targetPosition - tr.position;
    }

    protected void PatrolFunc()
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
            tr.position = Vector3.MoveTowards(tr.position, targetWaypoint, Time.deltaTime * unitCommonData.MoveSpeed * slowSpeedPer);
            if (Vector3.Distance(tr.position, targetWaypoint) <= 0.5f)
            {
                currentWaypointIndex++;
                if (currentWaypointIndex >= movePath.Count)
                {
                    if (isPatrolMove)
                    {
                        checkPathCoroutine = StartCoroutine(CheckPath(patrolPos, "Patrol"));
                    }
                    else
                    {
                        checkPathCoroutine = StartCoroutine(CheckPath(targetPosition, "Patrol"));
                    }
                    isPatrolMove = !isPatrolMove;
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
            animator.SetBool("isMove", false);
        }
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

    [ServerRpc(RequireOwnership = false)]
    public void HoldFuncServerRpc()
    {
        isHold = true;
        isAttackMove = true;
        isTargetSet = false;
        isPatrolMove = true;
        playerUnitPortalIn = false;
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
                checkPathCoroutine = StartCoroutine(CheckPath(aggroTarget.transform.position, "Norm alTrace"));
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TargetSetServerRpc(NetworkObjectReference networkObjectReference)
    {
        isTargetSet = true;
        networkObjectReference.TryGet(out NetworkObject networkObject);
        aggroTarget = networkObject.gameObject;
        AttackTargetDisCheck();
        if (checkPathCoroutine == null)
            checkPathCoroutine = StartCoroutine(CheckPath(aggroTarget.transform.position, "NormalTrace"));
        else
        {
            StopCoroutine(checkPathCoroutine);
            checkPathCoroutine = StartCoroutine(CheckPath(aggroTarget.transform.position, "Norm alTrace"));
        }
    }

    protected override void NormalTrace()
    {
        if (isHold || aggroTarget == null)
        {
            animator.SetBool("isMove", false);
            return;
        }
        if (animator.GetBool("isAttack"))
        {
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

            tr.position = Vector3.MoveTowards(tr.position, targetWaypoint, Time.deltaTime * unitCommonData.MoveSpeed * slowSpeedPer);

            if (Vector3.Distance(tr.position, targetWaypoint) <= 0.3f)
            {
                aggropointIndex++;

                if (aggropointIndex >= aggroPath.Count)
                    return;
            }
        }
    }

    [ServerRpc]
    public void RepairServerRpc(float repairAmount)
    {
        hp += repairAmount;
        SelfHealingClientRpc(hp);
    }

    [ClientRpc]
    public override void TakeDamageClientRpc(float damage, int attackType, float option)
    {
        if (!unitCanvas.activeSelf)
            unitCanvas.SetActive(true);

        float reducedDamage = damage;

        if (attackType == 0 || attackType == 4)
        {
            reducedDamage = Mathf.Max(damage - defense, 5);
            if (attackType == 4)
            {
                if (!slowDebuffOn)
                    StartCoroutine(SlowDebuffDamage(option));
            }
        }
        else if (attackType == 2)
        {
            reducedDamage = Mathf.Max(damage - (defense * (option / 100)), 5);
        }
        else if (attackType == 3)
        {
            reducedDamage = 0;
            if (!takePoisonDamgae)
                StartCoroutine(PoisonDamage(damage, option));
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

        if (hp <= 0f && !dieCheck)
        {
            aIState = AIState.AI_Die;
            hp = 0f;
            dieCheck = true;
            if (unitSelect)
            {
                unitGroupCtrl.DieUnitCheck(this.gameObject);
                InfoUI.instance.UnitAmountSub((unitCommonData.name, unitLevel));
            }
            if (IsServer)
            {
                DieFuncServerRpc();
            }
        }       
    }

    [ClientRpc]
    protected override void DieFuncClientRpc()
    {
        //base.DieFuncClientRpc();
        DieFunc();

        UnitRemove();
    }

    public void UnitRemove()
    {
        onEffectUpgradeCheck -= IncreasedStructureCheck;

        if (InfoUI.instance.unit == this)
            InfoUI.instance.SetDefault();

        unitSelImg.color = new Color(1f, 1f, 1f, 0f);

        if (!IsServer)
            return;

        foreach (GameObject monster in targetList)
        {
            if (monster != null && monster.TryGetComponent(out MonsterAi monsterAi))
            {
                monsterAi.RemoveTarget(this.gameObject);
            }
        }

        //NetworkObjManager.instance.NetObjRemove(NetworkObject);

        if (IsServer && NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
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
            unitLastState = AIState.AI_Idle;
        }
        if (isTargetSet && aggroTarget == target)
        {
            aggroTarget = null;
            isTargetSet = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void PortalUnitInFuncServerRpc(bool mapIndex)
    {
        PortalUnitInFuncClientRpc(mapIndex);
    }

    [ClientRpc]
    public void PortalUnitInFuncClientRpc(bool mapIndex)
    {
        if (unitSelect)
        {
            UnitSelImg(false);
            unitGroupCtrl.DieUnitCheck(gameObject);
            InfoUI.instance.UnitAmountSub((unitCommonData.name, unitLevel));
        }
        playerUnitPortalIn = true;
        hostClientUnitIn = mapIndex;
        gameObject.SetActive(false);
    }


    [ServerRpc(RequireOwnership = false)]
    public void PortalUnitOutFuncServerRpc(bool isInHostMap, Vector3 spawnPos)
    {
        PortalUnitOutFuncClientRpc(isInHostMap, spawnPos);
    }

    [ClientRpc]
    public void PortalUnitOutFuncClientRpc(bool isInHostMap, Vector3 spawnPos)
    {
        transform.position = spawnPos;
        gameObject.SetActive(true);
        if (unitLevelData.Length > 0)
            animator.SetFloat("Level", unitLevel);
        AStarSet(isInHostMap);
    }

    UnitSaveData unitSaveData;

    public override void GameStartSet(UnitSaveData unitSave)
    {
        base.GameStartSet(unitSave);
        unitSaveData = unitSave;
        if (unitLevelData.Length > 0)
            unitLevel = unitSave.level;
        SetUnitCommonData();
        StartCoroutine(AstarScanCheck());
    }

    void stateLoad(UnitSaveData unitSaveData)
    {
        AStarSet(isInHostMap);
        Vector3 targetPos = Vector3Extensions.ToVector3(unitSaveData.moveTragetPos);
        Vector3 moveStartPos = Vector3Extensions.ToVector3(unitSaveData.moveStartPos);

        if (unitSaveData.aiState == 1)   // Move 상태
        {
            MovePosSetServerRpc(targetPos, 0, true);
        }
        else if (unitSaveData.aiState == 2)  // patrol 상태
        {
            if (unitSaveData.patrolDir) // true 일때 targetPos로 이동
            {
                PatrolPosSetServerRpc(targetPos);
                patrolPos = moveStartPos;
                isPatrolMove = true;
            }
            else    // false 일때 moveStartPos로 이동
            {
                PatrolPosSetServerRpc(moveStartPos);
                patrolPos = targetPos;
                isPatrolMove = false;
            }
        }
        else    // 이전 상태체크
        {
            if (unitSaveData.lastState == 1)
            {
                MovePosSetServerRpc(targetPos, 0, true);
            }
            else if (unitSaveData.aiState == 2)
            {
                if (unitSaveData.patrolDir) // true 일때 targetPos로 이동
                {
                    PatrolPosSetServerRpc(targetPos);
                    patrolPos = moveStartPos;
                    isPatrolMove = true;
                }
                else    // false 일때 moveStartPos로 이동
                {
                    PatrolPosSetServerRpc(moveStartPos);
                    patrolPos = targetPos;
                    isPatrolMove = false;
                }
            }
        }

        if (unitSaveData.holdState)
        {
            HoldFuncServerRpc();
        }
    }

    IEnumerator AstarScanCheck()
    {
        while (!MapGenerator.instance.isCompositeDone)
        {
            yield return null;
        }

        stateLoad(unitSaveData);
    }

    public override UnitSaveData SaveData()
    {
        UnitSaveData data = base.SaveData();

        data.aiState = (int)aIState;
        data.lastState = (int)unitLastState;
        data.holdState = isHold;
        data.patrolDir = isPatrolMove;
        data.moveTragetPos = Vector3Extensions.FromVector3(targetPosition);
        data.moveStartPos = Vector3Extensions.FromVector3(patrolPos);
        data.portalUnitIn = playerUnitPortalIn;
        data.hostClientUnitIn = hostClientUnitIn;
        data.level = unitLevel;

        return data;
    }

    public void IncreasedStructureCheck()
    {
        increasedUnit = ScienceDb.instance.IncreasedStructureCheck(2);

        if (increasedUnit[0])
        {
            bool isHpFull = false;
            if (maxHp == hp)
            {
                isHpFull = true;
            }
            maxHp = unitCommonData.UpgradeMaxHp;
            if (isHpFull)
                hp = maxHp;
        }
        if (increasedUnit[1])
        {
            damage = unitCommonData.UpgradeDamage;
        }
        if (increasedUnit[2])
        {
            attackSpeed = unitCommonData.UpgradeAttDelayTime;
        }
        if (increasedUnit[3])
        {
            defense = unitCommonData.UpgradeDefense;
        }
    }

    [ServerRpc (RequireOwnership = false)]
    public void UnitLevelUpFuncServerRpc()
    {
        UnitLevelUpFuncClientRpc();
    }

    [ClientRpc]
    void UnitLevelUpFuncClientRpc()
    {
        if (unitLevel < unitLevelData.Length - 1)
        {
            unitLevel++;
            SetUnitCommonData();
        }
    }

    void SetUnitCommonData()
    {
        if(unitLevelData.Length > 0)
        {
            unitCommonData = unitLevelData[unitLevel];
            animator.SetFloat("Level", unitLevel);
        }
        maxHp = unitCommonData.MaxHp;
        damage = unitCommonData.Damage;
        attackSpeed = unitCommonData.AttDelayTime;
        defense = unitCommonData.Defense;
    }

    public bool CanUpgrade()
    {
        if (unitLevel < unitLevelData.Length - 1)
            return true;
        else
            return false;
    }
}
