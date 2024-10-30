using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TowerAttackFx : NetworkBehaviour
{
    protected float damage = 0;

    [SerializeField]
    protected Animator animator;
    protected GameObject attackUnit;

    protected NetworkObjectPool networkObjectPool;

    protected bool slowDebuff;        // 공속 느리게
    protected float slowTime;         // 디버프 시간
    protected bool poisonTrueAttack;  // 추가 고정 데미지
    protected float poisonDamage;     // 독 데미지
    protected bool ignoreDdefense;    // 방어력 무시
    protected float ignorePercent;     // 독 데미지

    // Start is called before the first frame update
    protected virtual void Start()
    {
        networkObjectPool = NetworkObjectPool.Singleton;
    }

    public void DestroyBullet()
    {
        if (IsServer)
        {
            NetworkObject.Despawn();
            Debug.Log("??");
        }
    }

    public void SlowDebuffSet(float time)
    {
        slowDebuff = true;
        slowTime = time;
    }

    public void PoisonTrueAttackSet(float damage)
    {
        poisonTrueAttack = true;
        poisonDamage = damage;
    }

    public void IgnoreDdefenseSet(float percent)
    {
        ignoreDdefense = true;
        ignorePercent = percent;
    }

    protected void TakeDamage(MonsterAi monster)
    {
        if (ignoreDdefense)
        {
            monster.TakeDamage(damage, 2, ignorePercent);
        }
        else
        {
            monster.TakeDamage(damage, 0);
        }

        if (slowDebuff)
        {
            monster.TakeSlowDebuff(slowTime);
        }
        else if (poisonTrueAttack)
        {
            monster.TakeDamage(poisonDamage, 1);
        }
    }

    protected void TakeDamage(MonsterSpawner spawner)
    {
        spawner.TakeDamage(damage, attackUnit);
    }
}
