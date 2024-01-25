using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnergyColony : MonoBehaviour
{
    //[SerializeField]
    //List<EnergyColony> othColonyList = new List<EnergyColony>();

    public List<EnergyGroupConnector> connectors;
    [HideInInspector]
    public EnergyGroup energyGroup;
    [SerializeField]
    //List<MonsterSpawner> monsterSpawners = new List<MonsterSpawner>();
    LayerMask targetLayer;
    public bool mainColony;
    public float energy;
    public float consumption;

    protected float spawnerCheckTime;
    protected float spawnerCheckInterval;

    void Start()
    {
        targetLayer = 1 << LayerMask.NameToLayer("Spawner");
        spawnerCheckInterval = 10f;
    }

    void Update()
    {
        if (energyGroup != null && mainColony)
        {
            spawnerCheckTime += Time.deltaTime;
            if (spawnerCheckTime > spawnerCheckInterval)
            {
                EnergyCheck();
                MonsterSpawnerCheck();
                //MonsterSpawnerWaveChcek();
                spawnerCheckTime = 0f;
            }
        }
    }

    //public void OthColonyListAdd(List<EnergyColony> list)
    //{
    //    othColonyList = list.ToList();
    //    foreach (EnergyColony oth in othColonyList)
    //    {
    //        oth.connectors = connectors;
    //    }
    //}

    public void DataClear()
    {
        //othColonyList.Clear();
        connectors.Clear();
    }

    public void DestoryThisScipt()
    {
        Destroy(this);
    }

    public void EnergyCheck()
    {
        Charge();
        Consume();
    }

    public void Charge()
    {
        float temp = 0f;
        for (int i = 0; i < connectors.Count; i++)
        {
            if (connectors[i].energyGenerator != null && connectors[i].energyGenerator.isOperate)
            {
                temp += connectors[i].energyGenerator.energyProduction;
            }
            else if (connectors[i].steamGenerator != null && connectors[i].steamGenerator.isOperate)
            {
                temp += connectors[i].steamGenerator.energyProduction;
            }
        }
        energy = temp;
    }

    public void Consume()
    {
        float temp = 0f;
        for (int i = 0; i < connectors.Count; i++)
        {
            for (int j = 0; j < connectors[i].consumptions.Count; j++)
            {
                if (connectors[i].consumptions[j].isOperate)
                {
                    temp += connectors[i].consumptions[j].energyConsumption;
                }
            }
        }
        consumption = temp;
    }
    
    //void SubColonyEnergyCheck(EnergyColony subColony)
    //{
    //    subColony.energy = energy;
    //    subColony.consumption = consumption;
    //}

    //public void SubColonyMonsterSpawnerCheck()
    //{
    //    foreach (EnergyColony subColony in othColonyList)
    //    {
    //        SubColonyEnergyCheck(subColony);
    //        subColony.MonsterSpawnerCheck();
    //    }
    //}

    public void MonsterSpawnerCheck()
    {
        EnergyCheck();
        //monsterSpawners.Clear();
        float scanDist = ConvergeFunction(energy);
        Debug.Log("ScanDist : " + scanDist + " , energy : " + energy);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, scanDist, targetLayer);
        Debug.Log(colliders.Length);
        for (int i = 0; i < colliders.Length; i++)
        {
            GameObject obj = colliders[i].gameObject;

            if (obj.TryGetComponent(out MonsterSpawner spawner))
            {
                spawner.ColonyCall(this);
            }
            //if (obj.TryGetComponent(out MonsterSpawner spawner) && !monsterSpawners.Contains(spawner))
            //{
            //    monsterSpawners.Add(spawner);
            //}
        }
    }

    float ConvergeFunction(float x)
    {
        float b = 0.0002f;
        return 300f * (1f - Mathf.Exp(-b * x));
    }

    //void MonsterSpawnerWaveChcek()
    //{
    //    foreach (MonsterSpawner spawner in monsterSpawners)
    //    {
    //        spawner.ColonyCall(this);
    //    }
    //}
}
