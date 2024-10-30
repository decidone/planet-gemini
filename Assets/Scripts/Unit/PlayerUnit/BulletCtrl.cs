using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Unity.Netcode;

// UTF-8 설정
public class BulletCtrl : NetworkBehaviour
{
    float damage = 0;
    public Transform aggroTarget = null;    // 타겟
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

    private void FixedUpdate()
    {   
        if (Time.timeScale == 0)
        {
            return;
        }

        transform.position += moveNextStep * 10 * Time.fixedDeltaTime;
    }

    public void DestroyBullet()
    {
        if(IsServer)
        {
            NetworkObject.Despawn();
        }
    }

    public void GetTarget(Vector3 target, float GetDamage, GameObject obj)
    {
        Vector3 direction = target - transform.position;
        direction.z = 0;
        moveNextStep = direction.normalized;
        damage = GetDamage;
        attackUnit = obj;
        alreadyHit = false;
        timerCoroutine = StartCoroutine(nameof(RemoveTimer));
    }

    IEnumerator RemoveTimer()
    {
        yield return new WaitForSeconds(5.0f);
        if(!alreadyHit)
            DestroyBullet();
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
                DestroyBullet();
                alreadyHit = true;
            }
            else
                return;

            if (collision.TryGetComponent(out MonsterAi monster))
            {
                monster.TakeDamage(damage, 0);
            }
            else if (collision.TryGetComponent(out MonsterSpawner spawner))
            {
                spawner.TakeDamage(damage, attackUnit);
            }
        }
    }
}
