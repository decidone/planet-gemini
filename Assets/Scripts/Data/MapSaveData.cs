using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapSaveData
{
    public CellSaveData[,] mapData;
    public int spawnTileX;
    public int spawnTileY;
}
