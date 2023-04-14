using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerAi : MonoBehaviour
{
    public enum UnitAttackState
    {
        Waiting,
        Attack,
        AttackDelay,
        Die
    }

    // 유닛 상태 관련
    UnitAttackState attackState = UnitAttackState.Waiting;

    public GameObject unitCanvers = null;

    [SerializeField]
    protected Animator animator;

    // 공격 관련 변수
    protected GameObject aggroTarget = null;   // 타겟
    float mstDisCheckTime = 0f;
    float mstDisCheckInterval = 0.5f; // 0.5초 간격으로 몬스터 거리 체크
    float targetDist = 0.0f;         // 타겟과의 거리
    bool isTargetSet = false;               // 유저를 놓쳤는지 체크
    List<GameObject> monsterList = new List<GameObject>();
    bool isDelayAfterAttackCoroutine = false;

    // HpBar 관련
    public Image hpBar;
    float hp = 200.0f;

    CircleCollider2D circle2D = null;
    CapsuleCollider2D capsule2D = null;
    // 나중에 타워 데이타 용
    float radius = 10;
    float attackDist = 5;
    float attackDelayTime = 2f;
    float maxHp = 200.0f;
    float damage = 4;

    // 나중 하위 스크립트에서 사용
    public GameObject attackFX;
    public GameObject RuinExplo;


    // Start is called before the first frame update
    void Start()
    {
        circle2D = GetComponent<CircleCollider2D>();
        capsule2D = GetComponent<CapsuleCollider2D>();
        circle2D.radius = radius;
        hp = maxHp;

    }

    // Update is called once per frame
    void Update()
    {
        if(attackState != UnitAttackState.Die)
        {
            TowerAiCtrl();
            if (monsterList.Count > 0)
            {
                mstDisCheckTime += Time.deltaTime;
                if (mstDisCheckTime > mstDisCheckInterval)
                {
                    mstDisCheckTime = 0f;
                    if (monsterList.Count > 0)
                        AttackTargetCheck(); // 몬스터 거리 체크 함수 호출
                }
                AttackTargetDisCheck();
            }   
        }     
    }

    void TowerAiCtrl()
    {
        switch (attackState)
        {
            case UnitAttackState.Waiting:                
                AttackCheck();
                break;
            case UnitAttackState.Attack:                
                Attack();            
                break;
        }
    }

    void AttackCheck()
    {
        if (targetDist == 0)
            return;
        else if (targetDist > attackDist)  // 공격 범위 밖으로 나갈 때
        {
            attackState = UnitAttackState.Waiting;
        }
        else if (targetDist <= attackDist)  // 공격 범위 내로 들어왔을 때        
        {
            attackState = UnitAttackState.Attack;
        }
    }//void Attack()

    void AttackTargetCheck()
    {
        if (isTargetSet == false)
        {
            float closestDistance = float.MaxValue;

            // 모든 몬스터에 대해 거리 계산
            foreach (GameObject monster in monsterList)
            {
                float distance = Vector3.Distance(this.transform.position, monster.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    aggroTarget = monster;
                }
            }
        }
    }

    void AttackTargetDisCheck()
    {
        if (aggroTarget != null)
        {
            targetDist = Vector3.Distance(transform.position, aggroTarget.transform.position);
        }
    }
    void Attack()
    {
        if (!isDelayAfterAttackCoroutine)
        {
            attackState = UnitAttackState.AttackDelay;
            StartCoroutine(DelayAfterAttack(attackDelayTime)); // 1.5초 후 딜레이 적용
        }
    }
    IEnumerator DelayAfterAttack(float delayTime)
    {
        isDelayAfterAttackCoroutine = true;
        AttackStart();

        yield return new WaitForSeconds(delayTime);
        
        attackState = UnitAttackState.Waiting;
        isDelayAfterAttackCoroutine = false;
    }

    protected virtual void AttackStart()
    {
        if (aggroTarget != null)
        {
            GameObject attackFXSpwan;
            attackFXSpwan = Instantiate(attackFX, new Vector2(aggroTarget.transform.position.x, aggroTarget.transform.position.y + 0.5f), aggroTarget.transform.rotation);

            attackFXSpwan.GetComponent<TowerFxRangeCtrl>().GetTarget(damage);
        }
    }

    public void TakeDamage(float damage)
    {
        if (hp <= 0f)
            return;

        hp -= damage;
        hpBar.fillAmount = hp / maxHp;

        if (hp <= 0f)
        {
            hp = 0f;
            DieFunc();
        }
    }

    void DieFunc()
    {
        unitCanvers.SetActive(false);

        capsule2D.enabled = false;
        circle2D.enabled = false;

        foreach (GameObject monster in monsterList)
        {
            if (monster.GetComponent<MonsterAi>())
                monster.GetComponent<MonsterAi>().RemoveTarget(this.gameObject);
        }

        GameObject exploFXSpwan;
        exploFXSpwan = Instantiate(RuinExplo, new Vector2(this.transform.position.x, this.transform.position.y), this.transform.rotation);

        animator.SetBool("isDie", true);
    }

    void FixFunc()
    {
        unitCanvers.SetActive(true);

        capsule2D.enabled = true;
        circle2D.enabled = true;

        animator.SetBool("isDie", false);
    }

    public void RemoveMonster(GameObject monster)
    {
        if (monsterList.Contains(monster))
        {
            monsterList.Remove(monster);

            if (aggroTarget == monster)
                aggroTarget = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster"))
        {
            if (!monsterList.Contains(collision.gameObject))
            {
                if (collision.isTrigger == true)
                {
                    monsterList.Add(collision.gameObject);
                }
                //unitLastState = unitAIState;
                //if (isAttackMove)
                //    unitAIState = UnitAIState.UAI_NormalTrace;
            }
        }//if (collision.CompareTag("Player"))
    }//private void OnTriggerEnter2D(Collider2D collision)

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster"))
        {
            if (collision.isTrigger == true)
            {
                monsterList.Remove(collision.gameObject);
            }
            if (monsterList.Count == 0)
            {
                aggroTarget = null;
            }
        }//if (collision.CompareTag("Player"))
    }//private void OnTriggerExit2D(Collider2D collision)
}
