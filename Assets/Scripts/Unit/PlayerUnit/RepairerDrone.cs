using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class RepairerDrone : UnitAi
{
    bool isDelayRepairCoroutine = false;
    [SerializeField]
    List<GameObject> unitTargetList = new List<GameObject>();
    [SerializeField]
    List<GameObject> strTargetList = new List<GameObject>();
    [SerializeField]
    int repairFullAmount;
    protected override void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        fogTimer += Time.deltaTime;
        if (fogTimer > MapGenerator.instance.fogCheckCooldown)
        {
            MapGenerator.instance.RemoveFogTile(transform.position, visionRadius);
            fogTimer = 0;
        }

        if (!IsServer)
            return;

        if (aIState != AIState.AI_Die)
        {
            searchTimer += Time.deltaTime;

            if (searchTimer >= searchInterval)
            {
                SearchObjectsInRange();
                searchTimer = 0f; // 탐색 후 타이머 초기화
            }

            if (unitTargetList.Count > 0 || strTargetList.Count > 0)
            {
                tarDisCheckTime += Time.deltaTime;
                if (tarDisCheckTime > tarDisCheckInterval)
                {
                    tarDisCheckTime = 0f;
                    RemoveObjectsOutOfRange();
                    AttackTargetCheck();
                }
                AttackTargetDisCheck();
                RepairAiCtrl();
            }
            else
            {
                targetDist = 0;
            }
        }

        if (hp != maxHp && aIState != AIState.AI_Die)
        {
            selfHealTimer += Time.deltaTime;

            if (selfHealTimer >= selfHealInterval)
            {
                SelfHealingServerRpc();
                selfHealTimer = 0f;
            }
        }
    }

    void RepairAiCtrl()
    {
        if (!isDelayRepairCoroutine)
        {
            StartCoroutine(DelayRepair(attackSpeed));
        }
    }

    IEnumerator DelayRepair(float delayTime)
    {
        isDelayRepairCoroutine = true;
        RepairStart();

        yield return new WaitForSeconds(delayTime);

        isDelayRepairCoroutine = false;
    }


    void RepairStart()
    {
        int repairAmount = repairFullAmount;

        var sortedUnitTargets = unitTargetList
            .Where(target => target != null)
            .Select(target => target.GetComponent<UnitAi>())
            .Where(structure => structure != null)
            .OrderByDescending(structure => structure.maxHp - structure.hp) // 정렬
            .Take(repairAmount)
            .ToList();

        foreach (UnitAi unit in sortedUnitTargets)
        {
            if (unit.hp != unit.maxHp)
            {
                repairAmount--;
                unit.RepairServerRpc(damage);

                if (repairAmount == 0)
                {
                    return;
                }
            }
        }

        if (repairAmount > 0)
        {
            var sortedStrTargets = strTargetList
                .Where(target => target != null)
                .Select(target => target.GetComponent<Structure>())
                .Where(structure => structure != null)
                .OrderByDescending(structure => structure.maxHp - structure.hp) // 정렬
                .Take(repairAmount)
                .ToList();

            foreach (Structure str in sortedStrTargets)
            {
                if (repairAmount > 0 && str.hp != str.maxHp)
                {
                    repairAmount--;
                    str.RepairFunc(damage);
                }
                
                if (repairAmount == 0)
                {
                    return;
                }
            }
        } 
    }

    protected override void UnitAiCtrl()
    {
        switch (aIState)
        {
            case AIState.AI_Move:
                MoveFunc();
                break;
            case AIState.AI_Patrol:
                PatrolFunc();
                break;
            case AIState.AI_Attack:
                break;
            case AIState.AI_NormalTrace:
                {
                    NormalTrace();
                    AttackCheck();
                }
                break;
        }
    }

    protected override void SearchObjectsInRange()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(tr.position, unitCommonData.ColliderRadius);

        if (colliders.Length > 0)
        {
            foreach (Collider2D collider in colliders)
            {
                GameObject obj = collider.gameObject;
                if (obj.TryGetComponent(out UnitAi unit) && unit != this)
                {
                    if (!unitTargetList.Contains(obj))
                    {
                        unitTargetList.Add(obj);
                    }
                }
                else if (obj.TryGetComponent(out Structure structure))
                {
                    if (!structure.isPreBuilding && !strTargetList.Contains(obj) && !structure.GetComponent<Portal>())
                    {
                        strTargetList.Add(obj);
                    }
                }
                else if (obj.CompareTag("Monster") || obj.CompareTag("Spawner"))
                {
                    if (!targetList.Contains(obj))
                    {
                        targetList.Add(obj);
                    }
                }
            }
        }
    }

    protected override void RemoveObjectsOutOfRange()
    {
        targetList.RemoveAll(target =>
            !target || Vector2.Distance(tr.position, target.transform.position) > unitCommonData.ColliderRadius);
        unitTargetList.RemoveAll(target =>
            !target || Vector2.Distance(tr.position, target.transform.position) > unitCommonData.ColliderRadius);
        strTargetList.RemoveAll(target =>
            !target || Vector2.Distance(tr.position, target.transform.position) > unitCommonData.ColliderRadius);
    }

    public override void RemoveTarget(GameObject target)
    {
        base.RemoveTarget(target);
        if (unitTargetList.Contains(target))
        {
            unitTargetList.Remove(target);
        }
        else if (strTargetList.Contains(target))
        {
            strTargetList.Remove(target);
        }
    }
}
