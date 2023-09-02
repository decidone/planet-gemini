using UnityEngine;

// UTF-8 설정
public class TowerSingleAttack : AttackTower
{
    public GameObject attackFX;

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
}
