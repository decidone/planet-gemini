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
    //���� ��ɵ� �߰�
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

    // �̵� ����
    Transform tr;
    Vector3 targetPosition;
    Vector2 lastMoveDirection = Vector2.zero; // ���������� �̵��� ����
    Vector3 direction = Vector3.zero;
    float moveRadi = 0f;
    Vector3 lastPosition;
    bool isMoveCheckCoroutine = false;
    bool isNewPosSet = false;
    float movedDistance = 0f;

    Seeker seeker;
    protected Coroutine checkPathCoroutine; // ���� ���� �ڷ�ƾ�� �����ϴ� ����
    private int currentWaypointIndex; // ���� �̵� ���� ��� �� �ε���
    private int indexCheck = -1; // ���� �̵� ���� ��� �� �ε���
    List<Vector3> movePath = new List<Vector3>();
    // ��Ʈ�� ����
    Vector3 patrolStartPos;
    bool isPatrolGo = true;

    // Ȧ�� ����
    bool isHold = false;

    // ���� ���� ����
    [SerializeField]
    UnitAIState unitAIState = UnitAIState.UAI_Idle; // ���� �� ��Ʈ�� ����
    bool isLastStateOn = false;
    UnitAIState unitLastState = UnitAIState.UAI_Idle; // ���� �� ��Ʈ�� ����
    UnitAttackState attackState = UnitAttackState.Waiting;
    public SpriteRenderer unitSelImg = null;
    bool unitSelect = false;
    UnitGroupCtrl unitGroupCtrl = null;
    Vector3 groupCenter;
    // ���� ���� ����
    [SerializeField]
    protected GameObject aggroTarget = null;   // Ÿ��
    float mstDisCheckTime = 0f;
    float mstDisCheckInterval = 1f; // 1�� �������� ���� �Ÿ� üũ
    float targetDist = 0.0f;         // Ÿ�ٰ��� �Ÿ�
    //CircleCollider2D circle = null;
    Vector3 targetVec = Vector3.zero;
    bool isTargetSet = false;               // ������ ���ƴ��� üũ
    bool isAttackMove = true;
    public List<GameObject> monsterList = new List<GameObject>();
    bool isDelayAfterAttackCoroutine = false;
    private float searchInterval = 1f; // ������ ���� ����
    private float searchTimer = 0f;
    CapsuleCollider2D capsule2D = null;

    // HpBar ����
    public Image hpBar;
    float hp = 100.0f;

    bool isFlip = false;

    // Start is called before the first frame update
    void Start()
    {
        tr = GetComponent<Transform>();
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

    // Update is called once per frame
    void Update()
    {
        if (unitAIState != UnitAIState.UAI_Die)
        {
            searchTimer += Time.deltaTime;

            if (searchTimer >= searchInterval)
            {
                SearchObjectsInRange();
                searchTimer = 0f; // Ž�� �� Ÿ�̸� �ʱ�ȭ
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
        //if(monsterList.Count > 0)
        //    AttackTargetDisCheck();
    }

    void IdleFunc()
    {
        animator.SetBool("isMove", false);
        isMoveCheckCoroutine = false;
    }

    //public void MovePosSet(Vector3 dir, List<Vector3> targetWaypoint, float radi, bool isAttack, Vector3 gCenter)
    //{
    //    isNewPosSet = true;
    //    isHold = false;
    //    isAttackMove = isAttack;
    //    isMoveCheckCoroutine = false;
    //    isTargetSet = false;
    //    targetPosition = dir;
    //    moveRadi = radi;
    //    groupCenter = gCenter;
    //    currentWaypointIndex = 0;
    //    movePath = targetWaypoint;

    //    lastMoveDirection = (targetPosition - tr.position).normalized; // �̵����� ����
    //    unitAIState = UnitAIState.UAI_Move;
    //    direction = targetPosition - tr.position;
    //}

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

        lastMoveDirection = (targetPosition - tr.position).normalized; // �̵����� ����

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
        //AutoRepathPolicy autoRepath = new AutoRepathPolicy();
        //autoRepath.DidRecalculatePath(targetPos);

        // Wait... (may take some time depending on how complex the path is)
        // The rest of the game will continue to run while waiting
        yield return StartCoroutine(path.WaitForPath());
        // The path is calculated now
        currentWaypointIndex = 0;
        movePath = path.vectorPath;

        if(moveFunc == "Move")
            unitAIState = UnitAIState.UAI_Move;
        else if(moveFunc == "Patrol")
        {
            isPatrolGo = true;
            unitAIState = UnitAIState.UAI_Patrol;
        }

        direction = targetPosition - tr.position;

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

        if(isFlip == true)
        {
           if (direction.x > 0)
            {
                if(unitSprite.flipX == false)
                {
                    unitSprite.flipX = true;
                }
            }
            else if (direction.x < 0)
            {
                if (unitSprite.flipX == true)
                {
                    unitSprite.flipX = false;
                }
            }
        }
        else
        {
            if (direction.x > 0)
            {
                if (unitSprite.flipX == true)
                {
                    unitSprite.flipX = false;
                }
            }
            else if (direction.x < 0)
            {
                if (unitSprite.flipX == false)
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
            //���������� �����ϸ� �̵� ����
            //if (Vector3.Distance(tr.position, targetPosition) <= moveRadi / 2)
            if (tr.position == targetPosition)
            {
                if (!isMoveCheckCoroutine)
                    StartCoroutine(UnitMoveCheck());
            }
            //if (Vector3.Distance(tr.position, targetWaypoint) <= moveRadi/2)
            if (tr.position == targetWaypoint)

            {
                currentWaypointIndex++;

                if (currentWaypointIndex >= movePath.Count)
                {
                    Debug.Log("????");
                    AnimSetFloat(lastMoveDirection, false);
                    isAttackMove = true;
                    unitAIState = UnitAIState.UAI_Idle;
                }
            }

            // ���⿡ ���� �ִϸ��̼� ���
            animator.SetBool("isMove", true);

            if (direction.magnitude > 0.5f)
            {
                AnimSetFloat(direction, true);
            }
        }
        else
            animator.SetBool("isMove", false);
    }

    //void MoveFunc()
    //{
    //    // �̵�
    //    direction = targetPosition - tr.position;
    //    if (!isHold)
    //    {
    //        tr.position = Vector3.MoveTowards(tr.position, targetPosition, Time.deltaTime * unitData.MoveSpeed);
    //        // ���������� �����ϸ� �̵� ����

    //        if (Vector3.Distance(tr.position, targetPosition) < moveRadi)
    //        {
    //            if (!isMoveCheckCoroutine)
    //                StartCoroutine(UnitMoveCheck());
    //        }
    //        else if (Vector3.Distance(tr.position, targetPosition) < 0.3f)
    //        {
    //            AnimSetFloat(lastMoveDirection, false);
    //            isAttackMove = true;
    //            unitAIState = UnitAIState.UAI_Idle;
    //        }

    //        // ���⿡ ���� �ִϸ��̼� ���
    //        animator.SetBool("isMove", true);

    //        if (direction.magnitude > 0.5f)
    //        {
    //            AnimSetFloat(direction, true);
    //        }
    //    }
    //    else
    //        animator.SetBool("isMove", false);
    //}

    public void PatrolPosSet(Vector2 dir)
    {
        isHold = false;
        isAttackMove = true;
        isMoveCheckCoroutine = false;
        isTargetSet = false;

        targetPosition = dir;
        patrolStartPos = tr.position;
        indexCheck = -1;

        lastMoveDirection = (targetPosition - tr.position).normalized; // �̵����� ����
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

        // �̵�
        Vector3 targetWaypoint = movePath[currentWaypointIndex];

        direction = targetWaypoint - tr.position;
        direction.Normalize();

        if (!isHold)
        {
            tr.position = Vector3.MoveTowards(tr.position, targetWaypoint, Time.deltaTime * unitData.MoveSpeed);
            // ���������� �����ϸ� �̵� ����

            if (tr.position == targetWaypoint)
            //if (Vector3.Distance(tr.position, targetWaypoint) <= 1f)
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

            //if(currentWaypointIndex >= movePath.Count)
            //{
            //    isPatrolGo = !isPatrolGo;
            //    currentWaypointIndex = movePath.Count - 1;
            //}
            //else if (currentWaypointIndex < 0)
            //{
            //    isPatrolGo = !isPatrolGo;
            //    currentWaypointIndex = 0;
            //}

            animator.SetBool("isAttack", false);

            // ���⿡ ���� �ִϸ��̼� ���
            animator.SetBool("isMove", true);
            if (direction.magnitude > 0.5f)
            {
                AnimSetFloat(direction, true);
            }

            if (aggroTarget != null && targetDist < unitData.ColliderRadius)
            {
                unitAIState = UnitAIState.UAI_NormalTrace;
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

        // ���� �Ÿ� �����̸� �̵��� �����
        if (movedDistance < 0.1f)
        {
            if (!isNewPosSet)
            {
                if (aggroTarget != null && targetDist < unitData.ColliderRadius)
                {
                    unitAIState = UnitAIState.UAI_NormalTrace; 
                }
                else
                {
                    Debug.Log("dd");
                    unitAIState = UnitAIState.UAI_Idle;
                    isAttackMove = true;
                }
                AnimSetFloat(lastMoveDirection, false);
                isMoveCheckCoroutine = false;
                yield break; // �ڷ�ƾ ����         
            }
            else if (isNewPosSet)
            {
                isNewPosSet = false;
                isMoveCheckCoroutine = false;
                yield break; // �ڷ�ƾ ����  
            }
        }
        isMoveCheckCoroutine = false;
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
                    if (unitAIState == UnitAIState.UAI_Idle || (unitAIState == UnitAIState.UAI_Move && isAttackMove))
                        unitAIState = UnitAIState.UAI_NormalTrace;
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

        if (monsterList.Count == 0)
        {
            aggroTarget = null;

            if (isLastStateOn == false)
            {
                unitAIState = unitLastState;
                isLastStateOn = false;
            }
        }
    }

    void AttackTargetCheck()
    {
        if (isTargetSet == false)
        {
            float closestDistance = float.MaxValue;

            // ��� ���Ϳ� ���� �Ÿ� ���
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
        }
    }

    void AttackTargetDisCheck()
    {
        if (aggroTarget != null)
        {
            targetVec = (new Vector3(aggroTarget.transform.position.x, aggroTarget.transform.position.y, 0) - tr.position).normalized;
            targetDist = Vector3.Distance(tr.position, aggroTarget.transform.position);
        }

        if (targetDist < unitData.ColliderRadius && isAttackMove == true && unitAIState != UnitAIState.UAI_NormalTrace && unitAIState != UnitAIState.UAI_Attack)
        {
            if(isLastStateOn == false)
            {
                unitLastState = unitAIState;
                isLastStateOn = true;
            }
            unitAIState = UnitAIState.UAI_NormalTrace;
            attackState = UnitAttackState.Waiting;
        }
    }

    void AttackCheck()
    {
        if (targetDist == 0)
            return;
        else if (targetDist > unitData.AttackDist)  // ���� ���� ������ ���� ��
        {
            animator.SetBool("isAttack", false);
            unitAIState = UnitAIState.UAI_NormalTrace;
            attackState = UnitAttackState.Waiting;
        }
        else if (targetDist <= unitData.AttackDist)  // ���� ���� ���� ������ ��        
        {
            unitAIState = UnitAIState.UAI_Attack;
            attackState = UnitAttackState.Attack;
        }

        if (aggroTarget == null && monsterList.Count == 0)
        {
            unitAIState = unitLastState;
            isLastStateOn = false;
        }
    }//void Attack()


    void Attack()
    {
        AnimSetFloat(targetVec, true);

        if (!isDelayAfterAttackCoroutine)
        {
            //animator.Play("Attack", -1, 0);
            //animator.SetBool("isAttack", true);
            attackState = UnitAttackState.AttackDelay;
            StartCoroutine(DelayAfterAttack(unitData.AttDelayTime)); // 1.5�� �� ������ ����
        }
    }

    IEnumerator DelayAfterAttack(float delayTime)
    {
        isDelayAfterAttackCoroutine = true;
        AttackStart();

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1.0f)
        {
            yield return null;
        }

        yield return new WaitForSeconds(delayTime);

        //animator.SetBool("isAttack", false);
        //animator.SetBool("isMove", false);
        //AnimSetFloat(targetVec, false);
        //attackState = UnitAttackState.Waiting;
        isDelayAfterAttackCoroutine = false;
    }

    //void AttackStart()
    //{
    //    animator.Play("Attack", -1, 0);
    //    animator.SetBool("isAttack", true);

    //    if(aggroTarget != null)
    //    {
    //        GameObject attackFXSpwan;
    //        Vector3 dir = aggroTarget.transform.position - transform.position;
    //        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    //        attackFXSpwan = Instantiate(attackFX, new Vector2(this.transform.position.x, this.transform.position.y), this.transform.rotation);
    //        if (Quaternion.AngleAxis(angle + 180, Vector3.forward).z < 0)
    //            attackFXSpwan.transform.rotation = Quaternion.AngleAxis(angle + 180, Vector3.forward);
    //        else
    //            attackFXSpwan.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

    //        attackFXSpwan.GetComponent<BulletCtrl>().GetTarget(aggroTarget.transform.position, unitData.Damage);
    //    }
    //}

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
            //if(aggroTarget != null)
            //    aggroTarget.GetComponent<MonsterAi>().TakeDamage(damage);
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
        if (!isHold)
        {
            if(aggroTarget != null)
            {
                animator.SetBool("isMove", true);
                AnimSetFloat(targetVec, true);
                if(targetDist > unitData.AttackDist)
                    tr.position = Vector3.MoveTowards(tr.position, aggroTarget.transform.position, Time.deltaTime * unitData.MoveSpeed);
            }
        }
        else
            animator.SetBool("isMove", false);
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

        if (unitSelect == true)
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

            if (isLastStateOn == false)
            {
                unitAIState = unitLastState;
                isLastStateOn = false;
            }
        } 
    }
}
