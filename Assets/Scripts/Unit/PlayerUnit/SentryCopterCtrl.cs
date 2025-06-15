using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

// UTF-8 설정
public class SentryCopterCtrl : UnitAi
{
    public GameObject attackFX;

    protected override bool AttackStart()
    {
        bool isAttacked = false;

        if (aggroTarget != null)
        {
            //AnimPlayCtrl("Attack");
            //AnimBoolCtrl("isAttack", true);
            animator.Play("Attack", -1, 0);
            animator.SetBool("isAttack", true);
            isAttacked = true;

            Vector3 dir = aggroTarget.transform.position - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            //var bulletPool = BulletPoolManager.instance.Pool.Get();

            var rot = Quaternion.identity;
            if (Quaternion.AngleAxis(angle + 180, Vector3.forward).z < 0)
                rot = Quaternion.AngleAxis(angle + 180, Vector3.forward);
            else
                rot = Quaternion.AngleAxis(angle, Vector3.forward);
            NetworkObject bulletPool = networkObjectPool.GetNetworkObject(attackFX, new Vector2(this.transform.position.x, this.transform.position.y), rot);
            if (!bulletPool.IsSpawned) bulletPool.Spawn(true);

            bulletPool.GetComponent<BulletCtrl>().GetTarget(aggroTarget.transform.position, damage, gameObject);
            soundManager.PlaySFX(gameObject, "unitSFX", "laserAttack");

            aggroAmount.SetAggroAmount(damage, attackSpeed);
        }

        return isAttacked;
    }
}
