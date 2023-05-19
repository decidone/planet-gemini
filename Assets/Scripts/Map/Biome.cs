using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Biome : MonoBehaviour
{
    public List<Tile> tiles;
    public List<Tile> plantTiles;
    public List<GameObject> objects;

    public Tile SetTile(System.Random random)
    {
        Tile tile;
        if (random.Next(0, 100) > 90 && plantTiles.Count > 0)
        {
            tile = plantTiles[random.Next(0, plantTiles.Count)];
        }
        else
        {
            tile = tiles[random.Next(0, tiles.Count)];
        }

        return tile;
    }

    public GameObject SetObject(System.Random random)
    {
        GameObject obj = null;
        if (random.Next(0, 100) > 98 && objects.Count > 0)
        {
            obj = objects[random.Next(0, objects.Count)];
        }

        return obj;
    }
}
