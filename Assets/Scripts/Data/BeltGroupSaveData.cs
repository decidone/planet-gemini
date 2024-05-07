using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BeltGroupSaveData
{
    /*
     * 그룹에 속한 벨트 리스트
     * 벨트 위에 생성된 아이템은 자동이면 여기 넣을필요 없고 아니면 따로 벨트용 아이템 세이브 데이터를 만들어야 함
     */

    public List<(BeltSaveData, StructureSaveData)> beltList = new List<(BeltSaveData, StructureSaveData)>();
}
