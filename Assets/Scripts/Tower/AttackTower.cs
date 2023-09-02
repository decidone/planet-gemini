using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class AttackTower : TowerAi
{
    // 공격 관련 변수
    protected GameObject aggroTarget = null;   // 타겟
    float mstDisCheckTime = 0f;
    float mstDisCheckInterval = 0.5f; // 0.5초 간격으로 몬스터 거리 체크
    float targetDist = 0.0f;         // 타겟과의 거리
    bool isTargetSet = false; 
    bool isDelayAfterAttackCoroutine = false;

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
                    searchTimer = 0f; // 탐색 후 타이머 초기화
                }

                AttackTowerAiCtrl();
                if (monsterList.Count > 0)
                {
                    mstDisCheckTime += Time.deltaTime;
                    if (mstDisCheckTime > mstDisCheckInterval)
                    {
                        mstDisCheckTime = 0f;
                        AttackTargetCheck(); // 몬스터 거리 체크 함수 호출
                        RemoveObjectsOutOfRange();                        
                    }
                    AttackTargetDisCheck();
                }
            }
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
        else if (targetDist > towerData.AttackDist)  // 공격 범위 밖으로 나갈 때
        {
            towerState = TowerState.Waiting;
        }
        else if (targetDist <= towerData.AttackDist)  // 공격 범위 내로 들어왔을 때        
        {
            towerState = TowerState.Attack;
        }
    }
    
    //void Attack()

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
        if (!isTargetSet)
        {
            float closestDistance = float.MaxValue;

            // 모든 몬스터에 대해 거리 계산
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
        Collider2D[] colliders = Physics2D.OverlapCircleAll(this.transform.position, structureData.ColliderRadius);

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
                if (Vector2.Distance(this.transform.position, monster.transform.position) > structureData.ColliderRadius)
                {
                    monsterList.RemoveAt(i);
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
            StartCoroutine(DelayAfterAttack(towerData.AttDelayTime)); // 1.5초 후 딜레이 적용            
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
    protected override void DieFunc()
    {
        base.DieFunc();

        Instantiate(RuinExplo, new Vector2(this.transform.position.x, this.transform.position.y), this.transform.rotation);

        animator.SetBool("isDie", true);
    }

    public void RemoveMonster(GameObject monster)
    {
        if (monsterList.Contains(monster))
        {
            monsterList.Remove(monster);
        }
        if (monsterList.Count == 0)
        {
            aggroTarget = null;
        }
    }
}
