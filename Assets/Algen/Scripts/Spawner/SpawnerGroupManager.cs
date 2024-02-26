using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerGroupManager : MonoBehaviour
{
    public List<GameObject> spawnerList = new List<GameObject>();

    public void SpawnerSet(GameObject spawner)
    {
        if(spawner != null)
        {
            spawnerList.Add(spawner);
            spawner.transform.parent = gameObject.transform;
        }
    }

    public void WaveSet(Vector3 WaveCenterPos)
    {
        foreach (GameObject spawner in spawnerList)
        {
            spawner.GetComponent<MonsterSpawner>().WaveTeleport(WaveCenterPos);
        }
    }
}
