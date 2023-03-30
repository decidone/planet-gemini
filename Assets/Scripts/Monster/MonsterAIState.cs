using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum MonsterAIState   // ���� ���� ����
{
    MAI_Patrol,         // ��Ʈ�� ����
    MAI_AggroTrace,     // �����κ��� ������ ������ �� ���� ����
    MAI_NormalTrace,    // �Ϲ� ���� ����
    MAI_ReturnPos,      // ���� ������ �� ���ڸ��� ���ƿ��� ����
    MAI_Attack,         // ���� ����
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
    GetMonsterData getMonsterData;

    //�̵� ���� ����
    Vector2 moveDir = Vector2.zero;         // �̵� ���� ����ȭ
    Vector3 moveNextStep = Vector3.zero;    // �̵� ���� ����
    Vector2 targetVector = Vector2.zero;    // �̵� ��ġ
    public Vector3 targetVelocity = Vector3.zero;
    float moveStep = 0.0f;                  // �����Ӵ� �̵� �Ÿ�
    //�̵� ���� ����

    //�ִϸ��̼� ���� ����
    public Animator animator;
    //�ִϸ��̼� ���� ����

    // ��Ʈ�� ����
    Transform spawnPos;                     // ���� ��ġ
    bool isPatrol = false;                  // ��Ʈ�� ���� üũ
    bool isFollowEnd = false;               // ������ ���ƴ��� üũ
    Vector3 patrolPos = Vector3.zero;       // ��Ʈ�� ���� ��ġ
    Vector3 patRandomPos = Vector3.zero;    // ��Ʈ�� ���� ��ġ
    int idle = 0;                           // 0 �ϰ�� idle����
    // ��Ʈ�� ����

    //���� ���� ����
    public GameObject aggroTarget = null;   // Ÿ��
    public float targetDist = 0.0f;                // Ÿ�ٰ��� �Ÿ�
    public int attackMotion = 0;
    //���� ���� ����

    MonsterAIState monsterAI = MonsterAIState.MAI_Patrol; // ���� �� ��Ʈ�� ����
    public MonsterAttackState attackState = MonsterAttackState.Waiting;

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

    void MonsterMove()
    {
        if (attackState != MonsterAttackState.Attacking && attackState != MonsterAttackState.AttackDelay)
        {
            if (monsterAI == MonsterAIState.MAI_Patrol)
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

        if (targetDist < getMonsterData.monsteData.AttackDist)//�÷��̾���� �Ÿ��� ���ݹ��� ���� ����� �� ����
        {
            Invoke("TurnAttack", 0.1f);//0.1�� ���� ��߷� �ϴ� ����
        }
    }//void NormalTrace()

    void TurnAttack()
    {
        monsterAI = MonsterAIState.MAI_Attack;
    }

    void Attack()
    {
        if (targetDist > getMonsterData.monsteData.AttackDist)  // Ž�� ���� ������ ���� ��
        {
            animator.SetBool("isAttack", false);
            monsterAI = MonsterAIState.MAI_NormalTrace;              // ���󰡱� Ȱ��ȭ
            attackState = MonsterAttackState.Waiting;
        }
        else if (targetDist <= getMonsterData.monsteData.AttackDist)  // ���� ���� ���� ������ ��        
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
        attackState = MonsterAttackState.AttackDelay;
        yield return new WaitForSeconds(getMonsterData.monsteData.AttDelayTime);
        attackState = MonsterAttackState.Waiting;
    }
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
        if (monsterAI == MonsterAIState.MAI_Patrol)
            StartCoroutine("Patrol");
    }//IEnumerator Patrol()

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            monsterAI = MonsterAIState.MAI_NormalTrace;
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
            monsterAI = MonsterAIState.MAI_Patrol;
            isFollowEnd = true;
            aggroTarget = null;
        }//if (collision.CompareTag("Player"))
    }//private void OnTriggerExit2D(Collider2D collision)
}
