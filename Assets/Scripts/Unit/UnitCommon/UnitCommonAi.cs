using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Pathfinding;

public enum AIState   // ���� ���� ����
{
    AI_Idle,
    AI_Move,
    AI_Patrol,
    AI_AggroTrace,
    AI_NormalTrace,
    AI_ReturnPos,
    AI_Attack,
    AI_Die
}

public enum AttackState
{
    Waiting,
    AttackStart,
    Attacking,
    AttackDelay,
    AttackEnd,
}

public class UnitCommonAi : MonoBehaviour
{
    [SerializeField]
    protected UnitCommonData unitCommonData;
    protected UnitCommonData UnitCommonData { set { unitCommonData = value; } }

    protected Transform tr;
    protected Rigidbody2D rg;
    protected Seeker seeker;
    protected Coroutine checkPathCoroutine;             // ���� ���� �ڷ�ƾ�� �����ϴ� ����
    protected int currentWaypointIndex;                 // ���� �̵� ���� ��� �� �ε���
    protected Vector3 targetPosition;
    protected Vector2 lastMoveDirection = Vector2.zero; // ���������� �̵��� ����
    protected Vector3 direction = Vector3.zero;
    protected Vector3 targetVec = Vector3.zero;
    protected List<Vector3> movePath = new List<Vector3>();

    protected float searchInterval;
    protected float searchTimer;
    protected GameObject aggroTarget;
    protected float tarDisCheckTime;
    protected float tarDisCheckInterval;                // 0.3�� �������� ���� �Ÿ� üũ
    protected float targetDist;
    public List<GameObject> targetList = new List<GameObject>();

    protected Vector3 patrolStartPos;

    [SerializeField]
    protected Animator animator;

    protected CapsuleCollider2D capsule2D;

    public SpriteRenderer unitSprite;
    public GameObject unitCanvas;
    public Image hpBar;
    protected float hp;

    protected bool isFlip;
    protected bool isDelayAfterAttackCoroutine = false;

    public AIState aIState;
    public AttackState attackState;

    private void Awake()
    {
        tr = GetComponent<Transform>();
        rg = GetComponent<Rigidbody2D>();
        capsule2D = GetComponent<CapsuleCollider2D>();
        seeker = GetComponent<Seeker>();
        animator = GetComponent<Animator>();

        hp = unitCommonData.MaxHp;
        hpBar.fillAmount = hp / unitCommonData.MaxHp;

        isFlip = unitSprite.flipX;

        searchInterval = 0.3f;
        tarDisCheckInterval = 0.3f;
        patrolStartPos = Vector3.zero;
        hp = 100.0f;
        aIState = AIState.AI_Idle;
        attackState = AttackState.Waiting;
    }

    protected virtual void FixedUpdate()
    {
        if (aIState != AIState.AI_Die)
            UnitAiCtrl();
    }

    protected virtual void Update()
    {
        if (aIState != AIState.AI_Die)
        {
            searchTimer += Time.deltaTime;

            if (searchTimer >= searchInterval)
            {
                SearchObjectsInRange();
                searchTimer = 0f; // Ž�� �� Ÿ�̸� �ʱ�ȭ
            }

            if (targetList.Count > 0)
            {
                tarDisCheckTime += Time.deltaTime;
                if (tarDisCheckTime > tarDisCheckInterval)
                {
                    tarDisCheckTime = 0f;
                    AttackTargetCheck();
                    RemoveObjectsOutOfRange();
                }
                AttackTargetDisCheck();
            }
        }
    }

    protected virtual void UnitAiCtrl() { }
    protected virtual void SearchObjectsInRange() { }    
    protected virtual void AttackTargetCheck() { }

    protected void RemoveObjectsOutOfRange()
    {
        for (int i = targetList.Count - 1; i >= 0; i--)
        {
            if (!targetList[i])
                targetList.RemoveAt(i);
            else
            {
                GameObject target = targetList[i];
                if (Vector2.Distance(tr.position, target.transform.position) > unitCommonData.ColliderRadius)
                {
                    targetList.RemoveAt(i);
                }
            }
        }
    }

    protected void AttackTargetDisCheck()
    {
        if (aggroTarget)
        {
            targetVec = (new Vector3(aggroTarget.transform.position.x, aggroTarget.transform.position.y, 0) - tr.position).normalized;
            targetDist = Vector3.Distance(tr.position, aggroTarget.transform.position);
        }
    }

    protected virtual IEnumerator CheckPath(Vector3 targetPos, string moveFunc) { yield return null; }


    protected virtual void AnimSetFloat(Vector3 direction, bool isNotLast) { }
    protected virtual void NormalTrace() { }

    protected void AttackCheck()
    {
        if (targetDist == 0)
            return;
        else if (targetDist > unitCommonData.AttackDist)  // ���� ���� ������ ���� ��
        {
            animator.SetBool("isAttack", false);
            aIState = AIState.AI_NormalTrace;
            attackState = AttackState.Waiting;
        }
        else if (targetDist <= unitCommonData.AttackDist)  // ���� ���� ���� ������ ��        
        {
            aIState = AIState.AI_Attack;
            attackState = AttackState.AttackStart;
        }
    }

    protected void Attack()
    {
        AnimSetFloat(targetVec, true);

        if (!isDelayAfterAttackCoroutine)
        {
            attackState = AttackState.AttackDelay;
            StartCoroutine(DelayAfterAttack(unitCommonData.AttDelayTime)); // 1.5�� �� ������ ����
        }
    }
    protected IEnumerator DelayAfterAttack(float delayTime)
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

    protected virtual void AttackStart() { }

    protected virtual void AttackEnd(string str)
    {
        if (str == "false")
        {
            animator.SetBool("isAttack", false);
            animator.SetBool("isMove", false);
            AnimSetFloat(targetVec, false);
            attackState = AttackState.Waiting;
        }
    }

    public virtual void TakeDamage(float damage)
    {
        if (hp <= 0f)
            return;

        float reducedDamage = Mathf.Max(damage - unitCommonData.Defense, 5);

        hp -= reducedDamage;
        hpBar.fillAmount = hp / unitCommonData.MaxHp;

        if (hp <= 0f)
        {
            aIState = AIState.AI_Die;
            hp = 0f;
            DieFunc();
        }
    }

    protected virtual void DieFunc()
    {
        unitSprite.color = new Color(1f, 1f, 1f, 0f);
        unitCanvas.SetActive(false);

        capsule2D.enabled = false;
    }

    public virtual void RemoveTarget(GameObject target) 
    {
        if (targetList.Contains(target))
        {
            targetList.Remove(target);
        }
        if (targetList.Count == 0)
        {
            aggroTarget = null;
        }
    }

    protected virtual void SwBodyType(bool isMove)
    {
        if (isMove)
        {
            rg.bodyType = RigidbodyType2D.Kinematic;
        }
        else
        {
            rg.bodyType = RigidbodyType2D.Dynamic;
        }
    }
}