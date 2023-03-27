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
        UAI_AggroTrace,
        //홀드 기능 추가
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
    Rigidbody2D rb;
    [SerializeField]
    Animator animator;

    private Vector3 targetPosition;
    private Vector2 lastMoveDirection = Vector2.zero; // 마지막으로 이동한 방향

    private Vector3 patrolStartPos;

    private float movedDistance = 0f;
    private Vector3 lastPosition;
    bool isMoveCheckCoroutine = false;
    bool isNewPosSet = false;

    private Collider unitCollider; // Collider 컴포넌트

    public UnitAIState unitAIState = UnitAIState.UAI_Idle; // 시작 시 패트롤 상태
    UnitAttackState attackState = UnitAttackState.Waiting;

    // Start is called before the first frame update
    void Start()
    {
        unitCollider = GetComponent<Collider>();

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
            case UnitAIState.UAI_Attack:

                break;            
            case UnitAIState.UAI_Patrol:
                PatrolFunc();
                break;            
            case UnitAIState.UAI_AggroTrace:

                break;
        }
    }

    public void MovePosSet(Vector2 dir)
    {
        isNewPosSet = true;

        targetPosition = dir;
        lastMoveDirection = (targetPosition - transform.position).normalized; // 이동방향 저장
        unitAIState = UnitAIState.UAI_Move;

        // 방향에 따라 애니메이션 재생
        animator.SetBool("isMove", true);
        Vector3 direction = targetPosition - transform.position;
        if (direction.magnitude > 0.5f)
        {
            float angle = Vector2.SignedAngle(Vector2.up, direction);
            angle = Mathf.RoundToInt(angle / 90f) * 90f; // 90도 각도로 변환
            animator.SetFloat("Horizontal", -Mathf.Sin(angle * Mathf.Deg2Rad));
            animator.SetFloat("Vertical", Mathf.Cos(angle * Mathf.Deg2Rad));
        }
    }

    void IdleFunc()
    {
        animator.SetBool("isMove", false);
    }

    void MoveFunc()
    {        
        // 이동
        Vector3 direction = targetPosition - transform.position;
        rb.velocity = direction.normalized * moveSpeed;
        // 도착지점에 도착하면 이동 멈춤
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            rb.velocity = Vector3.zero;
            unitAIState = UnitAIState.UAI_Idle;
            LastMoveMovemont();
        }
        if (!isMoveCheckCoroutine)
            StartCoroutine(UnitMoveCheck());
    }

    public void PatrolPosSet(Vector2 dir)
    {
        isNewPosSet = true;

        targetPosition = dir;
        patrolStartPos = this.transform.position;

        lastMoveDirection = (targetPosition - transform.position).normalized; // 이동방향 저장
        unitAIState = UnitAIState.UAI_Patrol;

        // 방향에 따라 애니메이션 재생
        animator.SetBool("isMove", true);
        Vector3 direction = targetPosition - transform.position;
        if (direction.magnitude > 0.5f)
        {
            float angle = Vector2.SignedAngle(Vector2.up, direction);
            angle = Mathf.RoundToInt(angle / 90f) * 90f; // 90도 각도로 변환
            animator.SetFloat("Horizontal", -Mathf.Sin(angle * Mathf.Deg2Rad));
            animator.SetFloat("Vertical", Mathf.Cos(angle * Mathf.Deg2Rad));
        }
    }


    void PatrolFunc()
    {
        // 이동
        Vector3 direction = targetPosition - transform.position;
        rb.velocity = direction.normalized * moveSpeed;
        // 도착지점에 도착하면 이동 멈춤
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            rb.velocity = Vector3.zero;
            unitAIState = UnitAIState.UAI_Idle;
            LastMoveMovemont();
            PatrolPosSet(patrolStartPos);
        }
        //if (!isMoveCheckCoroutine)
        //    StartCoroutine(UnitMoveCheck());
    }

    IEnumerator UnitMoveCheck()
    {
        isMoveCheckCoroutine = true;
        yield return new WaitForSeconds(0.05f);

        movedDistance = Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position;

        // 일정 거리 이하이면 이동을 멈춘다
        if (movedDistance < 0.1f)
        {
            if (!isNewPosSet)
            {
                rb.velocity = Vector3.zero;
                unitAIState = UnitAIState.UAI_Idle;
                LastMoveMovemont();
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


    void LastMoveMovemont()
    {
        float angle = Vector2.SignedAngle(Vector2.up, lastMoveDirection);
        angle = Mathf.RoundToInt(angle / 90f) * 90f;
        animator.SetFloat("lastMoveX", -Mathf.Sin(angle * Mathf.Deg2Rad));
        animator.SetFloat("lastMoveY", Mathf.Cos(angle * Mathf.Deg2Rad));
    }
}
