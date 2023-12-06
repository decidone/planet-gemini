using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerGroupManager : MonoBehaviour
{
    List<GameObject> spawnerList = new List<GameObject>();

    public void SpawnerSet(GameObject spawner)
    {
        if(spawner != null)
        {
            spawnerList.Add(spawner);
            spawner.transform.parent = gameObject.transform;
        }
    }

    public void GroupMonsterScriptSet(bool scriptState)
    {
        foreach (GameObject spawner in spawnerList)
        {
            spawner.GetComponent<MonsterSpawner>().MonsterScriptSet(scriptState);
        }
    }
}
