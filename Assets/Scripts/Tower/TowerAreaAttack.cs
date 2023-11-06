using UnityEngine;

// UTF-8 설정
public class TowerAreaAttack : AttackTower
{
    public GameObject attackFX;

    protected override void AttackStart()
    {
        if (aggroTarget != null)
        {
            GameObject attackFXSpwan;
            attackFXSpwan = Instantiate(attackFX, new Vector2(aggroTarget.transform.position.x, aggroTarget.transform.position.y + 0.5f), aggroTarget.transform.rotation);
            inventory.Sub(0, 1);
            attackFXSpwan.GetComponent<TowerAreaAttackFx>().GetTarget(towerData.Damage);
        }
    }
}
