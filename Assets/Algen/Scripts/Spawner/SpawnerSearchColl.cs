using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerSearchColl : MonoBehaviour
{
    MonsterSpawner monsterSpawner;
    public List<GameObject> inObjList = new List<GameObject>();
    bool nearUserObjExist;

    private void Awake()
    {
        monsterSpawner = GetComponentInParent<MonsterSpawner>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!inObjList.Contains(collision.gameObject) && !collision.CompareTag("Monster") && !collision.CompareTag("Untagged") && !collision.CompareTag("Bullet"))
        {
            inObjList.Add(collision.gameObject);
            if (!nearUserObjExist && inObjList.Count > 0)
            {
                monsterSpawner.SearchObj(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (inObjList.Contains(collision.gameObject) && !collision.CompareTag("Monster") && !collision.CompareTag("Untagged") && !collision.CompareTag("Bullet"))
        {
            inObjList.Remove(collision.gameObject);
            if (nearUserObjExist && inObjList.Count == 0)
            {
                monsterSpawner.SearchObj(false);
            }
        }
    }
}
