using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// UTF-8 설정
public class Biome : MonoBehaviour
{
    public string biome;
    public string diffBiome;

    [Space]
    [Header("Tiles")]
    public List<Tile> tiles;
    public List<Tile> pointTiles;
    public List<Tile> exceptionalTiles;
    public List<Tile> top;
    public List<Tile> left;
    public List<Tile> right;
    public List<Tile> bottom;
    public List<Tile> corner;
    public List<Tile> innerCorner;

    [Space]
    [Header("Objects")]
    public List<GameObject> objects;
    
    public (Tile tile, string form) SetTile(System.Random random, Map map, int x, int y)
    {
        Tile tile = null;
        string form = "corner";
        bool isBorder = false;
        if (exceptionalTiles.Count > 0)
        {
            List<Cell> neighbors = new List<Cell>();
            List<int> nearDiff = new List<int>();
            List<int> diagonalDiff = new List<int>();

            for (int i = 0; i < 8; i++)
                neighbors.Add(new Cell());

            for (int i = 0; i < 9; i++)
            {
                int nx = x + (i % 3) - 1;
                int ny = y + -((i / 3) - 1);

                if (i != 4)
                {
                    int j = (i < 4) ? i : i - 1;
                    if (map.IsOnMap(nx, ny))
                    {
                        neighbors[j] = map.mapData[nx][ny];
                        if (neighbors[j].biome.biome == diffBiome)
                        {
                            if (i % 2 == 0)
                            {
                                diagonalDiff.Add(j);
                            }
                            else
                            {
                                nearDiff.Add(j);
                            }
                        }
                    }
                }
            }

            if (nearDiff.Count > 0 || diagonalDiff.Count > 0)
            {
                isBorder = true;
                if (nearDiff.Count == 1)
                {
                    form = "side";
                    if (nearDiff.Contains(1) && top.Count > 0)
                    {
                        tile = top[random.Next(0, top.Count)];
                    }
                    else if (nearDiff.Contains(3) && left.Count > 0)
                    {
                        tile = left[random.Next(0, left.Count)];
                    }
                    else if (nearDiff.Contains(4) && right.Count > 0)
                    {
                        tile = right[random.Next(0, right.Count)];
                    }
                    else if (nearDiff.Contains(6) && bottom.Count > 0)
                    {
                        tile = bottom[random.Next(0, bottom.Count)];
                    }
                }
                else if (nearDiff.Count == 2 && corner.Count > 0)
                {
                    if (nearDiff.Contains(1) && nearDiff.Contains(3))
                    {
                        tile = corner[0];
                    }
                    else if (nearDiff.Contains(1) && nearDiff.Contains(4))
                    {
                        tile = corner[1];
                    }
                    else if (nearDiff.Contains(6) && nearDiff.Contains(3))
                    {
                        tile = corner[2];
                    }
                    else if (nearDiff.Contains(6) && nearDiff.Contains(4))
                    {
                        tile = corner[3];
                    }
                }
                else if (nearDiff.Count == 0 && diagonalDiff.Count == 1 && innerCorner.Count > 0)
                {
                    if (diagonalDiff.Contains(0))
                    {
                        tile = innerCorner[0];
                    }
                    else if (diagonalDiff.Contains(2))
                    {
                        tile = innerCorner[1];
                    }
                    else if (diagonalDiff.Contains(5))
                    {
                        tile = innerCorner[2];
                    }
                    else if (diagonalDiff.Contains(7))
                    {
                        tile = innerCorner[3];
                    }
                }

                if (tile == null && exceptionalTiles.Count > 0)
                {
                    tile = exceptionalTiles[random.Next(0, exceptionalTiles.Count)];
                }
            }
        }

        if (!isBorder)
        {
            if (random.Next(0, 100) > 90 && pointTiles.Count > 0)
            {
                tile = pointTiles[random.Next(0, pointTiles.Count)];
            }
            else
            {
                tile = tiles[random.Next(0, tiles.Count)];
            }
        }

        return (tile, form);
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

    public void BiomeSmoother(Map map, int x, int y)
    {
        List<Cell> neighbors = new List<Cell>();
        List<int> nearDiff = new List<int>();
        List<int> diagonalDiff = new List<int>();

        for (int i = 0; i < 8; i++)
            neighbors.Add(new Cell());

        for (int i = 0; i < 9; i++)
        {
            int nx = x + (i % 3) - 1;
            int ny = y + -((i / 3) - 1);

            if (i != 4)
            {
                int j = (i < 4) ? i : i - 1;
                if (map.IsOnMap(nx, ny))
                {
                    neighbors[j] = map.mapData[nx][ny];
                    if (neighbors[j].biome.biome == diffBiome)
                    {
                        if (i % 2 == 0)
                        {
                            diagonalDiff.Add(j);
                        }
                        else
                        {
                            nearDiff.Add(j);
                        }
                    }
                }
            }
        }

        if (nearDiff.Count >= 3)
        {
            map.mapData[x][y].biome = neighbors[nearDiff[0]].biome;
        }
    }
}
