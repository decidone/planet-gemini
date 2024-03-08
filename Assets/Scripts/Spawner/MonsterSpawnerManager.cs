using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using QFSW.QC;

public class MonsterSpawnerManager : NetworkBehaviour
{
    public GameObject[] weakMonsters;
    public GameObject[] normalMonsters;
    public GameObject[] strongMonsters;
    public GameObject guardian;

    public List<MonsterSpawner> area1Group = new List<MonsterSpawner>();
    public List<MonsterSpawner> area2Group = new List<MonsterSpawner>();
    public List<MonsterSpawner> area3Group = new List<MonsterSpawner>();
    public List<MonsterSpawner> area4Group = new List<MonsterSpawner>();
    public List<MonsterSpawner> area5Group = new List<MonsterSpawner>();

    public GameObject[,] spawnerMatrix;
    SpawnerSetManager spawnerSetManager;
    int splitCount;
    int xIndex;
    int yIndex;

    [SerializeField]
    bool wave1Start = false;
    [SerializeField]
    bool wave2Start = false;
    [SerializeField]
    bool wave3Start = false;
    [SerializeField]
    bool wave4Start = false;
    [SerializeField]
    bool wave5Start = false;

    WavePoint wavePoint;

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
        spawnerMatrix = spawnerSetManager.spawnerMatrix;
        splitCount = spawnerSetManager.splitCount;
    }

    private void Update()
    {
        if (wave1Start)
        {
            WavePointSet(1);
            wave1Start = false;
        }
        if (wave2Start)
        {
            WavePointSet(2);
            wave2Start = false;
        }
        if (wave3Start)
        {
            WavePointSet(3);
            wave3Start = false;
        }
        if (wave4Start)
        {
            WavePointSet(4);
            wave4Start = false;
        }
        if (wave5Start)
        {
            WavePointSet(5);
            wave5Start = false;
        }
    }

    public void AreaGroupSet(MonsterSpawner spawner, int groupNum)
    {
        switch (groupNum)
        {
            case 1 :
                area1Group.Add(spawner);
                break;
            case 2 :
                area2Group.Add(spawner);
                break;
            case 3 :
                area3Group.Add(spawner);
                break;
            case 4 :
                area4Group.Add(spawner);
                break;
            case 5 :
                area5Group.Add(spawner);
                break;
            default :
                break;
        }
    }

    public void AreaGroupRemove(MonsterSpawner spawner, int groupNum)
    {
        switch (groupNum)
        {
            case 1:
                if(area1Group.Contains(spawner))
                    area1Group.Remove(spawner);
                break;
            case 2:
                if (area2Group.Contains(spawner))
                    area2Group.Remove(spawner);
                break;
            case 3:
                if (area3Group.Contains(spawner))
                    area3Group.Remove(spawner);
                break;
            case 4:
                if (area4Group.Contains(spawner))
                    area4Group.Remove(spawner);
                break;
            case 5:
                if (area5Group.Contains(spawner))
                    area5Group.Remove(spawner);
                break;
            default:
                break;
        }
    }

    [QFSW.QC.Command()]
    private void WavePointSet(int waveLevel)
    {
        MonsterSpawner waveSpawner = null; 
        switch (waveLevel)
        {
            case 1:
                waveSpawner = area1Group[Random.Range(0, area1Group.Count)];
                break;
            case 2:
                waveSpawner = area2Group[Random.Range(0, area2Group.Count)];
                break;
            case 3:
                waveSpawner = area3Group[Random.Range(0, area3Group.Count)];
                break;
            case 4:
                waveSpawner = area4Group[Random.Range(0, area4Group.Count)];
                break;
            case 5:
                waveSpawner = area5Group[Random.Range(0, area5Group.Count)];
                break;
            default:
                break;
        }

        if (waveSpawner != null)
        {
            SpawnerGroupManager spawnerGroup = waveSpawner.groupManager;
            if (FindMatrix(spawnerGroup))
            {
                Vector3 waveMainPos = spawnerMatrix[xIndex, yIndex].GetComponent<SpawnerGroupManager>().spawnerList[0].transform.position;
                Vector3 waveTrPos = WavePointSet(waveMainPos);
                for (int i = xIndex - 1; i <= xIndex + 1; i++)
                {
                    for (int j = yIndex - 1; j <= yIndex + 1; j++)
                    {
                        if (i == 5 && j == 5)
                        {
                            continue;
                        }

                        // 배열의 경계를 벗어나지 않는지 확인합니다.
                        if (i >= 0 && i < spawnerMatrix.GetLength(0) && j >= 0 && j < spawnerMatrix.GetLength(1))
                        {
                            if (spawnerMatrix[i, j] != null && spawnerMatrix[i, j].TryGetComponent(out SpawnerGroupManager group))
                            {
                                group.WaveSet(waveTrPos);
                            }
                        }
                    }
                }
                Debug.Log("waveStart : " + waveTrPos + " : " + waveMainPos);
                WaveStartClientRpc(waveTrPos);
            }
        }
    }

    [ClientRpc]
    void WaveStartClientRpc(Vector3 waveTrPos)
    {
        wavePoint.WaveStart(waveTrPos);
        BattleBGMCtrl.instance.WaveStart();
    }

    bool FindMatrix(SpawnerGroupManager spawnerGroup)
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

                if (spawnerMatrix[i, j] != null && spawnerMatrix[i, j].TryGetComponent(out SpawnerGroupManager group) && spawnerGroup == group)
                {
                    xIndex = i;
                    yIndex = j;
                    find = true;
                    return find;
                }
            }
        }
        return find;
    }

    Vector3 WavePointSet(Vector3 wavePoint)
    {
        Vector3 mapCenter = GameManager.instance.playerSpawnPos;

        Vector3 mapCornerA = new Vector3(0, 0, 0); // 좌측 하단
        Vector3 mapCornerB = new Vector3(spawnerSetManager.width, 0, 0); // 우측 하단
        Vector3 mapCornerC = new Vector3(spawnerSetManager.width, spawnerSetManager.height, 0); // 우측 상단
        Vector3 mapCornerD = new Vector3(0, spawnerSetManager.height, 0); // 좌측 상단

        Vector3 intersectionPointA = CalculateIntersection(mapCenter, wavePoint, mapCornerA, mapCornerB);
        Vector3 intersectionPointB = CalculateIntersection(mapCenter, wavePoint, mapCornerB, mapCornerC);
        Vector3 intersectionPointC = CalculateIntersection(mapCenter, wavePoint, mapCornerC, mapCornerD);
        Vector3 intersectionPointD = CalculateIntersection(mapCenter, wavePoint, mapCornerD, mapCornerA);

        Vector3 closestPoint = GetClosestPoint(wavePoint, new Vector3[] { intersectionPointA, intersectionPointB, intersectionPointC, intersectionPointD });

        float xPointSet = 0;
        float yPointSet = 0;

        if (closestPoint.x <= 30)
        {
            xPointSet = 30;
        }
        else if (closestPoint.x >= spawnerSetManager.width - 30)
        {
            xPointSet = spawnerSetManager.width - 30;
        }
        else
            xPointSet = closestPoint.x;

        if (closestPoint.y <= 30)
        {
            yPointSet = 30;
        }
        else if (closestPoint.y >= spawnerSetManager.height - 30)
        {
            yPointSet = spawnerSetManager.height - 30;
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
}
