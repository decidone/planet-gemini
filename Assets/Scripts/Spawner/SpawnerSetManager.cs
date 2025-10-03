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
                if (Math.Max(x, y) > 1)
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
        Map map;
        if (isHostMap)
            map = hostMap;
        else
            map = clientMap;

        int xIndex = 0;
        int yIndex = 0;

        MapSizeData mapSizeData = MapGenerator.instance.mapSizeData;
        subSpawnerCount = new int[mapSizeData.CountOfSpawnersByLevel.Length];
        spawnCount = (int[])basicSpawnerCount.Clone();
        upgradeSpawnerSetCount = (int[])mapSizeData.UpgradeSpawnerSet.Clone();

        for (int i = 0; i < subSpawnerCount.Length; i++) 
        {
            subSpawnerCount[i] = basicSpawnerCount[i] - mapSizeData.CountOfSpawnersByLevel[i];
        }

        foreach (var data in areaPosLevel)
        {
            if (basePos == (Vector3)data.Key)
            {
                xIndex++;
                if (xIndex >= splitCount)
                {
                    xIndex = 0;
                    yIndex++;
                }

                continue;
            }

            Vector2 centerPos = data.Key;
            int areaLevel = data.Value;
            int random = Random.Range(0, spawnCount[areaLevel - 1]);
            spawnCount[areaLevel - 1] -= 1;

            if (subSpawnerCount[areaLevel - 1] > 0 && random < subSpawnerCount[areaLevel - 1])
            {
                subSpawnerCount[areaLevel - 1] -= 1;
                continue;
            }

            //AreaLevelData levelData = arealevelData[(areaLevel - 1) * 2];
            AreaLevelData levelData = arealevelData[0];

            if((areaLevel - 1) * 2 < arealevelData.Length)
                levelData = arealevelData[(areaLevel - 1) * 2];
            else
                levelData = arealevelData[arealevelData.Length - 1];

            float xRadius;
            float yRadius;

            xRadius = areaWSize / 2 - 5;
            yRadius = areaHSize / 2 - 5;

            Vector2 newPoint;
            bool whileCheck = false;
            List<Vector2> cantPos = new List<Vector2>();

            do
            {
                int x;
                int y;

                x = (int)Random.Range(-xRadius, xRadius);
                y = (int)Random.Range(-yRadius, yRadius);

                newPoint = centerPos + new Vector2(x, y);
                
                if (cantPos.Contains(newPoint))
                {
                    whileCheck = true;
                    continue;
                }

                string biome = map.GetCellDataFromPos((int)newPoint.x, (int)newPoint.y).biome.biome;

                //if (biome == "lake" || biome == "cliff")
                //{
                //    whileCheck = true;
                //    cantPos.Add(newPoint);
                //    continue;
                //}

                for (int i = -2; i <= 2; i += 5)
                {
                    for (int j = -2; j <= 2; j += 5)
                    {
                        biome = map.GetCellDataFromPos((int)newPoint.x + i, (int)newPoint.y + j).biome.biome;
                        if (biome == "lake" || biome == "cliff")
                        {
                            whileCheck = true;
                            cantPos.Add(newPoint);
                            continue;
                        }
                    }
                }

                whileCheck = false;

            } while (whileCheck);

            GameObject spawnGroup = SpawnerGroupSet(centerPos);
            SpawnerGroupManager spawnerGroupManager = spawnGroup.GetComponent<SpawnerGroupManager>();

            GameObject spawnerObj = Instantiate(spawner);
            NetworkObject networkObject = spawnerObj.GetComponent<NetworkObject>();
            if(!networkObject.IsSpawned) networkObject.Spawn(true);

            spawnerObj.transform.position = newPoint;

            int levelSet = levelData.sppawnerLevel;
            AreaLevelData levelDataSet = levelData;

            if (upgradeSpawnerSetCount[areaLevel - 1] > 0)
            {
                random = Random.Range(0, spawnCount[areaLevel - 1]);

                if (random < upgradeSpawnerSetCount[areaLevel - 1])
                {
                    upgradeSpawnerSetCount[areaLevel - 1] -= 1;

                    levelSet += 1;
                    if (levelSet - 1 < arealevelData.Length)
                        levelDataSet = arealevelData[levelSet - 1];
                    else
                        levelDataSet = arealevelData[arealevelData.Length - 1];
                }
            }

            MapGenerator.instance.SetCorruption(map, newPoint, levelSet);

            Cell cellData = map.GetCellDataFromPos((int)newPoint.x, (int)newPoint.y);
            if (cellData.obj != null)
            {
                Destroy(map.GetCellDataFromPos((int)newPoint.x, (int)newPoint.y).obj);
            }
            spawnerGroupManager.SpawnerSet(spawnerObj);
            spawnerObj.TryGetComponent(out MonsterSpawner monsterSpawner);
            monsterSpawner.groupManager = spawnerGroupManager;
            monsterSpawner.SpawnerSetting(levelDataSet, cellData.biome.biome, basePos, isHostMap, areaLevel);
            monsterSpawnerManager.AreaGroupSet(monsterSpawner, areaLevel, isHostMap);

            if (isHostMap)
                spawnerMap1Matrix[xIndex, yIndex] = spawnGroup;
            else
                spawnerMap2Matrix[xIndex, yIndex] = spawnGroup;

            spawnerGroupManager.SpawnerGroupStatsSet((xIndex, yIndex));

            xIndex++;
            if(xIndex >= splitCount)
            {
                xIndex = 0;
                yIndex++;
            }
        }
        monsterSpawnerManager.MatrixSet(splitCount, spawnerMap1Matrix, spawnerMap2Matrix, isHostMap);
        stopwatch.Stop();
        Debug.Log((isHostMap? "host" : "client") + " spawnerSet Time : " + stopwatch.ElapsedMilliseconds.ToString() + "ms");
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
}
