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
        hpBar.enabled = false;

        capsule2D.enabled = false;
        circle2D.enabled = false;

        //towerState = TowerState.Die;
        isDie = true;

        Instantiate(RuinExplo, new Vector2(this.transform.position.x, this.transform.position.y), this.transform.rotation);

        animator.SetBool("isDie", true);
    }
}