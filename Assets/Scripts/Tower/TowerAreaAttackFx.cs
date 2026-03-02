using UnityEngine;

// UTF-8 설정
public class TowerAreaAttackFx : TowerAttackFx
{
    public void GetTarget(float GetDamage, GameObject obj)
    {
        damage = GetDamage;
        attackUnit = obj;
    }

    //에니메이션 사용
    void FxEnd(string str)
    {
        if (str == "false")
        {
            Invoke(nameof(DestroyBullet), 0.1f);
            //Destroy(this.gameObject, 0.1f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer)
            return;


        if (collision.TryGetComponent(out MonsterAi monster))
        {
            TakeDamage(monster);
        }
        else if (collision.TryGetComponent(out MonsterSpawner spawner))
        {
            TakeDamage(spawner);
        }        
    }
}
