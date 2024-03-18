using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Pathfinding;

// UTF-8 설정
public class MapGenerator : MonoBehaviour
{
    System.Random random;
    public bool randomSeed;
    public int seed;

    [SerializeField]
    MapSizeData mapSizeData;

    public int width;
    public int height;
    public float magnification;

    [Space]
    public Tilemap tilemap;
    public Tilemap lakeTilemap;
    public Tilemap resourcesTilemap;
    public Tilemap resourcesIconTilemap;
    public GameObject objects;
    //public Map map;
    public Map hostMap;
    public Map clientMap;
    public bool isMultiPlay;
    [SerializeField] int clientMapOffsetY;

    [Space]
    [Header("Biomes")]
    public Biome plain;
    public Biome desert;
    public Biome forest;
    public Biome snow;
    public Biome frozen;
    public Biome lake;
    List<List<Biome>> biomes;

    [Space]
    public List<Resource> resources = new List<Resource>();
    public List<Tile> resourcesIcon = new List<Tile>();

    public AstarPath astar;
    public CompositeCollider2D comp;
    bool isCompositeDone;
    Vector3 mapCenterPos;

    SpawnerSetManager spawnerPosSet;
    public static MapGenerator instance;

    void Awake()
    {
        // 현 테스트 중 맵 사이즈가 작아야 하는 상황이라서 예외처리 나중에 제거해야함
        // mapSizeData로만 세팅하도록
        if (mapSizeData == null)
        {
            hostMap.width = width;
            hostMap.height = height;
            if (isMultiPlay)
            {
                clientMap.width = width;
                clientMap.height = height;
            }
        }
        else
        {
            width = mapSizeData.MapSize;
            height = mapSizeData.MapSize;
            hostMap.width = mapSizeData.MapSize;
            hostMap.height = mapSizeData.MapSize;
            if (isMultiPlay)
            {
                clientMap.width = mapSizeData.MapSize;
                clientMap.height = mapSizeData.MapSize;
            }
        }

        hostMap.mapData = new List<List<Cell>>();
        if (isMultiPlay)
            clientMap.mapData = new List<List<Cell>>();

        mapCenterPos = new Vector3(Mathf.FloorToInt(width / 2), Mathf.FloorToInt(height / 2));
        AddGridGraph(mapCenterPos);
        if (isMultiPlay)
        {
            mapCenterPos = new Vector3(Mathf.FloorToInt(width / 2), Mathf.FloorToInt((height / 2) + height + clientMapOffsetY));
            AddGridGraph(mapCenterPos);
        }
        isCompositeDone = false;
        comp = lakeTilemap.GetComponent<CompositeCollider2D>();
        instance = this;
    }

    void Start()
    {
        SetRandomSeed();
        GenerateMap();
        SetSpawnPos(hostMap, true);
        if (isMultiPlay)
            SetSpawnPos(clientMap, false);

        // 현 테스트 중 맵 사이즈가 작아야 하는 상황이라서 예외처리 나중에 제거해야함
        // mapSizeData로만 세팅하도록
        spawnerPosSet = SpawnerSetManager.instance;
        
        //if (spawnerPosSet && mapSizeData != null)
        //{
        //    spawnerPosSet.AreaMapSet(mapCenterPos, mapSizeData.MapSplitCount);
        //}
        // PortalSet();
        // mapFog.transform.position = new Vector3(width / 2, height / 2, 0);
        // mapFog.transform.localScale = new Vector3(width, height, 1);
    }

    public void SpawnerAreaMapSet() // 임시로 호스트 선택 후 호스트쪽에만 진행 하도록 임시로 둠 
    {
        if (spawnerPosSet && mapSizeData != null)
        {
            spawnerPosSet.AreaMapSetServerRpc(mapCenterPos, mapSizeData.MapSplitCount);
        }
    }

    void Update()
    {
        if (!isCompositeDone)
        {
            if (comp.shapeCount != 0)
            {
                astar.Scan();
                isCompositeDone = true;
            }
        }
    }

    void SetSpawnPos(Map map, bool isHostMap)
    {
        int offsetY = 0;
        if (!isHostMap)
            offsetY = height + clientMapOffsetY;

        int x = Mathf.FloorToInt(width / 2);
        int y = Mathf.FloorToInt(height / 2);

        if (map.mapData[x][y].biome.biome == "lake")
        {
            string dir = "";
            int dist = 0;

            for (int i = 1; i < y; i++) //상
            {
                if (map.mapData[x][y + i].biome.biome != "lake")
                {
                    dir = "up";
                    dist = i;
                    break;
                }
            }
            for (int i = 1; i < y; i++) //하
            {
                if (map.mapData[x][y - i].biome.biome != "lake")
                {
                    if (dist > i)
                    {
                        dir = "down";
                        dist = i;
                    }
                    break;
                }
            }
            for (int i = 1; i < x; i++) //좌
            {
                if (map.mapData[x - i][y].biome.biome != "lake")
                {
                    if (dist > i)
                    {
                        dir = "left";
                        dist = i;
                    }
                    break;
                }
            }
            for (int i = 1; i < x; i++) //우
            {
                if (map.mapData[x + i][y].biome.biome != "lake")
                {
                    if (dist > i)
                    {
                        dir = "right";
                        dist = i;
                    }
                    break;
                }
            }

            switch (dir)
            {
                case "up": y += dist; break;
                case "down": y -= dist; break;
                case "left": x -= dist; break;
                case "right": x += dist; break;
            }
        }

        // 아래 -> 플레이어, 포탈 스폰포인트 지정, 스폰 시 데이터 설정
        var pos = PortalPosCheck(map, x, y);
        GameManager.instance.SetPlayerPos(pos.x, (pos.y + offsetY), isHostMap);

        Portal[] portal = GameManager.instance.portal;
        if (isHostMap)
        {
            portal[0].transform.position = new Vector3(pos.x, (pos.y + offsetY), 0);
            portal[0].MapDataSet(map);

            // 솔로 플레이일 경우 포탈이 다른 설정값을 가져야 함
        }
        else
        {
            portal[1].transform.position = new Vector3(pos.x, (pos.y + offsetY), 0);
            portal[1].MapDataSet(map);
        }
    }

    public (int x, int y) PortalPosCheck(Map map, int x, int y)
    {
        // 11x11 범위를 검사해서 호수가 있는 방향을 체크 + 가중치 적용, 체크된 방향의 반대쪽으로 가중치만큼 이동
        // 1번째 줄에 아무 곳이나 호수가 잡히면 가중치 1, 2번째 줄에 잡히면 2 이런 식으로 적용, 세로도 마찬가지
        // 나무나 바위를 추가하면 스폰지역 주변 일정 구간은 철거하는 방식으로 둘 듯

        int[] bias = new int[4];    // 0: 첫 x, 1: 마지막 x, 2: 첫 y, 3: 마지막 y
        int biasX = 0;
        int biasY = 0;
        int tempX = x;
        int tempY = y;
        bool first = true;

        tempX -= 5;
        int startY = tempY - 5;
        for (int i = 0; i < 11; i++)
        {
            tempY = startY;
            for (int j = 0; j < 11; j++)
            {
                if (map.mapData[tempX][tempY].biome.biome == "lake")
                {
                    if (first)
                    {
                        bias[0] = i;
                        first = false;
                    }

                    bias[1] = i;

                    if (i < 5)
                        biasX++;
                    else if (i > 5)
                        biasX--;

                    if (j < 5)
                        biasY++;
                    else if (j > 5)
                        biasY--;
                }
                tempY++;
            }
            tempX++;
        }

        tempX = x;
        tempY = y;
        first = true;

        tempY -= 5;
        int startX = tempX - 5;
        for (int i = 0; i < 11; i++)
        {
            tempX = startX;
            for (int j = 0; j < 11; j++)
            {
                if (map.mapData[tempX][tempY].biome.biome == "lake")
                {
                    if (first)
                    {
                        bias[2] = i;
                        first = false;
                    }

                    bias[3] = i;
                }
                tempX++;
            }
            tempY++;
        }

        if (biasX < 0)
        {
            Debug.Log("left: " + (11 - bias[0]));
            x -= (11 - bias[0]);
        }
        else if (biasX > 0)
        {
            Debug.Log("right: " + (bias[1] + 1));
            x += (bias[1] + 1);
        }

        if (biasY < 0)
        {
            Debug.Log("down: " + (11 - bias[2]));
            y -= (11 - bias[2]);
        }
        else if (biasY > 0)
        {
            Debug.Log("up: " + (bias[3] + 1));
            y += (bias[3] + 1);
        }

        return (x, y);
    }

    void SetRandomSeed()
    {
        if (randomSeed)
            seed = Random.Range(int.MinValue, int.MaxValue);

        random = new System.Random(seed);
    }

    void GenerateMap()
    {
        hostMap.SetOffsetY(0);
        SetBiomeTable(true);
        SetBiome(hostMap);
        SmoothBiome(hostMap);
        CreateTile(hostMap, true);
        CreateResource(hostMap, true);
        CreateObj(hostMap, true);

        if (isMultiPlay)
        {
            clientMap.SetOffsetY(height + clientMapOffsetY);
            SetBiomeTable(false);
            SetBiome(clientMap);
            SmoothBiome(clientMap);
            CreateTile(clientMap, false);
            CreateResource(clientMap, false);
            CreateObj(clientMap, false);
        }
    }

    void SetBiomeTable(bool isHostMap)
    {
        if (isHostMap)
        {
            biomes = new List<List<Biome>>() {
                new List<Biome> { lake, forest, plain, plain, plain, frozen, frozen, frozen },
                new List<Biome> { lake, forest, plain, plain, plain, frozen, frozen, frozen },
                new List<Biome> { lake, forest, plain, plain, plain, frozen, frozen, frozen },
                new List<Biome> { lake, forest, plain, plain, plain, snow, snow, snow },
                new List<Biome> { lake, forest, plain, plain, plain, snow, snow, snow },
                new List<Biome> { lake, forest, plain, plain, plain, snow, snow, snow },
                new List<Biome> { lake, forest, plain, plain, plain, snow, snow, snow },
                new List<Biome> { lake, forest, plain, plain, plain, snow, snow, snow },
            };
        }
        else
        {
            biomes = new List<List<Biome>>() {
                new List<Biome> { lake, forest, plain, plain, plain, desert, desert, desert },
                new List<Biome> { lake, forest, plain, plain, plain, desert, desert, desert },
                new List<Biome> { lake, forest, plain, plain, plain, desert, desert, desert },
                new List<Biome> { lake, forest, plain, plain, plain, desert, desert, desert },
                new List<Biome> { lake, forest, plain, plain, plain, desert, desert, desert },
                new List<Biome> { lake, forest, plain, plain, plain, desert, desert, desert },
                new List<Biome> { lake, forest, plain, plain, plain, desert, desert, desert },
                new List<Biome> { lake, forest, plain, plain, plain, desert, desert, desert },
            };
        }
    }

    void SetBiome(Map map)
    {
        int tempX = random.Next(0, 1000000);
        int tempY = random.Next(0, 1000000);
        int heightX = random.Next(0, 1000000);
        int heightY = random.Next(0, 1000000);

        for (int x = 0; x < width; x++)
        {
            map.mapData.Add(new List<Cell>());

            for (int y = 0; y < height; y++)
            {
                float tempNoise = Mathf.PerlinNoise(
                    (x - tempX) / magnification,
                    (y - tempY) / magnification
                );
                tempNoise = Mathf.Clamp01(tempNoise);
                float scaledTemp = tempNoise * biomes.Count;
                if (scaledTemp == biomes.Count)
                    scaledTemp = (biomes.Count - 1);

                float heightNoise = Mathf.PerlinNoise(
                    (x - heightX) / magnification,
                    (y - heightY) / magnification
                );
                heightNoise = Mathf.Clamp01(heightNoise);
                float scaledHeight = heightNoise * biomes.Count;
                if (scaledHeight == biomes.Count)
                    scaledHeight = (biomes.Count - 1);

                Cell cell = new Cell();
                cell.biome = biomes[Mathf.FloorToInt(scaledHeight)][Mathf.FloorToInt(scaledTemp)];
                map.mapData[x].Add(cell);
            }
        }
    }

    void SmoothBiome(Map map)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = map.mapData[x][y];
                Biome biome = cell.biome;
                biome.BiomeSmoother(map, x, y);
            }
        }
    }

    void CreateTile(Map map, bool isHostMap)
    {
        int offsetY = 0;
        if (!isHostMap)
            offsetY = height + clientMapOffsetY;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = map.mapData[x][y];
                Biome biome = cell.biome;
                var tile = biome.SetTile(random, map, x, y);
                cell.tile = tile.tile;

                if (biome.biome == "lake")
                {
                    lakeTilemap.SetTile(new Vector3Int(x, (y + offsetY), 0), tile.tile);
                    if (tile.form == "side")
                        cell.buildable.Add("pump");
                    else
                        cell.buildable.Add("none");
                }
                else
                {
                    tilemap.SetTile(new Vector3Int(x, (y + offsetY), 0), tile.tile);
                }
            }
        }
    }

    void CreateResource(Map map, bool isHostMap)
    {
        int offsetY = 0;
        if (!isHostMap)
            offsetY = height + clientMapOffsetY;

        for (int i = 0; i < resources.Count; i++)
        {
            Resource resource = resources[i];
            Debug.Log("ore gen : " + resource.name);
            int oreX = random.Next(0, 1000000);
            int oreY = random.Next(0, 1000000);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float oreNoise = Mathf.PerlinNoise(
                        (x - oreX) / resource.distribution,
                        (y - oreY) / resource.distribution
                    );

                    if (oreNoise < resource.scale)
                    {
                        Cell cell = map.mapData[x][y];
                        Biome biome = cell.biome;

                        if ((resource.biome == "all" || resource.biome == biome.biome)
                            && biome.biome != "lake" && cell.resource == null)
                        {
                            Tile resourceTile = resource.tiles[random.Next(0, resource.tiles.Count)];
                            resourcesTilemap.SetTile(new Vector3Int(x, (y + offsetY), 0), resourceTile);
                            resourcesIconTilemap.SetTile(new Vector3Int(x, (y + offsetY), 0), resourcesIcon[i]);
                            cell.resource = resource;

                            if (resource.type == "ore")
                            {
                                cell.buildable.Add("miner");
                            }
                            else if (resource.type == "oil")
                            {
                                cell.buildable.Add("extractor");
                            }
                        }
                    }
                }
            }
        }
    }

    void CreateObj(Map map, bool isHostMap)
    {
        int offsetY = 0;
        if (!isHostMap)
            offsetY = height + clientMapOffsetY;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = map.mapData[x][y];
                Biome biome = cell.biome;
                if (cell.obj == null)
                {
                    GameObject obj = biome.SetObject(random);
                    if (obj != null)
                    {
                        GameObject objInst = Instantiate(obj, objects.transform);
                        cell.obj = objInst;

                        objInst.name = string.Format("map_x{0}_y{1}", x, y);
                        objInst.transform.localPosition = new Vector3((float)(x + 0.5), (float)((y + offsetY) + 0.5), 0);
                    }
                }
            }
        }
    }

    void AddGridGraph(Vector3 centerPos)
    {
        AstarData data = AstarPath.active.data;
        GridGraph gg = data.AddGraph(typeof(GridGraph)) as GridGraph;
        gg.center = centerPos;
        gg.SetDimensions(width, height, 1);
        gg.is2D = true;
        gg.collision.use2D = true;
        gg.collision.mask |= 1 << LayerMask.NameToLayer("Map");
        gg.collision.mask |= 1 << LayerMask.NameToLayer("Obj");
        gg.collision.mask |= 1 << LayerMask.NameToLayer("MapObj");
        gg.collision.mask |= 1 << LayerMask.NameToLayer("Spawner");
        gg.collision.mask |= 1 << LayerMask.NameToLayer("PortalUnit");
    }
}