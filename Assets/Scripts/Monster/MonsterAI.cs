using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public enum MonsterAIState   // ���� ���� ����
{
    MAI_Patrol,         // ��Ʈ�� ����
    MAI_AggroTrace,     // �����κ��� ������ ������ �� ���� ����
    MAI_NormalTrace,    // �Ϲ� ���� ����
    MAI_ReturnPos,      // ���� ������ �� ���ڸ��� ���ƿ��� ����
    MAI_Attack,         // ���� ����
    MAI_Die,            // ��� ����
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
    //�̵� ���� ����
    Vector2 moveDir = Vector2.zero;         // �̵� ���� ����ȭ
    Vector3 moveNextStep = Vector3.zero;    // �̵� ���� ����
    Vector2 targetVector = Vector2.zero;    // �̵� ��ġ
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
    public List<GameObject> targetList = new List<GameObject>();
    float tarDisCheckTime = 0f;
    float tarDisCheckInterval = 0.5f; // 0.5�� �������� ���� �Ÿ� üũ
    public float targetDist = 0.0f;                // Ÿ�ٰ��� �Ÿ�
    public int attackMotion = 0;
    //float colRad = 10;

    CircleCollider2D circle2D = null;
    CapsuleCollider2D capsule2D = null;
    //���� ���� ����

    // HpBar ����
    public Image hpBar;
    float hp = 100.0f;

    public SpriteRenderer unitSprite = null;
    public GameObject unitCanvers = null;

    public MonsterAIState monsterAI = MonsterAIState.MAI_Patrol; // ���� �� ��Ʈ�� ����
    public MonsterAttackState attackState = MonsterAttackState.Waiting;

    bool isFlip = false;

    // Start is called before the first frame update
    void Start()
    {
        animator = gameObject.GetComponentInChildren<Animator>();
        spawnPos = GameObject.Find("SpawnPos").transform;
        circle2D = GetComponent<CircleCollider2D>();
        capsule2D = GetComponent<CapsuleCollider2D>();
        circle2D.radius = monsterData.ColliderRadius;
        patrolPos = spawnPos.position;
        StartCoroutine("Patrol");
        isFlip = unitSprite.flipX;
        hp = monsterData.MaxHp;
        hpBar.fillAmount = hp / monsterData.MaxHp;
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
            tarDisCheckTime += Time.deltaTime;
            if (tarDisCheckTime > tarDisCheckInterval)
            {
                tarDisCheckTime = 0f;
                if (targetList.Count > 0)
                    AttackTargetDisCheck(); // ���� �Ÿ� üũ �Լ� ȣ��
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

    void MonsterMove()
    {
        if (attackState != MonsterAttackState.Attacking && attackState != MonsterAttackState.AttackDelay)
        {
            if (monsterAI == MonsterAIState.MAI_Patrol)
            {
                targetVector = patrolPos - this.transform.position;
                moveStep = monsterData.MoveSpeed * Time.fixedDeltaTime;
            }//if ((monsterAI == MonsterAI.MAI_Patrol))
            else
            {
                if (aggroTarget != null)
                {
                    targetVector = (new Vector3(aggroTarget.transform.position.x, aggroTarget.transform.position.y, 0) - this.transform.position);
                    moveStep = (monsterData.MoveSpeed + 2) * Time.fixedDeltaTime;
                }
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

        if(aggroTarget != null)
        {
            if (targetDist < monsterData.AttackDist)//�÷��̾���� �Ÿ��� ���ݹ��� ���� ����� �� ����
            {
                Invoke("TurnAttack", 0.1f);//0.1�� ���� ��߷� �ϴ� ����
            }
        }
        else
        {
            isPatrol = false;
            monsterAI = MonsterAIState.MAI_Patrol;
        }
    }//void NormalTrace()

    void AttackTargetDisCheck()
    {
        float closestDistance = float.MaxValue;

        // ��� ���Ϳ� ���� �Ÿ� ���
        foreach (GameObject target in targetList)
        {
            if(target != null)
            {
                float distance = Vector3.Distance(this.transform.position, target.transform.position);
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
            if (targetDist > monsterData.AttackDist)  // Ž�� ���� ������ ���� ��
            {
                animator.SetBool("isAttack", false);
                monsterAI = MonsterAIState.MAI_NormalTrace;              // ���󰡱� Ȱ��ȭ
                attackState = MonsterAttackState.Waiting;
            }
            else if (targetDist <= monsterData.AttackDist)  // ���� ���� ���� ������ ��        
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

        this.transform.position = this.transform.position + moveNextStep;
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
                patRandomPos = Random.insideUnitCircle * monsterData.PatrolRad;
                patrolPos = this.transform.position + patRandomPos;

                animator.SetBool("isMoving", true);
            }//else
        }//else

        int RandomTime = Random.Range(3, 6);

        yield return new WaitForSeconds(RandomTime);
        if (monsterAI == MonsterAIState.MAI_Patrol)
            StartCoroutine("Patrol");
    }//IEnumerator Patrol()

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
        unitCanvers.SetActive(false);

        capsule2D.enabled = false;
        circle2D.enabled = false;

        Destroy(this.gameObject, 1f);
    }

    public void RemoveTarget(GameObject unit)
    {
        if (targetList.Contains(unit))
        {
            targetList.Remove(unit);

            if (aggroTarget == unit)
                aggroTarget = null;
        }

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (!targetList.Contains(collision.gameObject))
            {
                isPatrol = false;
                targetList.Add(collision.gameObject);
            }
        }//if (collision.CompareTag("Player"))
        else if(collision.CompareTag("Unit"))
        {
            if (!targetList.Contains(collision.gameObject))
            {
                isPatrol = false;
                if (collision.isTrigger == true)
                    targetList.Add(collision.gameObject);
            }
        }
        else if (collision.CompareTag("Factory"))
        {
            if (!targetList.Contains(collision.gameObject))
            {
                {
                    isPatrol = false;
                    targetList.Add(collision.gameObject);
                }
            }
        }
        else if (collision.CompareTag("Tower"))
        {
            if (!targetList.Contains(collision.gameObject))
            {
                {
                    isPatrol = false;
                    if (collision.isTrigger == true)
                        targetList.Add(collision.gameObject);
                }
            }
        }
    }//private void OnTriggerEnter2D(Collider2D collision)

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if(Vector3.Distance(transform.position, collision.transform.position) > circle2D.radius)
            {
                isFollowEnd = true;
                targetList.Remove(collision.gameObject);
                //aggroTarget = null;            
            }
        }//if (collision.CompareTag("Player"))
        else if (collision.CompareTag("Unit"))
        {
            isFollowEnd = true;
            if (collision.isTrigger == true)
                targetList.Remove(collision.gameObject);
        }
        else if (collision.CompareTag("Factory"))
        {
            isFollowEnd = true;
            targetList.Remove(collision.gameObject);
        }
        else if (collision.CompareTag("Tower"))
        {
            isFollowEnd = true;
            if (collision.isTrigger == true)
                targetList.Remove(collision.gameObject);
        }
        if (targetList.Count == 0)
            aggroTarget = null;
    }//private void OnTriggerExit2D(Collider2D collision)
}
