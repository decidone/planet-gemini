using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Biome : MonoBehaviour
{
    public List<Tile> tiles;
    public List<Tile> plantTiles;

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
}
