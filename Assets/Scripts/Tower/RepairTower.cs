using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class RepairTower : TowerAi
{
    public List<GameObject> TowerList = new List<GameObject>();
    bool isDelayRepairCoroutine = false;

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

                RepairTowerAiCtrl();
            }
        }
        if (isRuin && isRepair)
        {
            RepairFunc(false);
        }
    }

    private void SearchObjectsInRange()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(this.transform.position, structureData.ColliderRadius);

        foreach (Collider2D collider in colliders)
        {
            GameObject tower = collider.gameObject;
            if (tower == this.gameObject)
                continue;
            if (tower.GetComponent<Structure>())
            {
                if (!TowerList.Contains(tower))
                {
                    TowerList.Add(tower);
                    tower.GetComponent<Structure>().repairTower = gameObject.GetComponent<RepairTower>();
                }
            }
        }
    }

    public void RemoveObjectsOutOfRange(GameObject obj)//근쳐 타워 삭제시 발동되게
    {
        if (TowerList.Contains(obj))
        {
            TowerList.Remove(obj);
        }
    }

    void RepairTowerAiCtrl()
    {
        if (!isDelayRepairCoroutine)
        {
            StartCoroutine(DelayRepair(towerData.AttDelayTime));
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
        foreach (GameObject tower in TowerList)
        {
            TowerAi towerAi = tower.GetComponent<TowerAi>();
            Structure factory = tower.GetComponent<Structure>();

            if (towerAi != null)
            {
                if (!towerAi.isRuin)
                {
                    towerAi.HealFunc(towerData.Damage);
                }
                else if (!towerAi.isRepair)
                {
                    towerAi.RepairSet(true);
                }
            }
            else if (factory != null)
            {
                if (!factory.isRuin)
                {
                    factory.HealFunc(towerData.Damage);
                }
                else if (!factory.isRepair)
                {
                    factory.RepairSet(true);
                }
            }
        }
    }

    protected override void DieFunc()
    {
        base.DieFunc();

        Instantiate(RuinExplo, new Vector2(this.transform.position.x, this.transform.position.y), this.transform.rotation);
                
        foreach (GameObject tower in TowerList)
        {
            if (tower.TryGetComponent(out TowerAi towerAi))
            {
                if (towerAi.isRuin)
                {
                    if (towerAi.isRepair)
                    {
                        towerAi.RepairSet(false);
                    }
                }
            }
        }
    }
}
