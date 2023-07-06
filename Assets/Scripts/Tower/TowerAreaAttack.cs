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

        DisableColliders();

        //towerState = TowerState.Die;
        isRuin = true;
        //isRepair = false;

        foreach (GameObject monster in monsterList)
        {
            if (monster.TryGetComponent(out MonsterAi monsterAi))
            {
                monsterAi.RemoveTarget(this.gameObject);
            }
        }

        Instantiate(RuinExplo, new Vector2(this.transform.position.x, this.transform.position.y), this.transform.rotation);

        animator.SetBool("isDie", true);
    }
}
