using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class TowerSingleAttackFx : MonoBehaviour
{
    float damage = 0;

    [SerializeField]
    protected Animator animator;
    Vector3 moveNextStep = Vector3.zero;    // 이동 방향 벡터
    bool isHit = false;

    void Update()
    {
        transform.position += moveNextStep * 2 * Time.fixedDeltaTime;
        Destroy(this.gameObject, 3f);
    }

    public void GetTarget(Vector3 target, float GetDamage)
    {
        moveNextStep = (target - transform.position).normalized;
        damage = GetDamage;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster") && !isHit)
        {
            if (!collision.isTrigger)
            {
                collision.GetComponent<MonsterAi>().TakeDamage(damage);
                isHit = true;
                Destroy(this.gameObject, 0.1f);
            }
        }
    }
}
