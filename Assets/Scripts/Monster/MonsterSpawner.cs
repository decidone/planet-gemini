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

    void Start()
    {        
        for(int a = 0; a < 12; a++)
        {
            //int ranMobType = Random.Range(0, monsterDatas.Count);
            //var monster = SpawnMonster((MonsterType)ranMobType, ranMobType);
            var monster = SpawnMonster((MonsterType)a, a);
        }        
    }

    public GetMonsterData SpawnMonster(MonsterType type, int typeNum)
    {
        var newMonster = Instantiate(monsterPrefab[typeNum]).GetComponent<GetMonsterData>();
        newMonster.transform.SetParent(this.transform, false);
        newMonster.MonsterData = monsterDatas[(int)type];
        return newMonster;
    }
}
