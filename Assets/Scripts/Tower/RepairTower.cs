using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairTower : TowerAi
{
    List<GameObject> TowerList = new List<GameObject>();
    bool isDelayRepairCoroutine = false;

    public GameObject RuinExplo;

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
        Collider2D[] colliders = Physics2D.OverlapCircleAll(this.transform.position, towerData.ColliderRadius);

        foreach (Collider2D collider in colliders)
        {
            GameObject tower = collider.gameObject;
            if (tower == this.gameObject)
                continue;
            if (tower.CompareTag("Tower"))
            {
                if (!TowerList.Contains(tower))
                {
                    TowerList.Add(tower);
                }
            }
        }
    }

    //public void RemoveObjectsOutOfRange(GameObject obj)//근쳐 타워 삭제시 발동되게
    //{
    //    if (obj.CompareTag("Tower"))
    //    {
    //        TowerList.Remove(obj);            
    //    }
    //}

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
        //unitCanvers.SetActive(false);
        hp = towerData.MaxHp;

        hpBar.enabled = false;
        repairBar.enabled = true;

        repairGauge = 0;
        repairBar.fillAmount = repairGauge / towerData.MaxBuildingGauge;

        //DisableColliders();
        ColliderTriggerOnOff(true);

        //towerState = TowerState.Die;
        isRuin = true;

        Instantiate(RuinExplo, new Vector2(this.transform.position.x, this.transform.position.y), this.transform.rotation);

        //animator.SetBool("isDie", true);

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
        foreach (GameObject monsterObj in monsterList)
        {
            if (monsterObj.TryGetComponent(out MonsterAi monsterAi))
            {
                monsterAi.RemoveTarget(this.gameObject);
            }
        }

        monsterList.Clear();
    }
}
