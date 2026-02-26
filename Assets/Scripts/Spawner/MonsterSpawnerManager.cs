using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class MonsterSpawnerManager : NetworkBehaviour
{
    public Dictionary<(int map, int area), List<MonsterSpawner>> monsterSpawners = new Dictionary<(int, int), List<MonsterSpawner>>();
    // map 1 = hostMap, 2 = clientMap

    GameObject[,] spawnerMap1Matrix;
    GameObject[,] spawnerMap2Matrix;
    int splitCount;

    WavePoint wavePoint;

    bool waveState;
    public bool hostMapWave;
    Vector3 wavePos;

    public Sprite[] spawnerSprite;

    public List<GameObject> waveMonsters = new List<GameObject>();
    private int baseSpawnCount = 17;

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

        if (!monsterSpawners.TryGetValue(key, out var list))
            return;

        list.Remove(spawner);

        if (list.Count > 0)
            return;

        monsterSpawners.Remove(key);

        if (!HasAnyMonsterSpawner())
        {
            GameManager.instance.BloodMoonOff();
        }
    }

    public bool HasAnyMonsterSpawner()
    {
        foreach (var kv in monsterSpawners)
        {
            if (kv.Value.Count > 0)
                return true;
        }
        return false;
    }

    public void AreaGroupLevelUp(MonsterSpawner spawner, int preGroupNum, int groupNum, bool isHostMap)
    {
        AreaGroupRemove(spawner, preGroupNum, isHostMap);
        AreaGroupSet(spawner, groupNum, isHostMap);
    }

    public void WaveStateLoad(SpawnerManagerSaveData data)
    {
        waveState = data.waveState;
        hostMapWave = data.hostMapWave;
        wavePos = Vector3Extensions.ToVector3(data.wavePos);

        if (waveState)
        {
            wavePoint.LoadWaveStart(wavePos, hostMapWave);
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
                if (spawnerMap1Matrix[x, y] != null)
                    group1Data[x, y] = spawnerMap1Matrix[x, y].GetComponent<SpawnerGroupManager>().SaveData();
                if (spawnerMap2Matrix[x, y] != null)
                    group2Data[x, y] = spawnerMap2Matrix[x, y].GetComponent<SpawnerGroupManager>().SaveData();
            }
        }
        data.waveState = waveState;
        data.hostMapWave = hostMapWave;
        data.wavePos = Vector3Extensions.FromVector3(wavePos);
        data.spawnerMap1Matrix = group1Data;
        data.spawnerMap2Matrix = group2Data;
        return data;
    }

    public void SetCorruption()
    {
        MonsterSpawner[] spawners = GetComponentsInChildren<MonsterSpawner>();
        for (int i = 0; i < spawners.Length; i++)
            spawners[i].SetCorruption();
    }

    public bool ViolentDayOn(bool hostMap, bool forcedOperation)
    {
        var allSpawners = monsterSpawners
            .SelectMany(kv => kv.Value)
            .Where(s => s.isInHostMap == hostMap)
            .ToList();

        int minLevel = allSpawners.Min(s => s.spawnerLevel);

        var lowestSpawners = allSpawners
            .Where(s => s.spawnerLevel == minLevel)
            .ToList();

        MonsterSpawner aggroSpawner = lowestSpawners[Random.Range(0, lowestSpawners.Count)];

        if (aggroSpawner)
        {
            waveState = true;
            hostMapWave = hostMap;
            aggroSpawner.violentDay = true;
            wavePos = aggroSpawner.transform.position;
            WavePointOnServerRpc(wavePos, hostMap);
            return true;
        }
        else
        {
            Debug.Log("ViolentDayOn");
            WavePointOnServerRpc();
            return false;
        }
    }

    [ServerRpc]
    void WavePointOnServerRpc()
    {
        WavePointOnClientRpc();
    }

    [ServerRpc]
    void WavePointOnServerRpc(Vector3 pos, bool hostMap)
    {
        WavePointOnClientRpc(pos, hostMap);
    }

    [ClientRpc]
    void WavePointOnClientRpc()
    {
        WarningWindow.instance.WarningTextSet("No Wave Detected.");
    }


    [ClientRpc]
    void WavePointOnClientRpc(Vector3 pos, bool hostMap)
    {
        wavePoint.WaveStart(pos, hostMap);
        WarningWindow.instance.WarningTextSet("Warning! Wave incoming at 8:00", hostMap);
    }

    public void ViolentDayStart()
    {
        foreach (var data in monsterSpawners)
        {
            foreach (MonsterSpawner spawner in data.Value)
            {
                if (spawner.violentDay)
                {
                    //spawner.WaveStart(); // 스폰 코드 변경해야함
                    SpawnWave(spawner);
                    WaveStartWarrningServerRpc();
                    spawner.violentDay = false;
                    return;
                }
            }
        }
    }

    [ServerRpc]
    void WaveStartWarrningServerRpc()
    {
        WaveStartWarrningClientRpc();
    }

    [ClientRpc]
    void WaveStartWarrningClientRpc()
    {
        WarningWindow.instance.WarningTextSet("Wave Incoming");
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

        if (IsServer)
        {
            WavePointOffServerRpc(hostMapWave);
            WaveEndServerRpc();
        }
    }

    [ServerRpc]
    void WaveEndServerRpc()
    {
        WaveEndClientRpc();
    }

    [ClientRpc]
    void WaveEndClientRpc()
    {
        waveState = false;
        SoundManager.instance.BattleStateSet(hostMapWave, waveState);
    }

    public bool HasMonsterSpawnerOnMap(bool isHostMap)
    {
        int mapIndex = isHostMap ? 1 : 2;

        foreach (var spawner in monsterSpawners)
        {
            if (spawner.Key.map == mapIndex && spawner.Value.Count > 0)
                return true;
        }
        return false;
    }

    // ===============================
    // 외부 진입점
    // ===============================
    public void SpawnWave(MonsterSpawner spawner)
    {
        float spawnMultiplier = GameManager.instance.spawnMultiplier;
        float difficulty = GameManager.instance.difficultyPercent;

        int totalSpawnCount = CalculateTotalSpawnCount(spawnMultiplier);
        SpawnRatio ratio = GetSpawnRatio(difficulty);

        SpawnCount counts = CalculateSpawnCounts(totalSpawnCount, ratio);

        spawner.ExecuteSpawn(counts.weak, counts.normal, counts.strong);
    }

    // ===============================
    // 총 스폰 수 계산
    // ===============================
    int CalculateTotalSpawnCount(float spawnMultiplier)
    {
        return Mathf.Max(1, Mathf.RoundToInt(baseSpawnCount * spawnMultiplier));
    }

    // ===============================
    // 몬스터 비율 정의
    // ===============================
    struct SpawnRatio
    {
        public float weak;
        public float normal;
        public float strong;
    }

    SpawnRatio GetSpawnRatio(float difficulty)
    {
        difficulty = Mathf.Clamp(difficulty, 0f, 1000f);

        if (difficulty < 400f)
        {
            float t = difficulty / 400f; // 0~1

            float weak = Mathf.Lerp(0.9f, 0.5f, t);
            float normal = Mathf.Lerp(0.1f, 0.5f, t * t);
            float strong = Mathf.Max(0f, 1f - (weak + normal));

            return new SpawnRatio
            {
                weak = weak,
                normal = normal,
                strong = strong
            };
        }

        float t2 = Mathf.Clamp01((difficulty - 400f) / 600f);

        float weak2 = Mathf.Lerp(0.5f, 0.25f, t2);
        float normal2 = Mathf.Lerp(0.5f, 0.4f, t2);
        float strong2 = Mathf.Lerp(0.0f, 0.35f, t2);

        NormalizeRatios(ref weak2, ref normal2, ref strong2);
        return new SpawnRatio { weak = weak2, normal = normal2, strong = strong2 };
    }

    void NormalizeRatios(ref float a, ref float b, ref float c)
    {
        float sum = a + b + c;
        a /= sum;
        b /= sum;
        c /= sum;
    }

    // ===============================
    // 스폰 수 계산
    // ===============================
    struct SpawnCount
    {
        public int weak;
        public int normal;
        public int strong;
    }

    SpawnCount CalculateSpawnCounts(int total, SpawnRatio ratio)
    {
        int weak = Mathf.RoundToInt(total * ratio.weak);
        int normal = Mathf.RoundToInt(total * ratio.normal);
        int strong = Mathf.RoundToInt(total * ratio.strong);

        // 오차 보정
        int diff = total - (weak + normal + strong);
        weak += diff;

        return new SpawnCount
        {
            weak = weak,
            normal = normal,
            strong = strong
        };
    }
}
