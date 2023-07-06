using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using Pathfinding;

public enum MonsterAIState   // 몬스터 상태 관리
{
    MAI_Patrol,         // 패트롤 상태
    MAI_AggroTrace,     // 적으로부터 공격을 당했을 때 추적 상태
    MAI_NormalTrace,    // 일반 추적 상태
    MAI_ReturnPos,      // 추적 놓쳤을 때 재자리로 돌아오는 상태
    MAI_Attack,         // 공격 상태
    MAI_Die,            // 사망 상태
}

public enum MonsterAttackState
{
    Waiting,
    AttackStart,
    Attacking,
    AttackDelay,
    AttackEnd,
}

public class MonsterAi : MonoBehaviour
{
    [SerializeField]
    protected MonsterData monsterData;
    protected MonsterData MonsterData { set { monsterData = value; } }

    //이동 관련 변수
    Transform tr;
    Vector2 moveDir = Vector2.zero;         // 이동 벡터 정규화
    Vector3 moveNextStep = Vector3.zero;    // 이동 방향 벡터
    Vector2 targetVector = Vector2.zero;    // 이동 위치
    float moveStep = 0.0f;                  // 프레임당 이동 거리
    Seeker seeker;
    protected Coroutine checkPathCoroutine; // 실행 중인 코루틴을 저장하는 변수
    private int currentWaypointIndex; // 현재 이동 중인 경로 점 인덱스
    private int indexCheck = -1; // 현재 이동 중인 경로 점 인덱스
    List<Vector3> movePath = new List<Vector3>();
    //이동 관련 변수

    //애니메이션 관련 변수
    public Animator animator;
    //애니메이션 관련 변수

    // 패트롤 변수
    Transform spawnPos;                     // 스폰 위치
    bool isPatrol = false;                  // 패트롤 상태 체크
    bool isFollowEnd = false;               // 유저를 놓쳤는지 체크
    Vector3 patrolPos = Vector3.zero;       // 패트롤 지정 위치
    Vector3 patRandomPos = Vector3.zero;    // 패트롤 랜덤 위치
    int idle = 0;                           // 0 일경우 idle상태
                                            // 패트롤 변수

    //공격 관련 변수
    public LayerMask targetLayer;
    private float searchInterval = 1f; // 딜레이 간격 설정
    private float searchTimer = 0f;
    public GameObject aggroTarget = null;   // 타겟
    public List<GameObject> targetList = new List<GameObject>();
    float tarDisCheckTime = 0f;
    float tarDisCheckInterval = 1f; // 1초 간격으로 몬스터 거리 체크
    public float targetDist = 0.0f;                // 타겟과의 거리
    public int attackMotion = 0;
    List<string> targetTags = new List<string> { "Player", "Unit", "Tower", "Factory" };

    //float colRad = 10;

    CapsuleCollider2D capsule2D = null;
    //공격 관련 변수

    // HpBar 관련
    public Image hpBar;
    float hp = 100.0f;

    public SpriteRenderer unitSprite = null;
    public GameObject unitCanvas = null;

    public MonsterAIState monsterAI = MonsterAIState.MAI_Patrol; // 시작 시 패트롤 상태
    public MonsterAttackState attackState = MonsterAttackState.Waiting;

    bool isFlip = false;

    // Start is called before the first frame update
    void Start()
    {
        tr = GetComponent<Transform>();
        animator = gameObject.GetComponentInChildren<Animator>();
        spawnPos = GameObject.Find("SpawnPos").transform;
        capsule2D = GetComponent<CapsuleCollider2D>();
        patrolPos = spawnPos.position;
        StartCoroutine("Patrol");
        isFlip = unitSprite.flipX;
        hp = monsterData.MaxHp;
        hpBar.fillAmount = hp / monsterData.MaxHp;
        targetLayer = LayerMask.GetMask("Player", "Unit");
    }//void Start()

    private void FixedUpdate()
    {
        if(monsterAI != MonsterAIState.MAI_Die)
        {
            MonsterMove();
            MonsterAICtrl();
        }
    }//private void FixedUpdate()

    // Update is called once per frame
    void Update()
    {
        if (monsterAI != MonsterAIState.MAI_Die)
        {
            searchTimer += Time.deltaTime;

            if (searchTimer >= searchInterval)
            {
                SearchObjectsInRange();
                RemoveObjectsOutOfRange();
                searchTimer = 0f; // 탐색 후 타이머 초기화
            }

            if(targetList.Count > 0)
            {
               tarDisCheckTime += Time.deltaTime;
                if (tarDisCheckTime > tarDisCheckInterval)
                {
                    tarDisCheckTime = 0f;
                    AttackTargetDisCheck(); // 몬스터 거리 체크 함수 호출
                    RemoveObjectsOutOfRange();
                }
            } 
        }
    }//void Update()

    void MonsterAICtrl()
    {
        if (monsterAI == MonsterAIState.MAI_Patrol)
        {
            if(isFollowEnd == true)            
                StartCoroutine("LastFollow");            
            else if (isPatrol == false)
                StartCoroutine("Patrol");
            //Debug.Log("MAI_Patrol");
        }//if (monsterAI == MonsterAI.MAI_Patrol)
        else if(monsterAI == MonsterAIState.MAI_AggroTrace)
        {

        }//else if(monsterAI == MonsterAI.MAI_AggroTrace)
        else if (monsterAI == MonsterAIState.MAI_NormalTrace)
        {
            NormalTrace();
            //Debug.Log("MAI_NormalTrace");

        }//else if (monsterAI == MonsterAI.MAI_NormalTrace)
        else if (monsterAI == MonsterAIState.MAI_ReturnPos)
        {

        }//else if (monsterAI == MonsterAI.MAI_ReturnPos)
        else if (monsterAI == MonsterAIState.MAI_Attack)
        {
            if (attackState == MonsterAttackState.Waiting)
            {
                Attack();
            }
            else if (attackState == MonsterAttackState.Attacking)
            {
                AttackMove();
            }
            //Debug.Log("MAI_Attack");
        }//else if (monsterAI == MonsterAI.MAI_Attack)
    }//void MonsterAICtrl()

    //void MonsterMove()
    //{
    //    if (attackState != MonsterAttackState.Attacking && attackState != MonsterAttackState.AttackDelay)
    //    {
    //        if (monsterAI == MonsterAIState.MAI_Patrol)
    //        {
    //            targetVector = patrolPos - tr.position;
    //        }//if ((monsterAI == MonsterAI.MAI_Patrol))
    //        else
    //        {
    //            if (aggroTarget != null)
    //            {
    //                targetVector = (new Vector3(aggroTarget.transform.position.x, aggroTarget.transform.position.y, 0) - tr.position);
    //            }
    //        }//else

    //        moveStep = (monsterData.MoveSpeed + 2) * Time.fixedDeltaTime;

    //        targetDist = targetVector.magnitude;
    //        moveDir = targetVector.normalized;
    //        moveNextStep = moveDir * moveStep;
    //        float patrolPosDis = targetVector.magnitude;

    //        if (patrolPosDis > 0.10f)
    //            ImgMrror();
    //        else
    //            animator.SetBool("isMoving", false);
    //    }
    //    if (moveNextStep.y > 0)
    //        animator.SetFloat("moveNextStepY", 1.0f);
    //    else if (moveNextStep.y <= 0)
    //        animator.SetFloat("moveNextStepY", -1.0f);

    //    if (moveDir.x > 0.75f || moveDir.x < -0.75f)
    //        animator.SetFloat("moveNextStepX", 1.0f);
    //    else if(moveDir.x <= 0.75f && moveDir.x >= -0.75f)
    //        animator.SetFloat("moveNextStepX", 0.0f);
    //}//void MonsterMove()
    void MonsterMove()
    {
        if (attackState != MonsterAttackState.Attacking && attackState != MonsterAttackState.AttackDelay)
        {
            if (monsterAI == MonsterAIState.MAI_Patrol)
            {
                targetVector = patrolPos - tr.position;
            }//if ((monsterAI == MonsterAI.MAI_Patrol))
            else
            {
                if (aggroTarget != null)
                {
                    targetVector = (new Vector3(aggroTarget.transform.position.x, aggroTarget.transform.position.y, 0) - tr.position);
                }
            }//else

            moveStep = (monsterData.MoveSpeed + 2) * Time.fixedDeltaTime;
            targetDist = targetVector.magnitude;
            moveDir = targetVector.normalized;
            moveNextStep = moveDir * moveStep;
            float patrolPosDis = targetVector.magnitude;

            if (patrolPosDis > 0.10f)
                ImgMrror();
            else
                animator.SetBool("isMoving", false);

            tr.position = Vector3.MoveTowards(tr.position, tr.position + (Vector3)targetVector, moveStep);

            //tr.position = tr.position + moveNextStep;
        }
        if (moveNextStep.y > 0)
            animator.SetFloat("moveNextStepY", 1.0f);
        else if (moveNextStep.y <= 0)
            animator.SetFloat("moveNextStepY", -1.0f);

        if (moveDir.x > 0.75f || moveDir.x < -0.75f)
            animator.SetFloat("moveNextStepX", 1.0f);
        else if (moveDir.x <= 0.75f && moveDir.x >= -0.75f)
            animator.SetFloat("moveNextStepX", 0.0f);
    }//void MonsterMove()

    IEnumerator CheckPath(Vector3 targetPos, string moveFunc)
    {
        ABPath path = ABPath.Construct(tr.position, targetPos, null);
        seeker.CancelCurrentPathRequest();
        seeker.StartPath(path);
        //AutoRepathPolicy autoRepath = new AutoRepathPolicy();
        //autoRepath.DidRecalculatePath(targetPos);

        // Wait... (may take some time depending on how complex the path is)
        // The rest of the game will continue to run while waiting
        yield return StartCoroutine(path.WaitForPath());
        // The path is calculated now
        currentWaypointIndex = 0;
        movePath = path.vectorPath;

        checkPathCoroutine = null;
    }

    void NormalTrace()
    {
        animator.SetBool("isMoving", true);

        if(aggroTarget != null)
        {
            if (targetDist < monsterData.AttackDist)//플레이어와의 거리가 공격범위 보다 가까울 때 공격
            {
                Invoke("TurnAttack", 0.1f);//0.1초 지연 즉발로 하니 꼬임
            }
        }
        else
        {
            isPatrol = false;
            monsterAI = MonsterAIState.MAI_Patrol;
        }
    }//void NormalTrace()
    private void SearchObjectsInRange()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(tr.position, monsterData.ColliderRadius);
        bool isPatrolUpdated = false;

        foreach (Collider2D collider in colliders)
        {
            GameObject target = collider.gameObject;
            string targetTag = target.tag;

            if (targetTags.Contains(targetTag))
            {
                if (!targetList.Contains(target))
                {
                    targetList.Add(target);
                    isPatrolUpdated = true;
                }
            }
        }

        if (isPatrolUpdated)
            isPatrol = false;
    }

    private void RemoveObjectsOutOfRange()
    {
        for (int i = targetList.Count - 1; i >= 0; i--)
        {
            if(targetList[i] == null)
                targetList.RemoveAt(i);
            else
            {
                GameObject target = targetList[i];
                if (Vector2.Distance(tr.position, target.transform.position) > monsterData.ColliderRadius)
                {
                    targetList.RemoveAt(i);
                    isFollowEnd = true;
                }
            }
        }
        if (targetList.Count == 0)
            aggroTarget = null;
    }
    void AttackTargetDisCheck()
    {
        float closestDistance = float.MaxValue;

        // 모든 몬스터에 대해 거리 계산
        foreach (GameObject target in targetList)
        {
            if(target != null)
            {
                float distance = Vector3.Distance(tr.position, target.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    aggroTarget = target;
                }
            }
        }        

        if(aggroTarget != null && targetDist < monsterData.ColliderRadius && monsterAI != MonsterAIState.MAI_NormalTrace && monsterAI != MonsterAIState.MAI_Attack)
        {
            monsterAI = MonsterAIState.MAI_NormalTrace;
        }
    }


    void TurnAttack()
    {
        monsterAI = MonsterAIState.MAI_Attack;
    }

    void Attack()
    {
        if(aggroTarget != null)
        {
            if (targetDist > monsterData.AttackDist)  // 탐색 범위 밖으로 나갈 때
            {
                animator.SetBool("isAttack", false);
                monsterAI = MonsterAIState.MAI_NormalTrace;              // 따라가기 활성화
                attackState = MonsterAttackState.Waiting;
            }
            else if (targetDist <= monsterData.AttackDist)  // 공격 범위 내로 들어왔을 때
            { 
                RandomAttackNum(monsterData.AttackNum, aggroTarget.transform);
            }
        }
        else
        {
            isPatrol = false;
            monsterAI = MonsterAIState.MAI_Patrol;
        }
    }//void Attack()

    protected void AttackObjCheck(GameObject Obj)
    {
        if (Obj != null)
        {
            if (Obj.GetComponent<PlayerStatus>())
                Obj.GetComponent<PlayerStatus>().TakeDamage(monsterData.Damage);
            else if (Obj.GetComponent<UnitAi>())
                Obj.GetComponent<UnitAi>().TakeDamage(monsterData.Damage);
            else if (Obj.GetComponent<TowerAi>())
                Obj.GetComponent<TowerAi>().TakeDamage(monsterData.Damage);
            else if (Obj.GetComponent<Structure>())
                Obj.GetComponent<Structure>().TakeDamage(monsterData.Damage);
        }
    }

    protected virtual void RandomAttackNum(int attackNum, Transform targetTr)
    {
        
    }

    protected virtual void AttackMove()
    {

    }
    public void ImgMrror()
    {
        if (isFlip == true)
        {
            if (moveNextStep.x > 0)
            {
                if (unitSprite.flipX == false)
                {
                    unitSprite.flipX = true;
                }
            }
            else if (moveNextStep.x < 0)
            {
                if (unitSprite.flipX == true)
                {
                    unitSprite.flipX = false;
                }
            }
        }
        else
        {
            if (moveNextStep.x > 0)
            {
                if (unitSprite.flipX == true)
                {
                    unitSprite.flipX = false;
                }
            }
            else if (moveNextStep.x < 0)
            {
                if (unitSprite.flipX == false)
                {
                    unitSprite.flipX = true;
                }
            }
        }

        //tr.position = tr.position + moveNextStep;
    }//void ImgMrror()

    IEnumerator AttackDelay()
    {
        attackState = MonsterAttackState.AttackDelay;
        yield return new WaitForSeconds(monsterData.AttDelayTime);
        attackState = MonsterAttackState.Waiting;
    }
    IEnumerator LastFollow()
    {
        animator.SetBool("isMoving", false);
        patrolPos = tr.position;

        yield return new WaitForSeconds(1f);
        isFollowEnd = false;
    }//IEnumerator LastFollow()
    IEnumerator Patrol()
    {
        isPatrol = true;

        float spawnDis = (tr.position - spawnPos.position).magnitude;

        if (spawnDis > 7)
        {
            bool hasObj = true;

            while (hasObj)
            {
                // Generate a random position within spawnPos range
                float randomAngle = Random.Range(0f, 2f * Mathf.PI);
                float randomDistance = Random.Range(0f, monsterData.PatrolRad);
                Vector3 randomPosition = spawnPos.position + new Vector3(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * randomDistance;

                patrolPos = randomPosition;

                Collider2D[] hitColliders = Physics2D.OverlapCircleAll(patrolPos, 0.5f, LayerMask.GetMask("Obj"));
                if (hitColliders.Length == 0)
                    hasObj = false;
            }

            animator.SetBool("isMoving", true);
        }
        else
        {
            idle = Random.Range(0, 4);
            if (idle == 0)
            {
                patrolPos = tr.position;
                animator.SetBool("isMoving", false);
            }
            else
            {
                patrolPos = Vector3.zero;
                bool hasObj = true;

                while (hasObj)
                {
                    patRandomPos = Random.insideUnitCircle * monsterData.PatrolRad;
                    patrolPos = tr.position + patRandomPos;

                    Collider2D[] hitColliders = Physics2D.OverlapCircleAll(patrolPos, 0.5f, LayerMask.GetMask("Obj"));
                    if (hitColliders.Length == 0)                    
                        hasObj = false;                    
                }

                animator.SetBool("isMoving", true);
            }
        }

        int RandomTime = Random.Range(3, 6);

        yield return new WaitForSeconds(RandomTime);

        if (monsterAI == MonsterAIState.MAI_Patrol)
            StartCoroutine("Patrol");           
    }
    public void TakeDamage(float damage)
    {
        if (hp <= 0f)
            return;

        float reducedDamage = Mathf.Max(damage - monsterData.Defense, 5);

        hp -= reducedDamage;
        hpBar.fillAmount = hp / monsterData.MaxHp;

        if (hp <= 0f)
        {
            monsterAI = MonsterAIState.MAI_Die;
            hp = 0f;
            DieFunc();
        }
    }

    void DieFunc()
    {
        unitSprite.color = new Color(1f, 1f, 1f, 0f);
        unitCanvas.SetActive(false);

        capsule2D.enabled = false;

        foreach (GameObject target in targetList)
        {
            if (target != null)
            {
               if(target.TryGetComponent(out UnitAi unit))
                {
                    unit.RemoveMonster(this.gameObject);
                }
                else if (target.TryGetComponent(out AttackTower tower))
                {
                    tower.RemoveMonster(this.gameObject);
                }
            } 
        }
        Destroy(this.gameObject, 1f);
    }

    public void RemoveTarget(GameObject unit)
    {
        if (targetList.Contains(unit))
        {
            targetList.Remove(unit);
        }
        if(targetList.Count == 0)
        {
            aggroTarget = null;
            isFollowEnd = true;
        }
    }
    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.CompareTag("Player"))
    //    {
    //        if (!targetList.Contains(collision.gameObject))
    //        {
    //            isPatrol = false;
    //            targetList.Add(collision.gameObject);
    //        }
    //    }//if (collision.CompareTag("Player"))
    //    else if(collision.CompareTag("Unit"))
    //    {
    //        if (!targetList.Contains(collision.gameObject))
    //        {
    //            isPatrol = false;
    //            if (collision.isTrigger == true)
    //                targetList.Add(collision.gameObject);
    //        }
    //    }
    //    else if (collision.CompareTag("Factory"))
    //    {
    //        if (!targetList.Contains(collision.gameObject))
    //        {
    //            {
    //                isPatrol = false;
    //                targetList.Add(collision.gameObject);
    //            }
    //        }
    //    }
    //    else if (collision.CompareTag("Tower"))
    //    {
    //        if (!targetList.Contains(collision.gameObject))
    //        {
    //            {
    //                isPatrol = false;
    //                if (collision.isTrigger == true)
    //                    targetList.Add(collision.gameObject);
    //            }
    //        }
    //    }
    //}//private void OnTriggerEnter2D(Collider2D collision)

    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    if (collision.CompareTag("Player"))
    //    {
    //        if(Vector3.Distance(tr.position, collision.transform.position) > 1)
    //        {
    //            isFollowEnd = true;
    //            targetList.Remove(collision.gameObject);
    //            //aggroTarget = null;            
    //        }
    //    }//if (collision.CompareTag("Player"))
    //    else if (collision.CompareTag("Unit"))
    //    {
    //        isFollowEnd = true;
    //        if (collision.isTrigger == true)
    //            targetList.Remove(collision.gameObject);
    //    }
    //    else if (collision.CompareTag("Factory"))
    //    {
    //        isFollowEnd = true;
    //        targetList.Remove(collision.gameObject);
    //    }
    //    else if (collision.CompareTag("Tower"))
    //    {
    //        isFollowEnd = true;
    //        if (collision.isTrigger == true)
    //            targetList.Remove(collision.gameObject);
    //    }
    //    if (targetList.Count == 0)
    //        aggroTarget = null;
    //}//private void OnTriggerExit2D(Collider2D collision)
}
