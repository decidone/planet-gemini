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

        if (collision.TryGetComponent(out PlayerStatus player))
        {
            if (!collision.isTrigger)
            {
                player.TakeDamage(damage);
            }
        }
        else if (collision.TryGetComponent(out UnitAi unitAi))
        {
            if (!collision.isTrigger)
            {
                unitAi.TakeDamage(damage, 0);
            }
        }
        else if (collision.TryGetComponent(out Structure structure))
        {
            if (!collision.isTrigger)
            {
                structure.TakeDamage(damage);
            }
        }
    }
}
