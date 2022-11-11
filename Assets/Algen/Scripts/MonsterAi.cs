using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum MonsterAI
{
    MAI_Patrol,        //패트롤 상태
    MAI_AggroTrace,    //적으로부터 공격을 당했을 때 추적 상태
    MAI_NormalTrace,   //일반 추적 상태
    MAI_ReturnPos,     //추적 놓쳤을 때 재자리로 돌아오는 상태
    MAI_Attack,        //공격 상태
}

public class MonsterAi : MonoBehaviour
{
    float moveSpeed = 3.0f;
    Rigidbody2D rb;
    CircleCollider2D cirColl;
    Animator animator;

    Transform spawnPos;

    Vector2 movement;
    int idle = 0;
    int moveUpDown = 0;
    int moveLeftRight = 0;
    bool isPatrol = false;
    bool isFollowEnd = false;
    float colRadius = 10.0f;//콜리더 탐색 범위
    float targetDist = 0.0f;//타겟과의 거리
    float attackDist = 2.0f;//공격범위
    public GameObject aggroTarget = null;
    Vector2 targetVector = Vector2.zero;
    Vector2 moveDir = Vector2.zero;
    Vector3 moveNextStep = Vector3.zero;
    float moveStep = 0.0f;

    MonsterAI monsterAI = MonsterAI.MAI_Patrol;

    // Start is called before the first frame update
    void Start()
    {
        animator = gameObject.GetComponentInChildren<Animator>();
        rb = GetComponentInChildren<Rigidbody2D>();
        cirColl = GetComponentInChildren<CircleCollider2D>();
        cirColl.radius = colRadius;
        spawnPos = this.gameObject.transform;
        StartCoroutine("Patrol");
    }

    private void FixedUpdate()
    {
        MonsterMove();
    }

    // Update is called once per frame
    void Update()
    {
        MonsterAICtrl();
    }

    void MonsterAICtrl()
    {
        if (monsterAI == MonsterAI.MAI_Patrol)
        {
            if(isFollowEnd == true)
            {
                StartCoroutine("LastFollow");
            }
            else if (isPatrol == false)
                StartCoroutine("Patrol");

            //Debug.Log("MAI_Patrol");
        }
        else if(monsterAI == MonsterAI.MAI_AggroTrace)
        {

        }
        else if (monsterAI == MonsterAI.MAI_NormalTrace)
        {
            NormalTrace();
            //Debug.Log("MAI_NormalTrace");

        }
        else if (monsterAI == MonsterAI.MAI_ReturnPos)
        {

        }
        else if (monsterAI == MonsterAI.MAI_Attack)
        {
            Attack();
            //Debug.Log("MAI_Attack");
        }
    }

    void MonsterMove()
    {
        if ((monsterAI == MonsterAI.MAI_Patrol))
        {
            if (movement.x == 1)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            else if (movement.x == -1)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            if (aggroTarget != null)
            {
                animator.SetBool("isMoving", true);

                targetVector = aggroTarget.transform.position - this.transform.position;
                targetDist = targetVector.magnitude;

                moveDir = targetVector.normalized;

                moveStep = (moveSpeed + 2) * Time.fixedDeltaTime;
                moveNextStep = moveDir * moveStep;

                if (moveNextStep.x > 0)
                {
                    transform.localScale = new Vector3(1, 1, 1);
                }
                else if (moveNextStep.x < 0)
                {
                    transform.localScale = new Vector3(-1, 1, 1);
                }

                this.transform.position = this.transform.position + moveNextStep;
            }
        }
    }

    void NormalTrace()
    {
        if (targetDist < attackDist)//플레이어와의 거리가 공격범위 보다 가까울 때 공격
        {
            monsterAI = MonsterAI.MAI_Attack;
        }
    }

    void Attack()
    {
        if(targetDist > attackDist)
        {
            monsterAI = MonsterAI.MAI_NormalTrace;
        }
    }
    IEnumerator LastFollow()
    {
        animator.SetBool("isMoving", false);
        movement.x = 0;
        movement.y = 0;

        yield return new WaitForSeconds(1f);
        isFollowEnd = false;
    }


    IEnumerator Patrol()
    {
        isPatrol = true;
        idle = Random.Range(0, 3);
        Debug.Log(idle);
        if (idle == 0)
        {
            animator.SetBool("isMoving", false);
            movement.x = 0;
            movement.y = 0;
        }
        else
        {
            movement.x = (Random.Range(0, 2) * 2) - 1;//-1,1
            movement.y = (Random.Range(0, 2) * 2) - 1;
            animator.SetBool("isMoving", true);
        }

        yield return new WaitForSeconds(3f);
        if(monsterAI == MonsterAI.MAI_Patrol)
            StartCoroutine("Patrol");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            monsterAI = MonsterAI.MAI_NormalTrace;
            isPatrol = false;
            aggroTarget = collision.gameObject;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            monsterAI = MonsterAI.MAI_Patrol;
            //isPatrol = false;
            isFollowEnd = true;
            aggroTarget = null;
        }
    }
}
