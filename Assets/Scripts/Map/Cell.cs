using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// UTF-8 설정
public class Cell
{
    public Tile tile;
    public string tileType;
    public Biome biome;
    public Resource resource;
    public GameObject obj;
    public GameObject structure;
    public List<string> buildable = new List<string>();
    public int x;
    public int y;

    public bool isCorrupted;
    public GameObject corruptionObj;

    //0: 기본, 1: 외곽, 2: 위쪽 경계, 3: 왼쪽 위 안쪽 코너, 4: 오른쪽 위 안쪽 코너, 5: 왼쪽 아래 안쪽 코너, 6: 오른쪽 아래 안쪽 코너
    public int cliffType = -1;
    public bool spawnArea = false;
    
    public bool BuildCheck(string str)
    {
        bool build = false;
        if (buildable.Contains(str))
        {
            build = true;
        }
        return build;
    }
}
