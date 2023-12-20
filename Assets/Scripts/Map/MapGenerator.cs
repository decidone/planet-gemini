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
    public int width;
    public int height;
    public float magnification;
    public bool hostMap;

    [Space]
    public Tilemap tilemap;
    public Tilemap lakeTilemap;
    public Tilemap resourcesTilemap;
    public Tilemap resourcesIconTilemap;
    public GameObject objects;
    public Map map;
    public GameObject mapFog;

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

    SpawnerSetManager spawnerPosSet;

    void Awake()
    {
        map.width = width;
        map.height = height;
        map.mapData = new List<List<Cell>>();
        isCompositeDone = false;
        comp = lakeTilemap.GetComponent<CompositeCollider2D>();
    }

    void Start()
    {
        Init();
        Generate();
        SetSpawnPos();
        spawnerPosSet = SpawnerSetManager.instance;
        if(spawnerPosSet)
            spawnerPosSet.AreaMapSet();
        mapFog.transform.position = new Vector3(width / 2, height / 2, 0);
        mapFog.transform.localScale = new Vector3(width, height, 1);
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

    void Init()
    {
        if (hostMap)
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

    void Generate()
    {
        SetRandomSeed();
        GenerateMap();
    }

    void SetSpawnPos()
    {
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
            dist++;

            switch (dir)
            {
                case "up": y += dist; break;
                case "down": y -= dist; break;
                case "left": x -= dist; break;
                case "right": x += dist; break;
            }
        }

        GameManager.instance.SetPlayerPos(x, y);
    }

    void SetRandomSeed()
    {
        if (randomSeed)
            seed = Random.Range(int.MinValue, int.MaxValue);

        random = new System.Random(seed);
    }

    void GenerateMap()
    {
        SetBiome();
        SmoothBiome();
        CreateTile();
        CreateResource();
        CreateObj();
    }

    void SetBiome()
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

    void SmoothBiome()
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

    void CreateTile()
    {
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
                    lakeTilemap.SetTile(new Vector3Int(x, y, 0), tile.tile);
                    if (tile.form == "side")
                        cell.buildable.Add("pump");
                    else
                        cell.buildable.Add("none");
                }
                else
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), tile.tile);
                }
            }
        }
    }

    void CreateResource()
    {
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
                            resourcesTilemap.SetTile(new Vector3Int(x, y, 0), resourceTile);
                            resourcesIconTilemap.SetTile(new Vector3Int(x, y, 0), resourcesIcon[i]);
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

    void CreateObj()
    {
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
                        objInst.transform.localPosition = new Vector3((float)(x + 0.5), (float)(y + 0.5), 0);
                    }
                }
            }
        }
    }
}