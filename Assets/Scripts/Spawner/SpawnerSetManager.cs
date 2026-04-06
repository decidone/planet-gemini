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

    // ────────────────────────────────────────────────
    //  공개 진입점: splitCount 홀짝에 따라 자동 분기
    // ────────────────────────────────────────────────
    public void AreaMapSet(Vector2 centerPos, int mapSplitCount, bool isHostMap)
    {
        if (mapSplitCount % 2 == 0)
            AreaMapSet_Even(centerPos, mapSplitCount, isHostMap);
        else
            AreaMapSet_Odd(centerPos, mapSplitCount, isHostMap);
    }

    // ────────────────────────────────────────────────
    //  홀수 splitCount  (중앙 1칸 = 안전, 2칸씩 레벨 상승)
    //  예) splitCount=15 → 중앙 3×3=9칸 안전지역
    // ────────────────────────────────────────────────
    void AreaMapSet_Odd(Vector2 centerPos, int mapSplitCount, bool isHostMap)
    {
        hostMap = GameManager.instance.hostMap;
        clientMap = GameManager.instance.clientMap;
        width = hostMap.width;
        height = hostMap.height;
        splitCount = mapSplitCount;
        basicSpawnerCount = new int[5] { 16, 24, 32, 40, 48 };

        spawnerMap1Matrix = new GameObject[splitCount, splitCount];
        spawnerMap2Matrix = new GameObject[splitCount, splitCount];

        areaWSize = width / splitCount;
        areaHSize = height / splitCount;
        areaPosLevel.Clear();

        float centerX = centerPos.x;
        float centerY = centerPos.y;
        int centerNum = Mathf.FloorToInt(splitCount / 2); // 중앙 인덱스

        for (int i = 0; i < splitCount; i++)
        {
            for (int j = 0; j < splitCount; j++)
            {
                float areaCenterX = centerX + (i - (float)splitCount / 2) * areaWSize + areaWSize / 2;
                float areaCenterY = centerY + (j - (float)splitCount / 2) * areaHSize + areaHSize / 2;
                Vector2 areaCenter = new Vector2(areaCenterX, areaCenterY);

                // 단일 중앙 셀로부터의 체비쇼프 거리
                int x = Math.Abs(centerNum - i);
                int y = Math.Abs(centerNum - j);

                if (x == 0 && y == 0)
                {
                    // 정중앙 → basePos 기록, 안전지역
                    basePos = new Vector3(areaCenterX, areaCenterY, 0);
                }
                else if (Math.Max(x, y) > 1 && Math.Max(x, y) < 6)
                {
                    // Max(x,y) 2 → Lv1, 3~4 → Lv2, 5~6 → Lv3 ...
                    //int level = Mathf.CeilToInt((Math.Max(x, y) - 1) / 2f);
                    //areaPosLevel.Add(areaCenter, level);

                    areaPosLevel.Add(areaCenter, Math.Max(x, y) - 1);
                }
                // Max(x,y) == 1 → 안전지역(3×3 테두리), 등록 안 함
            }
        }

        SpawnerSet(isHostMap);
    }

    // ────────────────────────────────────────────────
    //  짝수 splitCount  (중앙 2×2 밴드 = 안전, 2칸씩 레벨 상승)
    //  예) splitCount=16 → 중앙 4×4=16칸 안전지역
    // ────────────────────────────────────────────────
    void AreaMapSet_Even(Vector2 centerPos, int mapSplitCount, bool isHostMap)
    {
        hostMap = GameManager.instance.hostMap;
        clientMap = GameManager.instance.clientMap;
        width = hostMap.width;
        height = hostMap.height;
        splitCount = mapSplitCount;
        basicSpawnerCount = new int[5] { 20, 28, 36, 44, 52 };

        spawnerMap1Matrix = new GameObject[splitCount, splitCount];
        spawnerMap2Matrix = new GameObject[splitCount, splitCount];

        areaWSize = width / splitCount;
        areaHSize = height / splitCount;
        areaPosLevel.Clear();

        float centerX = centerPos.x;
        float centerY = centerPos.y;

        // 짝수: 밴드 인덱스 = [centerNum-1, centerNum]
        // 예) splitCount=16 → centerNum=8 → 밴드 [7, 8]
        int centerNum = splitCount / 2;

        // basePos = 맵의 기하학적 중심 (2×2 블록 사이)
        basePos = new Vector3(centerX, centerY, 0);

        for (int i = 0; i < splitCount; i++)
        {
            for (int j = 0; j < splitCount; j++)
            {
                float areaCenterX = centerX + (i - (float)splitCount / 2) * areaWSize + areaWSize / 2;
                float areaCenterY = centerY + (j - (float)splitCount / 2) * areaHSize + areaHSize / 2;
                Vector2 areaCenter = new Vector2(areaCenterX, areaCenterY);

                // 2×2 중앙 밴드 [centerNum-1, centerNum] 로부터의 거리
                //  밴드 안쪽(=0), 밴드 왼쪽 바깥(양수), 밴드 오른쪽 바깥(양수)
                int x = (i < centerNum - 1) ? (centerNum - 1 - i) :
                         (i > centerNum) ? (i - centerNum) : 0;
                int y = (j < centerNum - 1) ? (centerNum - 1 - j) :
                         (j > centerNum) ? (j - centerNum) : 0;

                if (Math.Max(x, y) > 1 && Math.Max(x, y) < 6)
                {
                    // Max(x,y) 2 → Lv1, 3~4 → Lv2, 5~6 → Lv3 ...
                    //int level = Mathf.CeilToInt((Math.Max(x, y) - 1) / 2f);
                    //areaPosLevel.Add(areaCenter, level);
                    areaPosLevel.Add(areaCenter, Math.Max(x, y) - 1);
                }
                // Max(x,y) == 0 → 중앙 2×2 (4칸)
                // Max(x,y) == 1 → 안전지역 테두리 (4×4 나머지 12칸)
                // → 둘 다 등록 안 함
            }
        }

        SpawnerSet(isHostMap);
    }

    // ────────────────────────────────────────────────
    //  이하 기존 로직 동일
    // ────────────────────────────────────────────────
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

        HashSet<Vector2> selectedAreas = PreSelectAreas(mapSizeData);
        List<Vector2> placedSpawnerPos = new List<Vector2>();
        float minSpawnerDistance = Mathf.Min(areaWSize, areaHSize) * 0.4f;

        Dictionary<int, List<Vector2>> selectedByLevel = new();
        foreach (var pos in selectedAreas)
        {
            int lv = areaPosLevel[pos];
            if (!selectedByLevel.ContainsKey(lv))
                selectedByLevel[lv] = new List<Vector2>();
            selectedByLevel[lv].Add(pos);
        }

        // 레벨별로 정확히 upgradeSpawnerSetCount[lv-1] 개만 강화 대상으로 추첨
        HashSet<Vector2> upgradeTargets = new HashSet<Vector2>();
        foreach (var kv in selectedByLevel)
        {
            int lv = kv.Key;
            int lvIndex = Mathf.Clamp(lv - 1, 0, upgradeSpawnerSetCount.Length - 1);
            int count = Mathf.Min(upgradeSpawnerSetCount[lvIndex], kv.Value.Count);

            // 셔플 후 앞에서 count개 추출
            List<Vector2> pool = new List<Vector2>(kv.Value);
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }
            for (int i = 0; i < count; i++)
                upgradeTargets.Add(pool[i]);
        }

        foreach (var data in areaPosLevel)
        {
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

            Vector2 newPoint = Vector2.zero;
            List<Vector2> cantPos = new List<Vector2>();

            bool placed = false;

            float localMinDistance = minSpawnerDistance;
            int maxAttempts = 60;
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                if (attempts > 0 && attempts % 5 == 0)
                {
                    localMinDistance *= 0.5f;
                }

                attempts++;

                int rx = (int)Random.Range(-xRadius, xRadius);
                int ry = (int)Random.Range(-yRadius, yRadius);
                newPoint = centerPos + new Vector2(rx, ry);

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
                    if (Vector2.Distance(newPoint, placedPos) < localMinDistance)
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

            if (!placed)
            {
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

            if (upgradeTargets.Contains(centerPos))
            {
                levelSet += 1;
                levelDataSet = (levelSet - 1 < arealevelData.Length)
                    ? arealevelData[levelSet - 1]
                    : arealevelData[arealevelData.Length - 1];
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

    public GameObject SpawnerGroupSet(Vector2 pos)
    {
        GameObject spawnObj = Instantiate(spawnerGroup);
        NetworkObject networkObject = spawnObj.GetComponent<NetworkObject>();
        if (!networkObject.IsSpawned) networkObject.Spawn(true);

        spawnObj.transform.position = pos;
        spawnObj.transform.parent = gameObject.transform;

        return spawnObj;
    }

    HashSet<Vector2> PreSelectAreas(MapSizeData mapSizeData)
    {
        // 레벨별로 구역 분류
        Dictionary<int, List<Vector2>> areasByLevel = new();
        foreach (var data in areaPosLevel)
        {
            if (!areasByLevel.ContainsKey(data.Value))
                areasByLevel[data.Value] = new List<Vector2>();
            areasByLevel[data.Value].Add(data.Key);
        }

        // 구역 크기 기반 최소 안전 거리
        // - 같은 레벨 내에서 구역 중심 간 거리
        // - areaWSize/areaHSize의 짧은 쪽 * 배율로 계산
        // - 배율을 높일수록 더 멀리 떨어짐 (너무 높으면 targetCount 충족 불가)
        float minSafeDistance = Mathf.Min(areaWSize, areaHSize) * 1.2f;

        HashSet<Vector2> selected = new HashSet<Vector2>();
        // 레벨 경계를 넘어서도 거리 체크할 전체 선택 목록
        List<Vector2> allSelected = new List<Vector2>();

        foreach (var level in areasByLevel)
        {
            int levelIndex = Mathf.Clamp(level.Key - 1, 0, mapSizeData.CountOfSpawnersByLevel.Length - 1);
            int targetCount = mapSizeData.CountOfSpawnersByLevel[levelIndex];
            List<Vector2> allAreas = new List<Vector2>(level.Value);

            // 후보가 targetCount 이하면 전부 선택
            if (targetCount >= allAreas.Count)
            {
                foreach (var pos in allAreas)
                {
                    selected.Add(pos);
                    allSelected.Add(pos);
                }
                continue;
            }

            // allSelected: 이전 레벨에서 선택된 위치도 거리 제약에 포함
            List<Vector2> result = GreedyFarthestPoint(allAreas, targetCount, minSafeDistance, allSelected);
            foreach (var pos in result)
            {
                selected.Add(pos);
                allSelected.Add(pos);
            }
        }

        return selected;
    }

    // ── Greedy Farthest Point Selection ──────────────────────────────────────
    // 이미 선택된 점들과의 최소 거리가 가장 먼 후보를 순서대로 선택
    // minSafeDistance: 이 거리 미만인 후보는 원천 제외
    // globalSelected:  다른 레벨 포함 이미 확정된 전체 위치 목록 (거리 초기값에 반영)
    List<Vector2> GreedyFarthestPoint(
        List<Vector2> candidates,
        int targetCount,
        float minSafeDistance,
        List<Vector2> globalSelected)
    {
        // ── 1. 후보 셔플 (시작점 랜덤화) ─────────────────────
        List<Vector2> pool = new List<Vector2>(candidates);
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        // ── 2. globalSelected 기준으로 minSafeDistance 위반 후보 선제 제거 ──
        if (globalSelected.Count > 0)
        {
            pool.RemoveAll(p =>
            {
                foreach (var g in globalSelected)
                    if (Vector2.Distance(p, g) < minSafeDistance) return true;
                return false;
            });
        }

        // 제거 후 후보가 targetCount보다 적으면 거리 조건 완화해서 재시도
        // (minSafeDistance * 0.5 까지 단계적으로 낮춤)
        if (pool.Count < targetCount && globalSelected.Count > 0)
        {
            float relaxed = minSafeDistance * 0.7f;
            pool = new List<Vector2>(candidates);
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }
            pool.RemoveAll(p =>
            {
                foreach (var g in globalSelected)
                    if (Vector2.Distance(p, g) < relaxed) return true;
                return false;
            });

            if (pool.Count < targetCount)
            {
                // 완화해도 부족하면 조건 없이 전체 후보 사용
                pool = new List<Vector2>(candidates);
                for (int i = pool.Count - 1; i > 0; i--)
                {
                    int j = Random.Range(0, i + 1);
                    (pool[i], pool[j]) = (pool[j], pool[i]);
                }
                Debug.LogWarning($"[PreSelectAreas] 안전거리 조건 충족 불가 → 조건 해제 (level pool:{pool.Count}, target:{targetCount})");
            }
        }

        List<Vector2> result = new List<Vector2>();
        if (pool.Count == 0) return result;

        // ── 3. 시작점 선택 ───────────────────────────────────
        // globalSelected가 있으면 그것들로부터 가장 먼 후보를 시작점으로
        // 없으면 pool[0] (셔플된 랜덤)
        if (globalSelected.Count > 0)
        {
            int startIdx = 0;
            float startDist = float.MinValue;
            for (int i = 0; i < pool.Count; i++)
            {
                float minD = float.MaxValue;
                foreach (var g in globalSelected)
                {
                    float d = Vector2.Distance(pool[i], g);
                    if (d < minD) minD = d;
                }
                if (minD > startDist) { startDist = minD; startIdx = i; }
            }
            result.Add(pool[startIdx]);
            pool.RemoveAt(startIdx);
        }
        else
        {
            result.Add(pool[0]);
            pool.RemoveAt(0);
        }

        // ── 4. 각 후보의 minDist 초기화 ─────────────────────
        // result[0] 과의 거리로 초기화한 뒤 globalSelected 와도 비교
        float[] minDist = new float[pool.Count];
        for (int i = 0; i < pool.Count; i++)
        {
            minDist[i] = Vector2.Distance(pool[i], result[0]);
            foreach (var g in globalSelected)
            {
                float d = Vector2.Distance(pool[i], g);
                if (d < minDist[i]) minDist[i] = d;
            }
        }

        // ── 5. Greedy 선택 루프 ──────────────────────────────
        while (result.Count < targetCount && pool.Count > 0)
        {
            // minDist 가장 큰 후보 탐색
            int bestIdx = 0;
            float bestDist = minDist[0];
            for (int i = 1; i < pool.Count; i++)
            {
                if (minDist[i] > bestDist)
                {
                    bestDist = minDist[i];
                    bestIdx = i;
                }
            }

            // minSafeDistance 위반 시 경고 (완화 단계를 이미 거쳤으므로 로그만)
            if (bestDist < minSafeDistance)
                Debug.LogWarning($"[GreedyFarthest] 최소 안전거리 미달: {bestDist:F1} < {minSafeDistance:F1}");

            Vector2 newPoint = pool[bestIdx];
            result.Add(newPoint);

            // 선택된 후보 pool / minDist 에서 제거
            pool.RemoveAt(bestIdx);
            float[] newMinDist = new float[pool.Count];
            for (int i = 0; i < bestIdx; i++) newMinDist[i] = minDist[i];
            for (int i = bestIdx; i < pool.Count; i++) newMinDist[i] = minDist[i + 1];
            minDist = newMinDist;

            // 새로 선택된 점 기준으로 minDist 갱신
            for (int i = 0; i < pool.Count; i++)
            {
                float d = Vector2.Distance(pool[i], newPoint);
                if (d < minDist[i]) minDist[i] = d;
            }
        }

        return result;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (areaWSize == 0 || areaHSize == 0) return;

        Color[] levelColors = new Color[]
        {
            new Color(0f, 1f, 0f, 0.3f),    // 레벨 1 - 초록
            new Color(1f, 1f, 0f, 0.3f),    // 레벨 2 - 노랑
            new Color(1f, 0.5f, 0f, 0.3f),  // 레벨 3 - 주황
            new Color(1f, 0f, 0f, 0.3f),    // 레벨 4 - 빨강
            new Color(0.5f, 0f, 1f, 0.3f),  // 레벨 5 - 보라
        };

        // ── 레벨 구역 표시 ──────────────────────────────────
        if (areaPosLevel != null)
        {
            foreach (var data in areaPosLevel)
            {
                Vector2 pos = data.Key;
                int level = data.Value;

                Color fillColor = levelColors[Mathf.Clamp(level - 1, 0, levelColors.Length - 1)];
                Color outlineColor = fillColor;
                outlineColor.a = 1f;

                Gizmos.color = fillColor;
                Gizmos.DrawCube(new Vector3(pos.x, pos.y, 0), new Vector3(areaWSize, areaHSize, 0));

                Gizmos.color = outlineColor;
                Gizmos.DrawWireCube(new Vector3(pos.x, pos.y, 0), new Vector3(areaWSize, areaHSize, 0));

                UnityEditor.Handles.color = Color.white;
                UnityEditor.Handles.Label(new Vector3(pos.x, pos.y, 0), $"Lv.{level}");
            }
        }

        // ── 안전지역 표시 ───────────────────────────────────
        // basePos 기준으로 홀짝에 따라 안전지역 셀을 재계산해서 그린다
        if (basePos != Vector3.zero && splitCount > 0)
        {
            bool isEven = splitCount % 2 == 0;
            int centerNum = splitCount / 2;

            Color safeZoneFill = new Color(0f, 1f, 1f, 0.15f);
            Color safeZoneOutline = new Color(0f, 1f, 1f, 0.8f);

            // 맵 좌하단 원점 복원
            // basePos = 홀수: 정중앙 셀 중심 / 짝수: 맵 기하 중심(셀 경계)
            float originX, originY;
            if (isEven)
            {
                // 짝수: basePos가 맵 기하 중심 = (0,0) 기준 셀 경계
                originX = basePos.x - centerNum * areaWSize;
                originY = basePos.y - centerNum * areaHSize;
            }
            else
            {
                // 홀수: basePos가 정중앙 셀 중심
                originX = basePos.x - centerNum * areaWSize - areaWSize / 2f;
                originY = basePos.y - centerNum * areaHSize - areaHSize / 2f;
            }

            for (int i = 0; i < splitCount; i++)
            {
                for (int j = 0; j < splitCount; j++)
                {
                    int x, y;

                    if (isEven)
                    {
                        // 짝수: 2×2 밴드 [centerNum-1, centerNum] 로부터 거리
                        x = (i < centerNum - 1) ? (centerNum - 1 - i) :
                             (i > centerNum) ? (i - centerNum) : 0;
                        y = (j < centerNum - 1) ? (centerNum - 1 - j) :
                             (j > centerNum) ? (j - centerNum) : 0;
                    }
                    else
                    {
                        // 홀수: 단일 중앙 인덱스로부터 거리
                        x = Math.Abs(centerNum - i);
                        y = Math.Abs(centerNum - j);
                    }

                    // 안전지역 = Max(x,y) <= 1
                    if (Math.Max(x, y) > 1) continue;

                    float cellCenterX = originX + i * areaWSize + areaWSize / 2f;
                    float cellCenterY = originY + j * areaHSize + areaHSize / 2f;
                    Vector3 cellPos = new Vector3(cellCenterX, cellCenterY, 0);

                    Gizmos.color = safeZoneFill;
                    Gizmos.DrawCube(cellPos, new Vector3(areaWSize, areaHSize, 0));

                    Gizmos.color = safeZoneOutline;
                    Gizmos.DrawWireCube(cellPos, new Vector3(areaWSize, areaHSize, 0));

                    // 정중앙(basePos) 셀은 별도 표시
                    bool isCenter = isEven
                        ? (x == 0 && y == 0 && (i == centerNum - 1 || i == centerNum) && (j == centerNum - 1 || j == centerNum))
                        : (x == 0 && y == 0);

                    UnityEditor.Handles.color = Color.cyan;
                    UnityEditor.Handles.Label(cellPos, isCenter ? "BASE" : "SAFE");
                }
            }
        }
    }
#endif
}