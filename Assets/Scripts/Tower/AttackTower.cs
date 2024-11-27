using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

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

    public TwBulletDataManager bulletDataManager;
    public Dictionary<string, BulletData> bulletDic;
    BulletData loadedBullet;

    public GameObject attackFX;
    [SerializeField]
    bool isSingleAttack;

    TowerAttackOption towerAttackOption;

    protected override void Start()
    {
        base.Start();
        towerAttackOption = GetComponent<TowerAttackOption>();
        bulletDataManager = TwBulletDataManager.instance;
        bulletDic = bulletDataManager.bulletDic;
    }

    protected override void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        if (!removeState)
        {
            if (isRepair)
            {
                RepairFunc(false);
            }
            else if (isPreBuilding && isSetBuildingOk)
            {
                RepairFunc(true);
            }

            WarningStateCheck();
        }

        if (destroyStart)
        {
            destroyTimer -= Time.deltaTime;
            repairBar.fillAmount = destroyTimer / destroyInterval;

            if (destroyTimer <= 0)
            {
                ObjRemoveFunc();
                destroyStart = false;
            }
        }

        if (!structureData.EnergyUse[level])
        {
            if (isSetBuildingOk)
            {
                for (int i = 0; i < nearObj.Length; i++)
                {
                    if (nearObj[i] == null && sizeOneByOne)
                    {
                        CheckNearObj(checkPos[i], i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
                    }
                    else if (nearObj[i] == null && !sizeOneByOne)
                    {
                        int dirIndex = i / 2;
                        CheckNearObj(startTransform[indices[i]], directions[dirIndex], i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
                    }
                }
            }

            if (IsServer && !isPreBuilding && checkObj)
            {
                if (!isMainSource && inObj.Count > 0 && !itemGetDelay)
                    GetItem();
            }
            if (DelayGetList.Count > 0 && inObj.Count > 0)
            {
                GetDelayFunc(DelayGetList[0], 0);
            }
        }

        if (!isPreBuilding)
        {
            if (IsServer)
            {
                if ((structureData.EnergyUse[level] && conn != null && conn.group != null && conn.group.efficiency > 0)
                    || !structureData.EnergyUse[level])
                {
                    searchTimer += Time.deltaTime;

                    if (searchTimer >= searchInterval)
                    {
                        SearchObjectsInRange();
                        searchTimer = 0f; // 탐색 후 타이머 초기화
                    }

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

                        if (structureData.EnergyUse[level])
                        {
                            if (loadedBullet == null)
                            {
                                loadedBullet = bulletDic["EnergyBullet"];
                            }
                            AttackTowerAiCtrl();
                        }
                        else
                        {
                            var slot = inventory.SlotCheck(0);
                            if (slot.item != null && slot.amount > 0)
                            {
                                if (loadedBullet == null)
                                {
                                    BulletCheck();
                                }
                                AttackTowerAiCtrl();
                            }
                            else if (slot.item == null && loadedBullet != null)
                            {
                                loadedBullet = null;
                            }
                        }
                    }
                }
            }
        }
    }

    void AttackCheck()
    {
        if (targetDist == 0)
        {
            towerState = TowerState.Waiting;
            return;
        }

        else if (targetDist > towerData.AttackDist + loadedBullet.range)  // 공격 범위 밖으로 나갈 때
        {
            towerState = TowerState.Waiting;
        }
        else if (targetDist <= towerData.AttackDist + loadedBullet.range)  // 공격 범위 내로 들어왔을 때        
        {
            towerState = TowerState.Attack;
        }
    }
    
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
            if (monster.CompareTag("Monster") || monster.CompareTag("Spawner"))
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
            StartCoroutine(DelayAfterAttack(towerData.AttDelayTime + loadedBullet.fireRate)); // 1.5초 후 딜레이 적용            
        }
    }

    void BulletCheck()
    {
        var slot = inventory.SlotCheck(0);

        if (bulletDic.ContainsKey(slot.item.name))
        {
            loadedBullet = bulletDic[slot.item.name];
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

    protected void AttackStart()  
    {
        if (aggroTarget != null)
        {
            if (isSingleAttack)
            {
                Vector3 dir = aggroTarget.transform.position - transform.position;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                var rot = Quaternion.identity;
                if (Quaternion.AngleAxis(angle + 180, Vector3.forward).z < 0)
                    rot = Quaternion.AngleAxis(angle + 180, Vector3.forward);
                else
                    rot = Quaternion.AngleAxis(angle, Vector3.forward);

                if (IsServer && !structureData.EnergyUse[level])
                {
                    var slot = inventory.SlotCheck(0);
                    Overall.instance.OverallConsumption(slot.item, 1); 
                    inventory.SubServerRpc(0, 1);
                }

                NetworkObject bulletPool = networkObjectPool.GetNetworkObject(attackFX, new Vector2(this.transform.position.x, this.transform.position.y), rot);
                if (!bulletPool.IsSpawned) bulletPool.Spawn(true);

                bulletPool.TryGetComponent(out TowerSingleAttackFx fx);
                towerAttackOption.TowerAttackFxSet(fx);
                fx.GetTarget(aggroTarget.transform.position, towerData.Damage + loadedBullet.damage, gameObject, loadedBullet.explosion);
            }
            else
            {
                if (IsServer && !structureData.EnergyUse[level])
                {
                    var slot = inventory.SlotCheck(0);
                    Overall.instance.OverallConsumption(slot.item, 1);
                    inventory.SubServerRpc(0, 1);
                }

                NetworkObject bulletPool = networkObjectPool.GetNetworkObject(attackFX, new Vector2(aggroTarget.transform.position.x, aggroTarget.transform.position.y + 0.5f), Quaternion.identity);
                if (!bulletPool.IsSpawned) bulletPool.Spawn(true);

                bulletPool.TryGetComponent(out TowerAreaAttackFx fx);
                towerAttackOption.TowerAttackFxSet(fx);
                fx.GetTarget(towerData.Damage + loadedBullet.damage, gameObject);
            }

            Debug.Log("Attack");
            soundManager.PlaySFX(gameObject, "unitSFX", "TowerAttack");
        }
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
