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
    public int energyBulletAmount;
    public int energyBulletMaxAmount;

    public GameObject attackFX;
    [SerializeField]
    bool isSingleAttack;

    TowerAttackOption towerAttackOption;
    AggroAmount aggroAmount;

    protected override void Start()
    {
        base.Start();
        aggroAmount = GetComponent<AggroAmount>();
        towerAttackOption = GetComponent<TowerAttackOption>();
        bulletDataManager = TwBulletDataManager.instance;
        bulletDic = bulletDataManager.bulletDic;

        energyBulletMaxAmount = towerData.MaxEnergyBulletAmount;
        cooldown = towerData.ReloadCooldown;
        effiCooldown = cooldown;
        StrBuilt();
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
            else if (isPreBuilding)
            {
                RepairFunc(true);
            }
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
            //if (isSetBuildingOk)
            //{
            //    for (int i = 0; i < nearObj.Length; i++)
            //    {
            //        if (nearObj[i] == null && sizeOneByOne)
            //        {
            //            CheckNearObj(checkPos[i], i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
            //        }
            //        else if (nearObj[i] == null && !sizeOneByOne)
            //        {
            //            int dirIndex = i / 2;
            //            CheckNearObj(startTransform[indices[i]], directions[dirIndex], i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
            //        }
            //    }
            //}

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
        else
        {
            if (!isPreBuilding && conn != null && conn.group != null && conn.group.efficiency > 0 && energyBulletAmount < energyBulletMaxAmount)
            {
                EfficiencyCheck();

                OperateStateSet(true);
                prodTimer += Time.deltaTime;
                if (prodTimer > effiCooldown - ((overclockOn ? effiCooldown * overclockPer / 100 : 0) + effiCooldownUpgradeAmount))
                {
                    if (IsServer)
                    {
                        SetEnergyBulletServerRpc(++energyBulletAmount);
                    }
                    prodTimer = 0;
                }
            }
            else
            {
                OperateStateSet(false);
                prodTimer = 0;
            }
        }

        if (!isPreBuilding)
        {
            if (IsServer)
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
                        RemoveObjectsOutOfRange();
                        AttackTargetCheck(); // 몬스터 거리 체크 함수 호출
                    }
                    AttackTargetDisCheck();

                    if (structureData.EnergyUse[level])
                    {
                        if (energyBulletAmount > 0)
                        {
                            if (loadedBullet == null)
                            {
                                loadedBullet = bulletDic["EnergyBullet"];
                            }
                            AttackTowerAiCtrl();
                        }
                    }
                    else
                    {
                        if (slot.Item1 != null && slot.Item2 > 0)
                        {
                            if (loadedBullet == null)
                            {
                                BulletCheck();
                            }
                            AttackTowerAiCtrl();
                        }
                        else if (slot.Item1 == null && loadedBullet != null)
                        {
                            loadedBullet = null;
                        }
                    }
                }
            }
        }
    }

    public override void CheckSlotState(int slotindex)
    {
        // update에서 검사해야 하는 특정 슬롯들 상태를 인벤토리 콜백이 있을 때 미리 저장
        if (inventory != null)
            slot = inventory.SlotCheck(0);
    }

    public override void NearStrBuilt()
    {
        // 건물을 지었을 때나 근처에 새로운 건물이 지어졌을 때 동작
        // 변경사항이 생기면 DelayNearStrBuiltCoroutine()에도 반영해야 함
        if (IsServer)
        {
            CheckPos();
            for (int i = 0; i < nearObj.Length; i++)
            {
                if (nearObj[i] == null && sizeOneByOne)
                {
                    CheckNearObj(checkPos[i], i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
                }
                else if (nearObj[i] == null && !sizeOneByOne)
                {
                    CheckNearObj(i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
                }
            }
        }
        else
        {
            DelayNearStrBuilt();
        }
    }

    public override void DelayNearStrBuilt()
    {
        // 동시 건설, 클라이언트 동기화 등의 이유로 딜레이를 주고 NearStrBuilt()를 실행할 때 사용
        StartCoroutine(DelayNearStrBuiltCoroutine());
    }

    protected override IEnumerator DelayNearStrBuiltCoroutine()
    {
        // 동시 건설이나 그룹핑을 따로 예외처리 하는 경우가 아니면 NearStrBuilt()를 그대로 사용
        yield return new WaitForEndOfFrame();

        CheckPos();
        for (int i = 0; i < nearObj.Length; i++)
        {
            if (nearObj[i] == null && sizeOneByOne)
            {
                CheckNearObj(checkPos[i], i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
            }
            else if (nearObj[i] == null && !sizeOneByOne)
            {
                CheckNearObj(i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
            }
        }
    }

    protected override void OnClientConnectedCallback(ulong clientId)
    {
        base.OnClientConnectedCallback(clientId);
        ClientTowerSyncServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ClientTowerSyncServerRpc()
    {
        ClientTowerSyncClientRpc(energyBulletAmount);
    }

    [ClientRpc]
    public void ClientTowerSyncClientRpc(int amount)
    {
        if (!IsServer)
        {
            energyBulletAmount = amount;
        }
    }

    public override (float, float) PopUpStoredCheck()
    {
        if (structureData.EnergyUse[level])
            return (energyBulletAmount, energyBulletMaxAmount);
        else
        {
            return (0f, 0f);
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
                if (monster != null)
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
            if (AttackStart())
            {
                StartCoroutine(DelayAfterAttack(attDelayTime + loadedBullet.fireRate)); // 1.5초 후 딜레이 적용
            }
            else
            {
                towerState = TowerState.Waiting;
            }
        }
    }

    void BulletCheck()
    {
        if (bulletDic.ContainsKey(slot.Item1.name))
        {
            loadedBullet = bulletDic[slot.Item1.name];
        }
    }

    IEnumerator DelayAfterAttack(float delayTime)
    {
        isDelayAfterAttackCoroutine = true;
        towerState = TowerState.AttackDelay;

        yield return new WaitForSeconds(delayTime);

        towerState = TowerState.Waiting;
        isDelayAfterAttackCoroutine = false;
    }

    protected bool AttackStart()
    {
        bool isAttacked = false;

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

                if (IsServer)
                {
                    if (!structureData.EnergyUse[level])
                    {
                        Overall.instance.OverallConsumption(slot.Item1, 1);
                        inventory.SlotSubServerRpc(0, 1);
                    }
                    else
                    {
                        SetEnergyBulletServerRpc(--energyBulletAmount);
                    }
                }

                NetworkObject bulletPool = networkObjectPool.GetNetworkObject(attackFX, new Vector2(this.transform.position.x, this.transform.position.y), rot);
                if (!bulletPool.IsSpawned) bulletPool.Spawn(true);

                bulletPool.TryGetComponent(out TowerSingleAttackFx fx);
                towerAttackOption.TowerAttackFxSet(fx);
                fx.GetTarget(aggroTarget.transform.position, damage + loadedBullet.damage, gameObject, loadedBullet.explosion);
            }
            else
            {
                if (IsServer)
                {
                    if (!structureData.EnergyUse[level])
                    {
                        Overall.instance.OverallConsumption(slot.Item1, 1);
                        inventory.SlotSubServerRpc(0, 1);
                    }
                    else
                    {
                        SetEnergyBulletServerRpc(--energyBulletAmount);
                    }
                }

                NetworkObject bulletPool = networkObjectPool.GetNetworkObject(attackFX, new Vector2(aggroTarget.transform.position.x, aggroTarget.transform.position.y + 0.5f), Quaternion.identity);
                if (!bulletPool.IsSpawned) bulletPool.Spawn(true);

                bulletPool.TryGetComponent(out TowerAreaAttackFx fx);
                towerAttackOption.TowerAttackFxSet(fx);
                fx.GetTarget(damage + loadedBullet.damage, gameObject);
            }

            Debug.Log("Attack");
            isAttacked = true;
            soundManager.PlaySFX(gameObject, "unitSFX", "TowerAttack");
            aggroAmount.SetAggroAmount(damage, attDelayTime + loadedBullet.fireRate);
        }

        return isAttacked;
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

    [ServerRpc (RequireOwnership = false)]
    public void SetEnergyBulletServerRpc(int amount)
    {
        SetEnergyBulletClientRpc(amount);
    }

    [ClientRpc]
    public void SetEnergyBulletClientRpc(int amount)
    {
        energyBulletAmount = amount;
    }

    public override StructureSaveData SaveData()
    {
        StructureSaveData data = base.SaveData();
        data.energyBulletAmount = energyBulletAmount;

        return data;
    }
}
