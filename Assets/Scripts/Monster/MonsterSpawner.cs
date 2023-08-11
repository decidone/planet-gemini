using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject[] monsterPrefab;
    int spawnNum = 0;

    void Start()
    {        
        if(spawnNum > 0)
        {
            for (int a = 0; a < spawnNum; a++)
            {
                var monster = SpawnMonster(a);
            }
        }
        else
        {
            for(int a = 0; a < 12; a++)
            {
                var monster = SpawnMonster(a);
            }
        }        
    }

    public GameObject SpawnMonster(int typeNum)
    {
        var newMonster = Instantiate(monsterPrefab[typeNum]);
        newMonster.transform.SetParent(this.transform, false);
        return newMonster;
    }
}
