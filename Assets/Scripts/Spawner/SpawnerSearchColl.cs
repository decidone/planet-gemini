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

    private void Awake()
    {
        monsterSpawner = GetComponentInParent<MonsterSpawner>();
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
        GetComponent<CircleCollider2D>().enabled = false;
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
            if(!structures.Contains(structure))
                structures.Add(structure);
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
                structures.Remove(structure);
        }

        if (nearUserObjExist && inObjList.Count == 0 && structures.Count == 0)
        {
            nearUserObjExist = false;
            monsterSpawner.SearchObj(false);
        }
    }
}
