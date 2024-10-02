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
    bool explosion;

    Coroutine timerCoroutine;

    NetworkObjectPool networkObjectPool;
    [SerializeField]
    GameObject bulletExploFx;

    private void Start()
    {
        networkObjectPool = NetworkObjectPool.Singleton;
        alreadyHit = false;
    }

    void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

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

    public void GetTarget(Vector3 target, float GetDamage, GameObject obj, bool explo)
    {
        moveNextStep = (target - transform.position).normalized;
        damage = GetDamage;
        attackUnit = obj;
        alreadyHit = false;
        explosion = explo;
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

            if (explosion)
            {
                NetworkObject bulletPool = networkObjectPool.GetNetworkObject(bulletExploFx, new Vector2(this.transform.position.x, this.transform.position.y), Quaternion.identity);
                if (!bulletPool.IsSpawned) bulletPool.Spawn(true);
                bulletPool.GetComponent<TowerAreaAttackFx>().GetTarget(damage, attackUnit);
            }
            else
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
}
