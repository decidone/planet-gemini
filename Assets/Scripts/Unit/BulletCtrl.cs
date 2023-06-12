using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCtrl : MonoBehaviour
{
    float damage = 0;
    public Transform aggroTarget = null;   // ≈∏∞Ÿ
    Vector3 moveNextStep = Vector3.zero;    // ¿Ãµø πÊ«‚ ∫§≈Õ

    // Update is called once per frame
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
        if (collision.CompareTag("Monster"))
        {
            if (collision.isTrigger == false)
            {
                collision.GetComponent<MonsterAi>().TakeDamage(damage);
                Destroy(this.gameObject, 0.1f);
            }
        }//if (collision.CompareTag("Player"))
    }//private void OnTriggerEnter2D(Collider2D collision)
}
