using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class SentryCopterCtrl : UnitAi
{
    public GameObject attackFX;

    protected override void AttackStart()
    {
        animator.Play("Attack", -1, 0);
        animator.SetBool("isAttack", true);

        if (aggroTarget != null)
        {
            Vector3 dir = aggroTarget.transform.position - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            var bulletPool = BulletPoolManager.instance.Pool.Get();
            bulletPool.transform.position = new Vector2(this.transform.position.x, this.transform.position.y);

            //bulletPool = Instantiate(attackFX, new Vector2(this.transform.position.x, this.transform.position.y), this.transform.rotation);
            if (Quaternion.AngleAxis(angle + 180, Vector3.forward).z < 0)
                bulletPool.transform.rotation = Quaternion.AngleAxis(angle + 180, Vector3.forward);
            else
                bulletPool.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            bulletPool.GetComponent<BulletCtrl>().GetTarget(aggroTarget.transform.position, unitCommonData.Damage, gameObject);
        }
    }
}
