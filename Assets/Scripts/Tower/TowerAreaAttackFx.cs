using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class TowerAreaAttackFx : MonoBehaviour
{
    float damage = 0;

    [SerializeField]
    protected Animator animator;

    public void GetTarget(float GetDamage)
    {
        damage = GetDamage;
    }

    void FxEnd(string str)
    {
        if (str == "false")
        {
            Destroy(this.gameObject, 0.1f);            
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster"))
        {
            if (!collision.isTrigger)
            {
                collision.GetComponent<MonsterAi>().TakeDamage(damage);
            }
        }
    }
}
