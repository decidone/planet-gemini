using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using QFSW.QC;
using Mono.CSharp;

public class MonsterSpawnerManager : NetworkBehaviour
{
    public Dictionary<(int map, int area), List<MonsterSpawner>> monsterSpawners = new Dictionary<(int, int), List<MonsterSpawner>>();
    // map 1 = hostMap, 2 = clientMap

    public GameObject[,] spawnerMap1Matrix;
    public GameObject[,] spawnerMap2Matrix;
    SpawnerSetManager spawnerSetManager;
    int splitCount;
    int xIndex;
    int yIndex;

    WavePoint wavePoint;

    bool waveState;
    public bool hostMapWave;
    Vector3 wavePos;
    Vector3 targetPos;

    public Sprite[] spawnerSprite;

    public List<GameObject> waveMonsters = new List<GameObject>();

    #region Singleton
    public static MonsterSpawnerManager instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    #endregion

    private void Start()
    {
        spawnerSetManager = SpawnerSetManager.instance;
        wavePoint = WavePoint.instance;
    }

    private void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }
    }

    public void MatrixSet(int spCount, GameObject[,] map1Matrix, GameObject[,] map2Matrix, bool isHostMap)
    {
        splitCount = spCount;
        if (isHostMap)
        {
            spawnerMap1Matrix = new GameObject[splitCount, splitCount];
            spawnerMap1Matrix = map1Matrix;
        }
        else
        {
            spawnerMap2Matrix = new GameObject[splitCount, splitCount];
            spawnerMap2Matrix = map2Matrix;
        }
    }

    public void MatrixSet(GameObject obj, (int, int) matrixIndex, bool isHostMap)
    {
        if (isHostMap)
        {
            spawnerMap1Matrix[matrixIndex.Item1, matrixIndex.Item2] = obj;
        }
        else
        {
            spawnerMap2Matrix[matrixIndex.Item1, matrixIndex.Item2] = obj;
        }
    }

    public void SplitCountSet(int spCount)
    {
        splitCount = spCount;
        spawnerMap1Matrix = new GameObject[spCount, spCount];
        spawnerMap2Matrix = new GameObject[spCount, spCount];
    }

    public void AreaGroupSet(MonsterSpawner spawner, int groupNum, bool isHostMap)
    {
        (int map, int area) key = (isHostMap ? 1 : 2, groupNum);

        if (!monsterSpawners.ContainsKey(key))
            monsterSpawners.Add(key, new List<MonsterSpawner>());
        
        monsterSpawners[key].Add(spawner);
    }

    public void AreaGroupRemove(MonsterSpawner spawner, int groupNum, bool isInHostMap)
    {
        (int map, int area) key = (isInHostMap ? 1 : 2, groupNum);

        if (monsterSpawners.ContainsKey(key))
        {
            monsterSpawners[key].Remove(spawner);

            if (monsterSpawners[key].Count == 0)
                monsterSpawners.Remove(key);
        }
    }

    public void AreaGroupLevelUp(MonsterSpawner spawner, int preGroupNum, int groupNum, bool isHostMap)
    {
        AreaGroupRemove(spawner, preGroupNum, isHostMap);
        AreaGroupSet(spawner, groupNum, isHostMap);
    }

    public void WaveStateLoad(SpawnerManagerSaveData data)
    {
        waveState = data.waveState;
        Debug.Log("Load waveState" + waveState);
        hostMapWave = data.hostMapWave;
        wavePos = Vector3Extensions.ToVector3(data.wavePos);

        if (waveState)
        {
            wavePoint.LoadWaveStart(wavePos, hostMapWave);
            //if (IsServer)
            //    BattleBGMCtrl.instance.WaveStart(hostMapWave);
        }
    }

    public SpawnerManagerSaveData SaveData()
    {
        SpawnerManagerSaveData data = new SpawnerManagerSaveData();

        data.splitCount = splitCount;
        SpawnerGroupData[,] group1Data = new SpawnerGroupData[splitCount, splitCount];
        SpawnerGroupData[,] group2Data = new SpawnerGroupData[splitCount, splitCount];

        for (int x = 0; x < splitCount; x++)
        {
            for (int y = 0; y < splitCount; y++)
            {
                if(spawnerMap1Matrix[x, y] != null)
                    group1Data[x, y] = spawnerMap1Matrix[x, y].GetComponent<SpawnerGroupManager>().SaveData();
                if (spawnerMap2Matrix[x, y] != null)
                    group2Data[x, y] = spawnerMap2Matrix[x, y].GetComponent<SpawnerGroupManager>().SaveData();
            }
        }
        data.waveState = waveState;
        Debug.Log("save waveState" + waveState);
        data.hostMapWave = hostMapWave;
        data.wavePos = Vector3Extensions.FromVector3(wavePos);
        data.spawnerMap1Matrix = group1Data;
        data.spawnerMap2Matrix = group2Data;
        return data;
    }

    public void SetCorruption()
    {
        MonsterSpawner[] spawners = GetComponentsInChildren<MonsterSpawner>();
        Debug.Log("Client Spawner Set Count: " + spawners.Length);
        for (int i = 0; i < spawners.Length; i++)
            spawners[i].SetCorruption();
    }

    public bool ViolentDayOn(bool hostMap, bool forcedOperation)
    {
        float maxAggroAmount = 0;
        MonsterSpawner aggroSpawner = null;

        foreach (var data in monsterSpawners)
        {
            foreach (MonsterSpawner spawner in data.Value)
            {
                if (spawner.isInHostMap == hostMap)
                {
                    if (spawner.safeCount == 0)
                    {
                        if (!forcedOperation)
                            spawner.SearchCollExtend();
                        else
                            spawner.SearchCollFullExtend();

                        Collider2D[] colliders = Physics2D.OverlapCircleAll(spawner.transform.position, spawner.spawnerSearchColl.violentCollSize);
                        float aggroValue = 0;

                        foreach (Collider2D collider in colliders)
                        {
                            if (collider.CompareTag("Factory") && collider.TryGetComponent(out Structure str))
                            {
                                if (str.isOperate)
                                {
                                    aggroValue += str.energyConsumption;
                                }
                            }
                        }

                        if (aggroValue > maxAggroAmount)
                        {
                            aggroSpawner = spawner;
                            maxAggroAmount = aggroValue;
                        }
                        spawner.SearchCollReturn();
                    } 
                    spawner.SafeCountDown();
                }
            }
        }

        if (aggroSpawner)
        {
            waveState = true;
            hostMapWave = hostMap;
            aggroSpawner.ViolentDaySet();
            wavePos = aggroSpawner.transform.position;
            WavePointOnServerRpc(wavePos, hostMap);
            Debug.Log("Aggro Spawner transform is :" + wavePos + ", maxAggroAmount : " + maxAggroAmount);
            return true;
        }
        else
            return false;
    }

    [ServerRpc]
    void WavePointOnServerRpc(Vector3 pos, bool hostMap)
    {
        WavePointOnClientRpc(pos, hostMap);
    }

    [ClientRpc]
    void WavePointOnClientRpc(Vector3 pos, bool hostMap)
    {
        wavePoint.WaveStart(pos, hostMap);
    }

    public void ViolentDayOff()
    {
        foreach (var data in monsterSpawners)
        {
            foreach (MonsterSpawner spawner in data.Value)
            {
                spawner.SearchCollReturn();
            }
        }
    }

    public void ViolentDayStart()
    {
        foreach (var data in monsterSpawners)
        {
            foreach (MonsterSpawner spawner in data.Value)
            {
                if (spawner.violentDay)
                {
                    spawner.WaveStart();
                    spawner.SpawnerLevelUp();
                    spawner.SearchCollReturn();
                    return;
                }
            }
        }
    }

    public void WavePointOff()
    {
        WavePointOffServerRpc(hostMapWave);
    }

    [ServerRpc]
    public void WavePointOffServerRpc(bool hostMap)
    {
        WavePointOffClientRpc(hostMap);
    }

    [ClientRpc]
    void WavePointOffClientRpc(bool hostMap)
    {
        wavePoint.WaveEnd(hostMap);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        }
    }

    void OnClientConnectedCallback(ulong clientId)
    {
        SetCorruption();
        WaveStateSyncServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void WaveStateSyncServerRpc()
    {
        if (waveState)
        {
            WaveStateSyncClientRpc(wavePos, hostMapWave);
        }
    }

    [ClientRpc]
    void WaveStateSyncClientRpc(Vector3 pos, bool hostMap)
    {
        Debug.Log("WaveStateSyncClientRpc");
        waveState = true;
        hostMapWave = hostMap;
        wavePos = pos;
        WavePointOnClientRpc(wavePos, hostMapWave);
        SoundManager.instance.BattleStateSet(hostMapWave, GameManager.instance.violentDay);
    }
    
    public void WaveAddMonster(GameObject monster)
    {
        waveMonsters.Add(monster);
    }

    public void WaveAddMonster(List<GameObject> monsters)
    {
        waveMonsters.AddRange(monsters);
        SoundManager.instance.BattleStateSet(hostMapWave, waveState);
    }

    public void BattleRemoveMonster(GameObject monster)
    {
        if (waveMonsters.Contains(monster))
        {
            waveMonsters.Remove(monster);

            if (waveMonsters.Count == 0)
            {
                ViolentDayEnd();
            }
        }
    }

    void ViolentDayEnd()
    {
        foreach (var data in monsterSpawners)
        {
            foreach (MonsterSpawner spawner in data.Value)
            {
                if (spawner.violentDay)
                {
                    spawner.violentDay = false;
                }
            }
        }
        waveState = false;
        SoundManager.instance.BattleStateSet(hostMapWave, waveState);
    }

    public void WaveEndSet()
    {
        foreach (var monster in waveMonsters)
        {
            monster.GetComponent<MonsterAi>().WaveEnd();
        }
    }
}
