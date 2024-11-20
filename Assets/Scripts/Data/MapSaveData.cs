using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapSaveData
{
    /*
     * 시드, 맵 사이즈, 멀티 플레이 행성 젠 여부
     * 지금은 없는데 추가할 기능 관련 - 파괴된 오브젝트(나무) 좌표값, 절벽에 다리를 깔았을 경우 처리
     */

    public List<SerializedVector3> objects = new List<SerializedVector3>();
    public int[,] fogState;
}
