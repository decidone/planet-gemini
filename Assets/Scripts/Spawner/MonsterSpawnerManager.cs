using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using QFSW.QC;

public class MonsterSpawnerManager : NetworkBehaviour
{
    public List<MonsterSpawner> map1Area1Group = new List<MonsterSpawner>();
    public List<MonsterSpawner> map1Area2Group = new List<MonsterSpawner>();
    public List<MonsterSpawner> map1Area3Group = new List<MonsterSpawner>();
    public List<MonsterSpawner> map1Area4Group = new List<MonsterSpawner>();
    public List<MonsterSpawner> map1Area5Group = new List<MonsterSpawner>();

    public List<MonsterSpawner> map2Area1Group = new List<MonsterSpawner>();
    public List<MonsterSpawner> map2Area2Group = new List<MonsterSpawner>();
    public List<MonsterSpawner> map2Area3Group = new List<MonsterSpawner>();
    public List<MonsterSpawner> map2Area4Group = new List<MonsterSpawner>();
    public List<MonsterSpawner> map2Area5Group = new List<MonsterSpawner>();

    public GameObject[,] spawnerMap1Matrix;
    public GameObject[,] spawnerMap2Matrix;
    SpawnerSetManager spawnerSetManager;
    int splitCount;
    int xIndex;
    int yIndex;

    bool isInHostMap;

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

    WavePoint wavePoint;
    bool hostMapWave;
    bool clientMapWave;

    public Sprite[] spawnerSprite;

    #region Singleton
    public static MonsterSpawnerManager instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of recipeList found!");
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
    }

    public void MatrixSet(int spCount, GameObject[,] map1Matrix, GameObject[,] map2Matrix, bool isHostMap)
    {
        splitCount = spCount;
        Debug.Log(splitCount);
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
        isInHostMap = isHostMap;
        switch (groupNum)
        {
            case 1 :
                if(isInHostMap)
                    map1Area1Group.Add(spawner);
                else
                    map2Area1Group.Add(spawner);
                break;
            case 2 :
                if (isInHostMap)
                    map1Area2Group.Add(spawner);
                else
                    map2Area2Group.Add(spawner);
                break;
            case 3 :
                if (isInHostMap)
                    map1Area3Group.Add(spawner);
                else
                    map2Area3Group.Add(spawner);
                break;
            case 4 :
                if (isInHostMap)
                    map1Area4Group.Add(spawner);
                else
                    map2Area4Group.Add(spawner);
                break;
            case 5 :
                if (isInHostMap)
                    map1Area5Group.Add(spawner);
                else
                    map2Area5Group.Add(spawner);
                break;
            default :
                break;
        }
    }

    public void AreaGroupRemove(MonsterSpawner spawner, int groupNum, bool isInHostMap)
    {
        switch (groupNum)
        {
            case 1:
                if(isInHostMap && map1Area1Group.Contains(spawner))
                    map1Area1Group.Remove(spawner);
                else if (!isInHostMap && map2Area1Group.Contains(spawner))
                    map2Area1Group.Remove(spawner);        
                break;
            case 2:
                if (isInHostMap && map1Area2Group.Contains(spawner))
                    map1Area2Group.Remove(spawner);
                else if (!isInHostMap && map2Area2Group.Contains(spawner))
                    map2Area2Group.Remove(spawner);
                break;
            case 3:
                if (isInHostMap && map1Area3Group.Contains(spawner))
                    map1Area3Group.Remove(spawner);
                else if (!isInHostMap && map2Area3Group.Contains(spawner))
                    map2Area3Group.Remove(spawner);
                break;
            case 4:
                if (isInHostMap && map1Area4Group.Contains(spawner))
                    map1Area4Group.Remove(spawner);
                else if (!isInHostMap && map2Area4Group.Contains(spawner))
                    map2Area4Group.Remove(spawner);
                break;
            case 5:
                if (isInHostMap && map1Area5Group.Contains(spawner))
                    map1Area5Group.Remove(spawner);
                else if (!isInHostMap && map2Area5Group.Contains(spawner))
                    map2Area5Group.Remove(spawner);
                break;
            default:
                break;
        }
    }

    [QFSW.QC.Command()]
    private void WavePointSet(int waveLevel, bool isInHostMap)
    {
        MonsterSpawner waveSpawner = null; 
        switch (waveLevel)
        {
            case 1:
                if (isInHostMap)
                    waveSpawner = map1Area1Group[Random.Range(0, map1Area1Group.Count)];
                else
                    waveSpawner = map2Area1Group[Random.Range(0, map2Area1Group.Count)];
                break;
            case 2:
                if (isInHostMap)
                    waveSpawner = map1Area2Group[Random.Range(0, map1Area2Group.Count)];
                else
                    waveSpawner = map2Area2Group[Random.Range(0, map2Area2Group.Count)];
                break;
            case 3:
                if (isInHostMap)
                    waveSpawner = map1Area3Group[Random.Range(0, map1Area3Group.Count)];
                else
                    waveSpawner = map2Area3Group[Random.Range(0, map2Area3Group.Count)];
                break;
            case 4:
                if (isInHostMap)
                    waveSpawner = map1Area4Group[Random.Range(0, map1Area4Group.Count)];
                else
                    waveSpawner = map2Area4Group[Random.Range(0, map2Area4Group.Count)];
                break;
            case 5:
                if (isInHostMap)
                    waveSpawner = map1Area5Group[Random.Range(0, map1Area5Group.Count)];
                else
                    waveSpawner = map2Area5Group[Random.Range(0, map2Area5Group.Count)];
                break;
            default:
                break;
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
        wavePoint.WaveStart(waveTrPos, isInHostMap);

        GlobalWaveSet(true);

        if (IsServer)
            BattleBGMCtrl.instance.WaveStart(isInHostMap);
        WaveStateSet(isInHostMap, true);
    }

    public void WaveStateSet(bool isInHostMap, bool waveState)
    {
        if (isInHostMap)
            hostMapWave = waveState;
        else
            clientMapWave = waveState;

        WarningWindow.instance.WarningTextSet("Wave detected on", isInHostMap);
    }

    public void WaveEnd()
    {
        GlobalWaveSet(false);
    }

    void GlobalWaveSet(bool state)
    {
        foreach (MonsterSpawner spawner in map1Area1Group)
        {
            spawner.GlobalWaveState(state);
        }
        foreach (MonsterSpawner spawner in map1Area2Group)
        {
            spawner.GlobalWaveState(state);
        }
        foreach (MonsterSpawner spawner in map1Area3Group)
        {
            spawner.GlobalWaveState(state);
        }
        foreach (MonsterSpawner spawner in map1Area4Group)
        {
            spawner.GlobalWaveState(state);
        }
        foreach (MonsterSpawner spawner in map1Area5Group)
        {
            spawner.GlobalWaveState(state);
        }
        foreach (MonsterSpawner spawner in map2Area1Group)
        {
            spawner.GlobalWaveState(state);
        }
        foreach (MonsterSpawner spawner in map2Area2Group)
        {
            spawner.GlobalWaveState(state);
        }
        foreach (MonsterSpawner spawner in map2Area3Group)
        {
            spawner.GlobalWaveState(state);
        }
        foreach (MonsterSpawner spawner in map2Area4Group)
        {
            spawner.GlobalWaveState(state);
        }
        foreach (MonsterSpawner spawner in map2Area5Group)
        {
            spawner.GlobalWaveState(state);
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

        data.spawnerMap1Matrix = group1Data;
        data.spawnerMap2Matrix = group2Data;

        return data;
    }
}
