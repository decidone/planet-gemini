using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairTower : TowerAi
{
    [SerializeField]
    List<GameObject> TowerList = new List<GameObject>();
    bool isDelayRepairCoroutine = false;

    public GameObject RuinExplo;

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (!isPreBuilding)
        {
          if (!isRuin)       
            {
                RepairTowerAiCtrl();
            }
            //else if (isRuin && isRepair)
            //{
            //    RepairFunc(false);
            //}
        }
        if (isRuin && isRepair == true)
        {
            RepairFunc(false);
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
        //unitCanvers.SetActive(false);
        hp = towerData.MaxHp;

        hpBar.enabled = false;
        repairBar.enabled = true;

        repairGauge = 0;
        repairBar.fillAmount = repairGauge / towerData.MaxBuildingGauge;

        DisableColliders();

        //towerState = TowerState.Die;
        isRuin = true;

        Instantiate(RuinExplo, new Vector2(this.transform.position.x, this.transform.position.y), this.transform.rotation);

        animator.SetBool("isDie", true);

        foreach (GameObject tower in TowerList)
        {
            if (tower.TryGetComponent(out TowerAi towerAi))
            {
                if (towerAi.isRuin)
                {
                    if (towerAi.isRepair == true)
                    {
                        towerAi.RepairSet(false);
                    }
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!isPreBuilding)
        {
            if (collision.CompareTag("Tower"))
            {
                //if (!collision.GetComponent<TowerAi>().isPreBuilding)
                {
                    if (!TowerList.Contains(collision.gameObject))
                    {
                        if (collision.isTrigger == true)
                        {
                            TowerList.Add(collision.gameObject);
                        }
                    }
                }
            }
            else if (collision.CompareTag("Factory"))
            {
                //if (!collision.GetComponent<TowerAi>().isPreBuilding)
                {
                    if (!TowerList.Contains(collision.gameObject))
                    {
                        TowerList.Add(collision.gameObject);                        
                    }
                }
            }
        }
    }//private void OnTriggerEnter2D(Collider2D collision)
}