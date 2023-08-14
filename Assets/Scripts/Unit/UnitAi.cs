using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Pathfinding;

public class UnitAi : MonoBehaviour
{
    public enum UnitAIState  
    {
        UAI_Idle,
        UAI_Move,
        UAI_Attack,
        UAI_Patrol,
        UAI_NormalTrace, 
        UAI_AggroTrace,
        UAI_Die,
    }
    //스톱 기능도 추가

    public enum UnitAttackState
    {
        Waiting,
        Attack,
        AttackDelay,
    }

    [SerializeField]
    protected UnitData unitData;
    protected UnitData UnitData { set { unitData = value; } }

    [SerializeField]
    protected Animator animator;

    public SpriteRenderer unitSprite = null;
    public GameObject unitCanvas = null;

    // 이동 관련
    Transform tr;
    Rigidbody2D rg;
    Vector3 targetPosition;
    Vector2 lastMoveDirection = Vector2.zero; // 마지막으로 이동한 방향
    Vector3 direction = Vector3.zero;
    float moveRadi = 0f;
    Vector3 lastPosition;
    bool isMoveCheckCoroutine = false;
    bool isNewPosSet = false;
    float movedDistance = 0f;

    Seeker seeker;
    protected Coroutine checkPathCoroutine; // 실행 중인 코루틴을 저장하는 변수
    private int currentWaypointIndex; // 현재 이동 중인 경로 점 인덱스
    List<Vector3> movePath = new List<Vector3>();
    // 페트롤 관련
    Vector3 patrolStartPos;
    bool isPatrolGo = true;

    // 홀드 관련
    bool isHold = false;

    // 유닛 상태 관련
    UnitAIState unitAIState = UnitAIState.UAI_Idle; // 시작 시 패트롤 상태
    bool isLastStateOn = false;
    UnitAIState unitLastState = UnitAIState.UAI_Idle; // 시작 시 패트롤 상태
    UnitAttackState attackState = UnitAttackState.Waiting;
    public SpriteRenderer unitSelImg = null;
    bool unitSelect = false;
    UnitGroupCtrl unitGroupCtrl = null;
    Vector3 groupCenter;
    // 공격 관련 변수
    protected GameObject aggroTarget = null;   // 타겟
    List<Vector3> aggroPath = new List<Vector3>();    
    private int aggropointIndex; // 현재 이동 중인 경로 점 인덱스
    float mstDisCheckTime = 0f;
    float mstDisCheckInterval = 0.5f; // 1초 간격으로 몬스터 거리 체크
    float targetDist = 0.0f;         // 타겟과의 거리
    //CircleCollider2D circle = null;
    Vector3 targetVec = Vector3.zero;
    bool isTargetSet = false;               // 유저를 놓쳤는지 체크
    bool isAttackMove = true;
    public List<GameObject> monsterList = new List<GameObject>();
    bool isDelayAfterAttackCoroutine = false;
    private float searchInterval = 0.5f; // 딜레이 간격 설정
    private float searchTimer = 0f;
    CapsuleCollider2D capsule2D = null;

    // HpBar 관련
    public Image hpBar;
    float hp = 100.0f;

    bool isFlip = false;

    void Start()
    {
        tr = GetComponent<Transform>();
        rg = GetComponent<Rigidbody2D>();
        capsule2D = GetComponent<CapsuleCollider2D>();
        hp = unitData.MaxHp;
        hpBar.fillAmount = hp / unitData.MaxHp;
        unitGroupCtrl = GameObject.Find("UnitGroup").GetComponent<UnitGroupCtrl>();
        isFlip = unitSprite.flipX;
        seeker = GetComponent<Seeker>();
    }

    void FixedUpdate()
    {
        if(unitAIState != UnitAIState.UAI_Die)
            UnitAiCtrl();
    }

    void Update()
    {
        if (unitAIState != UnitAIState.UAI_Die)
        {
            searchTimer += Time.deltaTime;

            if (searchTimer >= searchInterval)
            {
                SearchObjectsInRange();
                searchTimer = 0f; // 탐색 후 타이머 초기화
            }

            if (monsterList.Count > 0)
            {
                mstDisCheckTime += Time.deltaTime;
                if (mstDisCheckTime > mstDisCheckInterval)
                {
                    mstDisCheckTime = 0f;
                    AttackTargetCheck();
                    RemoveObjectsOutOfRange();
                }
                AttackTargetDisCheck();
            }
        }
    }

    void UnitAiCtrl()
    {
        switch (unitAIState)
        {
            case UnitAIState.UAI_Idle:
                IdleFunc();
                break;            
            case UnitAIState.UAI_Move:
                MoveFunc();
                break;
            case UnitAIState.UAI_Patrol:
                PatrolFunc(isPatrolGo);
                break;                     
            case UnitAIState.UAI_Attack:
                if (attackState == UnitAttackState.Waiting)  
                {
                    AttackCheck();
                }
                else if (attackState == UnitAttackState.Attack)
                {
                    Attack();
                }
                break;
            case UnitAIState.UAI_NormalTrace:
                {
                    NormalTrace();
                    AttackCheck();
                }
                break;
            case UnitAIState.UAI_AggroTrace:
                {

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
        //if (isAttack)
        //    AttackTargetDisCheck();
        currentWaypointIndex = 0;
        isMoveCheckCoroutine = false;
        isTargetSet = false;
        targetPosition = dir;
        moveRadi = radi;

        lastMoveDirection = (targetPosition - tr.position).normalized; // 이동방향 저장

        if (checkPathCoroutine == null)
            checkPathCoroutine = StartCoroutine(CheckPath(targetPosition, "Move"));
        else
        {
            StopCoroutine(checkPathCoroutine);
            checkPathCoroutine = StartCoroutine(CheckPath(targetPosition, "Move"));
        }
        unitAIState = UnitAIState.UAI_Move;
        //direction = targetPosition - tr.position;
    }

    IEnumerator CheckPath(Vector3 targetPos, string moveFunc)
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
            unitAIState = UnitAIState.UAI_Move;
        }
        else if (moveFunc == "Patrol")
        {
            movePath = path.vectorPath;
            isPatrolGo = true;
            unitAIState = UnitAIState.UAI_Patrol;
        }
        else if (moveFunc == "NormalTrace")
        {
            aggroPath = path.vectorPath;

            if (!isLastStateOn)
            {
                unitLastState = unitAIState;
                isLastStateOn = true;
            }

            unitAIState = UnitAIState.UAI_NormalTrace;
        }

        SwBodyType(true);
        direction = targetPos - tr.position;
        checkPathCoroutine = null;
    }

    void AnimSetFloat(Vector3 direction, bool isNotLast)
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
        if (movePath.Count <= currentWaypointIndex)
            return;

        Vector3 targetWaypoint = movePath[currentWaypointIndex];

        direction = targetWaypoint - tr.position;
        direction.Normalize();

        if (!isHold)
        {
            tr.position = Vector3.MoveTowards(tr.position, targetWaypoint, Time.deltaTime * unitData.MoveSpeed);
            //도착지점에 도착하면 이동 멈춤
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
                    unitAIState = UnitAIState.UAI_Idle;
                    SwBodyType(false);
                }
            }

            // 방향에 따라 애니메이션 재생
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

        lastMoveDirection = (targetPosition - tr.position).normalized; // 이동방향 저장
        unitAIState = UnitAIState.UAI_Patrol;

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
        if (movePath.Count <= currentWaypointIndex)
            return;
        else if (currentWaypointIndex < 0)
            return;

        // 이동
        Vector3 targetWaypoint = movePath[currentWaypointIndex];

        direction = targetWaypoint - tr.position;
        direction.Normalize();

        if (!isHold)
        {
            tr.position = Vector3.MoveTowards(tr.position, targetWaypoint, Time.deltaTime * unitData.MoveSpeed);
            // 도착지점에 도착하면 이동 멈춤

            if (Vector3.Distance(tr.position, targetWaypoint) <= 0.5f)
            {
                if (isGo)
                {
                    currentWaypointIndex++;
                    if (currentWaypointIndex >= movePath.Count)
                    {
                        isPatrolGo = !isPatrolGo;
                        currentWaypointIndex = movePath.Count - 1;
                    }
                }
                else
                {
                    currentWaypointIndex--;
                    if (currentWaypointIndex < 0)
                    {
                        isPatrolGo = !isPatrolGo;
                        currentWaypointIndex = 0;
                    }
                }
            }

            animator.SetBool("isAttack", false);

            // 방향에 따라 애니메이션 재생
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

        // 일정 거리 이하이면 이동을 멈춘다
        if (movedDistance < 0.1f)
        {
            if (!isNewPosSet)
            {
                if(unitAIState == UnitAIState.UAI_Move)
                    unitAIState = UnitAIState.UAI_Idle;
                isAttackMove = true;
                //}
                AnimSetFloat(lastMoveDirection, false);
                isMoveCheckCoroutine = false;
                yield break; // 코루틴 종료         
            }
            else if (isNewPosSet)
            {
                isNewPosSet = false;
                isMoveCheckCoroutine = false;
                yield break; // 코루틴 종료  
            }
        }
        isMoveCheckCoroutine = false;
    }

    void SwBodyType(bool isMove)
    {
        if(isMove)
        {
            rg.bodyType = RigidbodyType2D.Kinematic;
        }
        else
        {
            rg.bodyType = RigidbodyType2D.Dynamic;
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

    public void HoldFunc()
    {
        isHold = true;
        isAttackMove = true;
        isTargetSet = false;
        AnimSetFloat(direction, false);
    }

    private void SearchObjectsInRange()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(tr.position, unitData.ColliderRadius);

        foreach (Collider2D collider in colliders)
        {
            GameObject monster = collider.gameObject;
            if (monster.CompareTag("Monster"))
            {
                if (!monsterList.Contains(monster))
                {
                    monsterList.Add(monster);
                }
            }
        }
    }

    private void RemoveObjectsOutOfRange()
    {
        for (int i = monsterList.Count - 1; i >= 0; i--)
        {
            if (monsterList[i] == null)
                monsterList.RemoveAt(i);
            else
            {
                GameObject monster = monsterList[i];
                if (Vector2.Distance(tr.position, monster.transform.position) > unitData.ColliderRadius)
                {
                    monsterList.RemoveAt(i);
                }
            }
        }
    }

    void AttackTargetCheck()
    {
        if (!isTargetSet)
        {
            float closestDistance = float.MaxValue;

            // 모든 몬스터에 대해 거리 계산
            foreach (GameObject monster in monsterList)
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
                if ((unitAIState == UnitAIState.UAI_Move && !isAttackMove) || isHold)
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

    void AttackTargetDisCheck()
    {
        if (aggroTarget != null)
        {
            targetVec = (new Vector3(aggroTarget.transform.position.x, aggroTarget.transform.position.y, 0) - tr.position).normalized;
            targetDist = Vector3.Distance(tr.position, aggroTarget.transform.position);
        }
    }

    void AttackCheck()
    {
        if (targetDist == 0)
            return;
        else if (targetDist > unitData.AttackDist)  // 공격 범위 밖으로 나갈 때
        {
            animator.SetBool("isAttack", false);
            unitAIState = UnitAIState.UAI_NormalTrace;
            attackState = UnitAttackState.Waiting;
        }
        else if (targetDist <= unitData.AttackDist)  // 공격 범위 내로 들어왔을 때        
        {
            unitAIState = UnitAIState.UAI_Attack;
            attackState = UnitAttackState.Attack;
        }
    }
    
    void Attack()
    {
        AnimSetFloat(targetVec, true);

        if (!isDelayAfterAttackCoroutine)
        {
            attackState = UnitAttackState.AttackDelay;
            StartCoroutine(DelayAfterAttack(unitData.AttDelayTime)); // 1.5초 후 딜레이 적용
        }
    }

    IEnumerator DelayAfterAttack(float delayTime)
    {
        isDelayAfterAttackCoroutine = true;
        AttackStart();
        SwBodyType(false);

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1.0f)
        {
            yield return null;
        }

        yield return new WaitForSeconds(delayTime);

        isDelayAfterAttackCoroutine = false;
    }

    protected virtual void AttackStart()
    {

    }

    void AttackEnd(string str)
    {
        if(str == "false")
        {
            animator.SetBool("isAttack", false);
            animator.SetBool("isMove", false);
            AnimSetFloat(targetVec, false);
            attackState = UnitAttackState.Waiting;
        }
    }

    public void TargetSet(GameObject obj)
    {
        isTargetSet = true;
        aggroTarget = obj;
        unitAIState = UnitAIState.UAI_NormalTrace;
        attackState = UnitAttackState.Waiting;
    }

    void NormalTrace()
    {
        if (isHold || aggroTarget == null)
        {
            animator.SetBool("isMove", false);
            return;
        }

        animator.SetBool("isMove", true);
        AnimSetFloat(targetVec, true);

        if (targetDist > unitData.AttackDist)
        {
            if (aggropointIndex >= aggroPath.Count)
                return;

            Vector3 targetWaypoint = aggroPath[aggropointIndex];
            direction = targetWaypoint - tr.position;
            direction.Normalize();

            tr.position = Vector3.MoveTowards(tr.position, targetWaypoint, Time.deltaTime * unitData.MoveSpeed);

            if (Vector3.Distance(tr.position, targetWaypoint) <= 0.3f)
            {
                aggropointIndex++;

                if (aggropointIndex >= aggroPath.Count)
                    return;
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (hp <= 0f)
            return;

        float reducedDamage = Mathf.Max(damage - unitData.Defense, 5);

        hp -= reducedDamage;
        hpBar.fillAmount = hp / unitData.MaxHp;

        if (hp <= 0f)
        {
            unitAIState = UnitAIState.UAI_Die;
            hp = 0f;
            DieFunc();
        }
    }

    void DieFunc()
    {
        unitSprite.color = new Color(1f, 1f, 1f, 0f);
        unitSelImg.color = new Color(1f, 1f, 1f, 0f);
        unitCanvas.SetActive(false);

        capsule2D.enabled = false;

        foreach (GameObject monster in monsterList)
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

        Destroy(this.gameObject, 1f);
    }

    public void RemoveMonster(GameObject monster)
    {
        if (monsterList.Contains(monster))
        {
            monsterList.Remove(monster);
        }
        if(monsterList.Count == 0)
        {
           aggroTarget = null;

            if (isLastStateOn)
            {
                unitAIState = unitLastState;
                isLastStateOn = false;
            }
        } 
    }
}
