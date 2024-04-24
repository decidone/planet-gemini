using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SpawnerGroupManager : NetworkBehaviour
{
    SpawnerGroupStats spawnerGroupStats;
    public List<MonsterSpawner> spawnerList = new List<MonsterSpawner>();

    public void SpawnerGroupStatsSet(bool inHostMap, int areaGroup, (int, int) spawnerMatrixIndex)
    {
        spawnerGroupStats = new SpawnerGroupStats(inHostMap, areaGroup, spawnerMatrixIndex);
    }

    public void SpawnerSet(GameObject spawner)
    {
        if(spawner != null)
        {
            MonsterSpawner monsterSpawner = spawner.GetComponent<MonsterSpawner>();
            spawnerList.Add(monsterSpawner);
            spawner.transform.parent = gameObject.transform;
        }
    }

    public void WaveSet(Vector3 WaveCenterPos)
    {
        foreach (MonsterSpawner spawner in spawnerList)
        {
            spawner.WaveTeleport(WaveCenterPos);
        }
    }

    public List<MonsterSpawner> MonsterSpawnerListData()
    {
        return spawnerList;
    }

    public List<UnitCommonAi> MonsterListData()
    {
        List<UnitCommonAi> monster = new List<UnitCommonAi>();

        foreach (MonsterSpawner spawner in spawnerList)
        {
            monster.AddRange(spawner.totalMonsterList);
            monster.AddRange(spawner.guardianList);
            monster.AddRange(spawner.waveMonsterList);
        }

        return monster; 
    }
}

public struct SpawnerGroupStats
{
    public bool isHostMap;
    public int areaGroup;
    public (int, int) spawnerMatrixIndex;

    // 생성자 정의
    public SpawnerGroupStats(bool _inHostMap, int _areaGroup, (int, int) _spawnerMatrixIndex)
    {
        isHostMap = _inHostMap;
        areaGroup = _areaGroup;
        spawnerMatrixIndex = _spawnerMatrixIndex;
    }
}
