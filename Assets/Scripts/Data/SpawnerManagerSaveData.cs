using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpawnerManagerSaveData
{
    public int splitCount;  //  구역 나누기

    public SpawnerGroupData[,] spawnerMap1Matrix;   // 호스트맵 스포너 그룹 배열
    public SpawnerGroupData[,] spawnerMap2Matrix;   // 클라이언트맵 스포너 그룹 배열

    public bool waveState;                          // 웨이브 상태
    public bool hostMapWave;                        // true : hostMap, false : clientMap
    public SerializedVector3 wavePos = new SerializedVector3(); // 웨이브 위치
}