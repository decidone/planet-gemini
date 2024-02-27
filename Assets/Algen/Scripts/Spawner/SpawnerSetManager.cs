using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnerSetManager : MonoBehaviour
{
    [SerializeField]
    MapGenerator mapGen;
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

    [SerializeField]
    AreaLevelData[] arealevelData;

    Vector3 basePos;

    [SerializeField]
    MonsterSpawnerManager monsterSpawnerManager;

    public GameObject[,] spawnerMatrix;
    int xIndex;
    int yIndex;

    #region Singleton
    public static SpawnerSetManager instance;

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
        spawnerMatrix = new GameObject[splitCount, splitCount];
        xIndex = 0;
        yIndex = 0;
    }

    public void AreaMapSet()
    {
        width = mapGen.width;
        height = mapGen.height;
        areaWSize = width / splitCount;
        areaHSize = height / splitCount;

        int centerNum = Mathf.FloorToInt(splitCount / 2);

        for (int i = 0; i < splitCount; i++)
        {
            for (int j = 0; j < splitCount; j++)
            {
                float centerX = i * areaWSize + areaWSize / 2;
                float centerY = j * areaHSize + areaHSize / 2;

                Vector2 centerPos = new Vector2(centerX, centerY);  // 구역의 중앙 좌표

                int x = Math.Abs(centerNum - i);
                int y = Math.Abs(centerNum - j);

                if (x == 0 && y == 0)
                {
                    basePos = centerPos;
                }

                areaPosLevel.Add(centerPos, Math.Max(x, y));    // 구역의 중앙 좌표 + 구역 레벨
            }
        }

        SpawnerSet();
    }

    void SpawnerSet()
    {
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
                } while (!IsDistanceValid(randomPoints, newPoint, minDistance) || mapGen.map.mapData[(int)newPoint.x][(int)newPoint.y].biome.biome == "lake");    // 거리 체크하여 가까우면 다시 돌리기
                
                randomPoints[i] = newPoint;
            }

            int index = 0;

            GameObject spawnGroup = SpawnerGroupSet(centerPos);
            for (int i = 0; i < levelData.maxSpawner; i++)
            {
                GameObject spawnerObj = Instantiate(spawner);
                spawnerObj.transform.position = randomPoints[index];

                Cell cellData = mapGen.map.mapData[(int)randomPoints[index].x][(int)randomPoints[index].y];
                if (cellData.obj != null)
                {
                    Destroy(mapGen.map.mapData[(int)randomPoints[index].x][(int)randomPoints[index].y].obj);
                }
                spawnGroup.GetComponent<SpawnerGroupManager>().SpawnerSet(spawnerObj);
                spawnerObj.TryGetComponent(out MonsterSpawner monsterSpawner);
                monsterSpawner.SpawnerSetting(levelData, cellData.biome.biome, basePos);
                monsterSpawnerManager.AreaGroupSet(monsterSpawner, areaLevel);
                index++;
            }

            spawnerMatrix[xIndex, yIndex] = spawnGroup;
            xIndex++;
            if(xIndex >= splitCount)
            {
                xIndex = 0;
                yIndex++;
            }
        }
    }

    GameObject SpawnerGroupSet(Vector2 pos)
    {
        GameObject spawnObj;

        spawnObj = Instantiate(spawnerGroup);
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
