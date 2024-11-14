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

    protected bool slowDebuff;          // 공속 느리게
    protected float slowTime;           // 디버프 시간
    protected bool poisonTrueAttack;    // 추가 고정 데미지
    protected float poisonTime;         // 디버프 시간
    protected bool ignoreDdefense;      // 방어력 무시
    protected float ignorePercent;      // 방어력 무시 퍼센트

    // Start is called before the first frame update
    protected virtual void Start()
    {
        networkObjectPool = NetworkObjectPool.Singleton;
    }

    public void DestroyBullet()
    {
        if (IsServer)
        {
            ResetOption();
            NetworkObject.Despawn();
        }
    }

    public void SlowDebuffSet(float time)
    {
        slowDebuff = true;
        slowTime = time;
    }

    public void PoisonTrueAttackSet(float time)
    {
        poisonTrueAttack = true;
        poisonTime = time;
    }

    public void IgnoreDdefenseSet(float percent)
    {
        ignoreDdefense = true;
        ignorePercent = percent;
    }

    protected void ResetOption()
    {
        slowDebuff = false;
        poisonTrueAttack = false;
        ignoreDdefense = false;
    }

    protected void TakeDamage(MonsterAi monster)
    {
        if (ignoreDdefense)
        {
            monster.TakeDamage(damage, 2, ignorePercent);
        }
        else if (poisonTrueAttack)
        {
            monster.TakeDamage(damage, 3, poisonTime);
        }
        else if (slowDebuff)
        {
            monster.TakeDamage(damage, 4, slowTime);
        }
        else
        {
            monster.TakeDamage(damage, 0);
        }
    }

    protected void TakeDamage(MonsterSpawner spawner)
    {
        spawner.TakeDamage(damage, attackUnit);
    }
}
