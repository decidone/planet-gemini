using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerSearchColl : MonoBehaviour
{
    MonsterSpawner monsterSpawner;
    public List<GameObject> inObjList = new List<GameObject>();
    bool nearUserObjExist = false;

    private void Awake()
    {
        monsterSpawner = GetComponentInParent<MonsterSpawner>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!inObjList.Contains(collision.gameObject))
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
        if (inObjList.Contains(collision.gameObject))
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
