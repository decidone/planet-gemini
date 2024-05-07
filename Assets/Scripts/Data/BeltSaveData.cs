using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BeltSaveData
{
    /*
     * 레벨, 좌표값, 방향, hp, 가지고 있는 아이템, 행성 정보는 StructureSaveData에 저장
     */

    public int modelMotion;     // 모양 번호
    public bool isTrun;         // 회전된 상태인지
    public bool isRightTurn;    // 회전된 상태일때 오른쪽인지
    public int beltState;       // 벨트의 상태정보

    public List<SerializedVector3> itemPos = new List<SerializedVector3>(); // 벨트위 아이템 위치
    public List<int> itemIndex = new List<int>();   // 벨트위 아이템 인덱스
}