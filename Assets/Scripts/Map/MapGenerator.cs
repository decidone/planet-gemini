using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    System.Random random;
    public bool randomSeed;
    public int seed;
    public int width;
    public int height;
    public float magnification;
    public float oreMagnification;
    public float oreSize;
    public bool hostMap;

    [Space]
    public Tilemap tilemap;
    public Tilemap lakeTile;
    public GameObject objects;
    public Map map;

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
    [Header("Ores")]
    List<List<GameObject>> ores;
    public List<GameObject> gold;
    public List<GameObject> iron;

    void Start()
    {
        Init();
        Generate();
    }

    void Init()
    {
        map.width = width;
        map.height = height;
        map.mapData = new List<List<Cell>>();

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

        ores = new List<List<GameObject>>();
        ores.Add(gold);
        ores.Add(iron);
    }

    void Generate()
    {
        SetRandomSeed();
        GenerateMap();
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
        CreateOre();
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
                    lakeTile.SetTile(new Vector3Int(x, y, 0), tile.tile);
                    if (tile.form == "side")
                        cell.buildable.Add("pump");
                }
                else
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), tile.tile);
                }
            }
        }
    }

    void CreateOre()
    {
        for (int i = 0; i < ores.Count; i++)
        {
            string genOre = ores[i][0].gameObject.GetComponent<ObjData>().item.name;
            Debug.Log("ore gen : " + genOre);

            int oreX = random.Next(0, 1000000);
            int oreY = random.Next(0, 1000000);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float oreNoise = Mathf.PerlinNoise(
                        (x - oreX) / oreMagnification,
                        (y - oreY) / oreMagnification
                    );

                    if (oreNoise < oreSize)
                    {
                        Cell cell = map.mapData[x][y];
                        Biome biome = cell.biome;
                        GameObject ore = ores[i][random.Next(0, ores[i].Count)];
                        if (cell.obj == null && ore != null)
                        {
                            GameObject objInst = Instantiate(ore, objects.transform);
                            cell.obj = objInst;
                            cell.buildable.Add("miner");
                            objInst.name = string.Format("map_x{0}_y{1}", x, y);
                            objInst.transform.localPosition = new Vector3((float)(x + 0.5), (float)(y + 0.5), 0);
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