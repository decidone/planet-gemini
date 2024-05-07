using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Map Data", menuName = "Data/Map Data", order = int.MaxValue)]
public class MapSizeData : ScriptableObject
{
    [SerializeField]
    private int mapSize;
    public int MapSize { get { return mapSize; } }

    [SerializeField]
    private int mapSplitCount;
    public int MapSplitCount { get { return mapSplitCount; } }

}
