using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpawnerGroupData
{
    public SerializedVector3 pos = new SerializedVector3();
    public (int, int) spawnerMatrixIndex;   // 그룹 배열 위치
    public List<SpawnerSaveData> spawnerSaveDataList = new List<SpawnerSaveData>();
}
