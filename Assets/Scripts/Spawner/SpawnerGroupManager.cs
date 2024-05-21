using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SpawnerGroupManager : NetworkBehaviour
{
    (int, int) matrixIndex;
    public List<MonsterSpawner> spawnerList = new List<MonsterSpawner>();

    public void SpawnerGroupStatsSet((int, int) spawnerMatrixIndex)
    {
        matrixIndex = spawnerMatrixIndex;
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

    public SpawnerGroupData SaveData()
    {
        SpawnerGroupData data = new SpawnerGroupData();

        data.spawnerMatrixIndex = matrixIndex;
        data.pos = Vector3Extensions.FromVector3(transform.position);

        foreach (MonsterSpawner spawner in spawnerList)
        {
            data.spawnerSaveDataList.Add(spawner.SaveData());
        }

        return data;
    }
}