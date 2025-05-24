using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Pathfinding;

// UTF-8 설정
public class MapGenerator : MonoBehaviour
{
    System.Random random;
    public int seed;

    public MapSizeData mapSizeData;
    [SerializeField]
    MapSizeData[] mapSizeDatas;
    public float spawnAreaSize;

    public int width;
    public int height;
    public float magnification;
    public float cliffMagnification;
    public float cliffScale;

    [Space]
    public Tilemap tilemap;
    public Tilemap lakeTilemap;
    public Tilemap cliffTilemap;
    public Tilemap corruptionTilemap;
    public Tilemap resourcesTilemap;
    public Tilemap resourcesIconTilemap;
    public Tilemap fogTilemap;
    public GameObject objects;
    public Transform mapFog;

    public Map hostMap;
    public Map clientMap;
    public int[,] fogState; //0: 안개x, 1: 안개o
    [SerializeField] EdgeCollider2D hostMapCol;
    [SerializeField] EdgeCollider2D clientMapCol;
    public bool isMultiPlay;
    public int clientMapOffsetY;

    [Space]
    [Header("Biomes")]
    public Biome plain;
    public Biome desert;
    public Biome forest;
    public Biome snow;
    public Biome frozen;
    public Biome lake;
    public Biome cliff;
    public Biome EasyCorruption;
    public Biome NormalCorruption;
    public Biome HardCorruption;
    List<List<Biome>> biomes;

    [Space]
    public List<Resource> resources = new List<Resource>();
    public List<Tile> resourcesIcon = new List<Tile>();
    public Tile fogTile;
    public float fogCheckCooldown;

    public AstarPath astar;
    public CompositeCollider2D comp;
    public bool isCompositeDone;
    Vector3 map1CenterPos;
    Vector3 map2CenterPos;

    [SerializeField]
    bool spawnerSet;
    SpawnerSetManager spawnerPosSet;
    [SerializeField] float corruptionRadius;
    [SerializeField] float corruptionClearRadius;

    public static MapGenerator instance;
    MainGameSetting gameSetting;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        gameSetting = MainGameSetting.instance;

        mapSizeData = mapSizeDatas[(gameSetting.mapSizeIndex * 3) + gameSetting.difficultylevel];
        spawnerSet = gameSetting.isNewGame ? true : false;
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

        map1CenterPos = new Vector3(Mathf.FloorToInt(width / 2), Mathf.FloorToInt(height / 2));
        AddGridGraph(map1CenterPos, true);
        if (isMultiPlay)
        {
            map2CenterPos = new Vector3(Mathf.FloorToInt(width / 2), Mathf.FloorToInt((height / 2) + height + clientMapOffsetY));
            AddGridGraph(map2CenterPos, false);
        }
        isCompositeDone = false;
        comp = lakeTilemap.GetComponent<CompositeCollider2D>();
    }

    void Start()
    {
        SetSeed();
        GenerateMap();
        SetFogTile();
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
        if (spawnerSet && spawnerPosSet && mapSizeData != null)
        {
            spawnerPosSet.AreaMapSet(map1CenterPos, mapSizeData.MapSplitCount, true);
            if (isMultiPlay)
                spawnerPosSet.AreaMapSet(map2CenterPos, mapSizeData.MapSplitCount, false);
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
                GameManager.instance.GameStartSet();
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

        if (map.mapData[x][y].biome == lake)
        {
            string dir = "";
            int dist = 0;

            for (int i = 1; i < y; i++) //상
            {
                if (map.mapData[x][y + i].biome != lake)
                {
                    dir = "up";
                    dist = i;
                    break;
                }
            }
            for (int i = 1; i < y; i++) //하
            {
                if (map.mapData[x][y - i].biome != lake)
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
                if (map.mapData[x - i][y].biome != lake)
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
                if (map.mapData[x + i][y].biome != lake)
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
        map.SetSpawnTile(pos.x, pos.y);
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
                if (map.mapData[tempX][tempY].biome == lake)
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
                if (map.mapData[tempX][tempY].biome == lake)
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

    void SetSeed()
    {
        seed = gameSetting.randomSeed;
        random = new System.Random(seed);
    }

    void GenerateMap()
    {
        hostMap.SetOffsetY(0);

        SetBiomeTable(true);
        SetBiome(hostMap);
        SmoothBiome(hostMap);
        CreateTile(hostMap, true);
        SetSpawnPos(hostMap, true);
        SetSpawnArea(hostMap);

        SetCliff(hostMap);
        SmoothCliff(hostMap);
        CreateCliffTile(hostMap, true);
        CreateCliffWallTile(hostMap, true);

        CreateResource(hostMap, true);
        CreateObj(hostMap, true);

        if (isMultiPlay)
        {
            clientMap.SetOffsetY(height + clientMapOffsetY);

            SetBiomeTable(false);
            SetBiome(clientMap);
            SmoothBiome(clientMap);
            CreateTile(clientMap, false);
            SetSpawnPos(clientMap, false);
            SetSpawnArea(clientMap);

            SetCliff(clientMap);
            SmoothCliff(clientMap);
            CreateCliffTile(clientMap, false);
            CreateCliffWallTile(clientMap, false);

            CreateResource(clientMap, false);
            CreateObj(clientMap, false);
        }

        SetMapFog();
        SetMapBorderCol();
    }

    void SetMapFog()
    {
        float width;
        float height;

        if (!isMultiPlay)
        {
            width = hostMap.width;
            height = hostMap.height;
        }
        else
        {
            width = hostMap.width;
            height = hostMap.height + clientMap.height + clientMapOffsetY;
        }

        mapFog.localScale = new Vector3(width, height, 1);
        mapFog.position = new Vector3(width / 2, height / 2, 1);
    }

    void SetSpawnArea(Map map)
    {
        var spawnTile = map.spawnTile;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float distance = Mathf.Sqrt(Mathf.Pow(spawnTile.x - x, 2) + Mathf.Pow(spawnTile.y - y, 2));
                if (distance < spawnAreaSize)
                {
                    map.mapData[x][y].spawnArea = true;
                }
            }
        }
    }

    void SetBiomeTable(bool isHostMap)
    {
        if (isHostMap)
        {
            biomes = new List<List<Biome>>() {
                new List<Biome> { lake, forest, plain, plain, plain, snow, frozen, frozen },
                new List<Biome> { lake, forest, plain, plain, plain, snow, frozen, frozen },
                new List<Biome> { lake, forest, plain, plain, plain, snow, frozen, frozen },
                new List<Biome> { lake, forest, plain, plain, plain, snow, frozen, frozen },
                new List<Biome> { lake, forest, plain, plain, plain, snow, frozen, frozen },
                new List<Biome> { lake, forest, plain, plain, plain, snow, frozen, frozen },
                new List<Biome> { lake, forest, plain, plain, plain, snow, frozen, frozen },
                new List<Biome> { lake, forest, plain, plain, plain, snow, frozen, frozen },
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
                cell.x = x;
                cell.y = y;
                cell.biome = biomes[Mathf.FloorToInt(scaledHeight)][Mathf.FloorToInt(scaledTemp)];
                map.mapData[x].Add(cell);
            }
        }
    }

    void SetCliff(Map map)
    {
        int tempX = random.Next(0, 1000000);
        int tempY = random.Next(0, 1000000);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float tempNoise = Mathf.PerlinNoise(
                    (x - tempX) / cliffMagnification,
                    (y - tempY) / cliffMagnification
                );

                if (tempNoise < cliffScale && map.mapData[x][y].biome != lake && !map.mapData[x][y].spawnArea)
                    map.mapData[x][y].biome = cliff;
            }
        }
    }

    void SmoothBiome(Map map)
    {
        for (int i = 0; i < 10; i++)
        {
            int exception = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Cell cell = map.mapData[x][y];
                    Biome biome = cell.biome;
                    exception += biome.BiomeSmoother(map, x, y, true);
                }
            }

            Debug.Log("rot: " + i + ", exception: " + exception);
            if (exception == 0)
                return;
        }
    }

    void SmoothCliff(Map map)
    {
        for (int i = 0; i < 10; i++)
        {
            int exception = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Cell cell = map.mapData[x][y];
                    Biome biome = cell.biome;
                    if (biome == cliff)
                        exception += biome.BiomeSmoother(map, x, y, false);
                }
            }

            Debug.Log("cliff rot: " + i + ", exception: " + exception);
            if (exception == 0)
                return;
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
                cell.tileType = tile.form;

                if (biome == lake)
                {
                    Tile backgroundTile = forest.SetNormalTile(random);
                    tilemap.SetTile(new Vector3Int(x, (y + offsetY), 0), backgroundTile);

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

    void CreateCliffTile(Map map, bool isHostMap)
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
                
                if (biome == cliff)
                {
                    Tile tile = biome.SetCliffTile(random, map, x, y);
                    cell.tile = tile;
                    cliffTilemap.SetTile(new Vector3Int(x, (y + offsetY), 0), tile);
                }
            }
        }
    }

    void CreateCliffWallTile(Map map, bool isHostMap)
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

                if (biome == cliff && (2 <= cell.cliffType && cell.cliffType <= 4))
                {
                    Tile tile = null;

                    if (map.IsOnMapData(x, y - 1)
                        && (map.mapData[x][y - 1].cliffType == 0 || map.mapData[x][y - 1].cliffType == 5 || map.mapData[x][y - 1].cliffType == 6))
                    {
                        tile = biome.SetCliffWallTile(random, map, x, y, 1);
                        map.mapData[x][y - 1].tile = tile;
                        cliffTilemap.SetTile(new Vector3Int(x, (y - 1 + offsetY), 0), tile);

                        if (map.IsOnMapData(x, y - 2)
                            && (map.mapData[x][y - 2].cliffType == 0 || map.mapData[x][y - 2].cliffType == 5 || map.mapData[x][y - 2].cliffType == 6))
                        {
                            tile = biome.SetCliffWallTile(random, map, x, y, 2);
                            map.mapData[x][y - 2].tile = tile;
                            cliffTilemap.SetTile(new Vector3Int(x, (y - 2 + offsetY), 0), tile);
                        }
                    }
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
            Debug.Log("resource gen : " + resource.name);
            int oreX = random.Next(0, 1000000);
            int oreY = random.Next(0, 1000000);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (resource.type == "oil")
                    {
                        int randomNum = random.Next(1, 1000);
                        if (randomNum <= resource.distribution)
                        {
                            Cell cell = map.mapData[x][y];
                            Biome biome = cell.biome;

                            if (resource.biome.Contains(biome.biome))
                            {
                                if (map.IsOnMapData(x + 1, y) && map.IsOnMapData(x, y + 1) && map.IsOnMapData(x + 1, y + 1))
                                {
                                    // 해당하는 바이옴인 경우 2x2 젠
                                    List<Cell> cellList = new List<Cell>();
                                    cellList.Add(map.mapData[x][y]);
                                    cellList.Add(map.mapData[x + 1][y]);
                                    cellList.Add(map.mapData[x][y + 1]);
                                    cellList.Add(map.mapData[x + 1][y + 1]);
                                    bool canSetResource = true;

                                    foreach (Cell tempCell in cellList)
                                    {
                                        if (tempCell.biome == lake || tempCell.biome == cliff || tempCell.resource != null)
                                        {
                                            canSetResource = false;
                                        }
                                    }

                                    if (canSetResource)
                                    {
                                        int randomSprite = UnityEngine.Random.Range(0, 2);
                                        int spriteIndex = 0;

                                        if(randomSprite == 0)                                        
                                            spriteIndex = 3;                                        
                                        else
                                            spriteIndex = 7;
                                        
                                        int cellListIndex = 0;

                                        foreach (Cell tempCell in cellList)
                                        {
                                            if (tempCell.tileType == "normal")
                                            {
                                                Tile mapTile = tempCell.biome.SetNormalTile(random);
                                                tilemap.SetTile(new Vector3Int(tempCell.x, (tempCell.y + offsetY), 0), mapTile);
                                            }

                                            //Tile resourceTile = resource.tiles[random.Next(0, resource.tiles.Count)];
                                            Tile resourceTile = resource.tiles[spriteIndex + cellListIndex];
                                            cellListIndex++;

                                            resourcesTilemap.SetTile(new Vector3Int(tempCell.x, (tempCell.y + offsetY), 0), resourceTile);
                                            resourcesIconTilemap.SetTile(new Vector3Int(tempCell.x, (tempCell.y + offsetY), 0), resourcesIcon[i]);
                                            tempCell.resource = resource;

                                            if (tempCell.buildable.Count == 0)
                                                tempCell.buildable.Add("extractor");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        float oreNoise = Mathf.PerlinNoise(
                        (x - oreX) / resource.distribution,
                        (y - oreY) / resource.distribution
                    );

                        if (oreNoise < resource.scale)
                        {
                            Cell cell = map.mapData[x][y];
                            Biome biome = cell.biome;

                            if ((resource.biome.Contains(biome.biome))
                                && biome != lake && biome != cliff && cell.resource == null)
                            {
                                if (cell.tileType == "normal")
                                {
                                    Tile mapTile = cell.biome.SetNormalTile(random);
                                    tilemap.SetTile(new Vector3Int(x, (y + offsetY), 0), mapTile);
                                }

                                Tile resourceTile = resource.tiles[random.Next(0, resource.tiles.Count)];
                                resourcesTilemap.SetTile(new Vector3Int(x, (y + offsetY), 0), resourceTile);
                                resourcesIconTilemap.SetTile(new Vector3Int(x, (y + offsetY), 0), resourcesIcon[i]);
                                cell.resource = resource;

                                if (resource.type == "ore" && cell.buildable.Count == 0)
                                    cell.buildable.Add("miner");
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
                if (cell.obj == null && cell.resource == null)
                {
                    GameObject obj = biome.SetRandomObject(random);
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

    void SetFogTile()
    {
        int offsetY = height + clientMapOffsetY;
        fogState = new int[width, height + offsetY];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                fogTilemap.SetTile(new Vector3Int(x, y, 0), fogTile);
                fogTilemap.SetTile(new Vector3Int(x, (y + offsetY), 0), fogTile);
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height + offsetY; y++)
            {
                fogState[x, y] = 1;
            }
        }
        Debug.Log(fogState[width - 1 , height+offsetY-1]);
    }

    public void RemoveFogTile(Vector3 pos, float radius)
    {
        for (int x = (int)(pos.x - radius); x < (int)(pos.x + radius); x++)
        {
            for (int y = (int)(pos.y - radius); y < (int)(pos.y + radius); y++)
            {
                if (0 <= x && x < width && 0 <= y && y < height + height + clientMapOffsetY)
                {
                    if (((pos.x - x) * (pos.x - x)) + ((pos.y - y) * (pos.y - y)) < radius * radius)
                    {
                        if (fogState[x, y] == 1)
                        {
                            fogTilemap.SetTile(new Vector3Int(x, y, 0), null);
                            fogState[x, y] = 0;
                        }
                    }
                }
            }
        }
    }

    public int CheckFogState(Vector2 pos)
    {
        // 0: 안개x, 1: 안개o
        int state = 1;
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        bool isOnMap = (0 <= x && x < width) && (0 <= y && y < height + height + clientMapOffsetY);
        if (isOnMap)
        {
            state = fogState[x, y];
        }

        return state;
    }

    public void LoadFogState(int[,] loadedFogState)
    {
        fogState = loadedFogState;
        for (int x = 0; x < fogState.GetLength(0); x++)
        {
            for (int y = 0; y < fogState.GetLength(1); y++)
            {
                if (fogState[x, y] == 0)
                {
                    fogTilemap.SetTile(new Vector3Int(x, y, 0), null);
                }
            }
        }
    }

    void AddGridGraph(Vector3 centerPos, bool isHostMap)
    {
        AstarData data = AstarPath.active.data;
        GridGraph gg = data.AddGraph(typeof(GridGraph)) as GridGraph;
        if(isHostMap)
            gg.name = "Map1PlayerUnit";
        else
            gg.name = "Map2PlayerUnit";

        gg.center = centerPos;
        gg.SetDimensions(width, height, 1);
        gg.is2D = true;
        gg.collision.use2D = true;
        gg.collision.diameter = 0.8f;
        gg.collision.mask |= 1 << LayerMask.NameToLayer("Map");
        gg.collision.mask |= 1 << LayerMask.NameToLayer("Obj");
        gg.collision.mask |= 1 << LayerMask.NameToLayer("MapObj");
        gg.collision.mask |= 1 << LayerMask.NameToLayer("Spawner");
        gg.collision.mask |= 1 << LayerMask.NameToLayer("PortalUnit");

        gg = data.AddGraph(typeof(GridGraph)) as GridGraph;
        if (isHostMap)
            gg.name = "Map1MonsterUnit";
        else
            gg.name = "Map2MonsterUnit";

        gg.center = centerPos;
        gg.SetDimensions(width, height, 1);
        gg.is2D = true;
        gg.collision.use2D = true;
        gg.collision.diameter = 0.8f;
        gg.collision.mask |= 1 << LayerMask.NameToLayer("Map");
        gg.collision.mask |= 1 << LayerMask.NameToLayer("MapObj");
        gg.collision.mask |= 1 << LayerMask.NameToLayer("Spawner");
    }

    public void SetCorruption(Vector3 spawnerPos, int level)
    {
        Map map;
        Vector2 vector2Pos = new Vector2(spawnerPos.x, spawnerPos.y);

        if (spawnerPos.y > height)
            map = clientMap;
        else
            map = hostMap;

        SetCorruption(map, vector2Pos, corruptionRadius, level);
    }

    public void SetCorruption(Map map, Vector2 spawnerPos, int level)
    {
        SetCorruption(map, spawnerPos, corruptionRadius, level);
    }

    public void SetCorruption(Map map, Vector2 spawnerPos, float radius, int level)
    {
        int offsetY = 0;
        if (map == clientMap)
            offsetY = height + clientMapOffsetY;
        if (spawnerPos.y > height)
            spawnerPos.y -= offsetY;

        Biome biome;
        if (level >= 7)
        {
            biome = HardCorruption;
        }
        else if (level <= 3)
        {
            biome = EasyCorruption;
        }
        else
        {
            biome = NormalCorruption;
        }
        //switch (level)
        //{
        //    case 1: biome = EasyCorruption;
        //        break;
        //    case 2: biome = NormalCorruption;
        //        break;
        //    case 3: biome = HardCorruption;
        //        break;
        //    default: biome = NormalCorruption;
        //        break;
        //}

        Vector2 tempPos;
        for (int x = Mathf.FloorToInt(spawnerPos.x - radius); x <= (spawnerPos.x + radius); x++)
        {
            for (int y = Mathf.FloorToInt(spawnerPos.y - radius); y <= (spawnerPos.y + radius); y++)
            {
                if (map.IsOnMapData(x, y))
                {
                    tempPos = new Vector2(x, y);
                    float dist = Vector2.Distance(tempPos, spawnerPos);
                    if (dist < radius)
                    {
                        map.mapData[x][y].isCorrupted = true;

                        Cell cell = map.mapData[x][y];
                        if (cell.obj != null)
                        {
                            Destroy(cell.obj);
                            GameObject obj = biome.SetObject(random);
                            if (obj != null)
                            {
                                GameObject objInst = Instantiate(obj, objects.transform);
                                cell.corruptionObj = objInst;

                                objInst.name = string.Format("map_x{0}_y{1}", x, y);
                                objInst.transform.localPosition = new Vector3((float)(x + 0.5), (float)((y + offsetY) + 0.5), 0);
                            }
                        }
                    }
                }
            }
        }

        for (int x = Mathf.FloorToInt(spawnerPos.x - radius); x <= (spawnerPos.x + radius); x++)
        {
            for (int y = Mathf.FloorToInt(spawnerPos.y - radius); y <= (spawnerPos.y + radius); y++)
            {
                if (map.IsOnMapData(x, y))
                {
                    Cell cell = map.mapData[x][y];
                    if (cell.isCorrupted)
                    {
                        Tile tile = biome.SetCorruptionTile(random, map, x, y);
                        corruptionTilemap.SetTile(new Vector3Int(x, (y + offsetY), 0), tile);
                    }
                }
            }
        }
    }

    public void ClearCorruption(Vector3 spawnerPos, int level)
    {
        Map map;
        Vector2 vector2Pos = new Vector2(spawnerPos.x, spawnerPos.y);

        if (spawnerPos.y > height)
            map = clientMap;
        else
            map = hostMap;

        StartCoroutine(ClearCorruptionCoroutine(map, vector2Pos, corruptionRadius, level));
    }

    public void ClearCorruption(Map map, Vector2 spawnerPos, int level)
    {
        StartCoroutine(ClearCorruptionCoroutine(map, spawnerPos, corruptionRadius, level));
    }

    IEnumerator ClearCorruptionCoroutine(Map map, Vector2 spawnerPos, float _radius, int level)
    {
        float radius = _radius;
        int offsetY = 0;
        if (map == clientMap)
            offsetY = height + clientMapOffsetY;
        if (spawnerPos.y > height)
            spawnerPos.y -= offsetY;

        while (true)
        {
            //yield return new WaitForSeconds(random.Next(2, 10));
            yield return new WaitForSeconds(2f);

            ClearCorruptionTiles(map, spawnerPos, radius);
            RemoveMushrooms(map, spawnerPos, radius, radius - corruptionClearRadius);
            radius -= corruptionClearRadius;
            if (radius <= corruptionClearRadius)
                yield break;

            SetCorruption(map, spawnerPos, radius, level);
        }
    }

    void ClearCorruptionTiles(Map map, Vector2 spawnerPos, float radius)
    {
        int offsetY = 0;
        if (map == clientMap)
            offsetY = height + clientMapOffsetY;

        for (int x = Mathf.FloorToInt(spawnerPos.x - radius); x <= (spawnerPos.x + radius); x++)
        {
            for (int y = Mathf.FloorToInt(spawnerPos.y - radius); y <= (spawnerPos.y + radius); y++)
            {
                if (map.IsOnMapData(x, y))
                {
                    map.mapData[x][y].isCorrupted = false;
                    corruptionTilemap.SetTile(new Vector3Int(x, (y + offsetY) , 0), null);
                }
            }
        }
    }

    void RemoveMushrooms(Map map, Vector2 spawnerPos, float before, float after)
    {
        Vector2 tempPos;
        for (int x = Mathf.FloorToInt(spawnerPos.x - before); x <= (spawnerPos.x + before); x++)
        {
            for (int y = Mathf.FloorToInt(spawnerPos.y - before); y <= (spawnerPos.y + before); y++)
            {
                if (map.IsOnMapData(x, y))
                {
                    Cell cell = map.mapData[x][y];
                    if (after <= corruptionClearRadius)
                    {
                        if (cell.corruptionObj != null)
                            Destroy(cell.corruptionObj);
                    }
                    else
                    {
                        tempPos = new Vector2(x, y);
                        float dist = Vector2.Distance(tempPos, spawnerPos);
                        if (dist < before && dist >= after)
                        {
                            if (cell.corruptionObj != null)
                                Destroy(cell.corruptionObj);
                        }
                    }
                }
            }
        }
    }

    void SetMapBorderCol()
    {
        int offsetY = 0;
        offsetY = height + clientMapOffsetY;

        Vector2[] colliderPoints = new Vector2[5];
        colliderPoints[0] = new Vector2(0, 0);
        colliderPoints[1] = new Vector2(0, height);
        colliderPoints[2] = new Vector2(width, height);
        colliderPoints[3] = new Vector2(width, 0);
        colliderPoints[4] = new Vector2(0, 0);
        hostMapCol.points = colliderPoints;

        colliderPoints[0] = new Vector2(0, offsetY);
        colliderPoints[1] = new Vector2(0, height + offsetY);
        colliderPoints[2] = new Vector2(width, height + offsetY);
        colliderPoints[3] = new Vector2(width, offsetY);
        colliderPoints[4] = new Vector2(0, offsetY);
        clientMapCol.points = colliderPoints;
    }

    public Vector3 GetNearGroundPos(Vector3 playerPos)
    {
        Map map = (GameManager.instance.isPlayerInHostMap) ? hostMap : clientMap;
        int posX = Mathf.FloorToInt(playerPos.x);
        int posY = Mathf.FloorToInt(playerPos.y);
        int range = 3;

        if (map.IsOnMap(posX, posY))
        {
            Cell cell = map.GetCellDataFromPos(posX, posY);
            if (cell.biome != cliff && cell.biome != lake)
            {
                return new Vector3(posX, posY, 0);
            }
        }
        
        while (true)
        {
            for (int i = posX - range; i <= posX + range; i++)
            {
                for (int j = posY - range; j <= posY + range; j++)
                {
                    if (map.IsOnMap(i, j))
                    {
                        Cell cell = map.GetCellDataFromPos(i, j);
                        if (cell.biome != cliff && cell.biome != lake)
                        {
                            return new Vector3(i, j, 0);
                        }
                    }
                }
            }

            range += 2;
        }
    }
}