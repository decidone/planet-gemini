using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

// UTF-8 설정
public class BulletCtrl : MonoBehaviour
{
    public IObjectPool<GameObject> bulletPool { get; set; }

    float damage = 0;
    public Transform aggroTarget = null;    // 타겟
    Vector3 moveNextStep = Vector3.zero;    // 이동 방향 벡터
    GameObject attackUnit;

    bool isRelease = false;

    void Update()
    {
        transform.position += moveNextStep * 5 * Time.fixedDeltaTime;
    }

    public void DestroyBullet()
    {
        if (!isRelease)
            bulletPool.Release(gameObject);
        else
            isRelease = false;
    }

    public void GetTarget(Vector3 target, float GetDamage, GameObject obj)
    {
        moveNextStep = (target - transform.position).normalized;
        damage = GetDamage;
        attackUnit = obj;
        Invoke(nameof(DestroyBullet), 5f);
    }
     
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster"))
        {
            if (collision.TryGetComponent(out MonsterAi monster))
            {
                monster.TakeDamage(damage);
            }
            else if (collision.TryGetComponent(out MonsterSpawner spawner))
            {
                spawner.GetComponent<MonsterSpawner>().TakeDamage(damage, attackUnit);
            }
            DestroyBullet();
            isRelease = true;
        }
    }
}
