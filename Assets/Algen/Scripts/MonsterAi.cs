using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum MonsterAI   // 몬스터 상태 관리
{
    MAI_Patrol,         // 패트롤 상태
    MAI_AggroTrace,     // 적으로부터 공격을 당했을 때 추적 상태
    MAI_NormalTrace,    // 일반 추적 상태
    MAI_ReturnPos,      // 추적 놓쳤을 때 재자리로 돌아오는 상태
    MAI_Attack,         // 공격 상태
}

public enum AttackState
{
    idle,
    AttackStart,
    Attacking,
    AttackDelay,
    AttackEnd,
}
public class MonsterAi : MonoBehaviour
{
    GetMonsterData getMonsterData;

    //이동 관련 변수
    Vector2 moveDir = Vector2.zero;         // 이동 벡터 정규화
    Vector3 moveNextStep = Vector3.zero;    // 이동 방향 벡터
    Vector2 targetVector = Vector2.zero;    // 이동 위치
    public Vector3 targetVelocity = Vector3.zero;
    float moveStep = 0.0f;                  // 프레임당 이동 거리
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
    public GameObject aggroTarget = null;   // 타겟
    public float targetDist = 0.0f;                // 타겟과의 거리
    public int attackMotion = 0;
    //공격 관련 변수

    MonsterAI monsterAI = MonsterAI.MAI_Patrol; // 시작 시 패트롤 상태
    public AttackState attackState = AttackState.idle;

    // Start is called before the first frame update
    void Start()
    {
        getMonsterData = GetComponentInChildren<GetMonsterData>();
        animator = gameObject.GetComponentInChildren<Animator>();
        spawnPos = GameObject.Find("SpawnPos").transform;
        this.gameObject.GetComponent<CircleCollider2D>().radius = getMonsterData.monsteData.ColliderRadius;
        patrolPos = spawnPos.position;
        StartCoroutine("Patrol");
    }//void Start()

    private void FixedUpdate()
    {
        MonsterMove();
        MonsterAICtrl();
    }//private void FixedUpdate()

    // Update is called once per frame
    void Update()
    {

    }//void Update()

    void MonsterAICtrl()
    {
        if (monsterAI == MonsterAI.MAI_Patrol)
        {
            if(isFollowEnd == true)            
                StartCoroutine("LastFollow");            
            else if (isPatrol == false)
                StartCoroutine("Patrol");
            //Debug.Log("MAI_Patrol");
        }//if (monsterAI == MonsterAI.MAI_Patrol)
        else if(monsterAI == MonsterAI.MAI_AggroTrace)
        {

        }//else if(monsterAI == MonsterAI.MAI_AggroTrace)
        else if (monsterAI == MonsterAI.MAI_NormalTrace)
        {
            NormalTrace();
            //Debug.Log("MAI_NormalTrace");

        }//else if (monsterAI == MonsterAI.MAI_NormalTrace)
        else if (monsterAI == MonsterAI.MAI_ReturnPos)
        {

        }//else if (monsterAI == MonsterAI.MAI_ReturnPos)
        else if (monsterAI == MonsterAI.MAI_Attack)
        {
            if (attackState == AttackState.idle)
            {
                Attack();
            }
            else if (attackState == AttackState.Attacking)
            {
                AttackMove();
            }
            //Debug.Log("MAI_Attack");
        }//else if (monsterAI == MonsterAI.MAI_Attack)
    }//void MonsterAICtrl()

    void MonsterMove()
    {
        if (attackState != AttackState.Attacking && attackState != AttackState.AttackDelay)
        {
            if (monsterAI == MonsterAI.MAI_Patrol)
            {
                targetVector = patrolPos - this.transform.position;
                moveStep = getMonsterData.monsteData.MoveSpeed * Time.fixedDeltaTime;
            }//if ((monsterAI == MonsterAI.MAI_Patrol))
            else
            {
                if (aggroTarget != null)
                {
                    //targetVector = aggroTarget.transform.position - this.transform.position;
                    targetVector = new Vector3(aggroTarget.transform.position.x, aggroTarget.transform.position.y - 0.5f) - this.transform.position;
                    moveStep = (getMonsterData.monsteData.MoveSpeed + 2) * Time.fixedDeltaTime;
                }//if (aggroTarget != null)
            }//else
            targetDist = targetVector.magnitude;
            moveDir = targetVector.normalized;
            moveNextStep = moveDir * moveStep;
            float patrolPosDis = targetVector.magnitude;

            if (patrolPosDis > 0.10f)
                ImgMrror();
            else
                animator.SetBool("isMoving", false);
        }
        if (moveNextStep.y > 0)
            animator.SetFloat("moveNextStepY", 1.0f);
        else if (moveNextStep.y <= 0)
            animator.SetFloat("moveNextStepY", -1.0f);

        if (moveDir.x > 0.75f || moveDir.x < -0.75f)
            animator.SetFloat("moveNextStepX", 1.0f);
        else if(moveDir.x <= 0.75f && moveDir.x >= -0.75f)
            animator.SetFloat("moveNextStepX", 0.0f);
    }//void MonsterMove()

    void NormalTrace()
    {
        animator.SetBool("isMoving", true);

        if (targetDist < getMonsterData.monsteData.AttackDist)//플레이어와의 거리가 공격범위 보다 가까울 때 공격
        {
            Invoke("TurnAttack", 0.1f);//0.1초 지연 즉발로 하니 꼬임
        }
    }//void NormalTrace()

    void TurnAttack()
    {
        monsterAI = MonsterAI.MAI_Attack;
    }

    void Attack()
    {
        if (targetDist > getMonsterData.monsteData.AttackDist)  // 탐색 범위 밖으로 나갈 때
        {
            animator.SetBool("isAttack", false);
            monsterAI = MonsterAI.MAI_NormalTrace;              // 따라가기 활성화
            attackState = AttackState.idle;
        }
        else if (targetDist <= getMonsterData.monsteData.AttackDist)  // 공격 범위 내로 들어왔을 때        
        { 
            RandomAttackNum(getMonsterData.monsteData.AttackNum, aggroTarget.transform);
        }
    }//void Attack()

    protected virtual void RandomAttackNum(int attackNum, Transform targetTr)
    {
        
    }

    protected virtual void AttackMove()
    {

    }
    public void ImgMrror()
    {
        if (moveNextStep.x >= 0)        
            transform.localScale = new Vector3(1, 1, 1);        
        else if (moveNextStep.x < 0)        
            transform.localScale = new Vector3(-1, 1, 1);

        this.transform.position = this.transform.position + moveNextStep;
    }//void ImgMrror()

    IEnumerator AttackDelay()
    {
        attackState = AttackState.AttackDelay;
        yield return new WaitForSeconds(getMonsterData.monsteData.AttDelayTime);
        attackState = AttackState.idle;
    }//IEnumerator LastFollow()
    IEnumerator LastFollow()
    {
        animator.SetBool("isMoving", false);
        patrolPos = this.transform.position;

        yield return new WaitForSeconds(1f);
        isFollowEnd = false;
    }//IEnumerator LastFollow()
    IEnumerator Patrol()
    {
        isPatrol = true;

        float spawnDis = (this.transform.position - spawnPos.position).magnitude;

        if (spawnDis > 7)
        {
            patrolPos = spawnPos.position;
            animator.SetBool("isMoving", true);
        }//if (spawnDis > 7)
        else
        {
            idle = Random.Range(0, 4);
            if (idle == 0)
            {
                patrolPos = this.transform.position;
                animator.SetBool("isMoving", false);
            }//if (idle == 0)
            else
            {
                patRandomPos = Random.insideUnitCircle * getMonsterData.monsteData.PatrolRad;
                patrolPos = this.transform.position + patRandomPos;

                animator.SetBool("isMoving", true);
            }//else
        }//else

        int RandomTime = Random.Range(3, 6);

        yield return new WaitForSeconds(RandomTime);
        if (monsterAI == MonsterAI.MAI_Patrol)
            StartCoroutine("Patrol");
    }//IEnumerator Patrol()

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            monsterAI = MonsterAI.MAI_NormalTrace;
            isPatrol = false;
            aggroTarget = collision.gameObject;
        }//if (collision.CompareTag("Player"))
    }//private void OnTriggerEnter2D(Collider2D collision)
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            targetVelocity = collision.GetComponentInChildren<PlayerMovement>().movement;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            monsterAI = MonsterAI.MAI_Patrol;
            isFollowEnd = true;
            aggroTarget = null;
        }//if (collision.CompareTag("Player"))
    }//private void OnTriggerExit2D(Collider2D collision)
}
