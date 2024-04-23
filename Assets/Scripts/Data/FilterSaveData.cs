using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FilterSaveData
{
    public int filterItemIndex = -1;    // 기본 -1, 필터링할 아이템 인덱스
    public bool filterOn = false;       // 필터 on/off
    public bool filterInvert = false;   // 필터 반전
}
