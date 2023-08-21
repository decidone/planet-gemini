using UnityEngine;

public class TowerAreaAttack : AttackTower
{
    public GameObject attackFX;

    protected override void AttackStart()
    {
        if (aggroTarget != null)
        {
            GameObject attackFXSpwan;
            attackFXSpwan = Instantiate(attackFX, new Vector2(aggroTarget.transform.position.x, aggroTarget.transform.position.y + 0.5f), aggroTarget.transform.rotation);

            attackFXSpwan.GetComponent<TowerAreaAttackFx>().GetTarget(towerData.Damage);
        }
    }
}
