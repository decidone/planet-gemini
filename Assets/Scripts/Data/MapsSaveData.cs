using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapsSaveData
{
    public int seed;
    public int width;
    public int height;
    public int offsetY;
    public int[,] fogState;

    public MapSaveData hostMap;
    public MapSaveData clientMap;
}

