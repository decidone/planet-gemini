using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map
{
    public int width;
    public int height;
    public List<List<Cell>> mapData;

    public bool IsOnMap(int x, int y)
    {
        bool isOnMap = (0 <= x && x < width) && (0 <= y && y < height);
        return isOnMap;
    }
}