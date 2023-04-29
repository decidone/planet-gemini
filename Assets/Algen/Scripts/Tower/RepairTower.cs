using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairTower : TowerAi
{
    List<GameObject> TowerList = new List<GameObject>();
    bool isDelayRepairCoroutine = false;

    public GameObject RuinExplo;

    // Update is called once per frame
    void Update()
    {
        if (!isPreBuilding)
        {
          if (!isDie)       
            {
                RepairTowerAiCtrl();
            }
            else if (isDie && isRepair == true)
            {
                RepairFunc();
            }
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
        foreach(GameObject tower in TowerList)
        {
            if(tower.TryGetComponent(out TowerAi towerAi))
            {
                if (!towerAi.isDie)
                {                   
                    towerAi.HealFunc(towerData.Damage);
                }
                else if (towerAi.isDie)
                {
                    if(towerAi.isRepair == false)
                    {
                        towerAi.RepairSet(true);
                    }
                }
            }
        }
    }

    protected override void DieFunc()
    {
        //unitCanvers.SetActive(false);
        hpBar.enabled = false;

        capsule2D.enabled = false;
        circle2D.enabled = false;

        //towerState = TowerState.Die;
        isDie = true;

        Instantiate(RuinExplo, new Vector2(this.transform.position.x, this.transform.position.y), this.transform.rotation);

        animator.SetBool("isDie", true);

        foreach (GameObject tower in TowerList)
        {
            if (tower.TryGetComponent(out TowerAi towerAi))
            {
                if (towerAi.isDie)
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
                if (!collision.GetComponent<TowerAi>().isPreBuilding)
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
        }
    }//private void OnTriggerEnter2D(Collider2D collision)
}
