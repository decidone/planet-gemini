using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum MonsterAI   // ���� ���� ����
{
    MAI_Patrol,         // ��Ʈ�� ����
    MAI_AggroTrace,     // �����κ��� ������ ������ �� ���� ����
    MAI_NormalTrace,    // �Ϲ� ���� ����
    MAI_ReturnPos,      // ���� ������ �� ���ڸ��� ���ƿ��� ����
    MAI_Attack,         // ���� ����
}

public class MonsterAi : MonoBehaviour
{
    //�̵� ���� ����
    float moveSpeed = 3.0f;                 // �̵��ӵ�
    Vector2 moveDir = Vector2.zero;         // �̵� ���� ����ȭ
    Vector3 moveNextStep = Vector3.zero;    // �̵� ���� ����
    float moveStep = 0.0f;                  // �����Ӵ� �̵� �Ÿ�
    //�̵� ���� ����

    //�ִϸ��̼� ���� ����
    Animator animator;
    //�ִϸ��̼� ���� ����

    // ��Ʈ�� ����
    Transform spawnPos;                     // ���� ��ġ
    bool isPatrol = false;                  // ��Ʈ�� ���� üũ
    bool isFollowEnd = false;               // ������ ���ƴ��� üũ
    Vector3 patrolPos = Vector3.zero;       // ��Ʈ�� ���� ��ġ
    Vector3 patRandomPos = Vector3.zero;    // ��Ʈ�� ���� ��ġ
    float patrolRad = 5.0f;                 // ���� ��Ʈ�� �Ÿ�
    int idle = 0;                           // 0 �ϰ�� idle����
    // ��Ʈ�� ����

    //���� ���� ����
    public GameObject aggroTarget = null;   // Ÿ��
    float targetDist = 0.0f;                // Ÿ�ٰ��� �Ÿ�
    float attackDist = 2.0f;                // ���ݹ���
    Vector2 targetVector = Vector2.zero;    // Ÿ�� ��ġ
    //���� ���� ����

    MonsterAI monsterAI = MonsterAI.MAI_Patrol; // ���� �� ��Ʈ�� ����

    // Start is called before the first frame update
    void Start()
    {
        animator = gameObject.GetComponentInChildren<Animator>();
        spawnPos = GameObject.Find("SpawnPos").transform;
        patrolPos = spawnPos.position;
        StartCoroutine("Patrol");
    }//void Start()

    private void FixedUpdate()
    {
        MonsterMove();
    }//private void FixedUpdate()

    // Update is called once per frame
    void Update()
    {
        MonsterAICtrl();
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
            Attack();
            //Debug.Log("MAI_Attack");
        }//else if (monsterAI == MonsterAI.MAI_Attack)
    }//void MonsterAICtrl()

    void MonsterMove()
    {
        if ((monsterAI == MonsterAI.MAI_Patrol))
        {
            targetVector = patrolPos - this.transform.position;
            moveStep = moveSpeed * Time.fixedDeltaTime;
        }//if ((monsterAI == MonsterAI.MAI_Patrol))
        else
        {
            if (aggroTarget != null)
            {
                targetVector = aggroTarget.transform.position - this.transform.position;
                moveStep = (moveSpeed + 2) * Time.fixedDeltaTime;
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
    }//void MonsterMove()

    void NormalTrace()
    {
        animator.SetBool("isMoving", true);

        if (targetDist < attackDist)//�÷��̾���� �Ÿ��� ���ݹ��� ���� ����� �� ����        
            monsterAI = MonsterAI.MAI_Attack;
    }//void NormalTrace()

    void Attack()
    {
        if(targetDist > attackDist)        
            monsterAI = MonsterAI.MAI_NormalTrace;        
    }//void Attack()

    void ImgMrror()
    {
        if (moveNextStep.x > 0)        
            transform.localScale = new Vector3(1, 1, 1);        
        else if (moveNextStep.x < 0)        
            transform.localScale = new Vector3(-1, 1, 1);
        
        this.transform.position = this.transform.position + moveNextStep;
    }//void ImgMrror()


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
            idle = Random.Range(0, 3);
            if (idle == 0)
            {
                patrolPos = this.transform.position;
                animator.SetBool("isMoving", false);
            }//if (idle == 0)
            else
            {
                patRandomPos = Random.insideUnitCircle * patrolRad;
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
