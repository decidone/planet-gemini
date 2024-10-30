using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SpawnerSearchColl : NetworkBehaviour
{
    MonsterSpawner monsterSpawner;
    public List<GameObject> inObjList = new List<GameObject>();
    public List<Structure> structures = new List<Structure>();
    bool nearUserObjExist = false;
    int level;
    public CircleCollider2D coll;
    int[] collSize = new int[8] { 55, 65, 75, 85, 95, 95, 95, 95 }; // 레벨 별 콜라이더 크기
    int[] maxCollSize = new int[8] { 105, 135, 175, 225, 285, 285, 285, 285 }; // 광폭화 시 최대 콜라이더 크기
    int increaseSize = 10; // 광폭화의날 콜라이더 크기 증가
    public float violentCollSize;

    private void Awake()
    {
        monsterSpawner = GetComponentInParent<MonsterSpawner>();
        coll = GetComponent<CircleCollider2D>();
    }

    void Start()
    {
        level = monsterSpawner.sppawnerLevel - 1;
        coll.radius = collSize[level];
    }

    public void DieFunc()
    {
        foreach (GameObject target in inObjList)
        {
            if (target != null)
            {
                if (target.TryGetComponent(out UnitAi unit))
                {
                    unit.RemoveTarget(monsterSpawner.gameObject);
                }
                else if (target.TryGetComponent(out AttackTower tower))
                {
                    tower.RemoveMonster(monsterSpawner.gameObject);
                }
            }
        }
        coll.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!inObjList.Contains(collision.gameObject) && IsServer &&
            (collision.GetComponent<TowerAi>() || collision.GetComponent<UnitAi>() || collision.GetComponent<PlayerController>()))
        {
            inObjList.Add(collision.gameObject);
        }
        else if (IsServer && collision.TryGetComponent(out Structure structure))
        {
            if (!structures.Contains(structure))
            {
                structures.Add(structure);
                monsterSpawner.energyUseStrs.Add(structure, structure.energyConsumption);
            }
        }

        if (!nearUserObjExist && (inObjList.Count > 0 || structures.Count > 0))
        {
            nearUserObjExist = true;
            monsterSpawner.SearchObj(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (inObjList.Contains(collision.gameObject) && IsServer &&
            (collision.GetComponent<TowerAi>() || collision.GetComponent<UnitAi>() || collision.GetComponent<PlayerController>()))
        {
            inObjList.Remove(collision.gameObject);
        }
        else if (IsServer && collision.TryGetComponent(out Structure structure))
        {
            if (structures.Contains(structure))
            {
                structures.Remove(structure);
                monsterSpawner.energyUseStrs.Remove(structure);
            }
        }

        if (nearUserObjExist && inObjList.Count == 0 && structures.Count == 0)
        {
            if (!NetworkObject.IsSpawned)
            {
                return;
            }

            nearUserObjExist = false;
            monsterSpawner.SearchObj(false);
        }
    }

    public void SearchCollExtend()
    {
        if (violentCollSize != 0)
        {
            coll.radius = violentCollSize;
        }

        if (maxCollSize[level] > coll.radius)
        {
            coll.radius += increaseSize;
            violentCollSize = coll.radius;
        }
    }

    public void SearchCollReduction()
    {
        float reSize = violentCollSize - ((violentCollSize - collSize[level]) / 2);
        coll.radius = reSize;
    }

    public void SearchCollReturn()
    {
        coll.radius = collSize[level];
    }
}
