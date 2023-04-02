using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitAi : MonoBehaviour
{
    public enum UnitAIState  
    {
        UAI_Idle,
        UAI_Move,
        UAI_Attack,
        UAI_Patrol,
        UAI_Hold,
        UAI_AggroTrace,
        //Ȧ�� ��� �߰�
    }

    public enum UnitAttackState
    {
        Waiting,
        AttackStart,
        Attacking,
        AttackDelay,
        AttackEnd,
    }

    [SerializeField]
    float moveSpeed;
    [SerializeField]
    Animator animator;

    // �̵� ����
    private Vector3 targetPosition;
    private Vector2 lastMoveDirection = Vector2.zero; // ���������� �̵��� ����
    Vector3 direction = Vector3.zero;
    [SerializeField]
    float moveRadi = 0f;
    private Vector3 lastPosition;
    bool isMoveCheckCoroutine = false;
    bool isNewPosSet = false;

    // ��ȸ ����
    [SerializeField]
    bool bypassOn = false;
    [SerializeField]
    Vector3 reDirection;
    private RaycastHit2D[] hits;
    private RaycastHit2D hitTemp;
    public LayerMask obstacleLayer;
    int numHits = 0;
    private float movedDistance = 0f;
    bool isBypassCheckCoroutine = false;

    // ��Ʈ�� ����
    private Vector3 patrolStartPos;

    // Ȧ�� ����

    // ���� ���� ����
    public UnitAIState unitAIState = UnitAIState.UAI_Idle; // ���� �� ��Ʈ�� ����
    UnitAttackState attackState = UnitAttackState.Waiting;

    // Start is called before the first frame update
    void Start()
    {
        hits = new RaycastHit2D[2];

    }

    void FixedUpdate()
    {
        UnitAiCtrl();
    }

    // Update is called once per frame
    void Update()
    {

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
                PatrolFunc();
                break;
            case UnitAIState.UAI_Hold:
                HoldFunc();
                break;                          
            case UnitAIState.UAI_Attack:

                break;    
            case UnitAIState.UAI_AggroTrace:

                break;
        }
    }

    void IdleFunc()
    {
        animator.SetBool("isMove", false);
        VariableReset();
    }

    void VariableReset()
    {
        bypassOn = false;
        isMoveCheckCoroutine = false;
        isBypassCheckCoroutine = false;
    }

    public void MovePosSet(Vector2 dir, float radi)
    {
        isNewPosSet = true;
        VariableReset();
        targetPosition = dir;
        moveRadi = radi;

        lastMoveDirection = (targetPosition - transform.position).normalized; // �̵����� ����
        unitAIState = UnitAIState.UAI_Move;

        // ���⿡ ���� �ִϸ��̼� ���
        animator.SetBool("isMove", true);
        direction = targetPosition - transform.position;
        //MoveCorrDirection = transform.position + direction.normalized * (direction.magnitude + (radi / 2));

        if (direction.magnitude > 0.5f)
        {
            float angle = Vector2.SignedAngle(Vector2.up, direction);
            angle = Mathf.RoundToInt(angle / 90f) * 90f; // 90�� ������ ��ȯ
            animator.SetFloat("Horizontal", -Mathf.Sin(angle * Mathf.Deg2Rad));
            animator.SetFloat("Vertical", Mathf.Cos(angle * Mathf.Deg2Rad));
        }
    }

    void MoveFunc()
    {        
        if(bypassOn == false)
        {
            // �̵�
            direction = targetPosition - transform.position;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * moveSpeed);

            // ���������� �����ϸ� �̵� ����

            if (Vector3.Distance(transform.position, targetPosition) < moveRadi)
            {
                if (!isMoveCheckCoroutine)
                    StartCoroutine(UnitMoveCheck());
            }
            else if (Vector3.Distance(transform.position, targetPosition) < 0.3f)
            {
                unitAIState = UnitAIState.UAI_Idle;
                LastMoveMovemont();
            }
        }
    }

    public void PatrolPosSet(Vector2 dir)
    {
        isNewPosSet = true;
        VariableReset();
        targetPosition = dir;
        patrolStartPos = this.transform.position;

        lastMoveDirection = (targetPosition - transform.position).normalized; // �̵����� ����
        unitAIState = UnitAIState.UAI_Patrol;

        // ���⿡ ���� �ִϸ��̼� ���
        animator.SetBool("isMove", true);
        direction = targetPosition - transform.position;
        if (direction.magnitude > 0.5f)
        {
            float angle = Vector2.SignedAngle(Vector2.up, direction);
            angle = Mathf.RoundToInt(angle / 90f) * 90f; // 90�� ������ ��ȯ
            animator.SetFloat("Horizontal", -Mathf.Sin(angle * Mathf.Deg2Rad));
            animator.SetFloat("Vertical", Mathf.Cos(angle * Mathf.Deg2Rad));
        }
    }


    void PatrolFunc()
    {
        if (bypassOn == false)
        {
            // �̵�
            direction = targetPosition - transform.position;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * moveSpeed);
            // ���������� �����ϸ� �̵� ����
            if (Vector3.Distance(transform.position, targetPosition) < 0.3f)
            {
                PatrolPosSet(patrolStartPos);
            }
        }
    }

    void HoldFunc()
    {
        animator.SetBool("isMove", false);
    }

    IEnumerator UnitMoveCheck()
    {
        isMoveCheckCoroutine = true;
        yield return new WaitForSeconds(0.05f);

        movedDistance = Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position;

        // ���� �Ÿ� �����̸� �̵��� �����
        if (movedDistance < 0.1f)
        {
            if (!isNewPosSet)
            {
                unitAIState = UnitAIState.UAI_Idle;
                LastMoveMovemont();
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


    void LastMoveMovemont()
    {
        float angle = Vector2.SignedAngle(Vector2.up, lastMoveDirection);
        angle = Mathf.RoundToInt(angle / 90f) * 90f;
        animator.SetFloat("lastMoveX", -Mathf.Sin(angle * Mathf.Deg2Rad));
        animator.SetFloat("lastMoveY", Mathf.Cos(angle * Mathf.Deg2Rad));
    }
}
