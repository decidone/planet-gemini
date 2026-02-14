using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

// UTF-8 설정
public class RepairTower : TowerAi
{
    public List<Structure> buildingList = new List<Structure>();
    [SerializeField]
    SpriteRenderer view;
    [SerializeField]
    int repairFullAmount;

    protected override void Awake()
    {
        base.Awake();
        int mask = (1 << LayerMask.NameToLayer("Obj"));

        contactFilter.SetLayerMask(mask);
        contactFilter.useLayerMask = true;
    }

    protected override void Start()
    {
        base.Start();
        StartCoroutine(EfficiencyCheckLoop());
    }

    protected override void Update()
    {
        base.Update();
        if (!isPreBuilding && IsServer)
        {
            if (conn != null && conn.group != null && conn.group.efficiency > 0)
            {
                searchTimer += Time.deltaTime;
                if (searchTimer >= attDelayTime)
                {
                    //SearchObjectsInRange();
                    searchTimer = 0f; // 탐색 후 타이머 초기화
                    RepairStart();
                }
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            searchManager.StructureListAdd(this);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsServer)
        {
            searchManager.StructureListRemove(this);
        }
    }

    public override void SearchObjectsInRange()
    {
        int hitCount = Physics2D.OverlapCircle(
            transform.position,
            structureData.ColliderRadius,
            contactFilter,
            targetColls
        );

        if (hitCount == 0)
            return;

        for (int i = 0; i < hitCount; i++)
        {
            GameObject building = targetColls[i].gameObject;
            if (!building.TryGetComponent(out Portal portal) 
                && building.TryGetComponent(out Structure structure) && !buildingList.Contains(structure))
            {
                buildingList.Add(structure);
                structure.repairTowers.Add(this);
            }
        }
    }

    void RepairStart()
    {
        int repairAmount = repairFullAmount;

        var sortedUnitTargets = buildingList
            .Where(target => target != null)
            .OrderByDescending(structure => structure.maxHp - structure.hp) // 정렬
            .Take(repairAmount)
            .ToList();

        bool isRepairing = false;
        foreach (Structure str in sortedUnitTargets)
        {
            if (str.hp != str.maxHp)
            {
                repairAmount--;
                str.RepairFunc(damage);
                isRepairing = true;
                OperateStateSet(true);
                if (repairAmount == 0)
                {
                    return;
                }
            }
        }

        if (!isRepairing)
        {
            OperateStateSet(false);
        }
    }

    public void RepairTowerRemove()
    {
        foreach (Structure str in buildingList)
        {
            if (str != null && str.repairTowers.Contains(this))
            {
                str.repairTowers.Remove(this);
            }
        }
    }

    public void RemoveObjectsOutOfRange(Structure obj) //근처 타워 삭제시 발동되게
    {
        if (buildingList.Contains(obj))
        {
            buildingList.Remove(obj);
        }
    }

    public override void SetBuild()
    {
        base.SetBuild();
        view.enabled = false;
    }

    public override void Focused()
    {
        view.enabled = true;
    }

    public override void DisableFocused()
    {
        view.enabled = false;
    }
}
