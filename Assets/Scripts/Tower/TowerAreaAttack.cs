using UnityEngine;

public class TowerAreaAttack : AttackTower
{
    public GameObject attackFX;
    public GameObject RuinExplo;

    protected override void AttackStart()
    {
        if (aggroTarget != null)
        {
            GameObject attackFXSpwan;
            attackFXSpwan = Instantiate(attackFX, new Vector2(aggroTarget.transform.position.x, aggroTarget.transform.position.y + 0.5f), aggroTarget.transform.rotation);

            attackFXSpwan.GetComponent<TowerAreaAttackFx>().GetTarget(towerData.Damage);
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
