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

    // ���� ���� ����
    UnitAttackState attackState = UnitAttackState.Waiting;

    public GameObject unitCanvers = null;

    [SerializeField]
    protected Animator animator;

    // ���� ���� ����
    protected GameObject aggroTarget = null;   // Ÿ��
    float mstDisCheckTime = 0f;
    float mstDisCheckInterval = 0.5f; // 0.5�� �������� ���� �Ÿ� üũ
    float targetDist = 0.0f;         // Ÿ�ٰ��� �Ÿ�
    bool isTargetSet = false;               // ������ ���ƴ��� üũ
    List<GameObject> monsterList = new List<GameObject>();
    bool isDelayAfterAttackCoroutine = false;

    // HpBar ����
    public Image hpBar;
    float hp = 200.0f;

    CircleCollider2D circle2D = null;
    CapsuleCollider2D capsule2D = null;
    // ���߿� Ÿ�� ����Ÿ ��
    float radius = 10;
    float attackDist = 5;
    float attackDelayTime = 2f;
    float maxHp = 200.0f;
    float damage = 4;

    // ���� ���� ��ũ��Ʈ���� ���
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
                        AttackTargetCheck(); // ���� �Ÿ� üũ �Լ� ȣ��
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
        else if (targetDist > attackDist)  // ���� ���� ������ ���� ��
        {
            attackState = UnitAttackState.Waiting;
        }
        else if (targetDist <= attackDist)  // ���� ���� ���� ������ ��        
        {
            attackState = UnitAttackState.Attack;
        }
    }//void Attack()

    void AttackTargetCheck()
    {
        if (isTargetSet == false)
        {
            float closestDistance = float.MaxValue;

            // ��� ���Ϳ� ���� �Ÿ� ���
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
            StartCoroutine(DelayAfterAttack(attackDelayTime)); // 1.5�� �� ������ ����
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
