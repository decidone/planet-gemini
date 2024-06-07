using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SpawnerSearchColl : NetworkBehaviour
{
    MonsterSpawner monsterSpawner;
    public List<GameObject> inObjList = new List<GameObject>();
    bool nearUserObjExist = false;

    private void Awake()
    {
        monsterSpawner = GetComponentInParent<MonsterSpawner>();
    }

    public void DieFunc()
    {
        Debug.Log(inObjList.Count);
        foreach (GameObject target in inObjList)
        {
            if (target != null)
            {
                Debug.Log(target.name);
                if (target.TryGetComponent(out UnitAi unit))
                {
                    unit.RemoveTarget(monsterSpawner.gameObject);
                    Debug.Log("unitCall");
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
        if (!inObjList.Contains(collision.gameObject) && IsServer)
        {
            inObjList.Add(collision.gameObject);
            if (!nearUserObjExist && inObjList.Count > 0)
            {
                nearUserObjExist = true;
                monsterSpawner.SearchObj(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (inObjList.Contains(collision.gameObject) && IsServer)
        {
            inObjList.Remove(collision.gameObject);
            if (nearUserObjExist && inObjList.Count == 0)
            {
                nearUserObjExist = false;
                monsterSpawner.SearchObj(false);
            }
        }
    }
}
