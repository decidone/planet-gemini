using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTower : TowerAi
{
    // ���� ���� ����
    protected GameObject aggroTarget = null;   // Ÿ��
    float mstDisCheckTime = 0f;
    float mstDisCheckInterval = 0.5f; // 0.5�� �������� ���� �Ÿ� üũ
    float targetDist = 0.0f;         // Ÿ�ٰ��� �Ÿ�
    bool isTargetSet = false; 
    List<GameObject> monsterList = new List<GameObject>();
    bool isDelayAfterAttackCoroutine = false;

    // Update is called once per frame
    void Update()
    {
        if (!isDie)
        {
            AttackTowerAiCtrl();
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
        else if(isDie && isRepair == true)
        {
            RepairFunc();
        }
    }

    void AttackCheck()
    {
        if (targetDist == 0)
            return;
        else if (targetDist > towerData.AttackDist)  // ���� ���� ������ ���� ��
        {
            towerState = TowerState.Waiting;
        }
        else if (targetDist <= towerData.AttackDist)  // ���� ���� ���� ������ ��        
        {
            towerState = TowerState.Attack;
        }
    }//void Attack()

    void AttackTowerAiCtrl()
    {
        switch (towerState)
        {
            case TowerState.Waiting:
                AttackCheck();
                break;
            case TowerState.Attack:
                Attack();
                break;
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
            towerState = TowerState.AttackDelay;
            StartCoroutine(DelayAfterAttack(towerData.AttDelayTime)); // 1.5�� �� ������ ����            
        }
    }
    IEnumerator DelayAfterAttack(float delayTime)
    {
        isDelayAfterAttackCoroutine = true;
        AttackStart();

        yield return new WaitForSeconds(delayTime);

        towerState = TowerState.Waiting;
        isDelayAfterAttackCoroutine = false;
    }

    protected virtual void AttackStart()
    {

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
