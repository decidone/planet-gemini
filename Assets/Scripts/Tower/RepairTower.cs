using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

// UTF-8 설정
public class RepairTower : TowerAi
{
    public List<GameObject> buildingList = new List<GameObject>();
    [SerializeField]
    SpriteRenderer view;
    [SerializeField]
    int repairFullAmount;

    protected override void Start()
    {
        base.Start();
        StartCoroutine(EfficiencyCheck());
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
                    SearchObjectsInRange();
                    searchTimer = 0f; // 탐색 후 타이머 초기화
                    RepairStart();
                }
            }
        }
    }

    private void SearchObjectsInRange()
    {
        buildingList.Clear();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(this.transform.position, structureData.ColliderRadius);

        foreach (Collider2D collider in colliders)
        {
            GameObject building = collider.gameObject;
            //if (tower == this.gameObject)
            //    continue;
            if (building.TryGetComponent(out Structure structure))
            {
                if (!structure.isPreBuilding
                    && !buildingList.Contains(building) && !structure.GetComponent<Portal>())
                {
                    buildingList.Add(building);
                }
            }
        }
    }

    public void RemoveObjectsOutOfRange(GameObject obj)//근쳐 타워 삭제시 발동되게
    {
        if (buildingList.Contains(obj))
        {
            buildingList.Remove(obj);
        }
    }

    void RepairStart()
    {
        int repairAmount = repairFullAmount;

        var sortedUnitTargets = buildingList
            .Where(target => target != null)
            .Select(target => target.GetComponent<Structure>())
            .Where(structure => structure != null)
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
