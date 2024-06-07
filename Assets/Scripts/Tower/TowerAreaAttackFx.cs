using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

// UTF-8 설정
public class TowerAreaAttackFx : NetworkBehaviour
{
    float damage = 0;

    [SerializeField]
    protected Animator animator;
    GameObject attackUnit;

    NetworkObjectPool networkObjectPool;

    private void Start()
    {
        networkObjectPool = NetworkObjectPool.Singleton;
    }

    [ClientRpc]
    public void DestroyBulletClientRpc()
    {
        if (IsServer)
        {
            NetworkObject.Despawn();
        }
    }

    public void GetTarget(float GetDamage, GameObject obj)
    {
        damage = GetDamage;
        attackUnit = obj;
    }

    void FxEnd(string str)
    {
        if (str == "false")
        {
            Invoke(nameof(DestroyBulletClientRpc), 0.1f);
            //Destroy(this.gameObject, 0.1f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer)
            return;
        if (collision.CompareTag("Monster") || collision.CompareTag("Spawner"))
        {
            if (collision.TryGetComponent(out MonsterAi monster))
            {
                monster.TakeDamage(damage);
            }
            else if (collision.TryGetComponent(out MonsterSpawner spawner))
            {
                spawner.TakeDamage(damage, attackUnit);
            }
        }
    }
}
