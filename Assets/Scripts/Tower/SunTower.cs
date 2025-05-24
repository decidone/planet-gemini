using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunTower : TowerAi
{
    [SerializeField] SpriteRenderer view;
    [SerializeField] float debuffTimer;
    [SerializeField] float debuffInterval;
    [SerializeField] float debuffPer;

    protected override void Update()
    {
        base.Update();
        if (!isPreBuilding && conn != null && conn.group != null && conn.group.efficiency > 0)
        {
            debuffTimer += Time.deltaTime;

            if (debuffTimer >= debuffInterval)
            {
                EfficiencyCheck();

                Collider2D[] colliders = Physics2D.OverlapCircleAll(this.transform.position, structureData.ColliderRadius);
                bool isMonsterNearby = false;

                foreach (Collider2D collider in colliders)
                {
                    GameObject monster = collider.gameObject;
                    if (monster.CompareTag("Monster"))
                    {
                        if (monster.TryGetComponent(out MonsterAi mon))
                        {
                            isMonsterNearby = true;
                            mon.RefreshDebuff(conn.group.efficiency, debuffPer);    // 서버, 클라이언트 상관없이 디버프 띄워주는데 데미지 계산은 서버 디버프 유무로만 계산
                        }
                    }
                }

                if (isMonsterNearby)
                {
                    OperateStateSet(true);
                    isMonsterNearby = false;
                }
                else
                {
                    OperateStateSet(false);
                }

                debuffTimer = 0f;
            }
        }
    }

    public override void SetBuild()
    {
        base.SetBuild();
        view.enabled = false;
    }

    public override void Focused()
    {
        view.enabled = true;
    }

    public override void DisableFocused()
    {
        view.enabled = false;
    }
}
