using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

// UTF-8 설정
public class BounceRobot : UnitAi
{
    public GameObject attackFX;

    protected override bool AttackStart()
    {
        Debug.Log("aggroTarget: " + aggroTarget);
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

            var rot = Quaternion.identity;
            if (Quaternion.AngleAxis(angle + 180, Vector3.forward).z < 0)
                rot = Quaternion.AngleAxis(angle + 180, Vector3.forward);
            else
                rot = Quaternion.AngleAxis(angle, Vector3.forward);
            NetworkObject bulletPool = networkObjectPool.GetNetworkObject(attackFX, new Vector2(this.transform.position.x, this.transform.position.y), rot);
            if (!bulletPool.IsSpawned) bulletPool.Spawn(true);

            //var bulletPool = BulletPoolManager.instance.Pool.Get();
            //bulletPool.transform.position = new Vector2(this.transform.position.x, this.transform.position.y);

            //bulletPool = Instantiate(attackFX, new Vector2(this.transform.position.x, this.transform.position.y), this.transform.rotation);
            //if (Quaternion.AngleAxis(angle + 180, Vector3.forward).z < 0)
            //    bulletPool.transform.rotation = Quaternion.AngleAxis(angle + 180, Vector3.forward);
            //else
            //    bulletPool.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            bulletPool.GetComponent<BulletCtrl>().GetTarget(aggroTarget.transform.position, unitCommonData.Damage, gameObject);
            soundManager.PlaySFX(gameObject, "unitSFX", "laserAttack");
        }

        return isAttacked;
    }
}
