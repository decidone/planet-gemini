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
    public Tilemap tilemap;
    List<List<Cell>> mapData;

    [Space]
    [Header("Biomes")]
    public Biome plain;
    public Biome desert;
    public Biome frozen;
    public Biome jungle;
    public Biome snow;

    public List<List<Biome>> biomes;

    void Start()
    {
        Init();
        Generate();
    }

    void Init()
    {
        mapData = new List<List<Cell>>();

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
            mapData.Add(new List<Cell>());

            for (int y = 0; y < height; y++)
            {
                SetBiome(x, y);
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CreateTile(x, y);
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
        mapData[x].Add(cell);
    }

    void CreateTile(int x, int y)
    {
        Cell cell = mapData[x][y];
        Biome biome = cell.biome;
        Tile tile = biome.tiles[random.Next(0, biome.tiles.Count)];
        cell.tile = tile;
        tilemap.SetTile(new Vector3Int(x, y, 0), tile);
    }
}