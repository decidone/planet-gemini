using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunTower : TowerAi
{
    [SerializeField] SpriteRenderer view;
    [SerializeField] float debuffTimer;
    [SerializeField] float debuffInterval;

    protected override void Update()
    {
        base.Update();
        if (!isPreBuilding && IsServer)
        {
            debuffTimer += Time.deltaTime;

            if (debuffTimer >= debuffInterval)
            {
                Collider2D[] colliders = Physics2D.OverlapCircleAll(this.transform.position, structureData.ColliderRadius);

                foreach (Collider2D collider in colliders)
                {
                    GameObject monster = collider.gameObject;
                    if (monster.CompareTag("Monster"))
                    {
                        if (monster.TryGetComponent(out MonsterAi mon))
                        {
                            mon.RefreshDebuff();
                        }
                    }
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
