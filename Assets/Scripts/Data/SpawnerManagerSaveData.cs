using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpawnerManagerSaveData
{
    public int splitCount;  //  구역 나누기

    public SpawnerGroupData[,] spawnerMap1Matrix;    // 호스트맵 스포너 그룹 배열
    public SpawnerGroupData[,] spawnerMap2Matrix;    // 클라이언트맵 스포너 그룹 배열

    bool hostMapWave;
    bool clientMapWave;
}