using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Unity.Netcode;

// UTF-8 설정
public class BulletCtrl : NetworkBehaviour
{
    public IObjectPool<GameObject> bulletPool { get; set; }

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

    void Update()
    {
        transform.position += moveNextStep * 5 * Time.fixedDeltaTime;
    }

    [ClientRpc]
    public void DestroyBulletClientRpc()
    {
        if(IsServer)
        {
            //NetworkObjectPool.Singleton.ReturnNetworkObject(NetworkObject, poolObj);
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
        if(!alreadyHit)
            DestroyBulletClientRpc();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer)
            return;
        if (collision.CompareTag("Monster"))
        {
            if (!alreadyHit)
            {
                StopCoroutine(timerCoroutine);
                DestroyBulletClientRpc();
                Debug.Log("??");
                alreadyHit = true;
            }
            else
                return;

            if (collision.TryGetComponent(out MonsterAi monster))
            {
                monster.TakeDamageClientRpc(damage);
            }
            else if (collision.TryGetComponent(out MonsterSpawner spawner))
            {
                spawner.GetComponent<MonsterSpawner>().TakeDamage(damage, attackUnit);
            }
        }
    }
}
