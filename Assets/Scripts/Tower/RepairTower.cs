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
            searchTimer += Time.deltaTime;

            if (searchTimer >= searchInterval)
            {
                SearchObjectsInRange();
                searchTimer = 0f; // 탐색 후 타이머 초기화
            }

            RepairTowerAiCtrl();            
        }
    }

    private void SearchObjectsInRange()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(this.transform.position, structureData.ColliderRadius);

        foreach (Collider2D collider in colliders)
        {
            GameObject tower = collider.gameObject;
            //if (tower == this.gameObject)
            //    continue;
            if (tower.TryGetComponent(out Structure structure))
            {
                if (structure.repairTower == null && !structure.isPreBuilding
                    && !TowerList.Contains(tower) && !structure.GetComponent<Portal>())
                {
                    TowerList.Add(tower);
                    structure.repairTower = this;
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
                towerAi.HealFunc(towerData.Damage);
            }
            else if (factory != null)
            {
                factory.HealFunc(towerData.Damage);
            }
        }
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
