using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    public Tilemap tilemap;
    public Tile tile1;
    public Tile tile2;
    public Tile tile3;
    public Tile tile4;
    
    // 나중에 타일셋 <int,List<Tile>>로 받아서 바이옴별 랜덤타일 설정할 것
    Dictionary<int, Tile> tileset;
    int x_offset = 0;
    int y_offset = 0;
    int map_width = 160;
    int map_height = 90;
    float magnification = 7.0f;

    void Start()
    {
        CreateTileset();
        GenerateMap();
    }

    void CreateTileset()
    {
        tileset = new Dictionary<int, Tile>();
        tileset.Add(0, tile1);
        tileset.Add(1, tile2);
        tileset.Add(2, tile3);
        tileset.Add(3, tile4);
    }

    void GenerateMap()
    {
        for (int x = 0; x < map_width; x++)
        {
            for (int y = 0; y < map_height; y++)
            {
                int tile_id = GetIdUsingPerlin(x, y);
                CreateTile(tile_id, x, y);
            }
        }
    }

    int GetIdUsingPerlin(int x, int y)
    {
        float raw_perlin = Mathf.PerlinNoise(
            (x - x_offset) / magnification,
            (y - y_offset) / magnification
        );
        float clamp_perlin = Mathf.Clamp01(raw_perlin);
        float scaled_perlin = clamp_perlin * tileset.Count;

        // FloorToInt에서 터지지 않게 값 조정
        if (scaled_perlin == tileset.Count)
            scaled_perlin = (tileset.Count - 1);

        return Mathf.FloorToInt(scaled_perlin);
    }

    void CreateTile(int tile_id, int x, int y)
    {
        Tile tile = tileset[tile_id];
        tilemap.SetTile(new Vector3Int(x, y, 0), tile);
    }
}
