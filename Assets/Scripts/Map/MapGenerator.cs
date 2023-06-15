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
    int tempX;
    int tempY;
    int heightX;
    int heightY;

    [Space]
    public Tilemap tilemap;
    public GameObject objects;
    public Map map;

    [Space]
    [Header("Biomes")]
    public Biome plain;
    public Biome desert;
    public Biome jungle;
    public Biome snow;
    public Biome frozen;
    List<List<Biome>> biomes;

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

        // x축 온도, y축 높이
        biomes = new List<List<Biome>>() {
            new List<Biome> { desert, desert, jungle, plain, plain, plain, frozen, frozen },
            new List<Biome> { desert, desert, jungle, plain, plain, plain, frozen, frozen },
            new List<Biome> { desert, desert, jungle, plain, plain, plain, snow, snow },
            new List<Biome> { desert, desert, jungle, plain, plain, plain, snow, snow },
            new List<Biome> { desert, desert, jungle, plain, plain, plain, snow, snow },
            new List<Biome> { desert, desert, jungle, plain, plain, plain, snow, snow },
            new List<Biome> { desert, desert, jungle, plain, plain, plain, snow, snow },
            new List<Biome> { desert, desert, jungle, plain, plain, plain, snow, snow },
        };
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
        tempX = random.Next(0, 1000000);
        tempY = random.Next(0, 1000000);
        heightX = random.Next(0, 1000000);
        heightY = random.Next(0, 1000000);

        for (int x = 0; x < width; x++)
        {
            map.mapData.Add(new List<Cell>());

            for (int y = 0; y < height; y++)
            {
                SetBiome(x, y);
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = map.mapData[x][y];
                Biome biome = cell.biome;
                biome.BiomeSmoother(map, x, y);
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CreateTile(x, y);
                CreateObj(x, y);
            }
        }
    }

    void SetBiome(int x, int y)
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

    void CreateTile(int x, int y)
    {
        Cell cell = map.mapData[x][y];
        Biome biome = cell.biome;
        Tile tile = biome.SetTile(random, map, x, y);
        cell.tile = tile;
        tilemap.SetTile(new Vector3Int(x, y, 0), tile);
    }

    void CreateObj(int x, int y)
    {
        Cell cell = map.mapData[x][y];
        Biome biome = cell.biome;
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