using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

// UTF-8 설정
public class TowerSingleAttackFx : TowerAttackFx
{
    Vector2 moveNextStep = Vector2.zero;    // 이동 방향 벡터

    bool alreadyHit;
    bool explosion;

    Coroutine timerCoroutine;

    [SerializeField]
    GameObject bulletExploFx;

    protected override void Start()
    {
        base.Start();
        alreadyHit = false;
    }

    private void FixedUpdate()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        transform.position += (Vector3)moveNextStep * 10 * Time.fixedDeltaTime;
    }

    public void GetTarget(Vector3 target, float GetDamage, GameObject obj, bool explo)
    {
        Vector3 direction = target - transform.position;
        direction.z = 0;
        moveNextStep = direction.normalized;
        damage = GetDamage;
        attackUnit = obj;
        alreadyHit = false;
        explosion = explo;
        timerCoroutine = StartCoroutine(nameof(RemoveTimer));
    }
    public void GetTarget2(Vector3 target, float GetDamage, GameObject obj, bool explo)
    {
        moveNextStep = target;
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

                if (explosion)
                {
                    NetworkObject bulletPool = networkObjectPool.GetNetworkObject(bulletExploFx, new Vector2(this.transform.position.x, this.transform.position.y), Quaternion.identity);
                    if (!bulletPool.IsSpawned) bulletPool.Spawn(true);
                    bulletPool.TryGetComponent(out TowerAreaAttackFx fx);
                    if (slowDebuff)
                    {
                        fx.SlowDebuffSet(slowTime);
                    }
                    else if (poisonTrueAttack)
                    {
                        fx.PoisonTrueAttackSet(poisonTime);
                    }
                    else if (ignoreDdefense)
                    {
                        fx.IgnoreDdefenseSet(ignorePercent);
                    }
                    fx.GetTarget(damage, attackUnit);
                }
                else
                {
                    if (collision.TryGetComponent(out MonsterAi monster))
                    {
                        TakeDamage(monster);
                    }
                    else if (collision.TryGetComponent(out MonsterSpawner spawner))
                    {
                        TakeDamage(spawner);
                    }
                }

                DestroyBullet();
                alreadyHit = true;
            }
        }
    }
}
