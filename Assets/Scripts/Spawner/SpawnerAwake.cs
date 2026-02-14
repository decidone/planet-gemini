using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SpawnerAwake : NetworkBehaviour
{
    MonsterSpawner monsterSpawner;
    public List<GameObject> inObjList = new List<GameObject>();
    bool nearUserObjExist = false;
    int level;
    public CircleCollider2D coll;

    private void Awake()
    {
        monsterSpawner = GetComponentInParent<MonsterSpawner>();
        coll = GetComponent<CircleCollider2D>();
    }

    void Start()
    {
        level = monsterSpawner.spawnerLevel - 1;
    }

    public void DieFunc()
    {
        coll.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!inObjList.Contains(collision.gameObject) && IsServer &&
            (collision.GetComponent<Structure>() || collision.GetComponent<UnitAi>()
            || (collision.GetComponent<PlayerController>() && !collision.GetComponent<PlayerController>().isTeleporting)))
        {
            inObjList.Add(collision.gameObject);

            if (inObjList.Count > 0)
            {
                nearUserObjExist = true;
                monsterSpawner.SearchObj(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (inObjList.Contains(collision.gameObject) && IsServer &&
            (collision.GetComponent<Structure>() || collision.GetComponent<UnitAi>()
            || (collision.GetComponent<PlayerController>() && !collision.GetComponent<PlayerController>().isTeleporting)))
        {
            inObjList.Remove(collision.gameObject);

            if (nearUserObjExist && inObjList.Count == 0)
            {
                if (!NetworkObject.IsSpawned)
                {
                    return;
                }

                nearUserObjExist = false;
                monsterSpawner.SearchObj(false);
            }
        }
    }
}
