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
    [HideInInspector]
    public List<GameObject> monsterList = new List<GameObject>();
    bool isDelayAfterAttackCoroutine = false;

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (!isPreBuilding)
        {
            if (!isRuin)
            {
                searchTimer += Time.deltaTime;

                if (searchTimer >= searchInterval)
                {
                    SearchObjectsInRange();
                    searchTimer = 0f; // Ž�� �� Ÿ�̸� �ʱ�ȭ
                }

                AttackTowerAiCtrl();
                if (monsterList.Count > 0)
                {
                    mstDisCheckTime += Time.deltaTime;
                    if (mstDisCheckTime > mstDisCheckInterval)
                    {
                        mstDisCheckTime = 0f;
                        AttackTargetCheck(); // ���� �Ÿ� üũ �Լ� ȣ��
                        RemoveObjectsOutOfRange();                        
                    }
                    AttackTargetDisCheck();
                }
            }
            //else if(isRuin && isRepair == true)
            //{
            //    RepairFunc(false);
            //}
        }
        if (isRuin && isRepair)
        {
            RepairFunc(false);
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
                if(monster != null)
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
    }
    private void SearchObjectsInRange()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(this.transform.position, towerData.ColliderRadius);

        foreach (Collider2D collider in colliders)
        {
            GameObject monster = collider.gameObject;
            if (monster.CompareTag("Monster"))
            {
                if (!monsterList.Contains(monster))
                {
                    monsterList.Add(monster);
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
                if (Vector2.Distance(this.transform.position, monster.transform.position) > towerData.ColliderRadius)
                {
                    monsterList.RemoveAt(i);
                }
            }
        }

        if (monsterList.Count == 0)
        {
            aggroTarget = null;
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
        if (monsterList.Count == 0)
        {
            aggroTarget = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if (!isPreBuilding)
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
            }
        }
    }//private void OnTriggerEnter2D(Collider2D collision)

    private void OnTriggerExit2D(Collider2D collision)
    {
        //if (!isPreBuilding)
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
            }
        }
    }//private void OnTriggerExit2D(Collider2D collision)
}
