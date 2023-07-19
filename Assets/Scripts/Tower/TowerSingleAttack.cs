using UnityEngine;

public class TowerSingleAttack : AttackTower
{
    public GameObject attackFX;
    public GameObject RuinExplo;

    protected override void AttackStart()
    {
        if (aggroTarget != null)
        {
            GameObject attackFXSpwan;
            Vector3 dir = aggroTarget.transform.position - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            attackFXSpwan = Instantiate(attackFX, new Vector2(this.transform.position.x, this.transform.position.y + 0.7f), this.transform.rotation);
            if (Quaternion.AngleAxis(angle + 180, Vector3.forward).z < 0)
                attackFXSpwan.transform.rotation = Quaternion.AngleAxis(angle + 180, Vector3.forward);
            else
                attackFXSpwan.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            attackFXSpwan.GetComponent<TowerSingleAttackFx>().GetTarget(aggroTarget.transform.position, towerData.Damage);
        }
    }

    protected override void DieFunc()
    {
        //unitCanvers.SetActive(false);        
        hp = towerData.MaxHp;

        repairBar.enabled = true;
        hpBar.enabled = false;

        repairGauge = 0;
        repairBar.fillAmount = repairGauge / towerData.MaxBuildingGauge;

        //DisableColliders();
        ColliderTriggerOnOff(true);

        //towerState = TowerState.Die;
        isRuin = true;
        //isRepair = false;

        if (!isPreBuilding)
        {
            foreach (GameObject monster in monsterList)
            {
                if (monster.TryGetComponent(out MonsterAi monsterAi))
                {
                    monsterAi.RemoveTarget(this.gameObject);
                }
            }
        }
        else
        {
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
        }

        Instantiate(RuinExplo, new Vector2(this.transform.position.x, this.transform.position.y), this.transform.rotation);

        animator.SetBool("isDie", true);
    }
}
