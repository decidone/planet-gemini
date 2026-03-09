using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Unity.Netcode;

public class SpawnerSetManager : NetworkBehaviour
{
    [HideInInspector]
    public int width;
    [HideInInspector]
    public int height;

    float areaWSize;
    float areaHSize;

    [Header("홀수로 지정")]
    public int splitCount;

    Dictionary<Vector2, int> areaPosLevel = new Dictionary<Vector2, int>();
    [SerializeField]
    GameObject spawner;
    [SerializeField]
    GameObject spawnerGroup;

    public AreaLevelData[] arealevelData;
    int[] basicSpawnerCount = new int[5] { 16, 24, 32, 40, 48 }; //  구역별 기본 스포너 개수
    int[] spawnCount;
    int[] subSpawnerCount;
    int[] upgradeSpawnerSetCount;
    Vector3 basePos;

    [SerializeField]
    MonsterSpawnerManager monsterSpawnerManager;

    public GameObject[,] spawnerMap1Matrix;
    public GameObject[,] spawnerMap2Matrix;

    Map hostMap;
    Map clientMap;

    #region Singleton
    public static SpawnerSetManager instance;

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

    public void AreaMapSet(Vector2 centerPos, int mapSplitCount, bool isHostMap)
    {
        hostMap = GameManager.instance.hostMap;
        clientMap = GameManager.instance.clientMap; 
        width = hostMap.width;
        height = hostMap.height;
        splitCount = mapSplitCount;
        spawnerMap1Matrix = new GameObject[splitCount, splitCount];
        spawnerMap2Matrix = new GameObject[splitCount, splitCount];

        areaWSize = width / splitCount;
        areaHSize = height / splitCount;
        areaPosLevel.Clear();
        float centerX = centerPos.x; // 맵의 중앙 x 좌표
        float centerY = centerPos.y; // 맵의 중앙 y 좌표

        int centerNum = Mathf.FloorToInt(splitCount / 2);

        for (int i = 0; i < splitCount; i++)
        {
            for (int j = 0; j < splitCount; j++)
            {
                float areaCenterX = centerX + (i - ((float)splitCount / 2)) * areaWSize + areaWSize / 2;
                float areaCenterY = centerY + (j - ((float)splitCount / 2)) * areaHSize + areaHSize / 2;

                Vector2 areaCenter = new Vector2(areaCenterX, areaCenterY);  // 구역의 중앙 좌표

                int x = Math.Abs(centerNum - i);
                int y = Math.Abs(centerNum - j);

                if (x == 0 && y == 0)
                {
                    basePos = areaCenter;
                }
                else if (Math.Max(x, y) > 1)
                {
                    areaPosLevel.Add(areaCenter, Math.Max(x, y) - 1);    // 구역의 중앙 좌표 + 구역 레벨
                }
            }
        }

        SpawnerSet(isHostMap);
    }

    void SpawnerSet(bool isHostMap)
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        Map map = isHostMap ? hostMap : clientMap;

        int xIndex = 0;
        int yIndex = 0;

        MapSizeData mapSizeData = MapGenerator.instance.mapSizeData;
        subSpawnerCount = new int[mapSizeData.CountOfSpawnersByLevel.Length];
        spawnCount = (int[])basicSpawnerCount.Clone();
        upgradeSpawnerSetCount = (int[])mapSizeData.UpgradeSpawnerSet.Clone();

        for (int i = 0; i < subSpawnerCount.Length; i++)
            subSpawnerCount[i] = basicSpawnerCount[i] - mapSizeData.CountOfSpawnersByLevel[i];

        // 배치할 구역 미리 선택
        HashSet<Vector2> selectedAreas = PreSelectAreas(mapSizeData);

        List<Vector2> placedSpawnerPos = new List<Vector2>();
        float minSpawnerDistance = Mathf.Min(areaWSize, areaHSize) * 0.4f;

        foreach (var data in areaPosLevel)
        {
            // 선택되지 않은 구역 스킵
            if (!selectedAreas.Contains(data.Key))
            {
                xIndex++;
                if (xIndex >= splitCount) { xIndex = 0; yIndex++; }
                continue;
            }

            Vector2 centerPos = data.Key;
            int areaLevel = data.Value;

            AreaLevelData levelData;
            if ((areaLevel - 1) * 2 < arealevelData.Length)
                levelData = arealevelData[(areaLevel - 1) * 2];
            else
                levelData = arealevelData[arealevelData.Length - 1];

            float xRadius = areaWSize / 2 - 10f;
            float yRadius = areaHSize / 2 - 10f;

            // 사분면을 베이스에서 먼 순서로 정렬
            List<Vector2> quadrants = GetQuadrantsByDistance(centerPos, xRadius, yRadius);

            Vector2 newPoint = Vector2.zero;
            List<Vector2> cantPos = new List<Vector2>();
            float qxRadius = xRadius / 2f;
            float qyRadius = yRadius / 2f;

            bool placed = false;
            foreach (var quadCenter in quadrants)
            {
                int maxAttempts = 30;
                int attempts = 0;

                while (attempts < maxAttempts)
                {
                    attempts++;

                    int rx = (int)Random.Range(-qxRadius, qxRadius);
                    int ry = (int)Random.Range(-qyRadius, qyRadius);
                    newPoint = quadCenter + new Vector2(rx, ry);

                    if (cantPos.Contains(newPoint))
                        continue;

                    bool invalidBiome = false;

                    string biome = map.GetCellDataFromPos((int)newPoint.x, (int)newPoint.y).biome.biome;
                    if (biome == "lake" || biome == "cliff")
                        invalidBiome = true;

                    if (!invalidBiome)
                    {
                        for (int i = -2; i <= 2 && !invalidBiome; i += 4)
                            for (int j = -2; j <= 2 && !invalidBiome; j += 4)
                            {
                                biome = map.GetCellDataFromPos((int)newPoint.x + i, (int)newPoint.y + j).biome.biome;
                                if (biome == "lake" || biome == "cliff")
                                    invalidBiome = true;
                            }
                    }

                    if (invalidBiome)
                    {
                        cantPos.Add(newPoint);
                        continue;
                    }

                    bool tooClose = false;
                    foreach (var placedPos in placedSpawnerPos)
                    {
                        if (Vector2.Distance(newPoint, placedPos) < minSpawnerDistance)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    if (tooClose)
                    {
                        cantPos.Add(newPoint);
                        continue;
                    }

                    placed = true;
                    break;
                }

                if (placed) break;
            }

            if (!placed)
            {
                Debug.LogWarning($"스포너 배치 실패 - 적합한 위치 없음 / pos:{centerPos}");
                xIndex++;
                if (xIndex >= splitCount) { xIndex = 0; yIndex++; }
                continue;
            }

            placedSpawnerPos.Add(newPoint);

            GameObject spawnGroup = SpawnerGroupSet(centerPos);
            SpawnerGroupManager spawnerGroupManager = spawnGroup.GetComponent<SpawnerGroupManager>();

            GameObject spawnerObj = Instantiate(spawner);
            NetworkObject networkObject = spawnerObj.GetComponent<NetworkObject>();
            if (!networkObject.IsSpawned) networkObject.Spawn(true);

            spawnerObj.transform.position = newPoint;

            int levelSet = levelData.sppawnerLevel;
            AreaLevelData levelDataSet = levelData;

            if (upgradeSpawnerSetCount[areaLevel - 1] > 0)
            {
                int random = Random.Range(0, spawnCount[areaLevel - 1]);
                if (random < upgradeSpawnerSetCount[areaLevel - 1])
                {
                    upgradeSpawnerSetCount[areaLevel - 1] -= 1;
                    levelSet += 1;
                    levelDataSet = (levelSet - 1 < arealevelData.Length)
                        ? arealevelData[levelSet - 1]
                        : arealevelData[arealevelData.Length - 1];
                }
            }

            Cell cellData = map.GetCellDataFromPos((int)newPoint.x, (int)newPoint.y);
            if (cellData.obj != null)
                Destroy(cellData.obj);

            spawnerGroupManager.SpawnerSet(spawnerObj);
            spawnerObj.TryGetComponent(out MonsterSpawner monsterSpawner);
            monsterSpawner.groupManager = spawnerGroupManager;
            monsterSpawner.SpawnerSetting(levelDataSet, cellData.biome.biome, basePos, isHostMap, areaLevel);
            monsterSpawnerManager.AreaGroupSet(monsterSpawner, areaLevel, isHostMap);

            MapGenerator.instance.SetCorruption(map, monsterSpawner, levelSet);

            if (isHostMap)
                spawnerMap1Matrix[xIndex, yIndex] = spawnGroup;
            else
                spawnerMap2Matrix[xIndex, yIndex] = spawnGroup;

            spawnerGroupManager.SpawnerGroupStatsSet((xIndex, yIndex));

            xIndex++;
            if (xIndex >= splitCount) { xIndex = 0; yIndex++; }
        }

        monsterSpawnerManager.MatrixSet(splitCount, spawnerMap1Matrix, spawnerMap2Matrix, isHostMap);
        stopwatch.Stop();
        Debug.Log((isHostMap ? "host" : "client") + " spawnerSet Time : " + stopwatch.ElapsedMilliseconds.ToString() + "ms");
    }

    List<Vector2> GetQuadrantsByDistance(Vector2 areaCenter, float xRadius, float yRadius)
    {
        float qx = xRadius / 2f;
        float qy = yRadius / 2f;

        // 4분할 중심 좌표
        List<Vector2> quadrants = new List<Vector2>
    {
        areaCenter + new Vector2(-qx,  qy),  // 좌상
        areaCenter + new Vector2( qx,  qy),  // 우상
        areaCenter + new Vector2(-qx, -qy),  // 좌하
        areaCenter + new Vector2( qx, -qy),  // 우하
    };

        // 베이스에서 먼 순서로 정렬
        quadrants.Sort((a, b) =>
            Vector2.Distance(b, (Vector2)basePos)
            .CompareTo(Vector2.Distance(a, (Vector2)basePos)));

        return quadrants;
    }

    public GameObject SpawnerGroupSet(Vector2 pos)
    {
        GameObject spawnObj;

        spawnObj = Instantiate(spawnerGroup);
        NetworkObject networkObject = spawnObj.GetComponent<NetworkObject>();
        if (!networkObject.IsSpawned) networkObject.Spawn(true);

        spawnObj.transform.position = pos;
        spawnObj.transform.parent = gameObject.transform;

        return spawnObj;
    }

    HashSet<Vector2> PreSelectAreas(MapSizeData mapSizeData)
    {
        Dictionary<int, List<Vector2>> areasByLevel = new();
        foreach (var data in areaPosLevel)
        {
            if (!areasByLevel.ContainsKey(data.Value))
                areasByLevel[data.Value] = new List<Vector2>();
            areasByLevel[data.Value].Add(data.Key);
        }

        HashSet<Vector2> selected = new HashSet<Vector2>();

        foreach (var level in areasByLevel)
        {
            int levelIndex = Mathf.Clamp(level.Key - 1, 0, mapSizeData.CountOfSpawnersByLevel.Length - 1);
            int targetCount = mapSizeData.CountOfSpawnersByLevel[levelIndex];

            List<Vector2> allAreas = new List<Vector2>(level.Value);
            int total = allAreas.Count;

            if (targetCount >= total)
            {
                foreach (var pos in allAreas)
                    selected.Add(pos);
                continue;
            }

            // 2D 격자로 영역 분할
            // 가로 세로 격자 수 계산 (targetCount에 맞게)
            int gridW = Mathf.CeilToInt(Mathf.Sqrt(targetCount));
            int gridH = Mathf.CeilToInt((float)targetCount / gridW);

            // 전체 구역의 min/max 범위 계산
            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;
            foreach (var pos in allAreas)
            {
                if (pos.x < minX) minX = pos.x;
                if (pos.x > maxX) maxX = pos.x;
                if (pos.y < minY) minY = pos.y;
                if (pos.y > maxY) maxY = pos.y;
            }

            float cellW = (maxX - minX + areaWSize) / gridW;
            float cellH = (maxY - minY + areaHSize) / gridH;

            // 각 격자 셀에 구역 분류
            Dictionary<(int, int), List<Vector2>> grid = new();
            foreach (var pos in allAreas)
            {
                int gx = Mathf.Clamp(Mathf.FloorToInt((pos.x - minX) / cellW), 0, gridW - 1);
                int gy = Mathf.Clamp(Mathf.FloorToInt((pos.y - minY) / cellH), 0, gridH - 1);
                var key = (gx, gy);
                if (!grid.ContainsKey(key))
                    grid[key] = new List<Vector2>();
                grid[key].Add(pos);
            }

            // 각 셀에서 1개씩 랜덤 선택
            List<Vector2> candidates = new List<Vector2>();
            foreach (var cell in grid.Values)
            {
                if (cell.Count > 0)
                    candidates.Add(cell[Random.Range(0, cell.Count)]);
            }

            // targetCount보다 많으면 랜덤으로 줄이기
            while (candidates.Count > targetCount)
                candidates.RemoveAt(Random.Range(0, candidates.Count));

            // targetCount보다 적으면 미선택 구역에서 추가
            if (candidates.Count < targetCount)
            {
                HashSet<Vector2> candidateSet = new HashSet<Vector2>(candidates);
                List<Vector2> remaining = allAreas.FindAll(p => !candidateSet.Contains(p));

                // 셔플 후 부족한 만큼 추가
                for (int i = remaining.Count - 1; i > 0; i--)
                {
                    int j = Random.Range(0, i + 1);
                    (remaining[i], remaining[j]) = (remaining[j], remaining[i]);
                }

                int needed = targetCount - candidates.Count;
                for (int i = 0; i < needed && i < remaining.Count; i++)
                    candidates.Add(remaining[i]);
            }

            foreach (var pos in candidates)
                selected.Add(pos);
        }

        return selected;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (areaPosLevel == null || areaPosLevel.Count == 0) return;

        // 레벨별 색상
        Color[] levelColors = new Color[]
        {
        new Color(0f, 1f, 0f, 0.3f),   // 레벨 1 - 초록
        new Color(1f, 1f, 0f, 0.3f),   // 레벨 2 - 노랑
        new Color(1f, 0.5f, 0f, 0.3f), // 레벨 3 - 주황
        new Color(1f, 0f, 0f, 0.3f),   // 레벨 4 - 빨강
        new Color(0.5f, 0f, 1f, 0.3f), // 레벨 5 - 보라
        };

        foreach (var data in areaPosLevel)
        {
            Vector2 pos = data.Key;
            int level = data.Value;

            // 레벨에 맞는 색상 (범위 초과 방지)
            Color fillColor = levelColors[Mathf.Clamp(level - 1, 0, levelColors.Length - 1)];
            Color outlineColor = fillColor;
            outlineColor.a = 1f;

            // 구역 채우기
            Gizmos.color = fillColor;
            Gizmos.DrawCube(new Vector3(pos.x, pos.y, 0), new Vector3(areaWSize, areaHSize, 0));

            // 구역 외곽선
            Gizmos.color = outlineColor;
            Gizmos.DrawWireCube(new Vector3(pos.x, pos.y, 0), new Vector3(areaWSize, areaHSize, 0));

            // 레벨 텍스트 (에디터에서만)
            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(new Vector3(pos.x, pos.y, 0), $"Lv.{level}");
        }

        // 베이스(중앙) 표시
        if (basePos != Vector3.zero)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.5f);
            Gizmos.DrawCube(basePos, new Vector3(areaWSize, areaHSize, 0));
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(basePos, new Vector3(areaWSize, areaHSize, 0));
            UnityEditor.Handles.Label(basePos, "BASE");
        }
    }
#endif
}
