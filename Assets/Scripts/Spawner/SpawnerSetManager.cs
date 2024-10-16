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
    int[] basicSpawnerCount = new int[4] { 8, 16, 24, 32 }; //  구역별 기본 스포너 개수
    int[] spawnCount;
    int[] subSpawnerCount;
    int[] upgradeSpawnerSetCount;
    Vector3 basePos;

    [SerializeField]
    MonsterSpawnerManager monsterSpawnerManager;

    public GameObject[,] spawnerMap1Matrix;
    public GameObject[,] spawnerMap2Matrix;
    //int xIndex;
    //int yIndex;

    Map hostMap;
    Map clientMap;

    //[SerializeField]
    //Sprite[] spawnerSprite;

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

    private void Start()
    {
        //hostMap = GameManager.instance.hostMap;
        //clientMap = GameManager.instance.clientMap;
        //spawnerMap1Matrix = new GameObject[splitCount, splitCount];
        //spawnerMap2Matrix = new GameObject[splitCount, splitCount];
        //xIndex = 0;
        //yIndex = 0;
    }

    [ServerRpc]
    public void AreaMapSetServerRpc(Vector2 centerPos, int mapSplitCount, bool isHostMap)
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

                areaPosLevel.Add(areaCenter, Math.Max(x, y));    // 구역의 중앙 좌표 + 구역 레벨
            }
        }

        SpawnerSet(isHostMap);
    }

    void SpawnerSet(bool isHostMap)
    {
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
            if(basePos == (Vector3)data.Key)
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

            AreaLevelData levelData = arealevelData[areaLevel - 1];

            Vector2[] randomPoints = new Vector2[levelData.maxSpawner];

            float xRadius;
            float yRadius;

            if(areaLevel == Mathf.RoundToInt(splitCount/2) || areaLevel == 1)
            {
                xRadius = areaWSize / 2 - 30;
                yRadius = areaHSize / 2 - 30;
            }
            else
            {
                xRadius = areaWSize / 2 - 10;
                yRadius = areaHSize / 2 - 10;
            }

            float minDistance = 10 ; // 오브젝트간 최소 거리

            Vector2 newPoint;

            for (int i = 0; i < levelData.maxSpawner; i++)
            {
                do
                {
                    int x = (int)Random.Range(-xRadius, xRadius);
                    int y = (int)Random.Range(-yRadius, yRadius);

                    newPoint = centerPos + new Vector2(x, y);
                } while (!IsDistanceValid(randomPoints, newPoint, minDistance) || map.GetCellDataFromPos((int)newPoint.x, (int)newPoint.y).biome.biome == "lake" || map.GetCellDataFromPos((int)newPoint.x, (int)newPoint.y).biome.biome == "cliff");    // 거리 체크하여 가까우면 다시 돌리기

                randomPoints[i] = newPoint;
            }

            int index = 0;

            GameObject spawnGroup = SpawnerGroupSet(centerPos);
            SpawnerGroupManager spawnerGroupManager = spawnGroup.GetComponent<SpawnerGroupManager>();

            for (int i = 0; i < levelData.maxSpawner; i++)
            {
                GameObject spawnerObj = Instantiate(spawner);
                NetworkObject networkObject = spawnerObj.GetComponent<NetworkObject>();
                if(!networkObject.IsSpawned) networkObject.Spawn(true);

                spawnerObj.transform.position = randomPoints[index];

                int levelSet = levelData.areaLevel;
                AreaLevelData levelDataSet = levelData;

                if (upgradeSpawnerSetCount[areaLevel - 1] > 0)
                {
                    random = Random.Range(0, spawnCount[areaLevel - 1]);

                    if (random < upgradeSpawnerSetCount[areaLevel - 1])
                    {
                        upgradeSpawnerSetCount[areaLevel - 1] -= 1;
                        levelSet += 1;
                        levelDataSet = arealevelData[levelSet - 1];
                    }
                }

                MapGenerator.instance.SetCorruption(map, randomPoints[index], levelSet);
                //MapGenerator.instance.SetCorruption(map, randomPoints[index], levelData.areaLevel);

                Cell cellData = map.GetCellDataFromPos((int)randomPoints[index].x, (int)randomPoints[index].y);
                if (cellData.obj != null)
                {
                    Destroy(map.GetCellDataFromPos((int)randomPoints[index].x, (int)randomPoints[index].y).obj);
                }
                spawnerGroupManager.SpawnerSet(spawnerObj);
                spawnerObj.TryGetComponent(out MonsterSpawner monsterSpawner);
                monsterSpawner.groupManager = spawnerGroupManager;
                monsterSpawner.SpawnerSetting(levelDataSet, cellData.biome.biome, basePos, isHostMap, levelData.areaLevel);
                //monsterSpawner.SpawnerSetting(levelData, cellData.biome.biome, basePos, isHostMap);
                monsterSpawnerManager.AreaGroupSet(monsterSpawner, areaLevel, isHostMap);

                index++;
            }

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

    bool IsDistanceValid(Vector2[] existingPoints, Vector2 newPoint, float minDistance)
    {
        foreach (Vector2 point in existingPoints)
        {
            if (Vector2.Distance(point, newPoint) < minDistance)
            {
                return false;
            }
        }
        return true;
    }
}
