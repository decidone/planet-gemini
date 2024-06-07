using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

// UTF-8 설정
public class TowerSingleAttackFx : NetworkBehaviour
{
    float damage = 0;

    [SerializeField]
    protected Animator animator;
    Vector3 moveNextStep = Vector3.zero;    // 이동 방향 벡터
    GameObject attackUnit;

    bool alreadyHit;

    Coroutine timerCoroutine;

    NetworkObjectPool networkObjectPool;

    private void Start()
    {
        networkObjectPool = NetworkObjectPool.Singleton;
        alreadyHit = false;
    }

    void Update()
    {
        transform.position += moveNextStep * 2 * Time.fixedDeltaTime;
        Destroy(this.gameObject, 3f);
    }

    [ClientRpc]
    public void DestroyBulletClientRpc()
    {
        if (IsServer)
        {
            NetworkObject.Despawn();
        }
    }

    public void GetTarget(Vector3 target, float GetDamage, GameObject obj)
    {
        moveNextStep = (target - transform.position).normalized;
        damage = GetDamage;
        attackUnit = obj;
        alreadyHit = false;
        timerCoroutine = StartCoroutine(nameof(RemoveTimer));
    }

    IEnumerator RemoveTimer()
    {
        yield return new WaitForSeconds(5.0f);
        if (!alreadyHit)
            DestroyBulletClientRpc();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer)
            return;
        if (collision.CompareTag("Monster") || collision.CompareTag("Spawner"))
        {
            if (!alreadyHit)
            {
                StopCoroutine(timerCoroutine);
                DestroyBulletClientRpc();
                alreadyHit = true;
            }
            else
                return;

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
