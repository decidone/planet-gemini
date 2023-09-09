using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// UTF-8 설정
public class Cell
{
    public Tile tile;
    public Biome biome;
    public string resource;
    public GameObject obj;
    public GameObject structure;
    public List<string> buildable = new List<string>();

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
