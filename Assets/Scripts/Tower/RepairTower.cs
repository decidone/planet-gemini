using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// UTF-8 설정
public class RepairTower : TowerAi
{
    public List<GameObject> BuildingList = new List<GameObject>();
    bool isDelayRepairCoroutine = false;
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
                if (searchTimer >= searchInterval)
                {
                    SearchObjectsInRange();
                    searchTimer = 0f; // 탐색 후 타이머 초기화
                }
                RepairTowerAiCtrl();
            }
        }
    }

    private void SearchObjectsInRange()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(this.transform.position, structureData.ColliderRadius);

        foreach (Collider2D collider in colliders)
        {
            GameObject building = collider.gameObject;
            //if (tower == this.gameObject)
            //    continue;
            if (building.TryGetComponent(out Structure structure))
            {
                if (structure.repairTower == null && !structure.isPreBuilding
                    && !BuildingList.Contains(building) && !structure.GetComponent<Portal>())
                {
                    BuildingList.Add(building);
                    structure.repairTower = this;
                }
            }
        }
    }

    public void RemoveObjectsOutOfRange(GameObject obj)//근쳐 타워 삭제시 발동되게
    {
        if (BuildingList.Contains(obj))
        {
            BuildingList.Remove(obj);
        }
    }

    void RepairTowerAiCtrl()
    {
        if (!isDelayRepairCoroutine)
        {
            StartCoroutine(DelayRepair(attDelayTime));
        }
    }

    IEnumerator DelayRepair(float delayTime)
    {
        isDelayRepairCoroutine = true;
        RepairStart();

        yield return new WaitForSeconds(delayTime);

        towerState = TowerState.Waiting;
        isDelayRepairCoroutine = false;
    }

    void RepairStart()
    {
        Debug.Log("RepairStart");
        int repairAmount = repairFullAmount;

        var sortedUnitTargets = BuildingList
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

    //protected override void DieFuncClientRpc()
    //{
    //    base.DieFuncClientRpc();

    //    Instantiate(RuinExplo, new Vector2(this.transform.position.x, this.transform.position.y), this.transform.rotation);

    //    foreach (GameObject tower in TowerList)
    //    {
    //        if (tower.TryGetComponent(out TowerAi towerAi))
    //        {
    //            if (towerAi.isRuin)
    //            {
    //                if (towerAi.isRepair)
    //                {
    //                    towerAi.RepairSet(false);
    //                }
    //            }
    //        }
    //    }
    //}
}
