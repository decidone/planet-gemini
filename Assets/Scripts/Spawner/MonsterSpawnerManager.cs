using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using QFSW.QC;

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

    //bool isInHostMap;

    [SerializeField]
    bool map1Wave1Start = false;
    [SerializeField]
    bool map1Wave2Start = false;
    [SerializeField]
    bool map1Wave3Start = false;
    [SerializeField]
    bool map1Wave4Start = false;
    [SerializeField]
    bool map1Wave5Start = false;

    [SerializeField]
    bool map2Wave1Start = false;
    [SerializeField]
    bool map2Wave2Start = false;
    [SerializeField]
    bool map2Wave3Start = false;
    [SerializeField]
    bool map2Wave4Start = false;
    [SerializeField]
    bool map2Wave5Start = false;

    [SerializeField]
    bool viDayOn = false;
    [SerializeField]
    bool viDayOff = false;

    WavePoint wavePoint;

    bool waveState;
    bool hostMapWave;
    Vector3 wavePos;

    public Sprite[] spawnerSprite;

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

        if (map1Wave1Start)
        {
            WavePointSet(1, true);
            map1Wave1Start = false;
        }
        if (map1Wave2Start)
        {
            WavePointSet(2, true);
            map1Wave2Start = false;
        }
        if (map1Wave3Start)
        {
            WavePointSet(3, true);
            map1Wave3Start = false;
        }
        if (map1Wave4Start)
        {
            WavePointSet(4, true);
            map1Wave4Start = false;
        }
        if (map1Wave5Start)
        {
            WavePointSet(5, true);
            map1Wave5Start = false;
        }
        if (map2Wave1Start)
        {
            WavePointSet(1, false);
            map2Wave1Start = false;
        }
        if (map2Wave2Start)
        {
            WavePointSet(2, false);
            map2Wave2Start = false;
        }
        if (map2Wave3Start)
        {
            WavePointSet(3, false);
            map2Wave3Start = false;
        }
        if (map2Wave4Start)
        {
            WavePointSet(4, false);
            map2Wave4Start = false;
        }
        if (map2Wave5Start)
        {
            WavePointSet(5, false);
            map2Wave5Start = false;
        }

        if (viDayOn)
        {
            ViolentDayOn();
            viDayOn = false;
        }
        if (viDayOff)
        {
            ViolentDayOff();
            viDayOff = false;
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
        {
            monsterSpawners.Add(key, new List<MonsterSpawner>());
        }

        monsterSpawners[key].Add(spawner);
    }

    public void AreaGroupRemove(MonsterSpawner spawner, int groupNum, bool isInHostMap)
    {
        (int map, int area) key = (isInHostMap ? 1 : 2, groupNum);

        monsterSpawners[key].Remove(spawner);

        if (monsterSpawners[key].Count == 0)
            monsterSpawners.Remove(key);
    }

    [QFSW.QC.Command()]
    public void WavePointSet(int waveLevel, bool isInHostMap)
    {
        MonsterSpawner waveSpawner = null;

        (int map, int area) key = (isInHostMap ? 1 : 2, waveLevel);

        waveSpawner = monsterSpawners[key][Random.Range(0, monsterSpawners[key].Count)];
        if (waveSpawner == null)
        {
            return;
        }

        if (waveSpawner != null)
        {
            SpawnerGroupManager spawnerGroup = waveSpawner.groupManager;
            if (FindMatrix(spawnerGroup, isInHostMap))
            {
                Vector3 waveMainPos;
                Debug.Log(xIndex + " : " + yIndex);

                if (isInHostMap)
                    waveMainPos = spawnerMap1Matrix[xIndex, yIndex].GetComponent<SpawnerGroupManager>().spawnerList[0].transform.position;
                else
                    waveMainPos = spawnerMap2Matrix[xIndex, yIndex].GetComponent<SpawnerGroupManager>().spawnerList[0].transform.position;

                Vector3 waveTrPos = WavePointSetPos(waveMainPos, isInHostMap);
                for (int i = xIndex - 1; i <= xIndex + 1; i++)
                {
                    for (int j = yIndex - 1; j <= yIndex + 1; j++)
                    {
                        if (i == 5 && j == 5)
                        {
                            continue;
                        }

                        if(isInHostMap)
                        {
                            if (i >= 0 && i < spawnerMap1Matrix.GetLength(0) && j >= 0 && j < spawnerMap1Matrix.GetLength(1))
                            {
                                if (spawnerMap1Matrix[i, j] != null && spawnerMap1Matrix[i, j].TryGetComponent(out SpawnerGroupManager group))
                                {
                                    group.WaveSet(waveTrPos);
                                }
                            }
                        }
                        else
                        {
                            if (i >= 0 && i < spawnerMap2Matrix.GetLength(0) && j >= 0 && j < spawnerMap2Matrix.GetLength(1))
                            {
                                if (spawnerMap2Matrix[i, j] != null && spawnerMap2Matrix[i, j].TryGetComponent(out SpawnerGroupManager group))
                                {
                                    group.WaveSet(waveTrPos);
                                }
                            }
                        }
                    }
                }
                WaveStart(waveTrPos, isInHostMap);
            }
        }
    }

    void WaveStart(Vector3 waveTrPos, bool isInHostMap)
    {
        Debug.Log("waveStart : " + waveTrPos);
        waveState = true;
        wavePos = waveTrPos;
        wavePoint.WaveStart(waveTrPos, isInHostMap);

        GlobalWaveSet(true);

        if (IsServer)
            BattleBGMCtrl.instance.WaveStart(isInHostMap);
        WaveStateSet(isInHostMap);
    }

    public void WaveStateSet(bool isInHostMap)
    {
        hostMapWave = isInHostMap;
        WarningWindowSetServerRpc();
    }


    [ServerRpc(RequireOwnership = false)]
    void WarningWindowSetServerRpc()
    {
        WarningWindowSetClientRpc();
    }

    [ClientRpc]
    void WarningWindowSetClientRpc()
    {
        WarningWindow.instance.WarningTextSet("Wave detected on", hostMapWave);
    }


    public void WaveEnd()
    {
        GlobalWaveSet(false);
    }

    void GlobalWaveSet(bool state)
    {
        foreach (var data in monsterSpawners)
        {
            foreach (MonsterSpawner spawner in data.Value)
            {
                spawner.GlobalWaveState(state);
            }            
        }
    }

    bool FindMatrix(SpawnerGroupManager spawnerGroup, bool isInHostMap)
    {
        bool find = false;
        xIndex = 0;
        yIndex = 0;

        for (int i = 0; i < splitCount; i++)
        {
            for (int j = 0; j < splitCount; j++)
            {
                if(i == 5 && j == 5)
                {
                    continue;
                }

                if(isInHostMap)
                {
                    if (spawnerMap1Matrix[i, j] != null && spawnerMap1Matrix[i, j].TryGetComponent(out SpawnerGroupManager group) && spawnerGroup == group)
                    {
                        xIndex = i;
                        yIndex = j;
                        find = true;
                        return find;
                    }
                }
                else
                {
                    if (spawnerMap2Matrix[i, j] != null && spawnerMap2Matrix[i, j].TryGetComponent(out SpawnerGroupManager group) && spawnerGroup == group)
                    {
                        xIndex = i;
                        yIndex = j;
                        find = true;
                        return find;
                    }
                }
            }
        }
        return find;
    }

    Vector3 WavePointSetPos(Vector3 wavePoint, bool isHostMap)
    {
        Vector3 mapCenter;
        if(isHostMap)
            mapCenter = GameManager.instance.hostPlayerSpawnPos;
        else
            mapCenter = GameManager.instance.clientPlayerSpawnPos;

        float mapHalfSize = (float)MapGenerator.instance.width / 2;
        Vector3 mapCornerA = new Vector3(mapCenter.x - mapHalfSize, mapCenter.y - mapHalfSize, 0); // 좌측 하단
        Vector3 mapCornerB = new Vector3(mapCenter.x + mapHalfSize, mapCenter.y - mapHalfSize, 0); // 우측 하단
        Vector3 mapCornerC = new Vector3(mapCenter.x + mapHalfSize, mapCenter.y + mapHalfSize, 0); // 우측 상단
        Vector3 mapCornerD = new Vector3(mapCenter.x - mapHalfSize, mapCenter.y + mapHalfSize, 0); // 좌측 상단

        Vector3 intersectionPointA = CalculateIntersection(mapCenter, wavePoint, mapCornerA, mapCornerB);
        Vector3 intersectionPointB = CalculateIntersection(mapCenter, wavePoint, mapCornerB, mapCornerC);
        Vector3 intersectionPointC = CalculateIntersection(mapCenter, wavePoint, mapCornerC, mapCornerD); 
        Vector3 intersectionPointD = CalculateIntersection(mapCenter, wavePoint, mapCornerD, mapCornerA);

        Vector3 closestPoint = GetClosestPoint(wavePoint, new Vector3[] { intersectionPointA, intersectionPointB, intersectionPointC, intersectionPointD });

        float xPointSet = 0;
        float yPointSet = 0;

        if(closestPoint.x >= mapCenter.x + mapHalfSize - 30)
        {
            xPointSet = closestPoint.x - 30;
        }
        else if(closestPoint.x <= mapCenter.x - mapHalfSize + 30)
        {
            xPointSet = closestPoint.x + 30;
        }
        else
            xPointSet = closestPoint.x;

        if (closestPoint.y >= mapCenter.y + mapHalfSize - 30)
        {
            yPointSet = closestPoint.y - 30;
        }
        else if (closestPoint.y <= mapCenter.y - mapHalfSize + 30)
        {
            yPointSet = closestPoint.y + 30;
        }
        else
            yPointSet = closestPoint.y;

        Vector3 correctionPoint = new Vector3(xPointSet, yPointSet , 0);

        return correctionPoint;
    }

    Vector3 CalculateIntersection(Vector3 line1Start, Vector3 line1End, Vector3 line2Start, Vector3 line2End)
    {
        float m1 = (line1End.y - line1Start.y) / (line1End.x - line1Start.x);
        float m2 = (line2End.y - line2Start.y) / (line2End.x - line2Start.x);

        // 두 선분이 수직이거나 거의 수직인 경우
        if (Mathf.Approximately(line1End.x, line1Start.x))
        {
            float x = line1Start.x;
            float y = m2 * (x - line2Start.x) + line2Start.y;
            return new Vector3(x, y, 0);
        }
        else if (Mathf.Approximately(line2End.x, line2Start.x))
        {
            float x = line2Start.x;
            float y = m1 * (x - line1Start.x) + line1Start.y;
            return new Vector3(x, y, 0);
        }

        float b1 = line1Start.y - m1 * line1Start.x;
        float b2 = line2Start.y - m2 * line2Start.x;

        float x1 = (b2 - b1) / (m1 - m2);
        float y1 = m1 * x1 + b1;

        return new Vector3(x1, y1, 0);
    }

    Vector3 GetClosestPoint(Vector3 origin, Vector3[] points)
    {
        // 네 개의 교차점 중 맵의 웨이브 포인트에 가장 가까운 점을 반환
        float minDistance = Mathf.Infinity;
        Vector3 closestPoint = origin;

        foreach (Vector3 point in points)
        {
            if (point.x < 0 || point.y < 0)
                continue;

            float distance = Vector3.Distance(origin, point);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPoint = point;
            }
        }

        return closestPoint;
    }

    public void WaveStateLoad(SpawnerManagerSaveData data)
    {
        waveState = data.waveState;
        hostMapWave = data.hostMapWave;
        wavePos = Vector3Extensions.ToVector3(data.wavePos);

        if (waveState)
        {
            wavePoint.LoadWaveStart(wavePos, hostMapWave);
            if (IsServer)
                BattleBGMCtrl.instance.WaveStart(hostMapWave);
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

    public void ViolentDayOn()
    {
        foreach (var data in monsterSpawners)
        {
            foreach (MonsterSpawner spawner in data.Value)
            {
                spawner.SearchCollExtend();
            }
        }
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


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (!IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        }
    }

    void OnClientConnectedCallback(ulong clientId)
    {
        SetCorruption();
        ClientConnWaveServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void ClientConnWaveServerRpc()
    {
        if (waveState)
        {
            ClientConnWaveClientRpc(wavePos, hostMapWave);
            BattleBGMCtrl.instance.WaveStart(hostMapWave);
        }
    }

    [ClientRpc]
    void ClientConnWaveClientRpc(Vector3 waveTrPos, bool isInHostMap)
    {
        if (!IsServer)
        {
            Debug.Log("waveStart : " + waveTrPos);
            wavePoint.WaveStart(waveTrPos, isInHostMap);
            WarningWindowSetClientRpc();
        }
    }
}
