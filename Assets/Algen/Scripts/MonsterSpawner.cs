using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MonsterType
{
    Golem_Armor, Metal_Monster_blue
}

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField]
    private List<MonsterData> monsterDatas;
    [SerializeField]
    private GameObject[] monsterPrefab;

    int mobCount = 0;

    void Start()
    {
        if(mobCount < 5)
        {
            for(int a = 0; a < 5; a++)
            {
                int ranMobType = Random.Range(0, monsterDatas.Count);
                var monster = SpawnMonster((MonsterType)ranMobType, ranMobType);
                mobCount++;
            }
        }
    }

    public MonsterAi SpawnMonster(MonsterType type, int typeNum)
    {
        var newMonster = Instantiate(monsterPrefab[typeNum]).GetComponent<MonsterAi>();
        newMonster.MonsterData = monsterDatas[(int)type];
        return newMonster;
    }
}
