using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

// UTF-8 설정
public class GolemFXCtrl : NetworkBehaviour
{
    public Animator animator;
    public Transform aggroTarget = null;   // 타겟
    float damage = 0;

    void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }
    }

    public void FXEnd()
    {
        if(IsServer)
            NetworkObject.Despawn();
    }

    public void FXMove() { }

    public void TargetPosAndDamage(float getDamage)
    {
        damage = getDamage;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer)
            return;

        if (collision.TryGetComponent(out WorldObj obj))
        {
            if (obj.TryGet(out PlayerStatus player))
            {
                if (!collision.isTrigger)
                {
                    player.TakeDamage(damage);
                }
            }
            else if(obj.TryGet(out UnitAi unitAi))
            {
                if (!collision.isTrigger)
                {
                    unitAi.TakeDamage(damage, 0);
                }
            }
            else if (obj.TryGet(out Structure str))
            {
                if (str.Get<Portal>() || str.Get<LocalPortal>())
                {
                    str.TakeDamage(damage);
                }
                else
                {
                    if (!collision.isTrigger)
                    {
                        str.TakeDamage(damage);
                    }
                }
            }
        }
    }
}
