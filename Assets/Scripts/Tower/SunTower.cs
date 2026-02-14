using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunTower : TowerAi
{
    [SerializeField] SpriteRenderer view;
    [SerializeField] float debuffTimer;
    [SerializeField] float debuffInterval;
    [SerializeField] float debuffPer;

    protected override void Awake()
    {
        base.Awake();
        int mask =
            (1 << LayerMask.NameToLayer("Monster")) |
            (1 << LayerMask.NameToLayer("Spawner"));

        contactFilter.SetLayerMask(mask);
        contactFilter.useLayerMask = true;
    }

    protected override void Start()
    {
        base.Start();
        view.enabled = false;
        StartCoroutine(EfficiencyCheckLoop());
    }

    //protected override void Update()
    //{
    //    base.Update();
    //    if (!isPreBuilding && conn != null && conn.group != null && conn.group.efficiency > 0)
    //    {
    //        debuffTimer += Time.deltaTime;

    //        if (debuffTimer >= debuffInterval)
    //        {
    //            int hitCount = Physics2D.OverlapCircle(
    //                transform.position,
    //                structureData.ColliderRadius,
    //                contactFilter,
    //                targetColls
    //            );

    //            bool isMonsterNearby = false;

    //            if (hitCount > 0)
    //            {
    //                for (int i = 0; i < hitCount; i++)
    //                {
    //                    GameObject monster = targetColls[i].gameObject;
    //                    if (monster.TryGetComponent(out MonsterAi mon))
    //                    {
    //                        isMonsterNearby = true;
    //                        mon.RefreshDebuff(conn.group.efficiency, debuffPer);    // 서버, 클라이언트 상관없이 디버프 띄워주는데 데미지 계산은 서버 디버프 유무로만 계산
    //                    }
    //                }
    //            }

    //            if (isMonsterNearby)
    //            {
    //                OperateStateSet(true);
    //                isMonsterNearby = false;
    //            }
    //            else
    //            {
    //                OperateStateSet(false);
    //            }

    //            debuffTimer = 0f;
    //        }
    //    }
    //}

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            searchManager.TowerListAdd(this);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsServer)
        {
            searchManager.TowerListRemove(this);
        }
    }

    public override void SearchObjectsInRange()
    {
        if (!isPreBuilding && conn != null && conn.group != null && conn.group.efficiency > 0)
        {
            int hitCount = Physics2D.OverlapCircle(
                transform.position,
                structureData.ColliderRadius,
                contactFilter,
                targetColls
            );

            bool isMonsterNearby = false;

            if (hitCount > 0)
            {
                for (int i = 0; i < hitCount; i++)
                {
                    GameObject monster = targetColls[i].gameObject;
                    if (monster.TryGetComponent(out MonsterAi mon))
                    {
                        isMonsterNearby = true;
                        mon.RefreshDebuffServerRpc(conn.group.efficiency, debuffPer);    // 서버, 클라이언트 상관없이 디버프 띄워주는데 데미지 계산은 서버 디버프 유무로만 계산
                    }
                }
            }

            if (isMonsterNearby)
            {
                OperateStateSet(true);
            }
            else
            {
                OperateStateSet(false);
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
